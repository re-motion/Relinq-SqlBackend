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
using Remotion.Data.Linq.SqlBackend.MappingResolution;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Data.Linq.UnitTests.TestDomain;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.UnitTests.SqlBackend
{
  public class SqlStatementResolverStub : ISqlStatementResolver
  {
    public virtual AbstractTableSource ResolveConstantTableSource (ConstantTableSource tableSource)
    {
      var tableName = string.Format ("{0}Table", tableSource.ItemType.Name);
      var tableAlias = tableName.Substring (0, 1).ToLower();
      return new SqlTableSource (typeof (string), tableName, tableAlias);
    }

    public virtual Expression ResolveTableReferenceExpression (SqlTableReferenceExpression tableReferenceExpression)
    {
      var tableSource = tableReferenceExpression.SqlTable.TableSource;
      if (tableSource.ItemType == typeof (Cook))
      {
        tableReferenceExpression.SqlTable.TableSource = new SqlTableSource (typeof (Cook), "Cook", "c");
        return new SqlColumnListExpression (
            tableReferenceExpression.Type,
            new[]
            {
                new SqlColumnExpression (typeof (Cook), "c", "ID"),
                new SqlColumnExpression (typeof (Cook), "c", "Name"),
                new SqlColumnExpression (typeof (Cook), "c", "City")
            });
      }
      throw new ArgumentTypeException ("tableReferenceExpression.SqlTable.TableSource", typeof (Cook), tableSource.ItemType);
    }

    public virtual Expression ResolveMemberExpression (SqlMemberExpression memberExpression, UniqueIdentifierGenerator generator)
    {
      if (memberExpression.MemberInfo == typeof (Cook).GetProperty ("Substitution"))
      {
        var sqlJoinedTableSource = ResolveJoinedTableSource ((JoinedTableSource) memberExpression.SqlTable.TableSource);

        //var table = memberExpression.SqlTable.GetOrAddJoin (memberExpression.MemberInfo, (JoinedTableSource) memberExpression.SqlTable.TableSource);
        memberExpression.SqlTable.TableSource = sqlJoinedTableSource;
        return new SqlColumnExpression (typeof (Cook), generator.GetUniqueIdentifier ("t"), "FirstName");
      }
      else
      {
        memberExpression.SqlTable.TableSource = new SqlTableSource (typeof (Cook), "Cook", "c");
        return new SqlColumnExpression (typeof (Cook), "c", "FirstName");
      }
    }

    public AbstractTableSource ResolveJoinedTableSource (JoinedTableSource tableSource)
    {
      return CreateSqlJoinedTableSource (tableSource);
    }

    private SqlJoinedTableSource CreateSqlJoinedTableSource (JoinedTableSource tableSource)
    {
      if (tableSource.MemberInfo.Name == "Substitution")
      {
        var primaryColumn = new SqlColumnExpression (typeof (int), "t1", "ID");
        var foreignColumn = new SqlColumnExpression (typeof (int), "t2", "SubstitutionID");
        return new SqlJoinedTableSource (tableSource, primaryColumn, foreignColumn);
      }
      throw new NotSupportedException ("Only Cook.Substitution is supported.");
    }
  }
}