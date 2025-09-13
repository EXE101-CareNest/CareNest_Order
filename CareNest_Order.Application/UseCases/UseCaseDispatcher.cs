using CareNest_Order.Application.Interfaces.CQRS;
using CareNest_Order.Application.Interfaces.CQRS.Commands;
using CareNest_Order.Application.Interfaces.CQRS.Queries;
using Microsoft.Extensions.DependencyInjection;

namespace CareNest_Order.Application.UseCases
{
    public class UseCaseDispatcher : IUseCaseDispatcher
    {
        private readonly IServiceProvider _provider;

        public UseCaseDispatcher(IServiceProvider provider)
        {
            _provider = provider;
        }

        public async Task<TResult> DispatchAsync<TCommand, TResult>(TCommand command) where TCommand : ICommand<TResult>
        {
            var handler = _provider.GetRequiredService<ICommandHandler<TCommand, TResult>>();
            return await handler.HandleAsync(command);
        }

        public async Task DispatchAsync<TCommand>(TCommand command) where TCommand : ICommand
        {
            var handler = _provider.GetRequiredService<ICommandHandler<TCommand>>();
            await handler.HandleAsync(command);
        }

        public async Task<TResult> DispatchQueryAsync<TQuery, TResult>(TQuery query) where TQuery : IQuery<TResult>
        {
            var handler = _provider.GetRequiredService<IQueryHandler<TQuery, TResult>>();
            return await handler.HandleAsync(query);
        }
    }
}
