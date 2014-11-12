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
using Remotion.Linq.SqlBackend.SqlGeneration;

namespace Remotion.Linq.SqlBackend.UnitTests.SqlGeneration
{
  public class TestableSqlGeneratingOuterSelectExpressionVisitor : SqlGeneratingOuterSelectExpressionVisitor
  {
    public TestableSqlGeneratingOuterSelectExpressionVisitor (
        ISqlCommandBuilder commandBuilder,
        ISqlGenerationStage stage,
        SetOperationsMode setOperationsMode)
        : base(commandBuilder, stage, setOperationsMode)
    {

    }

    public new int ColumnPosition
    {
      get { return base.ColumnPosition; }
    }

    public new Expression VisitNewExpression (NewExpression expression)
    {
      return base.VisitNewExpression (expression);
    }

    public new Expression VisitMethodCallExpression (MethodCallExpression expression)
    {
      return base.VisitMethodCallExpression (expression);
    }

    public new Expression VisitUnaryExpression (UnaryExpression expression)
    {
      return base.VisitUnaryExpression (expression);
    }
  }
}