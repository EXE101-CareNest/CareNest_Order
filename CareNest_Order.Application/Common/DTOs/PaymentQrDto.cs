namespace CareNest_Order.Application.Common.DTOs
{
    public class PaymentQrDto
    {
        public string QrCode { get; set; } = string.Empty;
        public string QrImageUrl { get; set; } = string.Empty;
        public string VietQRCode { get; set; } = string.Empty;
        public string BankName { get; set; } = string.Empty;
        public string BankCode { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public double Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public string OrderId { get; set; } = string.Empty;
        public string Template { get; set; } = "qronly";
        public bool Download { get; set; } = false;
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
    }

    public class PaymentQrResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public PaymentQrDto? Data { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}
