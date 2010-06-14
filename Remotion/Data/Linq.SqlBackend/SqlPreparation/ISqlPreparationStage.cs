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
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;

namespace Remotion.Data.Linq.SqlBackend.SqlPreparation
{
  /// <summary>
  /// Provides entry points for all transformations that occur during the SQL preparation phase.
  /// </summary>
  public interface ISqlPreparationStage
  {
    Expression PrepareSelectExpression (Expression expression, ISqlPreparationContext context);
    Expression PrepareWhereExpression (Expression expression, ISqlPreparationContext context);
    Expression PrepareTopExpression (Expression expression, ISqlPreparationContext context);
    Expression PrepareOrderByExpression (Expression expression, ISqlPreparationContext context);
    Expression PrepareItemExpression (Expression expression, ISqlPreparationContext context);
    FromExpressionInfo PrepareFromExpression (Expression fromExpression, ISqlPreparationContext context, Func<ITableInfo, SqlTableBase> tableGenerator);
    SqlStatement PrepareSqlStatement (QueryModel queryModel, ISqlPreparationContext parentContext);
  }
}