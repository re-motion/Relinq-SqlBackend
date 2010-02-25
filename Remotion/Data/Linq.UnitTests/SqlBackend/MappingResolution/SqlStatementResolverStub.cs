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
using Remotion.Data.Linq.SqlBackend.MappingResolution;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;

namespace Remotion.Data.Linq.UnitTests.SqlBackend.MappingResolution
{
  public class SqlStatementResolverStub : ISqlStatementResolver
  {
    public virtual SqlTableSource ResolveConstantTableSource (ConstantTableSource tableSource)
    {
      var tableName = tableSource.ConstantExpression.Value.ToString();
      var tableAlias = tableName.Substring (0, 1).ToLower();
      return new SqlTableSource (typeof(string), tableName, tableAlias);
    }

    public virtual Expression ResolveTableReferenceExpression (SqlTableReferenceExpression tableReferenceExpression)
    {
      // TODO: Change to get sqlTable from tableReferenceExpression instead of creating a new one.
      var sqlTable = new SqlTable();
      sqlTable.TableSource = new SqlTableSource (typeof(string), "Table", "t");
      return new SqlColumnListExpression (
          tableReferenceExpression.Type,
          new[]
          {
              new SqlColumnExpression (typeof (int), "t", "ID"),
              new SqlColumnExpression (typeof (int), "t", "Name"),
              new SqlColumnExpression (typeof (int), "t", "City")
          });
    }
  }
}