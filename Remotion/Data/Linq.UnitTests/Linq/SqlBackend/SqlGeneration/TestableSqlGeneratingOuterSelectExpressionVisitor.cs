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
using Remotion.Data.Linq.SqlBackend.SqlGeneration;
using Remotion.Data.Linq.UnitTests.Linq.Core.TestUtilities;

namespace Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlGeneration
{
  public class TestableSqlGeneratingOuterSelectExpressionVisitor : SqlGeneratingOuterSelectExpressionVisitor
  {
    public TestableSqlGeneratingOuterSelectExpressionVisitor (ISqlCommandBuilder commandBuilder, ISqlGenerationStage stage)
        : base(commandBuilder, stage)
    {

    }

    public int ColumnPosition
    {
      get { return (int) PrivateInvoke.GetNonPublicField (this, typeof (SqlGeneratingOuterSelectExpressionVisitor), "_columnPosition"); }
    }

    public new Expression VisitNewExpression (NewExpression expression)
    {
      return base.VisitNewExpression (expression);
    }

    public new Expression VisitUnaryExpression (UnaryExpression expression)
    {
      return base.VisitUnaryExpression (expression);
    }
  }
}