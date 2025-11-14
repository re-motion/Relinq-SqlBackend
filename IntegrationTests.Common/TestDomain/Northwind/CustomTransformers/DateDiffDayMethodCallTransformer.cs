// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (c) rubicon IT GmbH, www.rubicon.eu
// 
// re-linq is free software; you can redistribute it and/or modify it under 
// the terms of the GNU Lesser General Public License as published by the 
// Free Software Foundation; either version 2.1 of the License, 
// or (at your option) any later version.
// 
// re-linq is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-linq; if not, see http://www.gnu.org/licenses.
// 

using System;
using System.Data.Linq.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Remotion.Linq.SqlBackend.SqlGeneration;
using Remotion.Linq.SqlBackend.SqlPreparation;
using Remotion.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;
using Remotion.Utilities;

namespace Remotion.Linq.IntegrationTests.Common.TestDomain.Northwind.CustomTransformers
{
  /// <summary>
  /// Transforms the <see cref="SqlMethods.DateDiffDay(System.Nullable{System.DateTime},System.Nullable{System.DateTime})"/> method to SQL.
  /// </summary>
  public class DateDiffDayMethodCallTransformer : IMethodCallTransformer
  {
    public static readonly MethodInfo[] SupportedMethods = typeof (SqlMethods).GetMethods ().Where (mi => mi.Name == "DateDiffDay").ToArray ();

    public Expression Transform(MethodCallExpression methodCallExpression)
    {
      ArgumentUtility.CheckNotNull (nameof(methodCallExpression), methodCallExpression);

      return new SqlFunctionExpression (
          methodCallExpression.Type, 
          "DATEDIFF", 
          new SqlCustomTextExpression ("day", typeof (string)), 
          methodCallExpression.Arguments[0], 
          methodCallExpression.Arguments[1]);
    }
  }
}