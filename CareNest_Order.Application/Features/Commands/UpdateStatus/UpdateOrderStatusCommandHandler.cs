using CareNest_Order.Application.Interfaces.CQRS.Commands;
using CareNest_Order.Application.Interfaces.UOW;
using CareNest_Order.Domain.Entitites;
using Shared.Helper;

namespace CareNest_Order.Application.Features.Commands.UpdateStatus
{
    public class UpdateOrderStatusCommandHandler : ICommandHandler<UpdateOrderStatusCommand, Order>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UpdateOrderStatusCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Order> HandleAsync(UpdateOrderStatusCommand command)
        {
            Order? order = await _unitOfWork.GetRepository<Order>().GetByIdAsync(command.OrderId)
                ?? throw new Exception("Order not found");

            order.Status = command.Status;
            order.UpdatedAt = TimeHelper.GetUtcNow();

            _unitOfWork.GetRepository<Order>().Update(order);
            await _unitOfWork.SaveAsync();

            return order;
        }
    }
}


