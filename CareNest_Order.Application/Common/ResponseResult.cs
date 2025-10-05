namespace CareNest_Order.Application.Common
{
    public class ResponseResult<T>
    {
        public bool IsSuccess { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }
        public List<string> Errors { get; set; } = new();

        public ResponseResult(bool isSuccess, string? message = null, T? data = default)
        {
            IsSuccess = isSuccess;
            Message = message;
            Data = data;
        }

        public static ResponseResult<T> Success(T data, string? message = null)
            => new(true, message, data);

        public static ResponseResult<T> Failure(string message, List<string>? errors = null)
            => new(false, message) { Errors = errors ?? new List<string>() };
    }
}


