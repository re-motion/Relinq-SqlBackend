using System.Data.Linq.Mapping;
using System.Linq.Expressions;
using Remotion.Data.Linq.SqlBackend.SqlPreparation;
using System.Linq;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;

namespace Remotion.Data.Linq.IntegrationTests.TestDomain.Northwind.CustomTransformers
{
  /// <summary>
  /// Handles user-defined functions defined via the <see cref="FunctionAttribute"/>.
  /// </summary>
  public class UserDefinedFunctionTransformer : IMethodCallTransformer
  {
    public Expression Transform(MethodCallExpression methodCallExpression)
    {
      var attribute = (FunctionAttribute) methodCallExpression.Method.GetCustomAttributes (typeof (FunctionAttribute), false).Single ();
      return new SqlFunctionExpression (methodCallExpression.Type, attribute.Name, methodCallExpression.Arguments.ToArray());
    }
  }
}