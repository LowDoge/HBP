using HBP.Common;

namespace HBP.Persistence.Abstractions;

public interface IOutboxStore
{
    Task AddAsync(IDomainEvent @event, CancellationToken cancellationToken = default);
}
