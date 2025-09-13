using CareNest_Order.Application.Common;
using CareNest_Order.Application.Interfaces.CQRS.Queries;

namespace CareNest_Order.Application.Features.Queries.GetAllPaging
{
    public class GetAllPagingQuery : IQuery<PageResult<OrderResponse>>
    {
        public int Index { get; set; }
        public int PageSize { get; set; }
        public string? SortColumn { get; set; } // "Name", "Note", "CreatedAt"
        public string? SortDirection { get; set; } // "asc" or "desc"
    }
}
