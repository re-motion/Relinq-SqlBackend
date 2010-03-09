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
      return new SqlTableSource (tableSource.ItemType, tableName, tableAlias);
    }

    public virtual Expression ResolveTableReferenceExpression (SqlTableReferenceExpression tableReferenceExpression)
    {
      var resolvedTableSource = tableReferenceExpression.SqlTable.GetResolvedTableSource();
      return CreateColumnList (tableReferenceExpression.Type, resolvedTableSource);
    }

    public virtual Expression ResolveMemberExpression (SqlMemberExpression memberExpression, UniqueIdentifierGenerator generator)
    {
      var memberType = ReflectionUtility.GetFieldOrPropertyType (memberExpression.MemberInfo);
      if (memberExpression.MemberInfo.DeclaringType == typeof (Cook))
      {
        switch (memberExpression.MemberInfo.Name)
        {
          case "IsStarredCook":
          case "FirstName":
            return CreateColumn (memberType, memberExpression.SqlTable.GetResolvedTableSource(), memberExpression.MemberInfo.Name + "Column");
          case "Substitution":
            throw new NotImplementedException ("TODO"); // Integration test: select cook.Substitution; select cook.Substitution.FirstName; select cook.Substitution.Substitution.FirstName
        }
      }

      throw new NotSupportedException ("Cannot resolve member: " + memberExpression.MemberInfo);

      //var joinedTableSource = memberExpression.SqlTable.JoinInfo as JoinedTableSource;
      //if(joinedTableSource!=null && joinedTableSource.MemberInfo.Name=="Substitution")
      //{
      //  var sqlJoinedTableSource = ResolveJoinedTableSource ((JoinedTableSource) memberExpression.SqlTable.JoinInfo);
      //  memberExpression.SqlTable.JoinInfo = sqlJoinedTableSource;
      //  return new SqlColumnExpression (typeof (Cook),  "c", "FirstName");
      //}
      //else
      //{
      //  memberExpression.SqlTable.JoinInfo = new SqlTableSource (typeof (Cook), "Cook", "c");
      //  return new SqlColumnExpression (typeof (Cook), "c", "FirstName");
      //}

     
    }

    public AbstractJoinInfo ResolveJoinedTableSource (JoinedTableSource tableSource)
    {
      return CreateSqlJoinedTableSource (tableSource);
    }

    private SqlJoinedTableSource CreateSqlJoinedTableSource (JoinedTableSource tableSource)
    {
      if (tableSource.MemberInfo.Name == "Substitution")
      {
        var primaryColumn = new SqlColumnExpression (typeof (int), "c", "ID");
        var foreignColumn = new SqlColumnExpression (typeof (int), "s", "SubstitutionID");
        var newTableSource = new SqlTableSource (tableSource.ItemType, tableSource.MemberInfo.Name + "Table", "s");
        return new SqlJoinedTableSource (newTableSource, primaryColumn, foreignColumn);
      }
      throw new NotSupportedException ("Only Cook.Substitution is supported.");
    }

    private SqlColumnExpression CreateColumn (Type columnType, SqlTableSource sqlTableSource, string columnName)
    {
      return new SqlColumnExpression (columnType, sqlTableSource.TableAlias, columnName);
    }

    private Expression CreateColumnList (Type entityType, SqlTableSource tableSource)
    {
      if (tableSource.ItemType == typeof (Cook))
      {
        return new SqlColumnListExpression (
            entityType,
            new[]
            {
                new SqlColumnExpression (typeof (int), tableSource.TableAlias, "ID"),
                new SqlColumnExpression (typeof (string), tableSource.TableAlias, "Name"),
                new SqlColumnExpression (typeof (string), tableSource.TableAlias, "City")
            });
      }
      throw new ArgumentTypeException ("tableReferenceExpression.SqlTable.JoinInfo", typeof (Cook), tableSource.ItemType);
    }
  }
}