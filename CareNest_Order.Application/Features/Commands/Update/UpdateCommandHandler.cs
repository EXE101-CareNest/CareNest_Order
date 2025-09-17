using CareNest_Order.Application.Exceptions;
using CareNest_Order.Application.Exceptions.Validators;
using CareNest_Order.Application.Interfaces.CQRS.Commands;
using CareNest_Order.Application.Interfaces.UOW;
using CareNest_Order.Domain.Commons.Constant;
using CareNest_Order.Domain.Entitites;
using Shared.Helper;

namespace CareNest_Order.Application.Features.Commands.Update
{
    public class UpdateCommandHandler : ICommandHandler<UpdateCommand, Order>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UpdateCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Order> HandleAsync(UpdateCommand command)
        {
            // Gọi validator để kiểm tra dữ liệu
            Validate.ValidateUpdate(command);

            // Tìm để cập nhật
            Order? order = await _unitOfWork.GetRepository<Order>().GetByIdAsync(command.Id)
               ?? throw new BadRequestException("Id: " + MessageConstant.NotFound);

            order.Note = command.Note;
            order.Status = command.Status;
            order.CustomerId = command.CustomerId;
            order.PaymentMethod = command.PaymentMethod;
            order.ShipAddressId = command.ShipAddressId;
            order.TotalAmount = command.TotalAmount;
            order.Status = command.Status;
            order.IsPaid = command.IsPaid;
            order.BankId = command.BankId;
            order.BankTransactionId = command.BankTransactionId;
            order.UpdatedAt = TimeHelper.GetUtcNow();

            _unitOfWork.GetRepository<Order>().Update(order);
            await _unitOfWork.SaveAsync();
            return order;

        }
    }
}
