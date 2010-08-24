using System.Linq;
using System.Linq.Expressions;

namespace Remotion.Data.Linq.LinqToSqlAdapter
{
  // TODO Review: Rename this class to RelinqQueryable
  /// <summary>
  /// Default implementation of <see cref="QueryableBase{T}"/> 
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
