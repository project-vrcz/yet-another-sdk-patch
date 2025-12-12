using System;
using BestHTTP;

namespace YetAnotherPatchForVRChatSdk.Patches.NetworkResilience.Exceptions;

internal sealed class BestHttpRequestException : Exception
{
    public BestHttpRequestException(Exception innerException)
        : base("An error occurred during a BestHTTP request.", innerException)
    {
    }
}

internal sealed class BestHttpRequestAbortedException : OperationCanceledException
{
    public BestHttpRequestAbortedException()
        : base("The BestHTTP request was aborted.")
    {
    }
}

internal sealed class BestHttpRequestTimeoutException : TimeoutException
{
    public BestHttpRequestTimeoutException()
        : base("The BestHTTP request timed out.")
    {
    }
}

internal sealed class BestHttpRequestConnectionTimeoutException : TimeoutException
{
    public BestHttpRequestConnectionTimeoutException()
        : base("The BestHTTP request connection timed out.")
    {
    }
}

internal sealed class BestHttpRequestUnexpectedStateException : Exception
{
    public HTTPRequestStates State { get; }

    public BestHttpRequestUnexpectedStateException(HTTPRequestStates requestStates)
        : base("The BestHTTP request reached an unexpected state: " + requestStates)
    {
        State = requestStates;
    }
}