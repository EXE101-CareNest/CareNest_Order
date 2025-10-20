using CareNest_Order.Application.Interfaces.CQRS.Commands;
using CareNest_Order.Domain.Entitites;

namespace CareNest_Order.Application.Features.Commands.Update
{
    public class UpdateOrderStatusToCancelCommand : ICommand<Order>
    {
        public string OrderId { get; set; } = string.Empty;
    }
}
