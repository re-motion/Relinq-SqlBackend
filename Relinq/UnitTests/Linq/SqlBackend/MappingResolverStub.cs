// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// re-linq is free software; you can redistribute it and/or modify it under 
// the terms of the GNU Lesser General Public License as published by the 
// Free Software Foundation; either version 2.1 of the License, 
// or (at your option) any later version.
// 
// re-linq is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-linq; if not, see http://www.gnu.org/licenses.
// 
using System;
using System.Linq.Expressions;
using System.Reflection;
using Remotion.Linq.UnitTests.Linq.Core.TestDomain;
using Remotion.Linq;
using Remotion.Linq.SqlBackend.MappingResolution;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Linq.SqlBackend.SqlStatementModel.Unresolved;

namespace Remotion.Linq.UnitTests.Linq.SqlBackend
{
  public class MappingResolverStub : IMappingResolver
  {
    public virtual IResolvedTableInfo ResolveTableInfo (UnresolvedTableInfo tableInfo, UniqueIdentifierGenerator generator)
    {
      switch (tableInfo.ItemType.Name)
      {
        case "Cook":
        case "Kitchen":
        case "Restaurant":
        case "Company":
          return CreateResolvedTableInfo (tableInfo.ItemType, generator);
        case "Chef":
          return new ResolvedSimpleTableInfo (tableInfo.ItemType, "dbo."+tableInfo.ItemType.Name + "Table", generator.GetUniqueIdentifier ("t"));
      }

      throw new UnmappedItemException ("The type " + tableInfo.ItemType + " cannot be queried from the stub provider.");
    }

    public ResolvedJoinInfo ResolveJoinInfo (UnresolvedJoinInfo joinInfo, UniqueIdentifierGenerator generator)
    {
      if (joinInfo.MemberInfo.DeclaringType == typeof (Cook))
      {
        switch (joinInfo.MemberInfo.Name)
        {
          case "Substitution":
            return CreateResolvedJoinInfo (
                joinInfo.OriginatingEntity,
                "ID",
                typeof (int), true, CreateResolvedTableInfo (joinInfo.ItemType, generator), "SubstitutedID", typeof (int), false);
          case "Assistants":
            return CreateResolvedJoinInfo (
                joinInfo.OriginatingEntity,
                "ID",
                typeof (int), true, CreateResolvedTableInfo (joinInfo.ItemType, generator), "AssistedID", typeof (int), false);
        }
      }
      else if (joinInfo.MemberInfo.DeclaringType == typeof (Kitchen))
      {
        switch (joinInfo.MemberInfo.Name)
        {
          case "Cook":
            return CreateResolvedJoinInfo (
                joinInfo.OriginatingEntity,
                "ID", typeof (int), true, CreateResolvedTableInfo (joinInfo.ItemType, generator), "KitchenID", typeof (int), false);
          case "Restaurant":
            return CreateResolvedJoinInfo (
                joinInfo.OriginatingEntity,
                "RestaurantID", typeof (int), true, CreateResolvedTableInfo (joinInfo.ItemType, generator), "ID", typeof (int), false);
        }
      }
      else if (joinInfo.MemberInfo.DeclaringType == typeof (Restaurant))
      {
        switch (joinInfo.MemberInfo.Name)
        {
          case "SubKitchen":
            return CreateResolvedJoinInfo (
                joinInfo.OriginatingEntity,
                "ID", typeof (int), true, CreateResolvedTableInfo (joinInfo.ItemType, generator), "RestaurantID", typeof (int), false);
          case "Cooks":
            return CreateResolvedJoinInfo (
                joinInfo.OriginatingEntity,
                "ID", typeof (int), true, CreateResolvedTableInfo (joinInfo.ItemType, generator), "RestaurantID", typeof (int), false);
          case "CompanyIfAny":
            return CreateResolvedJoinInfo (
                joinInfo.OriginatingEntity,
                "CompanyID",
                typeof (int?), 
                false, 
                CreateResolvedTableInfo (joinInfo.ItemType, generator), 
                "ID", 
                typeof (int), 
                true);

        }
      }
      else if (joinInfo.MemberInfo.DeclaringType == typeof (Company))
      {
        switch (joinInfo.MemberInfo.Name)
        {
          case "AllRestaurants":
            return CreateResolvedJoinInfo (
                joinInfo.OriginatingEntity,
                "ID",
                typeof (int), true, CreateResolvedTableInfo (joinInfo.ItemType, generator), "CompanyID", typeof (int?), false);
        }
      }

      throw new UnmappedItemException ("Member " + joinInfo.MemberInfo + " is not a valid join member.");
    }

    public virtual SqlEntityDefinitionExpression ResolveSimpleTableInfo (
        IResolvedTableInfo tableInfo, UniqueIdentifierGenerator generator)
    {
      Type type = tableInfo.ItemType;
      if (type == typeof (Cook))
      {
        var primaryKeyColumn = CreateColumn (typeof (int), tableInfo.TableAlias, "ID", true);
        return new SqlEntityDefinitionExpression (
            tableInfo.ItemType,
            tableInfo.TableAlias, null,
            primaryKeyColumn,
            new[]
            {
                primaryKeyColumn,
                CreateColumn (typeof (string), tableInfo.TableAlias, "FirstName", false),
                CreateColumn (typeof (string), tableInfo.TableAlias, "Name", false),
                CreateColumn (typeof (bool), tableInfo.TableAlias, "IsStarredCook", false),
                CreateColumn (typeof (bool), tableInfo.TableAlias, "IsFullTimeCook", false),
                CreateColumn (typeof (int), tableInfo.TableAlias, "SubstitutedID", false),
                CreateColumn (typeof (int), tableInfo.TableAlias, "KitchenID", false)
            });
      }
      else if (type == typeof (Kitchen))
      {
        var primaryKeyColumn = CreateColumn (typeof (int), tableInfo.TableAlias, "ID", true);
        return new SqlEntityDefinitionExpression (
            tableInfo.ItemType,
            tableInfo.TableAlias, null,
            primaryKeyColumn,
            new[]
            {
                primaryKeyColumn,
                CreateColumn (typeof (int), tableInfo.TableAlias, "CookID", false),
                CreateColumn (typeof (string), tableInfo.TableAlias, "Name", false),
                CreateColumn (typeof (int), tableInfo.TableAlias, "RestaurantID", false),
                CreateColumn (typeof (int), tableInfo.TableAlias, "SubKitchenID", false),
                CreateColumn (typeof (DateTime?), tableInfo.TableAlias, "LastCleaningDay", false),
            });
      }
      else if (type == typeof (Restaurant))
      {
        var primaryKeyColumn = CreateColumn (typeof (int), tableInfo.TableAlias, "ID", true);
        return new SqlEntityDefinitionExpression (
            tableInfo.ItemType,
            tableInfo.TableAlias, null,
            primaryKeyColumn,
            new[]
            {
                primaryKeyColumn,
                CreateColumn (typeof (int), tableInfo.TableAlias, "CookID", false),
                CreateColumn (typeof (string), tableInfo.TableAlias, "Name", false),
            });
      }
      else if (type == typeof (Chef))
      {
        var primaryKeyColumn = CreateColumn (typeof (int), tableInfo.TableAlias, "ID", true);
        return new SqlEntityDefinitionExpression (
            tableInfo.ItemType,
            tableInfo.TableAlias, null,
            primaryKeyColumn,
            new[]
            {
                primaryKeyColumn,
                CreateColumn (typeof (string), tableInfo.TableAlias, "FirstName", false),
                CreateColumn (typeof (string), tableInfo.TableAlias, "Name", false),
                CreateColumn (typeof (bool), tableInfo.TableAlias, "IsStarredCook", false),
                CreateColumn (typeof (bool), tableInfo.TableAlias, "IsFullTimeCook", false),
                CreateColumn (typeof (int), tableInfo.TableAlias, "SubstitutedID", false),
                CreateColumn (typeof (int), tableInfo.TableAlias, "KitchenID", false),
                CreateColumn (typeof (string), tableInfo.TableAlias, "LetterOfRecommendation", false)
            });
      }
      else if (type == typeof (Company))
      {
        var primaryKeyColumn = CreateColumn (typeof (int), tableInfo.TableAlias, "ID", true);
        return new SqlEntityDefinitionExpression (
            tableInfo.ItemType,
            tableInfo.TableAlias, null,
            primaryKeyColumn,
            new[]
            {
                primaryKeyColumn
            });
      }
      throw new UnmappedItemException (string.Format ("Type '{0}' is not supported by the MappingResolverStub.", type.Name));
    }

    public virtual Expression ResolveMemberExpression (SqlEntityExpression originatingEntity, MemberInfo memberInfo)
    {
      var memberType = ReflectionUtility.GetMemberReturnType (memberInfo);
      if (memberInfo.DeclaringType == typeof (Cook))
      {
        switch (memberInfo.Name)
        {
          case "ID":
            return originatingEntity.GetColumn (memberType, memberInfo.Name, true);
          case "FirstName":
          case "Name":
          case "IsFullTimeCook":
          case "IsStarredCook":
          case "Weight":
          case "MetaID":
          case "SpecificInformation":
            return originatingEntity.GetColumn (memberType, memberInfo.Name, false);
          case "Substitution":
            return new SqlEntityRefMemberExpression (originatingEntity, memberInfo);
        }
      }
      else if (memberInfo.DeclaringType == typeof (ISpecificCook))
      {
        switch (memberInfo.Name)
        {
          case "SpecificInformation":
            return originatingEntity.GetColumn (memberType, memberInfo.Name, false);
        }
      }
      else if (memberInfo.DeclaringType == typeof (Chef))
      {
        switch (memberInfo.Name)
        {
          case "LetterOfRecommendation":
            return originatingEntity.GetColumn (memberType, memberInfo.Name, false);
        }
      }
      else if (memberInfo.DeclaringType == typeof (Kitchen))
      {
        switch (memberInfo.Name)
        {
          case "ID":
            return originatingEntity.GetColumn (memberType, memberInfo.Name, true);
          case "Name":
          case "RoomNumber":
          case "LastCleaningDay":
            return originatingEntity.GetColumn (memberType, memberInfo.Name, false);
          case "Cook":
          case "Restaurant":
            return new SqlEntityRefMemberExpression (originatingEntity, memberInfo);
        }
      }
      else if (memberInfo.DeclaringType == typeof (Restaurant))
      {
        switch (memberInfo.Name)
        {
          case "ID":
            return originatingEntity.GetColumn (memberType, memberInfo.Name, true);
          case "SubKitchen":
          case "CompanyIfAny":
            return new SqlEntityRefMemberExpression (originatingEntity, memberInfo);
        }
      }
      else if (memberInfo.DeclaringType == typeof (Company))
      {
        switch (memberInfo.Name)
        {
          case "ID":
            return originatingEntity.GetColumn (memberType, memberInfo.Name, true);
        }
      }

      throw new UnmappedItemException ("Cannot resolve member: " + memberInfo);
    }

    public Expression ResolveMemberExpression (SqlColumnExpression sqlColumnExpression, MemberInfo memberInfo)
    {
      if (memberInfo.DeclaringType == typeof (MetaID))
      {
        if (memberInfo.Name == "ClassID")
          return new SqlColumnDefinitionExpression (typeof (string), sqlColumnExpression.OwningTableAlias, "ClassID", false);
      }
      throw new UnmappedItemException ("Cannot resolve member for: " + memberInfo.Name);
    }

    public Expression ResolveConstantExpression (ConstantExpression constantExpression)
    {
      if (constantExpression.Value is Cook)
        return new SqlEntityConstantExpression (typeof (Cook), constantExpression.Value, ((Cook) constantExpression.Value).ID);
      else if (constantExpression.Value is Company)
        return new SqlEntityConstantExpression (typeof (Company), constantExpression.Value, ((Company) constantExpression.Value).ID);
      else
        return constantExpression;
    }

    public Expression ResolveTypeCheck (Expression checkedExpression, Type desiredType)
    {
       if (desiredType.IsAssignableFrom (checkedExpression.Type))
         return Expression.Constant (true);
       else if (desiredType == typeof (Chef) && checkedExpression.Type == typeof (Cook))
         return Expression.MakeMemberAccess (checkedExpression, typeof (Cook).GetProperty ("IsStarredCook"));
       else
         throw new UnmappedItemException ("Cannot resolve type for checkedExpression: " + checkedExpression.Type.Name);
    }

    private SqlColumnExpression CreateColumn (Type columnType, string tableAlias, string columnName, bool isPriamryKey)
    {
      return new SqlColumnDefinitionExpression (columnType, tableAlias, columnName, isPriamryKey);
    }

    private ResolvedSimpleTableInfo CreateResolvedTableInfo (Type entityType, UniqueIdentifierGenerator generator)
    {
      return new ResolvedSimpleTableInfo (entityType, entityType.Name + "Table", generator.GetUniqueIdentifier ("t"));
    }

    private ResolvedJoinInfo CreateResolvedJoinInfo (SqlEntityExpression originatingEntity, string leftColumnName, Type leftColumnType, bool leftColumnIsPrimaryKey, IResolvedTableInfo joinedTableInfo, string rightColumnName, Type rightColumnType, bool rightColumnIsPrimaryKey)
    {
      var leftColumn = originatingEntity.GetColumn (leftColumnType, leftColumnName, leftColumnIsPrimaryKey);
      var rightColumn = CreateColumn (rightColumnType, joinedTableInfo.TableAlias, rightColumnName, rightColumnIsPrimaryKey);

      return new ResolvedJoinInfo (joinedTableInfo, leftColumn, rightColumn);
    }
  }
}