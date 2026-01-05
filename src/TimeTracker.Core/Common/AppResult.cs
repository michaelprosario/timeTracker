namespace TimeTracker.Core.Common;

public class AppResult<T>
{
    public bool Success { get; private set; }
    public T? Data { get; private set; }
    public List<string> Errors { get; private set; } = new();
    public List<string> Messages { get; private set; } = new();
    public Dictionary<string, List<string>> ValidationErrors { get; private set; } = new();
    
    public static AppResult<T> SuccessResult(T data, string? message = null)
    {
        var result = new AppResult<T>
        {
            Success = true,
            Data = data
        };
        
        if (!string.IsNullOrEmpty(message))
        {
            result.Messages.Add(message);
        }
        
        return result;
    }
    
    public static AppResult<T> FailureResult(params string[] errors)
    {
        return new AppResult<T>
        {
            Success = false,
            Errors = errors.ToList()
        };
    }
    
    public static AppResult<T> ValidationFailure(Dictionary<string, List<string>> validationErrors)
    {
        return new AppResult<T>
        {
            Success = false,
            ValidationErrors = validationErrors,
            Errors = new List<string> { "Validation failed" }
        };
    }
}

public class AppResult
{
    public bool Success { get; private set; }
    public List<string> Errors { get; private set; } = new();
    public List<string> Messages { get; private set; } = new();
    public Dictionary<string, List<string>> ValidationErrors { get; private set; } = new();
    
    public static AppResult SuccessResult(string? message = null)
    {
        var result = new AppResult
        {
            Success = true
        };
        
        if (!string.IsNullOrEmpty(message))
        {
            result.Messages.Add(message);
        }
        
        return result;
    }
    
    public static AppResult FailureResult(params string[] errors)
    {
        return new AppResult
        {
            Success = false,
            Errors = errors.ToList()
        };
    }
    
    public static AppResult ValidationFailure(Dictionary<string, List<string>> validationErrors)
    {
        return new AppResult
        {
            Success = false,
            ValidationErrors = validationErrors,
            Errors = new List<string> { "Validation failed" }
        };
    }
}
