using CareNest_Order.Application.Interfaces.CQRS.Queries;
using CareNest_Order.Application.Interfaces.UOW;
using CareNest_Order.Domain.Commons.Enum;
using CareNest_Order.Domain.Entitites;

namespace CareNest_Order.Application.Features.Queries.CheckOrderStatus
{
    public class CheckOrderStatusQueryHandler : IQueryHandler<CheckOrderStatusQuery, bool>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CheckOrderStatusQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> HandleAsync(CheckOrderStatusQuery query)
        {
            var order = await _unitOfWork.GetRepository<Order>().GetByIdAsync(query.OrderId);
            
            if (order == null)
            {
                return false; // Order không tồn tại
            }

            // Kiểm tra status có bằng Cancel (4) hay không
            return order.Status == OrderStatus.Cancel;
        }
    }
}
