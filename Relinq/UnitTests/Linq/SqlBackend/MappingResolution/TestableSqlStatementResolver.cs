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
using Remotion.Linq.SqlBackend.MappingResolution;
using Remotion.Linq.SqlBackend.SqlStatementModel;

namespace Remotion.Data.Linq.UnitTests.Linq.SqlBackend.MappingResolution
{
  public class TestableSqlStatementResolver : SqlStatementResolver
  {
    public TestableSqlStatementResolver (IMappingResolutionStage stage, IMappingResolutionContext context)
        : base (stage, context)
    {
    }

    public new Expression ResolveSelectProjection (Expression selectProjection, SqlStatementBuilder sqlStatementBuilder)
    {
      return base.ResolveSelectProjection (selectProjection, sqlStatementBuilder);
    }

    public new void ResolveSqlTable (SqlTable sqlTable)
    {
      base.ResolveSqlTable (sqlTable);
    }

    public new Expression ResolveTopExpression (Expression topExpression)
    {
      return base.ResolveTopExpression (topExpression);
    }

    public new Expression ResolveWhereCondition (Expression whereCondition)
    {
      return base.ResolveWhereCondition (whereCondition);
    }

    public new Expression ResolveGroupByExpression (Expression expression)
    {
      return base.ResolveGroupByExpression (expression);
    }

    public new Expression ResolveOrderingExpression (Expression orderByExpression)
    {
      return base.ResolveOrderingExpression (orderByExpression);
    }

    public new void ResolveJoinedTable (SqlJoinedTable joinedTable)
    {
      base.ResolveJoinedTable (joinedTable);
    }

    public new SqlStatement ResolveSqlStatement (SqlStatement sqlStatement)
    {
      return base.ResolveSqlStatement (sqlStatement);
    }
  }
}