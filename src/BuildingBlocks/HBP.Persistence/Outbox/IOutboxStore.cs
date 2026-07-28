using HBP.Common;

namespace HBP.Persistence.Outbox;

public interface IOutboxStore
{
    Task AddAsync(IDomainEvent @event, CancellationToken cancellationToken = default);
}
