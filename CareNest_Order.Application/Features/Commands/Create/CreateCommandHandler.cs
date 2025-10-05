using CareNest_Order.Application.Exceptions.Validators;
using CareNest_Order.Application.Interfaces.CQRS.Commands;
using CareNest_Order.Application.Interfaces.UOW;
using CareNest_Order.Application.Interfaces.Services;
using CareNest_Order.Application.Exceptions;
using CareNest_Order.Domain.Entitites;
using CareNest_Order.Domain.Commons.Enum;
using Shared.Helper;
using System.Text.Json;

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

            // Validate ShopId qua Shop service nếu có
            if (!string.IsNullOrWhiteSpace(command.ShopId))
            {
                var shopIdStr = command.ShopId!.Trim();
                var shopCheckResult = await _apiService.GetAsync<object>("shop", $"/api/Shop/{shopIdStr}");
                if (!shopCheckResult.IsSuccess)
                {
                    throw new BadRequestException($"Shop với ID {command.ShopId} không hợp lệ hoặc không tồn tại: {shopCheckResult.Message}");
                }
            }

            // Mặc định status Pending nếu không truyền
            var initialStatus = command.Status ?? OrderStatus.Pending;

            Order order = new()
            {
                Status = initialStatus,
                CustomerId = command.CustomerId,
                Note = command.Note,
                PaymentMethod = command.PaymentMethod,
                ShipAddressId = command.ShipAddressId,
                TotalAmount = 0,
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
                double totalAmountSum = 0;
                foreach (var item in command.Items)
                {
                    var payload = new
                    {
                        productDetailId = item.ProductDetailId,
                        orderId = order.Id,
                        quantity = item.Quantity,
                        totalAmount = 0
                    };

                    var result = await _apiService.PostAsync<JsonElement>("orderdetail", "/api/OrderDetail", payload);
                    if (!result.IsSuccess)
                    {
                        // Tuỳ chính sách rollback; hiện tại ném lỗi để client biết thất bại
                        throw new Exception(result.Message ?? "Tạo OrderDetail thất bại");
                    }

                    try
                    {
                        // Đọc totalAmount từ data trả về
                        if (result.Data.ValueKind == JsonValueKind.Object && result.Data.TryGetProperty("totalAmount", out var totalProp))
                        {
                            totalAmountSum += totalProp.GetDouble();
                        }
                    }
                    catch
                    {
                        // Bỏ qua nếu parse lỗi, nhưng không chặn luồng
                    }
                }

                // Cập nhật tổng tiền cho Order sau khi tạo xong tất cả OrderDetail
                order.TotalAmount = totalAmountSum;
                await _unitOfWork.GetRepository<Order>().UpdateAsync(order);
                await _unitOfWork.SaveAsync();
            }

            return order;
        }
    }
}
