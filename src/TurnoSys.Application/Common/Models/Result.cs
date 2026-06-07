namespace TurnoSys.Application.Common.Models;

public class Result
{
    protected Result(bool success, string? error = null)
    {
        Success = success;
        Error = error;
    }

    public bool Success { get; }
    public bool Failure => !Success;
    public string? Error { get; }

    public static Result Ok() => new(true);
    public static Result Fail(string error) => new(false, error);

    public static Result<T> Ok<T>(T value) => Result<T>.Ok(value);
    public static Result<T> Fail<T>(string error) => Result<T>.Fail(error);
}

public class Result<T> : Result
{
    private Result(bool success, T? value, string? error = null)
        : base(success, error)
    {
        Value = value;
    }

    public T? Value { get; }

    public static Result<T> Ok(T value) => new(true, value);
    public new static Result<T> Fail(string error) => new(false, default, error);
}
