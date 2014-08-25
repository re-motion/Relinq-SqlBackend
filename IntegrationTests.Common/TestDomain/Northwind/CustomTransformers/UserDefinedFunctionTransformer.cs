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
using System.Data.Linq.Mapping;
using System.Linq;
using System.Linq.Expressions;
using Remotion.Linq.SqlBackend.SqlPreparation;
using Remotion.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;

namespace Remotion.Linq.IntegrationTests.Common.TestDomain.Northwind.CustomTransformers
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