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
using System.Linq;
using System.Linq.Expressions;
using Remotion.Data.Linq.SqlBackend.MappingResolution;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Data.Linq.UnitTests.TestDomain;

namespace Remotion.Data.Linq.UnitTests.SqlBackend
{
  public class MappingResolverStub : IMappingResolver
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
                (ResolvedSimpleTableInfo) originatingTable.GetResolvedTableInfo(),
                "ID",
                CreateResolvedTableInfo (joinInfo.ItemType, generator),
                "SubstitutedID");
          case "Assistants":
            return CreateResolvedJoinInfo (
                (ResolvedSimpleTableInfo) originatingTable.GetResolvedTableInfo(),
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
                (ResolvedSimpleTableInfo) originatingTable.GetResolvedTableInfo(),
                "ID",
                CreateResolvedTableInfo (joinInfo.ItemType, generator),
                "KitchenID");
          case "Restaurant":
            return CreateResolvedJoinInfo (
                (ResolvedSimpleTableInfo) originatingTable.GetResolvedTableInfo(),
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
                (ResolvedSimpleTableInfo) originatingTable.GetResolvedTableInfo(),
                "ID",
                CreateResolvedTableInfo (joinInfo.ItemType, generator),
                "RestaurantID");
          case "Cooks":
            return CreateResolvedJoinInfo (
                (ResolvedSimpleTableInfo) originatingTable.GetResolvedTableInfo(),
                "ID",
                CreateResolvedTableInfo (joinInfo.ItemType, generator),
                "RestaurantID");
        }
      }

      throw new NotSupportedException ("Member " + joinInfo.MemberInfo + " is not a valid join member.");
    }

    public virtual Expression ResolveTableReferenceExpression (
        SqlTableReferenceExpression tableReferenceExpression, UniqueIdentifierGenerator generator)
    {
      var resolvedTableInfo = tableReferenceExpression.SqlTable.GetResolvedTableInfo();
      //if (resolvedTableInfo is ResolvedSimpleTableInfo)
      return CreateColumnList (tableReferenceExpression.Type, resolvedTableInfo);
      //else
      //{
      //  var columnExpression = new SqlColumnExpression (
      //            ((ResolvedSubStatementTableInfo) resolvedTableInfo).ItemType,
      //            ((ResolvedSubStatementTableInfo) resolvedTableInfo).TableAlias,
      //            memberExpression.MemberInfo.Name);
      //  return new SqlEntityExpression (typeof (Cook), columnExpression, columnExpression);
      //  //return new SqlSubStatementExpression (
      //    //  ((ResolvedSubStatementTableInfo) resolvedTableInfo).SqlStatement, ((ResolvedSubStatementTableInfo) resolvedTableInfo).ItemType);
      //}
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
            return CreateColumn (
                memberType, memberExpression.SqlTable.GetResolvedTableInfo(), memberExpression.MemberInfo.Name);
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
            return CreateColumn (
                memberType, memberExpression.SqlTable.GetResolvedTableInfo(), memberExpression.MemberInfo.Name);
          case "SubKitchen":
            return new SqlEntityRefMemberExpression (memberExpression.SqlTable, memberExpression.MemberInfo);
        }
      }

      throw new NotSupportedException ("Cannot resolve member: " + memberExpression.MemberInfo);
    }

    public Expression ResolveConstantExpression (ConstantExpression constantExpression)
    {
      if (constantExpression.Value is Cook)
        return new SqlEntityConstantExpression (typeof (Cook), constantExpression.Value, ((Cook) constantExpression.Value).ID);
      else
        return constantExpression;
    }

    private SqlColumnExpression CreateColumn (Type columnType, IResolvedTableInfo resolvedSimpleTableInfo, string columnName)
    {
      return new SqlColumnExpression (columnType, resolvedSimpleTableInfo.TableAlias, columnName);
    }

    private Expression CreateColumnList (Type entityType, IResolvedTableInfo tableInfo)
    {
      Type type = ((AbstractTableInfo) tableInfo).ItemType;
      if (type == typeof (Cook) || type == typeof (IQueryable<Cook>))
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
      else if (type == typeof (Kitchen) || type == typeof (IQueryable<Kitchen>))
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
      else if (type == typeof (Restaurant) || type == typeof (IQueryable<Restaurant>))
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
      throw new NotSupportedException ("The type " + ((AbstractTableInfo) tableInfo).ItemType + " is not a queryable type.");
    }

    private ResolvedSimpleTableInfo CreateResolvedTableInfo (Type entityType, UniqueIdentifierGenerator generator)
    {
      return new ResolvedSimpleTableInfo (entityType, entityType.Name + "Table", generator.GetUniqueIdentifier ("t"));
    }

    private AbstractJoinInfo CreateResolvedJoinInfo (
        ResolvedSimpleTableInfo originatingTableInfo, string primaryKeyName, ResolvedSimpleTableInfo foreignTableInfo, string foreignKeyName)
    {
      var primaryColumn = CreateColumn (typeof (int), originatingTableInfo, primaryKeyName);
      var foreignColumn = CreateColumn (typeof (int), foreignTableInfo, foreignKeyName);

      return new ResolvedJoinInfo (foreignTableInfo, primaryColumn, foreignColumn);
    }
  }
}