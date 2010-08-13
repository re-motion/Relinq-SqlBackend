using System.Linq;
using System.Linq.Expressions;

namespace Remotion.Data.Linq.LinqToSqlAdapter
{
  /// <summary>
  /// Default implementation of QueryableBase
  /// </summary>
  public class QueryableAdapter<T> : QueryableBase<T>
  {
    public QueryableAdapter (IQueryExecutor executor)
        : base(executor)
    {
    }

    public QueryableAdapter (IQueryProvider provider, Expression expression)
        : base(provider, expression)
    {
    }
  }
}
