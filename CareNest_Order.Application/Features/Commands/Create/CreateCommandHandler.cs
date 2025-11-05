using CareNest_Order.Application.Exceptions.Validators;
using CareNest_Order.Application.Interfaces.CQRS.Commands;
using CareNest_Order.Application.Interfaces.UOW;
using CareNest_Order.Application.Interfaces.Services;
using CareNest_Order.Application.Exceptions;
using CareNest_Order.Domain.Entitites;
using CareNest_Order.Domain.Commons.Enum;
using CareNest_Order.Application.Common.DTOs;
using Shared.Helper;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace CareNest_Order.Application.Features.Commands.Create
{
    public class CreateCommandHandler : ICommandHandler<CreateCommand, CreateOrderResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAPIService _apiService;
        private readonly IEmailService _emailService;
        private readonly ILogger<CreateCommandHandler> _logger;

        public CreateCommandHandler(IUnitOfWork unitOfWork, IAPIService apiService, IEmailService emailService, ILogger<CreateCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _apiService = apiService;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<CreateOrderResult> HandleAsync(CreateCommand command)
        {
            Validate.ValidateCreate(command);

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

            // Validate ShopId qua Shop service và lấy thông tin Shop
            ShopDto? shopInfo = null;
            if (!string.IsNullOrWhiteSpace(command.ShopId))
            {
                var shopIdStr = command.ShopId!.Trim();
                var shopCheckResult = await _apiService.GetAsync<ShopDto>("shop", $"/api/Shop/{shopIdStr}");
                if (!shopCheckResult.IsSuccess)
                {
                    throw new BadRequestException($"Shop với ID {command.ShopId} không hợp lệ hoặc không tồn tại: {shopCheckResult.Message}");
                }
                shopInfo = shopCheckResult.Data;
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
                TotalAmount = command.TotalAmount, // Sử dụng totalAmount từ request thay vì hardcode 0
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
                double totalCalculatedAmount = 0;

                foreach (var item in command.Items)
                {
                    var payload = new
                    {
                        productDetailId = item.ProductDetailId,
                        orderId = order.Id,
                        quantity = item.Quantity
                        // Bỏ totalAmount để OrderDetail service tự tính toán từ quantity * price
                    };

                    var result = await _apiService.PostAsync<JsonElement>("orderdetail", "/api/OrderDetail", payload);
                    if (!result.IsSuccess)
                    {
                        // Tuỳ chính sách rollback; hiện tại ném lỗi để client biết thất bại
                        throw new Exception(result.Message ?? "Tạo OrderDetail thất bại");
                    }

                    // Parse response để lấy amount từ OrderDetail vừa tạo và cộng dồn
                    if (result.Data.ValueKind != JsonValueKind.Null)
                    {
                        try
                        {
                            // Tìm property chứa amount trong response (có thể là "amount", "totalAmount", "price", etc.)
                            if (result.Data.TryGetProperty("amount", out var amountValue) && amountValue.ValueKind == JsonValueKind.Number)
                            {
                                totalCalculatedAmount += amountValue.GetDouble();
                            }
                            else if (result.Data.TryGetProperty("totalAmount", out var totalAmountValue) && totalAmountValue.ValueKind == JsonValueKind.Number)
                            {
                                totalCalculatedAmount += totalAmountValue.GetDouble();
                            }
                            else if (result.Data.TryGetProperty("price", out var priceValue) && priceValue.ValueKind == JsonValueKind.Number)
                            {
                                totalCalculatedAmount += priceValue.GetDouble();
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Failed to parse amount from OrderDetail response: {ex.Message}");
                        }
                    }
                }

                // Cập nhật order.TotalAmount với tổng đã tính được
                if (totalCalculatedAmount > 0)
                {
                    order.TotalAmount = totalCalculatedAmount;
                    await _unitOfWork.SaveAsync();
                }
            }

            // Tạo Payment QR nếu có thông tin Shop và TotalAmount > 0
            PaymentQrDto? paymentQr = null;
            if (shopInfo != null && order.TotalAmount > 0 &&
                !string.IsNullOrWhiteSpace(shopInfo.BankCode) &&
                !string.IsNullOrWhiteSpace(shopInfo.BankAccountNumber) &&
                !string.IsNullOrWhiteSpace(shopInfo.BankAccountName))
            {
                try
                {
                    _logger.LogInformation("🔵 [SEPAY QR] Bắt đầu tạo Payment QR cho Order {OrderId}", order.Id);
                    _logger.LogInformation("🔵 [SEPAY QR] Thông tin Shop: BankCode={BankCode}, AccountNumber={AccountNumber}, AccountName={AccountName}",
                        shopInfo.BankCode, shopInfo.BankAccountNumber, shopInfo.BankAccountName);
                    _logger.LogInformation("🔵 [SEPAY QR] Thông tin Payment: Amount={Amount}, Description={Description}",
                        order.TotalAmount, $"Thanh toán đơn hàng #{order.Id}");

                    var paymentPayload = new
                    {
                        amount = order.TotalAmount,
                        description = $"Thanh toán đơn hàng #{order.Id}",
                        orderId = order.Id,
                        bankCode = shopInfo.BankCode,
                        accountNumber = shopInfo.BankAccountNumber,
                        accountName = shopInfo.BankAccountName,
                        template = "qronly",
                        download = false
                    };

                    _logger.LogInformation("🔵 [SEPAY QR] Gọi API Sepay để tạo QR Code...");
                    var paymentResult = await _apiService.PostAsyncDirect<PaymentQrResponse>("payment", "/api/payment/create-qr", paymentPayload);

                    if (paymentResult.IsSuccess && paymentResult.Data != null)
                    {
                        paymentQr = paymentResult.Data.Data;
                        _logger.LogInformation("✅ [SEPAY QR] Tạo Payment QR thành công cho Order {OrderId}", order.Id);
                        _logger.LogInformation("✅ [SEPAY QR] QR Code URL: {QrUrl}", paymentQr?.QrCode ?? "N/A");
                    }
                    else
                    {
                        _logger.LogWarning("⚠️ [SEPAY QR] Tạo Payment QR thất bại cho Order {OrderId}: {Message}",
                            order.Id, paymentResult.Message);
                        Console.WriteLine($"Payment QR creation failed: {paymentResult.Message}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ [SEPAY QR] Lỗi khi tạo Payment QR cho Order {OrderId}: {Message}",
                        order.Id, ex.Message);
                    Console.WriteLine($"Payment QR creation error: {ex.Message}");
                }
            }
            else
            {
                _logger.LogInformation("ℹ️ [SEPAY QR] Bỏ qua tạo Payment QR - Thiếu thông tin Shop hoặc TotalAmount = 0");
                if (shopInfo == null)
                    _logger.LogInformation("ℹ️ [SEPAY QR] Lý do: ShopInfo is null");
                else if (order.TotalAmount <= 0)
                    _logger.LogInformation("ℹ️ [SEPAY QR] Lý do: TotalAmount = {TotalAmount}", order.TotalAmount);
                else if (string.IsNullOrWhiteSpace(shopInfo.BankCode))
                    _logger.LogInformation("ℹ️ [SEPAY QR] Lý do: BankCode is empty");
                else if (string.IsNullOrWhiteSpace(shopInfo.BankAccountNumber))
                    _logger.LogInformation("ℹ️ [SEPAY QR] Lý do: BankAccountNumber is empty");
                else if (string.IsNullOrWhiteSpace(shopInfo.BankAccountName))
                    _logger.LogInformation("ℹ️ [SEPAY QR] Lý do: BankAccountName is empty");
            }

            // Gửi email xác nhận đơn hàng (best-effort, không chặn luồng)
            try
            {
                var createdAtText = (order.CreatedAt ?? TimeHelper.GetUtcNow()).ToString("dd/MM/yyyy HH:mm");
                var details = new List<object>();
                if (command.Items != null && command.Items.Count > 0)
                {
                    foreach (var item in command.Items)
                    {
                        details.Add(new
                        {
                            ProductDetailId = item.ProductDetailId,
                            Quantity = item.Quantity,
                            Note = order.Note,
                            TotalAmount = (object?)null
                        });
                    }
                }

                var emailResult = await _emailService.SendAppointmentConfirmationEmailAsync(
                    order.CustomerId ?? string.Empty,
                    string.Empty,
                    string.Empty,
                    order.Id,
                    createdAtText,
                    order.TotalAmount,
                    details
                );

                if (!emailResult.IsSuccess)
                {
                    Console.WriteLine($"Order email send failed: {emailResult.Message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Order email send error: {ex.Message}");
            }

            return new CreateOrderResult
            {
                Order = order,
                PaymentQr = paymentQr,
                Items = command.Items ?? new List<OrderItemDto>(),
                CreatedDate = order.CreatedAt
            };
        }
    }
}
