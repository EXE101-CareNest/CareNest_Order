using CareNest_Order.Application.Interfaces.CQRS.Queries;
using CareNest_Order.Application.Interfaces.UOW;
using CareNest_Order.Domain.Commons.Constant;
using CareNest_Order.Domain.Entitites;

namespace CareNest_Order.Application.Features.Queries.GetById
{
    public class GetByIdQueryHandler : IQueryHandler<GetByIdQuery, Order>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetByIdQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Order> HandleAsync(GetByIdQuery query)
        {
            Order? service = await _unitOfWork.GetRepository<Order>().GetByIdAsync(query.Id);

            if (service == null)
            {
                throw new Exception(MessageConstant.NotFound);
            }
            return service;
        }
    }
}
