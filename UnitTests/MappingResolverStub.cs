// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (c) rubicon IT GmbH, www.rubicon.eu
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
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using Remotion.Linq.SqlBackend.MappingResolution;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Linq.SqlBackend.UnitTests.TestDomain;
using Remotion.Linq.SqlBackend.Utilities;

namespace Remotion.Linq.SqlBackend.UnitTests
{
  public class MappingResolverStub : IMappingResolver
  {
    public virtual IResolvedTableInfo ResolveTableInfo (UnresolvedTableInfo tableInfo, UniqueIdentifierGenerator generator)
    {
      switch (tableInfo.ItemType.Name)
      {
        case "Cook":
        case "Knife":
        case "Kitchen":
        case "Restaurant":
        case "Company":
          return CreateResolvedTableInfo (tableInfo.ItemType, generator);
        case "Chef":
          return new ResolvedSimpleTableInfo (tableInfo.ItemType, "dbo."+tableInfo.ItemType.Name + "Table", generator.GetUniqueIdentifier ("t"));
      }

      throw new UnmappedItemException ("The type " + tableInfo.ItemType + " cannot be queried from the stub provider.");
    }

    public ITableInfo ResolveJoinTableInfo (UnresolvedJoinTableInfo tableInfo, UniqueIdentifierGenerator generator)
    {
      return new UnresolvedTableInfo (tableInfo.ItemType);
    }

    public Expression ResolveJoinCondition (SqlEntityExpression originatingEntity, MemberInfo memberInfo, IResolvedTableInfo joinedTableInfo)
    {
      if (memberInfo.DeclaringType == typeof (Cook))
      {
        switch (memberInfo.Name)
        {
          case "Substitution":
            return CreateJoinCondition (
                originatingEntity,
                "ID",
                typeof (int),
                true,
                joinedTableInfo.TableAlias,
                "SubstitutedID",
                typeof (int),
                false);
          case "Substituted":
            return CreateJoinCondition (
                originatingEntity,
                "SubstitutedID",
                typeof (int),
                false,
                joinedTableInfo.TableAlias,
                "ID",
                typeof (int),
                true);
          case "Assistants":
            return CreateJoinCondition (
                originatingEntity,
                "ID",
                typeof (int),
                true,
                joinedTableInfo.TableAlias,
                "AssistedID",
                typeof (int),
                false);
          case "Kitchen":
            return CreateJoinCondition (
                originatingEntity,
                "KitchenID",
                typeof (int),
                false,
                joinedTableInfo.TableAlias,
                "ID",
                typeof (int),
                true);
          case "Knife":
            var leftKey = ResolveMemberExpression (originatingEntity, typeof (Cook).GetProperty ("KnifeID"));
            var rightKey = ResolveTableToEntity (joinedTableInfo.ItemType, joinedTableInfo.TableAlias).GetIdentityExpression();
            return Expression.Equal (leftKey, rightKey);
        }
      }
      else if (memberInfo.DeclaringType == typeof (Kitchen))
      {
        switch (memberInfo.Name)
        {
          case "Cook":
            return CreateJoinCondition (
                originatingEntity,
                "ID", typeof (int), true, joinedTableInfo.TableAlias, "KitchenID", typeof (int), false);
          case "Restaurant":
            return CreateJoinCondition (
                originatingEntity,
                "RestaurantID", typeof (int), false, joinedTableInfo.TableAlias, "ID", typeof (int), true);
        }
      }
      else if (memberInfo.DeclaringType == typeof (Restaurant))
      {
        switch (memberInfo.Name)
        {
          case "SubKitchen":
            return CreateJoinCondition (
                originatingEntity,
                "ID", typeof (int), true, joinedTableInfo.TableAlias, "RestaurantID", typeof (int), false);
          case "Cooks":
            return CreateJoinCondition (
                originatingEntity,
                "ID", typeof (int), true, joinedTableInfo.TableAlias, "RestaurantID", typeof (int), false);
          case "CompanyIfAny":
            return CreateJoinCondition (
                originatingEntity,
                "CompanyID",
                typeof (int?), 
                false, 
                joinedTableInfo.TableAlias, 
                "ID", 
                typeof (int), 
                true);

        }
      }
      else if (memberInfo.DeclaringType == typeof (Company))
      {
        switch (memberInfo.Name)
        {
          case "AllRestaurants":
            return CreateJoinCondition (
                originatingEntity,
                "ID",
                typeof (int), true, joinedTableInfo.TableAlias, "CompanyID", typeof (int?), false);
        }
      }

      throw new UnmappedItemException ("Member " + memberInfo + " is not a valid join member.");
    }

    public virtual SqlEntityDefinitionExpression ResolveSimpleTableInfo (ResolvedSimpleTableInfo tableInfo)
    {
      return ResolveTableToEntity(tableInfo.ItemType, tableInfo.TableAlias);
    }

    private SqlEntityDefinitionExpression ResolveTableToEntity (Type itemType, string tableAlias)
    {
      if (itemType == typeof (Cook))
      {
        return new SqlEntityDefinitionExpression (
            itemType,
            tableAlias,
            null,
            e => e.GetColumn (typeof (int), "ID", true),
            new[]
            {
                CreateColumn (typeof (int), tableAlias, "ID", true),
                CreateColumn (typeof (string), tableAlias, "FirstName", false),
                CreateColumn (typeof (string), tableAlias, "Name", false),
                CreateColumn (typeof (bool), tableAlias, "IsStarredCook", false),
                CreateColumn (typeof (bool), tableAlias, "IsFullTimeCook", false),
                CreateColumn (typeof (int), tableAlias, "SubstitutedID", false),
                CreateColumn (typeof (int), tableAlias, "KitchenID", false),
                CreateColumn (typeof (int), tableAlias, "KnifeID", false),
                CreateColumn (typeof (string), tableAlias, "KnifeClassID", false),
                CreateColumn (typeof (CookRating), tableAlias, "CookRating", false)
            });
      }
      else if (itemType == typeof (Kitchen))
      {
        return new SqlEntityDefinitionExpression (
            itemType,
            tableAlias,
            null,
            e => e.GetColumn (typeof (int), "ID", true),
            new[]
            {
                CreateColumn (typeof (int), tableAlias, "ID", true),
                CreateColumn (typeof (string), tableAlias, "Name", false),
                CreateColumn (typeof (int), tableAlias, "RestaurantID", false),
                CreateColumn (typeof (DateTime?), tableAlias, "LastCleaningDay", false),
                CreateColumn (typeof (bool?), tableAlias, "PassedLastInspection", false),
                CreateColumn (typeof (int?), tableAlias, "LastInspectionScore", false)
            });
      }
      else if (itemType == typeof (Restaurant))
      {
        return new SqlEntityDefinitionExpression (
            itemType,
            tableAlias,
            null,
            e => e.GetColumn (typeof (int), "ID", true),
            new[]
            {
                CreateColumn (typeof (int), tableAlias, "ID", true),
                CreateColumn (typeof (int), tableAlias, "CompanyID", false)
            });
      }
      else if (itemType == typeof (Chef))
      {
        return new SqlEntityDefinitionExpression (
            itemType,
            tableAlias,
            null,
            e => e.GetColumn (typeof (int), "ID", true),
            new[]
            {
                CreateColumn (typeof (int), tableAlias, "ID", true),
                CreateColumn (typeof (string), tableAlias, "FirstName", false),
                CreateColumn (typeof (string), tableAlias, "Name", false),
                CreateColumn (typeof (bool), tableAlias, "IsStarredCook", false),
                CreateColumn (typeof (bool), tableAlias, "IsFullTimeCook", false),
                CreateColumn (typeof (int), tableAlias, "SubstitutedID", false),
                CreateColumn (typeof (int), tableAlias, "KitchenID", false),
                CreateColumn (typeof (int), tableAlias, "KnifeID", false),
                CreateColumn (typeof (int), tableAlias, "KnifeClassID", false),
                CreateColumn (typeof (CookRating), tableAlias, "CookRating", false),
                CreateColumn (typeof (string), tableAlias, "LetterOfRecommendation", false)
            });
      }
      else if (itemType == typeof (Company))
      {
        return new SqlEntityDefinitionExpression (
            itemType,
            tableAlias,
            null,
            e => e.GetColumn (typeof (int), "ID", true),
            new[]
            {
                CreateColumn (typeof (int), tableAlias, "ID", true),
                CreateColumn (typeof (DateTime), tableAlias, "DateOfIncorporation", false)
            });
      }
      else if (itemType == typeof (Knife))
      {
        return new SqlEntityDefinitionExpression (
            itemType,
            tableAlias,
            null,
            e => CreateMetaIDExpression (e.GetColumn (typeof (int), "ID", true), e.GetColumn (typeof (string), "ClassID", true)),
            new[]
            {
                CreateColumn (typeof (int), tableAlias, "ID", true),
                CreateColumn (typeof (string), tableAlias, "ClassID", true),
                CreateColumn (typeof (double), tableAlias, "Sharpness", false)
            });
      }
      throw new UnmappedItemException (string.Format ("Type '{0}' is not supported by the MappingResolverStub.", itemType.Name));
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
          case "SpecificInformation":
            return originatingEntity.GetColumn (memberType, memberInfo.Name, false);
          case "KnifeID":
            return CreateMetaIDExpression (
                originatingEntity.GetColumn (typeof (int), "KnifeID", false), 
                originatingEntity.GetColumn (typeof (string), "KnifeClassID", false));
          case "Substitution":
          case "Substituted":
          case "Kitchen":
          case "Knife":
            return new SqlEntityRefMemberExpression (originatingEntity, memberInfo);
          case "CookRating":
            return originatingEntity.GetColumn (typeof (CookRating), "CookRating", false);
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
          case "PassedLastInspection":
          case "LastInspectionScore":
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
          case "DateOfIncorporation":
            return originatingEntity.GetColumn (memberType, memberInfo.Name, false);
        }
      }
      else if (memberInfo.DeclaringType == typeof (Knife))
      {
        switch (memberInfo.Name)
        {
          case "ID":
            return CreateMetaIDExpression (
                originatingEntity.GetColumn (typeof (int), "ID", true), 
                originatingEntity.GetColumn (typeof (string), "ClassID", true));
          case "Sharpness":
            return originatingEntity.GetColumn (memberType, memberInfo.Name, false);
        }
      }

      throw new UnmappedItemException ("Cannot resolve member: " + memberInfo);
    }

    public Expression ResolveMemberExpression (SqlColumnExpression sqlColumnExpression, MemberInfo memberInfo)
    {
      throw new NotSupportedException (string.Format ("Member '{0}' applied to column '{1}' is not supported.", memberInfo.Name, sqlColumnExpression));
    }

    public Expression ResolveConstantExpression (ConstantExpression constantExpression)
    {
      if (constantExpression.Value is Cook)
        return new SqlEntityConstantExpression (typeof (Cook), constantExpression.Value, Expression.Constant (((Cook) constantExpression.Value).ID, typeof (int)));
      else if (constantExpression.Value is Company)
        return new SqlEntityConstantExpression (typeof (Company), constantExpression.Value, Expression.Constant (((Company) constantExpression.Value).ID, typeof (int)));
      else if (constantExpression.Value is Knife)
        return new SqlEntityConstantExpression (typeof (Knife), constantExpression.Value, Expression.Constant (((Knife) constantExpression.Value).ID, typeof (MetaID)));
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

    public Expression TryResolveOptimizedIdentity (SqlEntityRefMemberExpression entityRefMemberExpression)
    {
      if (entityRefMemberExpression.MemberInfo.DeclaringType == typeof (Cook) && entityRefMemberExpression.MemberInfo.Name == "Knife")
        return ResolveMemberExpression (entityRefMemberExpression.OriginatingEntity, typeof (Cook).GetProperty ("KnifeID"));

      // Prepare a faked join, then check if the foreign key column is on the left side => this is the identity. (Otherwise, return null.)

      var dummyUniqueIdentifierGenerator = new UniqueIdentifierGenerator();
      var unresolvedJoinedTableInfo =
          (UnresolvedTableInfo) ResolveJoinTableInfo (
              new UnresolvedJoinTableInfo (entityRefMemberExpression.OriginatingEntity, entityRefMemberExpression.MemberInfo, JoinCardinality.One),
              dummyUniqueIdentifierGenerator);
      var resolvedJoinedTableInfo = ResolveTableInfo (unresolvedJoinedTableInfo, dummyUniqueIdentifierGenerator);
      var joinCondition = 
          ResolveJoinCondition (entityRefMemberExpression.OriginatingEntity, entityRefMemberExpression.MemberInfo, resolvedJoinedTableInfo);

      var rightKey = ((BinaryExpression) joinCondition).Right;
      while (rightKey.NodeType == ExpressionType.Convert)
        rightKey = ((UnaryExpression) rightKey).Operand;

      if (((SqlColumnExpression) rightKey).IsPrimaryKey)
        return ((BinaryExpression) joinCondition).Left;

      return null;
    }

    public Expression TryResolveOptimizedMemberExpression (SqlEntityRefMemberExpression entityRefMemberExpression, MemberInfo memberInfo)
    {
      if (memberInfo.Name == "ID")
        return TryResolveOptimizedIdentity (entityRefMemberExpression);

      return null;
    }

    private SqlColumnExpression CreateColumn (Type columnType, string tableAlias, string columnName, bool isPriamryKey)
    {
      return new SqlColumnDefinitionExpression (columnType, tableAlias, columnName, isPriamryKey);
    }

    private ResolvedSimpleTableInfo CreateResolvedTableInfo (Type entityType, UniqueIdentifierGenerator generator)
    {
      return new ResolvedSimpleTableInfo (entityType, entityType.Name + "Table", generator.GetUniqueIdentifier ("t"));
    }

     private Expression CreateJoinCondition (
        SqlEntityExpression originatingEntity,
        string leftColumnName,
        Type leftColumnType,
        bool leftColumnIsPrimaryKey, 
        string joinedTableAlias,
        string rightColumnName,
        Type rightColumnType,
        bool rightColumnIsPrimaryKey)
    {
      var leftColumn = originatingEntity.GetColumn (leftColumnType, leftColumnName, leftColumnIsPrimaryKey);
      var rightColumn = CreateColumn (rightColumnType, joinedTableAlias, rightColumnName, rightColumnIsPrimaryKey);

      return ConversionUtility.MakeBinaryWithOperandConversion (ExpressionType.Equal, leftColumn, rightColumn, false, null);
    }

    private static Expression CreateMetaIDExpression (Expression valueExpression, Expression classIDColumn)
    {
      var metaIDCtor = typeof (MetaID).GetConstructor (new[] { typeof (int), typeof (string) });
      Trace.Assert (metaIDCtor != null);
      var newExpression = Expression.New (metaIDCtor, new[] { valueExpression, classIDColumn }, new[] { typeof (MetaID).GetProperty ("Value"), typeof (MetaID).GetProperty ("ClassID") });
      
      return NamedExpression.CreateNewExpressionWithNamedArguments (newExpression);
    }

  }
}