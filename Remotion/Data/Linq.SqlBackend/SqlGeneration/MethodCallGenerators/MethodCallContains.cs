// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// as published by the Free Software Foundation; either version 2.1 of the 
// License, or (at your option) any later version.
// 
// re-motion is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-motion; if not, see http://www.gnu.org/licenses.
// 
using System;
using System.Linq.Expressions;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.SqlBackend.SqlGeneration.MethodCallGenerators
{
  /// <summary>
  /// <see cref="MethodCallContains"/> implements <see cref="IMethodCallSqlGenerator"/> for the string like and contains method.
  /// </summary>
  // TODO Review 2364: Rename all generator classes to "...SqlGenerator", e.g., "ContainsSqlGenerator" or "ContainsMethodCallSqlGenerator".
  public class MethodCallContains : IMethodCallSqlGenerator
  {
    public void GenerateSql (MethodCallExpression methodCallExpression, SqlCommandBuilder commandBuilder, ExpressionTreeVisitor expressionTreeVisitor)
    {
      ArgumentUtility.CheckNotNull ("methodCallExpression", methodCallExpression);
      ArgumentUtility.CheckNotNull ("commandBuilder", commandBuilder);
      ArgumentUtility.CheckNotNull ("expressionTreeVisitor", expressionTreeVisitor);

      commandBuilder.Append ("LIKE(%");
      expressionTreeVisitor.VisitExpression (methodCallExpression.Object);
      commandBuilder.Append ("%)");
    }
  }
}