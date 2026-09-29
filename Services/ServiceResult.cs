namespace Projeto_Integrador2.Services;
public enum ResultStatus
{
    Ok,
    BadRequest,
    Conflict,
    NotFound,
    Forbidden
}
public sealed record ServiceResult(ResultStatus Status = ResultStatus.Ok, string? Error = null)
{
    public bool IsOk => Status == ResultStatus.Ok;

    public static ServiceResult Ok() => new();
    public static ServiceResult BadRequest(string error) => new(ResultStatus.BadRequest, error);
    public static ServiceResult Conflict(string error) => new(ResultStatus.Conflict, error);
    public static ServiceResult NotFound() => new(ResultStatus.NotFound);
    public static ServiceResult Forbidden() => new(ResultStatus.Forbidden);
}
public sealed record ServiceResult<T>(T? Value, ResultStatus Status = ResultStatus.Ok, string? Error = null)
{
    public bool IsOk => Status == ResultStatus.Ok;

    public static ServiceResult<T> Ok(T value) => new(value);
    public static ServiceResult<T> BadRequest(string error) => new(default, ResultStatus.BadRequest, error);
    public static ServiceResult<T> Conflict(string error) => new(default, ResultStatus.Conflict, error);
    public static ServiceResult<T> NotFound() => new(default, ResultStatus.NotFound);
    public static ServiceResult<T> Forbidden() => new(default, ResultStatus.Forbidden);
}
