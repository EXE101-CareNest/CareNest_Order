using CareNest_Order.Application.Common;
using CareNest_Order.Application.Interfaces.CQRS.Queries;

namespace CareNest_Order.Application.Features.Queries.Dashboard
{
    public class OrderDashboardQuery : IQuery<OrderDashboardResult>
    {
        public string? ShopId { get; set; }

        // paging/sorting for shops (aggregate mode)
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortBy { get; set; } // shopName | totalOrders | totalOrderDetails
        public string? SortDirection { get; set; } = "asc"; // asc | desc

        // nested orders controls in aggregate mode
        public int OrdersLimit { get; set; } = 5;
        public string? OrdersSortBy { get; set; } = "createdAt"; // createdAt | orderDetailCount
        public string? OrdersSortDirection { get; set; } = "desc"; // asc | desc

        // detail mode filters
        public string? OrderId { get; set; }
        public string? DetailSortBy { get; set; } // createdAt | totalAmount | quantity | productName
        public string? DetailSortDirection { get; set; } = "desc";
    }

    public class OrderDashboardResult
    {
        // If ShopId is null => aggregate mode
        public PageResult<DashboardShopAggregateItem>? Shops { get; set; }

        // If ShopId is set => detail mode
        public DashboardShopDetailResponse? ShopDetail { get; set; }

        // When aggregate mode (no ShopId): total review count across all orders
        public int ReviewCount { get; set; }
    }

    public class DashboardShopAggregateItem
    {
        public string ShopId { get; set; } = string.Empty;
        public string? ShopName { get; set; }
        public int TotalOrders { get; set; }
        public int TotalOrderDetails { get; set; }
        public int TotalOrdersCompleted { get; set; }
        public int TotalOrdersCancelled { get; set; }
        public int TotalSeller { get; set; }
        public double TotalRevenue { get; set; }
        public int OrdersTotal { get; set; } // real total order count for this shop
        public IEnumerable<DashboardOrderSummary> Orders { get; set; } = Enumerable.Empty<DashboardOrderSummary>();
    }

    public class DashboardOrderSummary
    {
        public string OrderId { get; set; } = string.Empty;
        public DateTime? CreatedAt { get; set; }
        public string? OrderCode { get; set; }
        public int OrderDetailCount { get; set; }
    }

    public class DashboardShopDetailResponse
    {
        public string ShopId { get; set; } = string.Empty;
        public string? ShopName { get; set; }
        public int TotalOrders { get; set; }
        public int TotalOrderDetails { get; set; }
        public int TotalOrdersCompleted { get; set; }
        public int TotalOrdersCancelled { get; set; }
        public int TotalSeller { get; set; }
        public double TotalRevenue { get; set; }
        public PageResult<OrderDetailWithProduct> Details { get; set; } = default!;

        // Total reviews for all orders of this shop
        public int ReviewCount { get; set; }
    }

    public class OrderDetailWithProduct
    {
        // From OrderDetail
        public string Id { get; set; } = string.Empty;
        public string OrderId { get; set; } = string.Empty;
        public string ProductDetailId { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public double TotalAmount { get; set; }
        public DateTime? CreatedAt { get; set; }

        // From ProductDetail
        public string? ProductName { get; set; }
        public double? Price { get; set; }
        public bool? ProductStatus { get; set; }
        public bool? IsDefault { get; set; }
        public IEnumerable<string>? ImgUrls { get; set; }
    }
}


