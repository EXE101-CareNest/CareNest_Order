using CareNest_Order.Application.Common;
using CareNest_Order.Application.Common.Options;
using CareNest_Order.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;

namespace CareNest_Order.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly HttpClient _httpClient;
        private readonly APIServiceOption _apiOptions;
        private readonly ILogger<EmailService> _logger;

        public EmailService(HttpClient httpClient, IOptions<APIServiceOption> apiOptions, ILogger<EmailService> logger)
        {
            _httpClient = httpClient;
            _apiOptions = apiOptions.Value;
            _logger = logger;
        }

        public async Task<ResponseResult<object>> SendAppointmentConfirmationEmailAsync(
            string customerId,
            string customerName,
            string shopName,
            string appointmentId,
            string startTime,
            double totalAmount,
            List<object> details)
        {
            if (string.IsNullOrWhiteSpace(_apiOptions.BaseUrlAuthorize))
            {
                return ResponseResult<object>.Failure("Missing APIService.BaseUrlAuthorize configuration");
            }

            // For Order module: use order-specific wording
            string subject = string.IsNullOrWhiteSpace(shopName)
                ? $"Xác nhận đơn hàng #{appointmentId}"
                : $"[{shopName}] Xác nhận đơn hàng #{appointmentId}";
            string authorizeEndpoint = $"{_apiOptions.BaseUrlAuthorize}/email/send-mail?userId={Uri.EscapeDataString(customerId)}&subject={Uri.EscapeDataString(subject)}";

            string html = GenerateAppointmentConfirmationHtml(customerName, shopName, appointmentId, startTime, totalAmount, details);

            using var content = new StringContent(html, Encoding.UTF8, "text/html");
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));

            try
            {
                _logger.LogInformation("Sending email via Authorize: url={Url}, userId={UserId}, subject={Subject}, htmlLength={Length}", authorizeEndpoint, customerId, subject, html.Length);

                var response = await _httpClient.PostAsync(authorizeEndpoint, content);
                var body = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return ResponseResult<object>.Success(new { }, "Email sent successfully");
                }

                return ResponseResult<object>.Failure($"Email service returned: {(int)response.StatusCode} - {body}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email to Authorize: userId={UserId}, subject={Subject}", customerId, subject);
                return ResponseResult<object>.Failure($"Error sending email: {ex.Message}");
            }
        }

        private static string GenerateAppointmentConfirmationHtml(
            string customerName,
            string shopName,
            string appointmentId,
            string startTime,
            double totalAmount,
            List<object> details)
        {
            // Simple HTML template; can be replaced by Razor/Handlebars later
            var builder = new StringBuilder();
            builder.Append("<html><head><meta charset=\"utf-8\"/></head><body>");
            builder.Append($"<h2>Xác nhận đơn hàng thành công</h2>");
            builder.Append($"<p>Xin chào {System.Net.WebUtility.HtmlEncode(customerName)},</p>");
            builder.Append("<table border=\"1\" cellpadding=\"6\" cellspacing=\"0\">");
            builder.Append($"<tr><td>Mã đơn hàng</td><td>{System.Net.WebUtility.HtmlEncode(appointmentId)}</td></tr>");
            if (!string.IsNullOrWhiteSpace(shopName))
            {
                builder.Append($"<tr><td>Cửa hàng</td><td>{System.Net.WebUtility.HtmlEncode(shopName)}</td></tr>");
            }
            builder.Append($"<tr><td>Thời gian tạo</td><td>{System.Net.WebUtility.HtmlEncode(startTime)}</td></tr>");
            builder.Append($"<tr><td>Tổng tiền</td><td>{totalAmount:N0} VNĐ</td></tr>");
            builder.Append("</table><br/>");

            builder.Append("<h3>Chi tiết đơn hàng</h3>");
            builder.Append("<table border=\"1\" cellpadding=\"6\" cellspacing=\"0\">");
            builder.Append("<tr><th>Sản phẩm</th><th>Số lượng</th><th>Ghi chú</th><th>Thành tiền</th></tr>");

            foreach (var item in details)
            {
                var dict = item
                    .GetType()
                    .GetProperties()
                    .ToDictionary(p => p.Name, p => p.GetValue(item));

                // Map Order item fields only (no Appointment fallbacks)
                string name =
                    Convert.ToString(dict.FirstOrDefault(k => string.Equals(k.Key, "ProductName", StringComparison.OrdinalIgnoreCase)).Value)
                    ?? Convert.ToString(dict.FirstOrDefault(k => string.Equals(k.Key, "ProductDetailId", StringComparison.OrdinalIgnoreCase)).Value)
                    ?? string.Empty;
                string qty =
                    Convert.ToString(dict.FirstOrDefault(k => string.Equals(k.Key, "Quantity", StringComparison.OrdinalIgnoreCase)).Value)
                    ?? string.Empty;
                string note = Convert.ToString(dict.FirstOrDefault(k => string.Equals(k.Key, "Note", StringComparison.OrdinalIgnoreCase)).Value) ?? string.Empty;
                string lineTotal = Convert.ToString(dict.FirstOrDefault(k => string.Equals(k.Key, "TotalAmount", StringComparison.OrdinalIgnoreCase)).Value) ?? string.Empty;

                builder.Append("<tr>");
                builder.Append($"<td>{System.Net.WebUtility.HtmlEncode(name)}</td>");
                builder.Append($"<td>{System.Net.WebUtility.HtmlEncode(qty)}</td>");
                builder.Append($"<td>{System.Net.WebUtility.HtmlEncode(note)}</td>");
                builder.Append($"<td>{System.Net.WebUtility.HtmlEncode(lineTotal)}</td>");
                builder.Append("</tr>");
            }

            builder.Append("</table>");
            builder.Append("<p>Trân trọng,<br/>CareNest</p>");
            builder.Append("</body></html>");
            return builder.ToString();
        }
    }
}


