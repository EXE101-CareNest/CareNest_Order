using CareNest_Order.Application.Common;
using CareNest_Order.Application.Interfaces.CQRS.Queries;
using CareNest_Order.Application.Interfaces.UOW;
using CareNest_Order.Domain.Entitites;

namespace CareNest_Order.Application.Features.Queries.GetAllPaging
{
    public class GetAllPagingQueryHandler : IQueryHandler<GetAllPagingQuery, PageResult<OrderResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetAllPagingQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<PageResult<OrderResponse>> HandleAsync(GetAllPagingQuery query)
        {
            var selector = ObjectMapperExtensions.CreateMapExpression<Order, OrderResponse>();

            var orderByFunc = GetOrderByFunc(query.SortColumn, query.SortDirection);

            // Build optional filters
            System.Linq.Expressions.Expression<Func<Order, bool>>? predicate = null;
            bool hasShop = !string.IsNullOrWhiteSpace(query.ShopId);
            bool hasCustomer = !string.IsNullOrWhiteSpace(query.CustomerId);
            if (hasShop && hasCustomer)
            {
                predicate = o => o.ShopId == query.ShopId && o.CustomerId == query.CustomerId;
            }
            else if (hasShop)
            {
                predicate = o => o.ShopId == query.ShopId;
            }
            else if (hasCustomer)
            {
                predicate = o => o.CustomerId == query.CustomerId;
            }

            IEnumerable<OrderResponse> a = await _unitOfWork.GetRepository<Order>().FindAsync(
                predicate: predicate,
                orderBy: orderByFunc,
                selector: selector,
                pageSize: query.PageSize,
                pageIndex: query.Index);

            return new PageResult<OrderResponse>(a, 1, query.PageSize, query.Index);
        }


        private Func<IQueryable<Order>, IOrderedQueryable<Order>> GetOrderByFunc(string? sortColumn, string? sortDirection)
        {
            var ascending = string.IsNullOrWhiteSpace(sortDirection) || sortDirection.ToLower() != "desc";

            return sortColumn?.ToLower() switch
            {
                "updateat" => q => ascending ? q.OrderBy(a => a.UpdatedAt) : q.OrderByDescending(a => a.UpdatedAt),
                // Mặc định: mới nhất lên đầu
                _ => q => q.OrderByDescending(a => a.CreatedAt)
            };
        }
    }
}
