using CareNest_Order.Application.Common;
using CareNest_Order.Application.Common.Options;
using CareNest_Order.Application.Interfaces.Services;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

namespace CareNest_Order.Infrastructure.Services
{
    public class APIService : IAPIService
    {
        private readonly HttpClient _httpClient;
        private readonly APIServiceOption _option;

        public APIService(HttpClient httpClient, IOptions<APIServiceOption> option)
        {
            _httpClient = httpClient;
            _option = option.Value;
        }

        public async Task<ResponseResult<T>> GetAsync<T>(string serviceType, string endpoint)
        {
            try
            {
                var baseUrl = GetBaseUrl(serviceType);
                var fullUrl = $"{baseUrl}{endpoint}";

                var response = await _httpClient.GetAsync(fullUrl);
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<T>>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (apiResponse?.Success == true)
                    {
                        return ResponseResult<T>.Success(apiResponse.Data!, apiResponse.Message);
                    }
                    else
                    {
                        return ResponseResult<T>.Failure(apiResponse?.Message ?? "API call failed");
                    }
                }
                else
                {
                    return ResponseResult<T>.Failure($"HTTP {response.StatusCode}: {content}");
                }
            }
            catch (Exception ex)
            {
                return ResponseResult<T>.Failure($"API call failed: {ex.Message}");
            }
        }

        public async Task<ResponseResult<T>> PostAsync<T>(string serviceType, string endpoint, object data)
        {
            try
            {
                var baseUrl = GetBaseUrl(serviceType);
                var fullUrl = $"{baseUrl}{endpoint}";

                var json = JsonSerializer.Serialize(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(fullUrl, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<T>>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (apiResponse?.Success == true)
                    {
                        return ResponseResult<T>.Success(apiResponse.Data!, apiResponse.Message);
                    }
                    else
                    {
                        return ResponseResult<T>.Failure(apiResponse?.Message ?? "API call failed");
                    }
                }
                else
                {
                    return ResponseResult<T>.Failure($"HTTP {response.StatusCode}: {responseContent}");
                }
            }
            catch (Exception ex)
            {
                return ResponseResult<T>.Failure($"API call failed: {ex.Message}");
            }
        }

        public async Task<ResponseResult<T>> PutAsync<T>(string serviceType, string endpoint, object data)
        {
            try
            {
                var baseUrl = GetBaseUrl(serviceType);
                var fullUrl = $"{baseUrl}{endpoint}";

                var json = JsonSerializer.Serialize(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync(fullUrl, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<T>>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (apiResponse?.Success == true)
                    {
                        return ResponseResult<T>.Success(apiResponse.Data!, apiResponse.Message);
                    }
                    else
                    {
                        return ResponseResult<T>.Failure(apiResponse?.Message ?? "API call failed");
                    }
                }
                else
                {
                    return ResponseResult<T>.Failure($"HTTP {response.StatusCode}: {responseContent}");
                }
            }
            catch (Exception ex)
            {
                return ResponseResult<T>.Failure($"API call failed: {ex.Message}");
            }
        }

        public async Task<ResponseResult<T>> DeleteAsync<T>(string serviceType, string endpoint)
        {
            try
            {
                var baseUrl = GetBaseUrl(serviceType);
                var fullUrl = $"{baseUrl}{endpoint}";

                var response = await _httpClient.DeleteAsync(fullUrl);
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<T>>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (apiResponse?.Success == true)
                    {
                        return ResponseResult<T>.Success(apiResponse.Data!, apiResponse.Message);
                    }
                    else
                    {
                        return ResponseResult<T>.Failure(apiResponse?.Message ?? "API call failed");
                    }
                }
                else
                {
                    return ResponseResult<T>.Failure($"HTTP {response.StatusCode}: {content}");
                }
            }
            catch (Exception ex)
            {
                return ResponseResult<T>.Failure($"API call failed: {ex.Message}");
            }
        }

        public async Task<ResponseResult<T>> PostAsyncDirect<T>(string serviceType, string endpoint, object data)
        {
            try
            {
                var baseUrl = GetBaseUrl(serviceType);
                var fullUrl = $"{baseUrl}{endpoint}";

                var json = JsonSerializer.Serialize(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(fullUrl, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    // Deserialize trực tiếp thành T thay vì ApiResponse<T>
                    var result = JsonSerializer.Deserialize<T>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return ResponseResult<T>.Success(result!, "Success");
                }
                else
                {
                    return ResponseResult<T>.Failure($"HTTP {response.StatusCode}: {responseContent}");
                }
            }
            catch (Exception ex)
            {
                return ResponseResult<T>.Failure($"API call failed: {ex.Message}");
            }
        }

        private string GetBaseUrl(string serviceType)
        {
            return serviceType.ToLower() switch
            {
                "orderdetail" => _option.BaseUrlOrderDetail,
                "shop" => _option.BaseUrlShop,
                "address" => _option.BaseUrlAddress,
                "product" => _option.BaseUrlProduct,
                "authorize" => _option.BaseUrlAuthorize,
                "payment" => _option.BaseUrlPay,
                _ => throw new ArgumentException($"Service type '{serviceType}' không hợp lệ!", nameof(serviceType))
            };
        }
    }
}


