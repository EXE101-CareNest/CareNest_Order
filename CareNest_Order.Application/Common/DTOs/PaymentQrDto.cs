using System.Text.Json.Serialization;

namespace CareNest_Order.Application.Common.DTOs
{
    public class PaymentQrDto
    {
        [JsonPropertyName("qrCode")]
        public string QrCode { get; set; } = string.Empty;

        [JsonPropertyName("qrImageUrl")]
        public string QrImageUrl { get; set; } = string.Empty;

        [JsonPropertyName("vietQRCode")]
        public string VietQRCode { get; set; } = string.Empty;

        [JsonPropertyName("bankName")]
        public string BankName { get; set; } = string.Empty;

        [JsonPropertyName("bankCode")]
        public string BankCode { get; set; } = string.Empty;

        [JsonPropertyName("accountNumber")]
        public string AccountNumber { get; set; } = string.Empty;

        [JsonPropertyName("accountName")]
        public string AccountName { get; set; } = string.Empty;

        [JsonPropertyName("amount")]
        public double Amount { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("orderId")]
        public string OrderId { get; set; } = string.Empty;

        [JsonPropertyName("template")]
        public string Template { get; set; } = "compact";

        [JsonPropertyName("download")]
        public bool Download { get; set; } = false;

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("expiresAt")]
        public DateTime ExpiresAt { get; set; }
    }

    public class PaymentQrResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("data")]
        public PaymentQrDto? Data { get; set; }

        [JsonPropertyName("errors")]
        public List<string> Errors { get; set; } = new();
    }
}
