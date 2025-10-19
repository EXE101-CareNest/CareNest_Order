using System.Text.Json.Serialization;

namespace CareNest_Order.Application.Common
{
    public class ApiResponse<T>
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }
        
        [JsonPropertyName("message")]
        public string? Message { get; set; }
        
        [JsonPropertyName("data")]
        public T? Data { get; set; }

        public ApiResponse(bool success, string? message = null, T? data = default)
        {
            Success = success;
            Message = message;
            Data = data;
        }

        // Shortcut static methods
        public static ApiResponse<T> SuccessResponse(T data, string? message = null)
            => new(true, message, data);

        public static ApiResponse<T> Failure(string message)
            => new(false, message);
    }
}
