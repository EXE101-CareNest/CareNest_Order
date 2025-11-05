using CareNest_Order.Application.Common;
using CareNest_Order.Application.Features.Commands.Create;
using CareNest_Order.Application.Features.Commands.Delete;
using CareNest_Order.Application.Features.Commands.Update;
using CareNest_Order.Application.Features.Queries.GetAllPaging;
using CareNest_Order.Application.Features.Queries.GetById;
using CareNest_Order.Application.Features.Queries.CheckOrderStatus;
using CareNest_Order.Application.Interfaces.CQRS;
using CareNest_Order.Domain.Commons.Constant;
using CareNest_Order.Domain.Entitites;
using CareNest_Order.Extensions;
using Microsoft.AspNetCore.Mvc;
using CareNest_Order.Application.Features.Queries.Dashboard;
using CareNest_Order.Application.Features.Commands.UpdateStatus;


namespace CareNest_Order.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IUseCaseDispatcher _dispatcher;

        public OrderController(IUseCaseDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        public class UpdateOrderStatusRequest
        {
            public string OrderId { get; set; } = string.Empty;
            public CareNest_Order.Domain.Commons.Enum.OrderStatus Status { get; set; }
        }
        /// <summary>
        /// Cập nhật trạng thái đơn hàng theo orderId
        /// </summary>
        /// <param name="request">orderId và status</param>
        /// <returns>Thông tin đơn hàng đã cập nhật</returns>
        [HttpPut("status")]
        public async Task<IActionResult> UpdateStatus([FromBody] UpdateOrderStatusRequest request)
        {
            var command = new UpdateOrderStatusCommand
            {
                OrderId = request.OrderId,
                Status = request.Status
            };
            var result = await _dispatcher.DispatchAsync<UpdateOrderStatusCommand, Order>(command);
            return this.OkResponse(result, "Order status updated successfully");
        }

        /// <summary>
        /// Hiển thị toàn bộ danh sách đơn hàng hiện có trong hệ thống với phân trang và sắp xếp
        /// </summary>
        /// <param name="pageIndex">trang hiện tại</param>
        /// <param name="pageSize">Số lượng phần tử trong trang</param>
        /// <param name="sortColumn">cột muốn sort: name, updateat,ownerid</param>
        /// <param name="sortDirection">cách sort asc or desc</param>
        /// <returns>Danh sách đơn hàng</returns>
        [HttpGet]
        public async Task<IActionResult> GetPaging(
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? sortColumn = null,
            [FromQuery] string? sortDirection = "asc",
            [FromQuery] string? shopId = null,
            [FromQuery] string? customerId = null)
        {
            var query = new GetAllPagingQuery()
            {
                Index = pageIndex,
                PageSize = pageSize,
                SortColumn = sortColumn,
                SortDirection = sortDirection,
                ShopId = shopId,
                CustomerId = customerId
            };
            var result = await _dispatcher.DispatchQueryAsync<GetAllPagingQuery, PageResult<OrderResponse>>(query);
            return this.OkResponse(result, MessageConstant.SuccessGet);
        }

        /// <summary>
        /// Dashboard theo shop (tổng hợp) hoặc chi tiết theo shop khi truyền shopId
        /// </summary>
        /// <param name="shopId">Id shop (tùy chọn). Không truyền: tổng hợp theo shop. Có truyền: chi tiết OrderDetail của shop.</param>
        /// <param name="pageIndex">Trang hiện tại (áp dụng cho danh sách shop hoặc danh sách OrderDetail)</param>
        /// <param name="pageSize">Số phần tử mỗi trang</param>
        /// <param name="sortBy">Tổng hợp: shopName | totalOrders | totalOrderDetails. Chi tiết: createdAt | totalAmount | quantity | productName</param>
        /// <param name="sortDirection">asc | desc</param>
        /// <param name="ordersLimit">Giới hạn số đơn trong mảng orders của từng shop (tổng hợp)</param>
        /// <param name="ordersSortBy">createdAt | orderDetailCount</param>
        /// <param name="ordersSortDirection">asc | desc</param>
        /// <param name="orderId">(Chi tiết) lọc theo 1 order cụ thể</param>
        /// <returns></returns>
        [HttpGet("dashboard")]
        public async Task<IActionResult> Dashboard(
            [FromQuery] string? shopId = null,
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? sortBy = null,
            [FromQuery] string? sortDirection = "asc",
            [FromQuery] int ordersLimit = 5,
            [FromQuery] string? ordersSortBy = "createdAt",
            [FromQuery] string? ordersSortDirection = "desc",
            [FromQuery] string? orderId = null)
        {
            var query = new OrderDashboardQuery
            {
                ShopId = shopId,
                PageIndex = pageIndex,
                PageSize = pageSize,
                SortBy = sortBy,
                SortDirection = sortDirection,
                OrdersLimit = ordersLimit,
                OrdersSortBy = ordersSortBy,
                OrdersSortDirection = ordersSortDirection,
                OrderId = orderId
            };

            var result = await _dispatcher.DispatchQueryAsync<OrderDashboardQuery, OrderDashboardResult>(query);
            return this.OkResponse(result, MessageConstant.SuccessGet);
        }

        /// <summary>
        /// Hiển thị chi tiết đơn hàng theo id
        /// </summary>
        /// <param name="id">Id đơn hàng</param>
        /// <returns>chi tiết đơn hàng</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var query = new GetByIdQuery() { Id = id };
            Order result = await _dispatcher.DispatchQueryAsync<GetByIdQuery, Order>(query);
            return this.OkResponse(result, MessageConstant.SuccessGet);
        }

        /// <summary>
        /// tạo mới đơn hàng
        /// </summary>
        /// <param name="command">thông tin đơn hàng</param>
        /// <returns>thông tin đơn hàng mới tạo xog</returns>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCommand command)
        {
            CreateOrderResult result = await _dispatcher.DispatchAsync<CreateCommand, CreateOrderResult>(command);
            return this.OkResponse(result, MessageConstant.SuccessCreate);
        }

        /// <summary>
        /// Cập nhật thông tin đơn hàng
        /// </summary>
        /// <param name="id">Id đơn hàng</param>
        /// <param name="request">các thông tin cần sửa</param>
        /// <returns></returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateRequest request)
        {

            var command = new UpdateCommand()
            {
                Id = id,
                CustomerId = request.CustomerId,
                Note = request.Note,
                PaymentMethod = request.PaymentMethod,
                ShipAddressId = request.ShipAddressId,
                ShopId = request.ShopId,
                Status = request.Status,
                TotalAmount = request.TotalAmount
            };
            Order result = await _dispatcher.DispatchAsync<UpdateCommand, Order>(command);

            return this.OkResponse(result, MessageConstant.SuccessUpdate);
        }

        /// <summary>
        /// xoá đơn hàng
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            await _dispatcher.DispatchAsync(new DeleteCommand { Id = id });
            return this.OkResponse(MessageConstant.SuccessDelete);
        }

        /// <summary>
        /// Cập nhật trạng thái đơn hàng thành "Cancel" - API cho Sepay gọi
        /// </summary>
        /// <param name="orderId">Id đơn hàng cần cập nhật</param>
        /// <returns>Thông tin đơn hàng đã được cập nhật</returns>
        [HttpPut("update-status-to-cancel/{orderId}")]
        public async Task<IActionResult> UpdateStatusToCancel(string orderId)
        {
            var command = new UpdateOrderStatusToCancelCommand { OrderId = orderId };
            Order result = await _dispatcher.DispatchAsync<UpdateOrderStatusToCancelCommand, Order>(command);
            return this.OkResponse(result, "Order status updated to Cancel successfully");
        }

        /// <summary>
        /// Kiểm tra trạng thái đơn hàng có bằng "Cancel" (4) hay không - API cho Frontend
        /// </summary>
        /// <param name="orderId">Id đơn hàng cần kiểm tra</param>
        /// <returns>true nếu status = Cancel, false nếu không</returns>
        [HttpGet("check-status-cancel/{orderId}")]
        public async Task<IActionResult> CheckOrderStatusCancel(string orderId)
        {
            var query = new CheckOrderStatusQuery { OrderId = orderId };
            bool isCancel = await _dispatcher.DispatchQueryAsync<CheckOrderStatusQuery, bool>(query);
            return this.OkResponse(new { isCancel, orderId }, isCancel ? "Order is Cancel" : "Order is not Cancel");
        }
    }
}
