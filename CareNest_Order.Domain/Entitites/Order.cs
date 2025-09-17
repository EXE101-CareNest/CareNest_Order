using CareNest_Order.Domain.Commons;
using CareNest_Order.Domain.Commons.Enum;

namespace CareNest_Order.Domain.Entitites
{
    public class Order : BaseEntity
    {
        /// <summary>
        /// Id khách hàng order
        /// </summary>
        public string? CustomerId { get; set; }
        /// <summary>
        /// Id cửa hàng bán
        /// </summary>
        public string? ShopId { get; set; }
        /// <summary>
        /// Id địa chỉ giao hàng của khách hàng
        /// </summary>
        public string? ShipAddressId { get; set; }
        /// <summary>
        /// tổng tiền đơn hàng
        /// </summary>
        public double TotalAmount { get; set; }
        /// <summary>
        /// phương thức thanh toán
        /// </summary>
        public string? PaymentMethod { get; set; }
        /// <summary>
        /// ghi chú
        /// </summary>
        public string? Note { get; set; }
        /// <summary>
        /// trạng thái: Pending / Confirmed / Checkin / Processing / Finished / Cancel
        /// </summary>
        public OrderStatus? Status { get; set; } = OrderStatus.Pending;
        /// <summary>
        /// id ngân hàng
        /// </summary>
        public string? BankId { get; set; }
        /// <summary>
        /// id mã giao dịch
        /// </summary>
        public string? BankTransactionId { get; set; }
        /// <summary>
        /// check đã thanh toán chưa 
        /// </summary>
        public bool IsPaid { get; set; }
    }
}
