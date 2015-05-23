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
using Remotion.Linq.SqlBackend.MappingResolution;

namespace Remotion.Linq.SqlBackend.UnitTests.SqlGeneration
{
  public class TestableSqlContextExpressionVisitor : SqlContextExpressionVisitor
  {
    public TestableSqlContextExpressionVisitor (SqlExpressionContext currentContext, IMappingResolutionStage stage, IMappingResolutionContext context)
        : base (currentContext, stage, context)
    {
    }

    public new Expression VisitBinary (BinaryExpression expression)
    {
      return base.VisitBinary (expression);
    }

    public new Expression VisitUnary (UnaryExpression expression)
    {
      return base.VisitUnary (expression);
    }

    public new Expression VisitNew (NewExpression expression)
    {
      return base.VisitNew (expression);
    }

    public new Expression VisitMethodCall (MethodCallExpression expression)
    {
      return base.VisitMethodCall (expression);
    }

    public new Expression VisitConstant (ConstantExpression expression)
    {
      return base.VisitConstant (expression);
    }
  }
}