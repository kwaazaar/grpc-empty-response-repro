Repro for this question: https://github.com/grpc/grpc-dotnet/issues/1679

1. Configure the solution for multi-startup debugging
2. Start debugging -> the client will run into the "Failed to deserialize response message" error
3. In appsettings.json set ApplicationInsights:LogRequests to Never
4. Start debugging -> no more errors and client works

Obviously AI/RequestGrabber.cs is the issue.
It replaces the Response.Body with a MemoryStream, so it can read the response afterwards and then copies its contents to the original response stream.
This works great for REST controllers, but somehow does not work for Grpc(-web) services.

