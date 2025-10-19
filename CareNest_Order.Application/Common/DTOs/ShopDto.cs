namespace CareNest_Order.Application.Common.DTOs
{
    /// <summary>
    /// DTO cho Shop service response
    /// </summary>
    public class ShopDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? BankAccountName { get; set; }     // Tên chủ tài khoản
        public string? BankAccountNumber { get; set; }   // Số tài khoản
        public string? BankName { get; set; }            // Tên ngân hàng (VD: Vietcombank)
        public string? BankCode { get; set; }            // Mã ngân hàng (nếu dùng chuẩn nội bộ/SDK)
    }
}
