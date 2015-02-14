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
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.SqlBackend.SqlPreparation;
using Remotion.Linq.SqlBackend.SqlStatementModel;

namespace Remotion.Linq.SqlBackend.UnitTests.SqlPreparation
{
  public class TestableSqlPreparationFromExpressionVisitor : SqlPreparationFromExpressionVisitor
  {
    public TestableSqlPreparationFromExpressionVisitor (
        UniqueIdentifierGenerator generator,
        ISqlPreparationStage stage,
        IMethodCallTransformerProvider provider,
        ISqlPreparationContext context,
        OrderingExtractionPolicy orderingExtractionPolicy)
        : base (generator, stage, provider, context, orderingExtractionPolicy)
    {
    }

    public new FromExpressionInfo? FromExpressionInfo
    {
      get { return base.FromExpressionInfo; }
    }

    public new Expression VisitQuerySourceReference (QuerySourceReferenceExpression expression)
    {
      return base.VisitQuerySourceReference (expression);
    }
  }
}