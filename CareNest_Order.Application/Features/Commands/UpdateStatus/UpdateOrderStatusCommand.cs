using CareNest_Order.Application.Interfaces.CQRS.Commands;
using CareNest_Order.Domain.Commons.Enum;

namespace CareNest_Order.Application.Features.Commands.UpdateStatus
{
    public class UpdateOrderStatusCommand : ICommand<CareNest_Order.Domain.Entitites.Order>
    {
        public string OrderId { get; set; } = string.Empty;
        public OrderStatus Status { get; set; }
    }
}


