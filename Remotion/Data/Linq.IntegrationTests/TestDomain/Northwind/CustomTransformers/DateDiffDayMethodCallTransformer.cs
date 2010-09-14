using System;
using System.Data.Linq.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Remotion.Data.Linq.SqlBackend.SqlGeneration;
using Remotion.Data.Linq.SqlBackend.SqlPreparation;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.IntegrationTests.TestDomain.Northwind.CustomTransformers
{
  /// <summary>
  /// Transforms the <see cref="SqlMethods.DateDiffDay(System.Nullable{System.DateTime},System.Nullable{System.DateTime})"/> method to SQL.
  /// </summary>
  public class DateDiffDayMethodCallTransformer : IMethodCallTransformer
  {
    public static readonly MethodInfo[] SupportedMethods = typeof (SqlMethods).GetMethods ().Where (mi => mi.Name == "DateDiffDay").ToArray ();

    public Expression Transform(MethodCallExpression methodCallExpression)
    {
      ArgumentUtility.CheckNotNull ("methodCallExpression", methodCallExpression);

      return new SqlFunctionExpression (
          methodCallExpression.Type, 
          "DATEDIFF", 
          new SqlCustomTextExpression ("day", typeof (string)), 
          methodCallExpression.Arguments[0], 
          methodCallExpression.Arguments[1]);
    }
  }
}