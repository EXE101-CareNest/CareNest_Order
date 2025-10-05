using CareNest_Order.Application.Exceptions.Validators;
using CareNest_Order.Application.Interfaces.CQRS.Commands;
using CareNest_Order.Application.Interfaces.UOW;
using CareNest_Order.Application.Interfaces.Services;
using CareNest_Order.Domain.Entitites;
using Shared.Helper;

namespace CareNest_Order.Application.Features.Commands.Create
{
    public class CreateCommandHandler : ICommandHandler<CreateCommand, Order>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAPIService _apiService;

        public CreateCommandHandler(IUnitOfWork unitOfWork, IAPIService apiService)
        {
            _unitOfWork = unitOfWork;
            _apiService = apiService;
        }

        public async Task<Order> HandleAsync(CreateCommand command)
        {
            Validate.ValidateCreate(command);

            Order order = new()
            {
                Status = command.Status,
                CustomerId = command.CustomerId,
                Note = command.Note,
                PaymentMethod = command.PaymentMethod,
                ShipAddressId = command.ShipAddressId,
                TotalAmount = command.TotalAmount,
                ShopId = command.ShopId,
                BankId = command.BankId,
                BankTransactionId = command.BankTransactionId,
                IsPaid = command.IsPaid,
                CreatedAt = TimeHelper.GetUtcNow()
            };
            await _unitOfWork.GetRepository<Order>().AddAsync(order);
            await _unitOfWork.SaveAsync();

            // Orchestrate tạo OrderDetail cho từng item nếu có
            if (command.Items != null && command.Items.Count > 0)
            {
                foreach (var item in command.Items)
                {
                    var payload = new
                    {
                        productDetailId = item.ProductDetailId,
                        orderId = order.Id,
                        quantity = item.Quantity,
                        totalAmount = command.TotalAmount // hoặc tính theo từng item nếu có
                    };

                    var result = await _apiService.PostAsync<object>("orderdetail", "/api/OrderDetail", payload);
                    if (!result.IsSuccess)
                    {
                        // Tuỳ chính sách rollback; hiện tại ném lỗi để client biết thất bại
                        throw new Exception(result.Message ?? "Tạo OrderDetail thất bại");
                    }
                }
            }

            return order;
        }
    }
}
