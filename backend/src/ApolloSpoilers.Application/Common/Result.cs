namespace ApolloSpoilers.Application.Common;

/// <summary>
/// Operation outcome pattern. Carries success/failure + payload without throwing,
/// so controllers map cleanly to HTTP codes.
/// </summary>
public class Result
{
    public bool IsSuccess { get; }
    public string? Error { get; }
    public string ErrorCode { get; }

    protected Result(bool isSuccess, string? error, string errorCode)
    {
        if (isSuccess && error != null) throw new InvalidOperationException("Success result cannot carry an error.");
        if (!isSuccess && error == null) throw new InvalidOperationException("Failure result must carry an error.");
        IsSuccess = isSuccess;
        Error = error;
        ErrorCode = errorCode;
    }

    public static Result Success() => new(true, null, "OK");
    public static Result Failure(string error, string code = "FAILURE") => new(false, error, code);
    public static Result<T> Success<T>(T value) => new(value, true, null, "OK");
    public static Result<T> Failure<T>(string error, string code = "FAILURE") => new(default, false, error, code);
}

public class Result<T> : Result
{
    public T? Value { get; }

    internal Result(T? value, bool isSuccess, string? error, string errorCode)
        : base(isSuccess, error, errorCode)
    {
        Value = value;
    }
}
