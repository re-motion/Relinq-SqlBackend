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
using System.Collections.Generic;
using System.Linq.Expressions;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.SqlBackend.SqlPreparation;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;

namespace Remotion.Data.Linq.UnitTests.SqlBackend.SqlPreparation
{
  public class TestableSqlPreparationQueryModelVisitor : SqlPreparationQueryModelVisitor
  {
    public TestableSqlPreparationQueryModelVisitor (SqlPreparationContext context, ISqlPreparationStage stage)
        : base(context, stage)
    {
    }

    public new Expression ProjectionExpression
    {
      get { return base.ProjectionExpression; }
    }

    public new Expression WhereCondition
    {
      get { return base.WhereCondition; }
    }

    public new List<Ordering> Orderings
    {
      get { return base.Orderings; }
    }

    public new bool IsCountQuery
    {
      get { return base.IsCountQuery; }
    }

    public new bool IsDistinctQuery
    {
      get { return base.IsDistinctQuery; }
    }

    public new Expression TopExpression
    {
      get { return base.TopExpression; }
    }

    public new List<SqlTableBase> SqlTables
    {
      get { return base.SqlTables; }
    }

    public new void AddWhereCondition (Expression translatedExpression)
    {
      base.AddWhereCondition (translatedExpression);
    }
  }
}