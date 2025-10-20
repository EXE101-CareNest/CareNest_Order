using CareNest_Order.Application.Interfaces.CQRS.Commands;
using CareNest_Order.Application.Interfaces.UOW;
using CareNest_Order.Domain.Commons.Enum;
using CareNest_Order.Domain.Entitites;

namespace CareNest_Order.Application.Features.Commands.Update
{
    public class UpdateOrderStatusToCancelCommandHandler : ICommandHandler<UpdateOrderStatusToCancelCommand, Order>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UpdateOrderStatusToCancelCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Order> HandleAsync(UpdateOrderStatusToCancelCommand command)
        {
            var order = await _unitOfWork.GetRepository<Order>().GetByIdAsync(command.OrderId);
            
            if (order == null)
            {
                throw new ArgumentException($"Order with ID {command.OrderId} not found.");
            }

            // Update status to Cancel
            order.Status = OrderStatus.Cancel;

            _unitOfWork.GetRepository<Order>().Update(order);
            await _unitOfWork.SaveAsync();

            return order;
        }
    }
}
