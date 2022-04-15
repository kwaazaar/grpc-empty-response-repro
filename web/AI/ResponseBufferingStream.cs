namespace GrpcGreeterServerCodeFirst.AI
{
    // https://github.com/grpc/grpc-dotnet/issues/1679
    internal class ResponseBufferingStream : MemoryStream
    {
        private readonly Stream _originalResponseStream;

        public ResponseBufferingStream(Stream originalResponseStream)
        {
            _originalResponseStream = originalResponseStream;
        }

        public override Task FlushAsync(CancellationToken cancellationToken)
        {
            return _originalResponseStream.FlushAsync(cancellationToken);
        }
    }
}
