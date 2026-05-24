namespace TheLeague.Shared.Domain.Results;

public class Result
{
    public bool IsSuccess { get; }
    public string? Message { get; }
    public List<string> Errors { get; } = new();

    protected Result(bool isSuccess, string? message = null, List<string>? errors = null)
    {
        IsSuccess = isSuccess;
        Message = message;
        if (errors != null) Errors = errors;
    }

    public static Result Success(string? message = null) => new(true, message);
    public static Result Failure(string message) => new(false, message);
    public static Result Failure(List<string> errors) => new(false, errors: errors);
    public static Result<T> Success<T>(T data, string? message = null) => new(data, true, message);
    public static Result<T> Failure<T>(string message) => new(default, false, message);
}

public class Result<T> : Result
{
    public T? Data { get; }

    internal Result(T? data, bool isSuccess, string? message = null, List<string>? errors = null)
        : base(isSuccess, message, errors)
    {
        Data = data;
    }
}
