using System.Data.Common;

namespace HBP.Data.Abstractions;

public interface IDbContext
{
    DbConnection Connection { get; }
    DbTransaction? Transaction { get; }
}
