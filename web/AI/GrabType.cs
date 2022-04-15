namespace GrpcGreeterServerCodeFirst.AI
{
    public enum GrabType
    {
        Never = 0,
        OnError,
        Always,
    }

    public enum RequestLogSource
    {
        Request,
        RequestAndResponse,
    }
}
