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

namespace Remotion.Data.Linq.UnitTests.SqlBackend
{
  public class SqlStatementResolverStub : ISqlStatementResolver
  {
    public virtual AbstractTableInfo ResolveTableInfo (UnresolvedTableInfo tableInfo, UniqueIdentifierGenerator generator)
    {
      switch (tableInfo.ItemType.Name)
      {
        case "Cook":
        case "Kitchen":
        case "Restaurant":
        case "Compyany":
          return CreateResolvedTableInfo (tableInfo.ItemType, generator);
      }

      throw new NotSupportedException ("The type " + tableInfo.ItemType + " cannot be queried from the stub provider.");
    }

    public AbstractJoinInfo ResolveJoinInfo (SqlTableBase originatingTable, UnresolvedJoinInfo joinInfo, UniqueIdentifierGenerator generator)
    {
      if (joinInfo.MemberInfo.DeclaringType == typeof (Cook))
      {
        switch (joinInfo.MemberInfo.Name)
        {
          case "Substitution":
            return CreateResolvedJoinInfo (
                originatingTable.GetResolvedTableInfo(),
                "ID",
                CreateResolvedTableInfo (joinInfo.ItemType, generator),
                "SubstitutedID");
          case "Assistants":
            return CreateResolvedJoinInfo (
                originatingTable.GetResolvedTableInfo (),
                "ID",
                CreateResolvedTableInfo (joinInfo.ItemType, generator),
                "AssistedID");
        }
      }
      else if (joinInfo.MemberInfo.DeclaringType == typeof (Kitchen))
      {
        switch (joinInfo.MemberInfo.Name)
        {
          case "Cook":
            return CreateResolvedJoinInfo (
                originatingTable.GetResolvedTableInfo(),
                "ID",
                CreateResolvedTableInfo (joinInfo.ItemType, generator),
                "KitchenID");
          case "Restaurant":
            return CreateResolvedJoinInfo (
                originatingTable.GetResolvedTableInfo(),
                "RestaurantID",
                CreateResolvedTableInfo (joinInfo.ItemType, generator),
                "ID");
        }
      }
      else if (joinInfo.MemberInfo.DeclaringType == typeof (Restaurant))
      {
        switch (joinInfo.MemberInfo.Name)
        {
          case "SubKitchen":
            return CreateResolvedJoinInfo (
               originatingTable.GetResolvedTableInfo (),
               "ID",
               CreateResolvedTableInfo (joinInfo.ItemType, generator),
               "RestaurantID");
          case "Cooks":
            return CreateResolvedJoinInfo (
                originatingTable.GetResolvedTableInfo (),
                "ID",
                CreateResolvedTableInfo (joinInfo.ItemType, generator),
                "RestaurantID");
        }
      }

      throw new NotSupportedException ("Member " + joinInfo.MemberInfo + " is not a valid join member.");
    }

    public virtual Expression ResolveTableReferenceExpression (SqlTableReferenceExpression tableReferenceExpression, UniqueIdentifierGenerator generator)
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
          case "ID":
          case "FirstName":
          case "Name":
          case "IsFullTimeCook":
          case "IsStarredCook":
          case "Weight":
            return CreateColumn (memberType, memberExpression.SqlTable.GetResolvedTableInfo(), memberExpression.MemberInfo.Name);
          case "Substitution":
            return new SqlEntityRefMemberExpression (memberExpression.SqlTable, memberExpression.MemberInfo);
        }
      }
      else if (memberExpression.MemberInfo.DeclaringType == typeof (Kitchen))
      {
        switch (memberExpression.MemberInfo.Name)
        {
          case "ID":
          case "Name":
          case "RoomNumber":
            return CreateColumn (memberType, memberExpression.SqlTable.GetResolvedTableInfo(), memberExpression.MemberInfo.Name);
          case "Cook":
          case "Restaurant":
            return new SqlEntityRefMemberExpression (memberExpression.SqlTable, memberExpression.MemberInfo);
        }
      }
      else if (memberExpression.MemberInfo.DeclaringType == typeof (Restaurant))
      {
        switch (memberExpression.MemberInfo.Name)
        {
          case "ID":
            return CreateColumn (memberType, memberExpression.SqlTable.GetResolvedTableInfo (), memberExpression.MemberInfo.Name);
          case "SubKitchen":
            return new SqlEntityRefMemberExpression (memberExpression.SqlTable, memberExpression.MemberInfo);
        }
      }

      throw new NotSupportedException ("Cannot resolve member: " + memberExpression.MemberInfo);
    }

    private SqlColumnExpression CreateColumn (Type columnType, ResolvedTableInfo resolvedTableInfo, string columnName)
    {
      return new SqlColumnExpression (columnType, resolvedTableInfo.TableAlias, columnName);
    }

    private Expression CreateColumnList (Type entityType, ResolvedTableInfo tableInfo)
    {
      if (tableInfo.ItemType == typeof (Cook))
      {
        var primaryKeyColumn = CreateColumn (typeof (int), tableInfo, "ID");
        return new SqlEntityExpression (
            entityType,
            primaryKeyColumn,
            new[]
            {
                primaryKeyColumn,
                CreateColumn (typeof (string), tableInfo, "FirstName"),
                CreateColumn (typeof (string), tableInfo, "Name"),
                CreateColumn (typeof (bool), tableInfo, "IsStarredCook"),
                CreateColumn (typeof (bool), tableInfo, "IsFullTimeCook"),
                CreateColumn (typeof (int), tableInfo, "SubstitutedID"),
                CreateColumn (typeof (int), tableInfo, "KitchenID")
            });
      }
      else if (tableInfo.ItemType == typeof (Kitchen))
      {
        var primaryKeyColumn = CreateColumn (typeof (int), tableInfo, "ID");
        return new SqlEntityExpression (
            entityType,
            primaryKeyColumn,
            new[]
            {
                primaryKeyColumn,
                CreateColumn (typeof (int), tableInfo, "CookID"),
                CreateColumn (typeof (string), tableInfo, "Name"),
                CreateColumn (typeof (int), tableInfo, "RestaurantID"),
                CreateColumn (typeof (int), tableInfo, "SubKitchenID"),
            });
      }
      else if (tableInfo.ItemType == typeof (Restaurant))
      {
        var primaryKeyColumn = CreateColumn (typeof (int), tableInfo, "ID");
        return new SqlEntityExpression (
            entityType,
            primaryKeyColumn,
            new[]
            {
                primaryKeyColumn,
                CreateColumn (typeof (int), tableInfo, "CookID"),
                CreateColumn (typeof (string), tableInfo, "Name"),
            });
      }
      throw new NotSupportedException ("The type " + tableInfo.ItemType + " is not a queryable type.");
    }

    private ResolvedTableInfo CreateResolvedTableInfo (Type entityType, UniqueIdentifierGenerator generator)
    {
      return new ResolvedTableInfo (entityType, entityType.Name + "Table", generator.GetUniqueIdentifier("t"));
    }

    private AbstractJoinInfo CreateResolvedJoinInfo (
        ResolvedTableInfo originatingTableInfo, string primaryKeyName, ResolvedTableInfo foreignTableInfo, string foreignKeyName)
    {
      var primaryColumn = CreateColumn (typeof (int), originatingTableInfo, primaryKeyName);
      var foreignColumn = CreateColumn (typeof (int), foreignTableInfo, foreignKeyName);

      return new ResolvedJoinInfo (foreignTableInfo, primaryColumn, foreignColumn);
    }
  }
}