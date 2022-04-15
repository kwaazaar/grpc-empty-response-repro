using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;

namespace GrpcGreeterServerCodeFirst.AI
{
    public class RequestGrabber : IMiddleware
    {
        private readonly TelemetryClient _telemetryClient;
        private readonly GrabType _grabType;
        private readonly RequestLogSource _requestLogSource;

        public RequestGrabber(TelemetryClient telemetryClient, IConfiguration configuration)
        {
            _telemetryClient = telemetryClient;
            _grabType = Enum.TryParse(configuration["ApplicationInsights:LogRequests"], true, out GrabType grabType) ? grabType : GrabType.Never;
            _requestLogSource = Enum.TryParse(configuration["ApplicationInsights:LogRequestSource"], true, out RequestLogSource requestLogSource) ? requestLogSource : RequestLogSource.Request;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var requestTelemetry = context.Features.Get<RequestTelemetry>();

            if (context?.Request == null
                || requestTelemetry == null // AI not active
                || _grabType == GrabType.Never)
            {
                await next(context!);
                return;
            }

            var request = context.Request;
            var response = context.Response;

            // Inbound (before the controller)
            request.EnableBuffering();  // Allows us to reuse the existing Request.Body

            Stream originalResponseBody = response!.Body;
            MemoryStream? replacementResponseBody = default;

            var logResponse = _requestLogSource == RequestLogSource.RequestAndResponse;

            if (logResponse)
            {
                // Swap the original Response.Body stream with one we can read / seek. After the request we swap them back
                replacementResponseBody = new ResponseBufferingStream(originalResponseBody);
                response.Body = replacementResponseBody;
            }

            try
            {
                // Continue processing (additional middleware, controller, etc.)
                await next(context);

                // Now 'grab' the request and response bodies and add them to the request telemetry
                if (_grabType == GrabType.Always
                    || _grabType == GrabType.OnError && context.Response?.StatusCode >= 500)
                {
                    if (request.Body.CanRead)
                    {
                        var requestBodyString = await ReadBodyStream(request.Body);
                        requestTelemetry.Properties.Add("RequestBody", requestBodyString);
                    }

                    if (logResponse)
                    {
                        //await context.Response.BodyWriter.FlushAsync();

                        if (replacementResponseBody!.CanRead)
                        {
                            var responseBodyString = await ReadBodyStream(replacementResponseBody);
                            requestTelemetry.Properties.Add("ResponseBody", responseBodyString);
                        }
                    }
                }

                if (logResponse)
                {
                    // Copy the response body to the original stream
                    replacementResponseBody!.Seek(0, SeekOrigin.Begin);
                    await replacementResponseBody.CopyToAsync(originalResponseBody);
                }
            }
            finally
            {
                replacementResponseBody?.Dispose();
                response.Body = originalResponseBody;
            }
        }

        private async Task<string> ReadBodyStream(Stream body, int maxChars = 8192) // limit: 8192 characters (see: https://github.com/MohanGsk/ApplicationInsights-Home/blob/master/EndpointSpecs/Schemas/Bond/RequestData.bond#L41)
        {
            if (body.Length == 0)
            {
                return default!;
            }

            var positionOnStart = body.Position;

            body.Position = 0;
            using var reader = new StreamReader(body, leaveOpen: true);
            var bodyString = await reader.ReadToEndAsync();
            body.Position = positionOnStart;

            if (bodyString.Length > maxChars)
            {
                bodyString = bodyString[..maxChars];
            }
            return bodyString;
        }
    }
}
