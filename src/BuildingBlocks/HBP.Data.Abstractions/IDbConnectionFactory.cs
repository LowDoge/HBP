using System.Data.Common;

namespace HBP.Data.Abstractions;

public interface IDbConnectionFactory
{
    DbConnection Open();
}
