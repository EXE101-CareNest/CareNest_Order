
using CareNest_Order.Application.Interfaces.CQRS.Commands;

namespace CareNest_Order.Application.Features.Commands.Delete
{
    public class DeleteCommand : ICommand
    {
        public required string Id { get; set; }
    }
}
