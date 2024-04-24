namespace Application.Common.Models;

public class Result
{
    public bool Success { get; set; }
    public List<string>? Errors { get; set; }
}

public class Result<T> where T : class
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public List<string>? Errors { get; set; }
}

public class Result<T, Y> where T : class
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public List<Y>? Errors { get; set; }
}