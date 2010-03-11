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
using System.Linq.Expressions;
using Remotion.Data.Linq.SqlBackend.SqlGeneration.BooleanSemantics;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;

namespace Remotion.Data.Linq.UnitTests.SqlBackend.SqlGeneration.BooleanSemantics
{
  public class TestableBooleanSemanticsConverter : BooleanSemanticsExpressionConverter
  {
    public TestableBooleanSemanticsConverter (BooleanSemanticsKind initialSemantics)
        : base (initialSemantics)
    {
    }

    public new Expression VisitConstantExpression (ConstantExpression expression)
    {
      return base.VisitConstantExpression (expression);
    }

    public new Expression VisitSqlColumnExpression (SqlColumnExpression expression)
    {
      return base.VisitSqlColumnExpression (expression);
    }

    public new Expression VisitBinaryExpression (BinaryExpression expression)
    {
      return base.VisitBinaryExpression (expression);
    }

    public new Expression VisitUnaryExpression (UnaryExpression expression)
    {
      return base.VisitUnaryExpression (expression);
    }

    public new Expression VisitSqlColumnListExpression (SqlColumnListExpression expression)
    {
      return (((IResolvedSqlExpressionVisitor) this)).VisitSqlColumnListExpression (expression);
    }
  }
}