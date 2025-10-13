using CareNest_Order.Application.Exceptions;
using CareNest_Order.Application.Exceptions.Validators;
using CareNest_Order.Application.Interfaces.CQRS.Commands;
using CareNest_Order.Application.Interfaces.UOW;
using CareNest_Order.Application.Interfaces.Services;
using CareNest_Order.Domain.Commons.Constant;
using CareNest_Order.Domain.Entitites;
using Shared.Helper;

namespace CareNest_Order.Application.Features.Commands.Update
{
    public class UpdateCommandHandler : ICommandHandler<UpdateCommand, Order>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAPIService _apiService;

        public UpdateCommandHandler(IUnitOfWork unitOfWork, IAPIService apiService)
        {
            _unitOfWork = unitOfWork;
            _apiService = apiService;
        }

        public async Task<Order> HandleAsync(UpdateCommand command)
        {
            // Gọi validator để kiểm tra dữ liệu
            Validate.ValidateUpdate(command);

            // Validate ShipAddressId qua Address service nếu có
            if (!string.IsNullOrWhiteSpace(command.ShipAddressId))
            {
                var addressId = command.ShipAddressId!.Trim();
                var addressCheck = await _apiService.GetAsync<object>("address", $"/api/address/{addressId}");
                if (!addressCheck.IsSuccess)
                {
                    throw new BadRequestException($"ShipAddressId không hợp lệ hoặc không tồn tại: {addressCheck.Message}");
                }
            }

            // Validate ShopId nếu có
            if (!string.IsNullOrWhiteSpace(command.ShopId))
            {
                var shopIdStr = command.ShopId!.Trim();
                var shopCheckResult = await _apiService.GetAsync<object>("shop", $"/api/Shop/{shopIdStr}");
                if (!shopCheckResult.IsSuccess)
                {
                    throw new BadRequestException($"Shop với ID {command.ShopId} không hợp lệ hoặc không tồn tại: {shopCheckResult.Message}");
                }
            }

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
            order.ShopId = command.ShopId;
            order.UpdatedAt = TimeHelper.GetUtcNow();

            _unitOfWork.GetRepository<Order>().Update(order);
            await _unitOfWork.SaveAsync();
            return order;

        }
    }
}
