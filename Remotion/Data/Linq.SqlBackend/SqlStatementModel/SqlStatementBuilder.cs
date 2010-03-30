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
using System.Collections.Generic;
using Remotion.Data.Linq.Clauses;

namespace Remotion.Data.Linq.SqlBackend.SqlStatementModel
{
  /// <summary>
  /// <see cref="SqlStatementBuilder"/> holds the specific SQL statement data and populates a build method.
  /// </summary>
  public class SqlStatementBuilder
  {
    public Expression ProjectionExpression { get; set; }
    public Expression WhereCondition { get; set; }
    public bool IsCountQuery { get; set; }
    public bool IsDistinctQuery { get; set; }
    public Expression TopExpression { get; set; }

    public List<SqlTableBase> SqlTables { get; private set; }
    public List<Ordering> Orderings { get; private set; }

    public SqlStatementBuilder ()
    {
      SqlTables = new List<SqlTableBase> ();
      Orderings = new List<Ordering> ();
    }
   
    public SqlStatement GetSqlStatement ()
    {
      var sqlStatement = new SqlStatement (ProjectionExpression, SqlTables, Orderings);

      sqlStatement.IsCountQuery = IsCountQuery;
      sqlStatement.IsDistinctQuery = IsDistinctQuery;
      sqlStatement.WhereCondition = WhereCondition;
      sqlStatement.TopExpression = TopExpression;

      return sqlStatement;
    }

  }
}