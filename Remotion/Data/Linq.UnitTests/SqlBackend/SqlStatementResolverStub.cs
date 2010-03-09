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
    public virtual AbstractTableInfo ResolveTableInfo (UnresolvedTableInfo tableInfo)
    {
      var tableName = string.Format ("{0}Table", tableInfo.ItemType.Name);
      var tableAlias = tableName.Substring (0, 1).ToLower();
      return new ResolvedTableInfo (tableInfo.ItemType, tableName, tableAlias);
    }

    public virtual Expression ResolveTableReferenceExpression (SqlTableReferenceExpression tableReferenceExpression)
    {
      var resolvedTableInfo = tableReferenceExpression.SqlTable.GetResolvedTableInfo();
      return CreateColumnList (tableReferenceExpression.Type, resolvedTableInfo);
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
            return CreateColumn (memberType, memberExpression.SqlTable.GetResolvedTableInfo(), memberExpression.MemberInfo.Name + "Column");
          case "Substitution":
            throw new NotImplementedException ("TODO"); // Integration test: select cook.Substitution; select cook.Substitution.FirstName; select cook.Substitution.Substitution.FirstName
        }
      }

      throw new NotSupportedException ("Cannot resolve member: " + memberExpression.MemberInfo);
    }

    public AbstractJoinInfo ResolveJoinInfo (UnresolvedJoinInfo joinInfo)
    {
      if (joinInfo.MemberInfo.Name == "Substitution")
      {
        var primaryColumn = new SqlColumnExpression (typeof (int), "c", "ID");
        var foreignColumn = new SqlColumnExpression (typeof (int), "s", "SubstitutionID");
        var foreignTableInfo = new ResolvedTableInfo (joinInfo.ItemType, joinInfo.MemberInfo.Name + "Table", "s");
        return new ResolvedJoinInfo (foreignTableInfo, primaryColumn, foreignColumn);
      }
      throw new NotSupportedException ("Only Cook.Substitution is supported.");
    }

    private SqlColumnExpression CreateColumn (Type columnType, ResolvedTableInfo resolvedTableInfo, string columnName)
    {
      return new SqlColumnExpression (columnType, resolvedTableInfo.TableAlias, columnName);
    }

    private Expression CreateColumnList (Type entityType, ResolvedTableInfo tableInfo)
    {
      if (tableInfo.ItemType == typeof (Cook))
      {
        return new SqlColumnListExpression (
            entityType,
            new[]
            {
                new SqlColumnExpression (typeof (int), tableInfo.TableAlias, "ID"),
                new SqlColumnExpression (typeof (string), tableInfo.TableAlias, "Name"),
                new SqlColumnExpression (typeof (string), tableInfo.TableAlias, "City")
            });
      }
      throw new ArgumentTypeException ("tableReferenceExpression.SqlTable.JoinInfo", typeof (Cook), tableInfo.ItemType);
    }
  }
}