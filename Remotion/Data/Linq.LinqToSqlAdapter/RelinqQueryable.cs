using System.Linq;
using System.Linq.Expressions;
using Remotion.Data.Linq.Parsing.Structure;

namespace Remotion.Data.Linq.LinqToSqlAdapter
{
  /// <summary>
  /// Default implementation of <see cref="QueryableBase{T}"/> 
  /// </summary>
  public class RelinqQueryable<T> : QueryableBase<T>
  {
    public RelinqQueryable (IQueryExecutor executor, MethodCallExpressionNodeTypeRegistry nodeTypeRegistry)
        : base (new DefaultQueryProvider (typeof (RelinqQueryable<>), executor, nodeTypeRegistry))
    {
    }

    public RelinqQueryable (IQueryProvider provider, Expression expression)
        : base(provider, expression)
    {
    }
  }
}
