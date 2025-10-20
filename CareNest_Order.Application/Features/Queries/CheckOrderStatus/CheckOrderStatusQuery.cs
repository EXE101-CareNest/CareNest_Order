using CareNest_Order.Application.Interfaces.CQRS.Queries;

namespace CareNest_Order.Application.Features.Queries.CheckOrderStatus
{
    public class CheckOrderStatusQuery : IQuery<bool>
    {
        public string OrderId { get; set; } = string.Empty;
    }
}
