using CareNest_Order.Application.Common;
using CareNest_Order.Application.Interfaces.CQRS.Queries;
using CareNest_Order.Application.Interfaces.Services;
using CareNest_Order.Application.Common.Options;
using CareNest_Order.Application.Common.DTOs;
using CareNest_Order.Application.Features.Queries.Dashboard;
using CareNest_Order.Application.Interfaces.UOW;
using CareNest_Order.Domain.Entitites;
using System.Linq;

namespace CareNest_Order.Application.Features.Queries.Dashboard
{
    public class OrderDashboardQueryHandler : IQueryHandler<OrderDashboardQuery, OrderDashboardResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAPIService _apiService;

        public OrderDashboardQueryHandler(IUnitOfWork unitOfWork, IAPIService apiService)
        {
            _unitOfWork = unitOfWork;
            _apiService = apiService;
        }

        public async Task<OrderDashboardResult> HandleAsync(OrderDashboardQuery query)
        {
            if (string.IsNullOrWhiteSpace(query.ShopId))
            {
                return await HandleAggregateAsync(query);
            }
            return await HandleDetailAsync(query);
        }

        private async Task<OrderDashboardResult> HandleAggregateAsync(OrderDashboardQuery query)
        {
            // Group orders by shopId and compute counts
            IQueryable<Order> orders = _unitOfWork.GetRepository<Order>().Entities;

            var grouped = orders
                .Where(o => o.ShopId != null)
                .GroupBy(o => o.ShopId!)
                .Select(g => new
                {
                    ShopId = g.Key,
                    TotalOrders = g.Count()
                });

            // Sorting by shop aggregates
            grouped = (query.SortBy?.ToLower()) switch
            {
                "totalorders" => (query.SortDirection?.ToLower() == "desc")
                    ? grouped.OrderByDescending(x => x.TotalOrders)
                    : grouped.OrderBy(x => x.TotalOrders),
                _ => grouped.OrderBy(x => x.ShopId) // fallback by ShopId
            };

            int skip = (query.PageIndex - 1) * query.PageSize;
            var pagedGroups = grouped.Skip(skip).Take(query.PageSize).ToList();
            int totalGroups = grouped.Count();

            var items = new List<DashboardShopAggregateItem>();

            foreach (var g in pagedGroups)
            {
                string shopId = g.ShopId;

                // Fetch shop name via Shop service if needed
                string? shopName = await GetShopNameAsync(shopId);

                // TotalOrderDetails across all orders of this shop
                int totalOrderDetails = await CountOrderDetailsForShopAsync(shopId);

                // Orders summaries limited and sorted
                var orderSummaries = await GetOrderSummariesAsync(shopId, query);

                items.Add(new DashboardShopAggregateItem
                {
                    ShopId = shopId,
                    ShopName = shopName,
                    TotalOrders = g.TotalOrders,
                    TotalOrderDetails = totalOrderDetails,
                    OrdersTotal = g.TotalOrders,
                    Orders = orderSummaries
                });
            }

            var page = new PageResult<DashboardShopAggregateItem>(
                items,
                totalGroups,
                query.PageSize,
                query.PageIndex
            );

            return new OrderDashboardResult
            {
                Shops = page
            };
        }

        private async Task<OrderDashboardResult> HandleDetailAsync(OrderDashboardQuery query)
        {
            string shopId = query.ShopId!;

            IQueryable<Order> orders = _unitOfWork.GetRepository<Order>().Entities.Where(o => o.ShopId == shopId);
            int totalOrders = orders.Count();

            // Gather orderIds
            var orderIds = orders.Select(o => o.Id).ToList();

            // Fetch OrderDetails across all orders and enrich with product info from external services
            var detailPage = await GetOrderDetailsWithProductsAsync(orderIds, query);

            int totalOrderDetails = detailPage.TotalItems;

            string? shopName = await GetShopNameAsync(shopId);

            return new OrderDashboardResult
            {
                ShopDetail = new DashboardShopDetailResponse
                {
                    ShopId = shopId,
                    ShopName = shopName,
                    TotalOrders = totalOrders,
                    TotalOrderDetails = totalOrderDetails,
                    Details = detailPage
                }
            };
        }

        private async Task<string?> GetShopNameAsync(string shopId)
        {
            var resp = await _apiService.GetAsync<ShopDto>("shop", $"/api/Shop/{shopId}");
            return resp.IsSuccess ? resp.Data?.Name : null;
        }

        private async Task<int> CountOrderDetailsForShopAsync(string shopId)
        {
            // Call external OrderDetail service to count per order, sum them up using TotalItems from paged result
            IQueryable<Order> orders = _unitOfWork.GetRepository<Order>().Entities.Where(o => o.ShopId == shopId);
            var orderIds = orders.Select(o => o.Id).ToList();

            int total = 0;
            foreach (var orderId in orderIds)
            {
                var resp = await _apiService.GetAsync<OrderDetailPageDto>("orderdetail", $"/api/OrderDetail?pageIndex=1&pageSize=1&orderId={orderId}&sortDirection=asc");
                if (resp.IsSuccess && resp.Data != null)
                {
                    total += resp.Data.TotalItems;
                }
            }
            return total;
        }

        private async Task<IEnumerable<DashboardOrderSummary>> GetOrderSummariesAsync(string shopId, OrderDashboardQuery query)
        {
            IQueryable<Order> orders = _unitOfWork.GetRepository<Order>().Entities.Where(o => o.ShopId == shopId);

            // Sort orders for selection
            orders = (query.OrdersSortBy?.ToLower()) switch
            {
                "orderdetailcount" => orders, // we'll compute counts then sort in memory
                _ => (query.OrdersSortDirection?.ToLower() == "asc")
                    ? orders.OrderBy(o => o.CreatedAt)
                    : orders.OrderByDescending(o => o.CreatedAt)
            };

            var selectedOrders = orders.Take(query.OrdersLimit).Select(o => new { o.Id, o.CreatedAt }).ToList();

            var list = new List<DashboardOrderSummary>(selectedOrders.Count);
            foreach (var o in selectedOrders)
            {
                var resp = await _apiService.GetAsync<OrderDetailPageDto>("orderdetail", $"/api/OrderDetail?pageIndex=1&pageSize=1&orderId={o.Id}&sortDirection=asc");
                int count = (resp.IsSuccess && resp.Data != null) ? resp.Data.TotalItems : 0;
                list.Add(new DashboardOrderSummary
                {
                    OrderId = o.Id,
                    CreatedAt = o.CreatedAt?.UtcDateTime,
                    OrderDetailCount = count
                });
            }

            if ((query.OrdersSortBy?.ToLower()) == "orderdetailcount")
            {
                list = (query.OrdersSortDirection?.ToLower() == "asc")
                    ? list.OrderBy(x => x.OrderDetailCount).ToList()
                    : list.OrderByDescending(x => x.OrderDetailCount).ToList();
            }

            return list;
        }

        private async Task<PageResult<OrderDetailWithProduct>> GetOrderDetailsWithProductsAsync(IEnumerable<string> orderIds, OrderDashboardQuery query)
        {
            // We fetch paged details from external OrderDetail service by orderId one by one and merge.
            // For simplicity, we'll page after merging (client-visible page).

            var details = new List<OrderDetailItemDto>();
            foreach (var orderId in orderIds)
            {
                var resp = await _apiService.GetAsync<OrderDetailPageDto>("orderdetail", $"/api/OrderDetail?pageIndex=1&pageSize=1000&orderId={orderId}&sortDirection=asc");
                if (resp.IsSuccess && resp.Data != null && resp.Data.Items != null)
                {
                    details.AddRange(resp.Data.Items);
                }
            }

            // Sorting
            IEnumerable<OrderDetailItemDto> sorted = details;
            switch (query.DetailSortBy?.ToLower())
            {
                case "totalamount":
                    sorted = (query.DetailSortDirection?.ToLower() == "asc")
                        ? details.OrderBy(x => x.TotalAmount)
                        : details.OrderByDescending(x => x.TotalAmount);
                    break;
                case "quantity":
                    sorted = (query.DetailSortDirection?.ToLower() == "asc")
                        ? details.OrderBy(x => x.Quantity)
                        : details.OrderByDescending(x => x.Quantity);
                    break;
                default:
                    sorted = details; // assume creation order
                    break;
            }

            // Paging
            int totalItems = sorted.Count();
            int skip = (query.PageIndex - 1) * query.PageSize;
            var pageItems = sorted.Skip(skip).Take(query.PageSize).ToList();

            // Enrich products with caching per request
            var productCache = new Dictionary<string, ProductDetailSingleDto>();
            var resultItems = new List<OrderDetailWithProduct>(pageItems.Count);

            foreach (var d in pageItems)
            {
                if (!productCache.TryGetValue(d.ProductDetailId, out var pd))
                {
                    var pResp = await _apiService.GetAsync<ProductDetailSingleDto>("product", $"/api/ProductDetails/{d.ProductDetailId}");
                    if (pResp.IsSuccess && pResp.Data != null)
                    {
                        productCache[d.ProductDetailId] = pResp.Data;
                        pd = pResp.Data;
                    }
                }

                resultItems.Add(new OrderDetailWithProduct
                {
                    Id = d.Id,
                    OrderId = d.OrderId,
                    ProductDetailId = d.ProductDetailId,
                    Quantity = d.Quantity,
                    TotalAmount = d.TotalAmount,
                    CreatedAt = d.CreatedAt,
                    ProductName = pd?.Name,
                    Price = pd?.Price,
                    ProductStatus = pd?.Status,
                    IsDefault = pd?.IsDefault,
                    ImgUrls = pd?.ImgUrls
                });
            }

            return new PageResult<OrderDetailWithProduct>(
                resultItems,
                totalItems,
                query.PageSize,
                query.PageIndex
            );
        }
    }

    // DTOs for external services

    internal class OrderDetailPageDto
    {
        public List<OrderDetailItemDto> Items { get; set; } = new();
        public int TotalItems { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    internal class OrderDetailItemDto
    {
        public string Id { get; set; } = string.Empty;
        public string ProductDetailId { get; set; } = string.Empty;
        public string OrderId { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public int TotalAmount { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    internal class ProductDetailSingleDto
    {
        public string Id { get; set; } = string.Empty;
        public string ProductId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public double Price { get; set; }
        public bool Status { get; set; }
        public object? Discount { get; set; }
        public bool IsDefault { get; set; }
        public IEnumerable<string>? ImgUrls { get; set; }
        public int QuantityInStock { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
    }
}


