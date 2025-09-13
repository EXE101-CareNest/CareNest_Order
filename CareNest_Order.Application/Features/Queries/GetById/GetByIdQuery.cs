using CareNest_Order.Application.Interfaces.CQRS.Queries;
using CareNest_Order.Domain.Entitites;

namespace CareNest_Order.Application.Features.Queries.GetById
{
    public class GetByIdQuery : IQuery<Order>
    {
        public required string Id { get; set; }
    }
}
