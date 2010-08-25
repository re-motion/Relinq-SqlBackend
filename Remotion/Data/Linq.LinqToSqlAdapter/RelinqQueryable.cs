using System.Linq;
using System.Linq.Expressions;

namespace Remotion.Data.Linq.LinqToSqlAdapter
{
  /// <summary>
  /// Default implementation of <see cref="QueryableBase{T}"/> 
  /// </summary>
  public class RelinqQueryable<T> : QueryableBase<T>
  {
    public RelinqQueryable (IQueryExecutor executor)
        : base(executor)
    {
    }

    public RelinqQueryable (IQueryProvider provider, Expression expression)
        : base(provider, expression)
    {
    }
  }
}
