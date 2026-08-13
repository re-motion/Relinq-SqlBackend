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
using System.Linq.Expressions;
using Remotion.Linq.Parsing;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Utilities;

namespace Remotion.Linq.SqlBackend.MappingResolution
{
  /// <summary>
  /// <see cref="SetOperationReconciliationVisitor"/> the selection projections in set operations to match a reconciled column view
  /// defined by <see cref="ISetOperationReconciliationContext"/>.
  /// </summary>
  public class SetOperationReconciliationVisitor
      : RelinqExpressionVisitor,
          IResolvedSqlExpressionVisitor
  {
    public static Expression ApplyReconciliation (
        Expression expression,
        ISetOperationReconciliationContext reconciliationContext,
        IMappingResolutionStage stage)
    {
      ArgumentUtility.CheckNotNull (nameof(reconciliationContext), reconciliationContext);
      ArgumentUtility.CheckNotNull (nameof(stage), stage);

      var visitor = new SetOperationReconciliationVisitor (reconciliationContext, stage);
      return visitor.Visit (expression);
    }

    private readonly ISetOperationReconciliationContext _reconciliationContext;
    private readonly IMappingResolutionStage _stage;

    protected SetOperationReconciliationVisitor (
        ISetOperationReconciliationContext reconciliationContext,
        IMappingResolutionStage stage)
    {
      ArgumentUtility.CheckNotNull (nameof(reconciliationContext), reconciliationContext);
      ArgumentUtility.CheckNotNull (nameof(stage), stage);

      _reconciliationContext = reconciliationContext;
      _stage = stage;
    }

    public Expression VisitSqlEntity (SqlEntityExpression expression)
    {
      ArgumentUtility.CheckNotNull (nameof(expression), expression);

      return _reconciliationContext.IsReconciliationRequired (expression)
          ? _stage.ApplySetOperationReconciliationContext (expression, _reconciliationContext)
          : expression;
    }

    public Expression VisitSqlColumn (SqlColumnExpression expression)
    {
      ArgumentUtility.CheckNotNull (nameof(expression), expression);

      return expression;
    }

    public Expression VisitSqlEntityConstant (SqlEntityConstantExpression expression)
    {
      ArgumentUtility.CheckNotNull (nameof(expression), expression);

      return expression;
    }
  }
}