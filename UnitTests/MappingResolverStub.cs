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
                typeof (int),
                true,
                CreateResolvedTableInfo (joinInfo.ItemType, generator),
                "SubstitutedID",
                typeof (int),
                false);
          case "Substituted":
            return CreateResolvedJoinInfo (
                joinInfo.OriginatingEntity,
                "SubstitutedID",
                typeof (int),
                false,
                CreateResolvedTableInfo (joinInfo.ItemType, generator),
                "ID",
                typeof (int),
                true);
          case "Assistants":
            return CreateResolvedJoinInfo (
                joinInfo.OriginatingEntity,
                "ID",
                typeof (int),
                true,
                CreateResolvedTableInfo (joinInfo.ItemType, generator),
                "AssistedID",
                typeof (int),
                false);
          case "Kitchen":
            return CreateResolvedJoinInfo (
                joinInfo.OriginatingEntity,
                "KitchenID",
                typeof (int),
                false,
                CreateResolvedTableInfo (joinInfo.ItemType, generator),
                "ID",
                typeof (int),
                true);
          case "Knife":
            var joinedTableInfo = CreateResolvedTableInfo (joinInfo.ItemType, generator);
            var leftKey = ResolveMemberExpression (joinInfo.OriginatingEntity, typeof (Cook).GetProperty ("KnifeID"));
            var rightKey = ResolveSimpleTableInfo (joinedTableInfo, generator).GetIdentityExpression();
            return new ResolvedJoinInfo (joinedTableInfo, Expression.Equal (leftKey, rightKey));
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
                "RestaurantID", typeof (int), false, CreateResolvedTableInfo (joinInfo.ItemType, generator), "ID", typeof (int), true);
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

    public ITableInfo ResolveJoinTableInfo (UnresolvedJoinTableInfo tableInfo, UniqueIdentifierGenerator generator)
    {
      return new UnresolvedTableInfo (tableInfo.ItemType);
    }

    public virtual SqlEntityDefinitionExpression ResolveSimpleTableInfo (
        IResolvedTableInfo tableInfo, UniqueIdentifierGenerator generator)
    {
      Type type = tableInfo.ItemType;
      if (type == typeof (Cook))
      {
        return new SqlEntityDefinitionExpression (
            tableInfo.ItemType,
            tableInfo.TableAlias, null,
            e => e.GetColumn (typeof (int), "ID", true),
            new[]
            {
                CreateColumn (typeof (int), tableInfo.TableAlias, "ID", true),
                CreateColumn (typeof (string), tableInfo.TableAlias, "FirstName", false),
                CreateColumn (typeof (string), tableInfo.TableAlias, "Name", false),
                CreateColumn (typeof (bool), tableInfo.TableAlias, "IsStarredCook", false),
                CreateColumn (typeof (bool), tableInfo.TableAlias, "IsFullTimeCook", false),
                CreateColumn (typeof (int), tableInfo.TableAlias, "SubstitutedID", false),
                CreateColumn (typeof (int), tableInfo.TableAlias, "KitchenID", false),
                CreateColumn (typeof (int), tableInfo.TableAlias, "KnifeID", false),
                CreateColumn (typeof (string), tableInfo.TableAlias, "KnifeClassID", false),
                CreateColumn (typeof (CookRating), tableInfo.TableAlias, "CookRating", false)
            });
      }
      else if (type == typeof (Kitchen))
      {
        return new SqlEntityDefinitionExpression (
            tableInfo.ItemType,
            tableInfo.TableAlias, null,
            e => e.GetColumn (typeof (int), "ID", true),
            new[]
            {
                CreateColumn (typeof (int), tableInfo.TableAlias, "ID", true),
                CreateColumn (typeof (string), tableInfo.TableAlias, "Name", false),
                CreateColumn (typeof (int), tableInfo.TableAlias, "RestaurantID", false),
                CreateColumn (typeof (DateTime?), tableInfo.TableAlias, "LastCleaningDay", false),
                CreateColumn (typeof (bool?), tableInfo.TableAlias, "PassedLastInspection", false),
                CreateColumn (typeof (int?), tableInfo.TableAlias, "LastInspectionScore", false)
            });
      }
      else if (type == typeof (Restaurant))
      {
        return new SqlEntityDefinitionExpression (
            tableInfo.ItemType,
            tableInfo.TableAlias, null,
            e => e.GetColumn (typeof (int), "ID", true),
            new[]
            {
                CreateColumn (typeof (int), tableInfo.TableAlias, "ID", true),
                CreateColumn (typeof (int), tableInfo.TableAlias, "CompanyID", false)
            });
      }
      else if (type == typeof (Chef))
      {
        return new SqlEntityDefinitionExpression (
            tableInfo.ItemType,
            tableInfo.TableAlias, null,
            e => e.GetColumn (typeof (int), "ID", true),
            new[]
            {
                CreateColumn (typeof (int), tableInfo.TableAlias, "ID", true),
                CreateColumn (typeof (string), tableInfo.TableAlias, "FirstName", false),
                CreateColumn (typeof (string), tableInfo.TableAlias, "Name", false),
                CreateColumn (typeof (bool), tableInfo.TableAlias, "IsStarredCook", false),
                CreateColumn (typeof (bool), tableInfo.TableAlias, "IsFullTimeCook", false),
                CreateColumn (typeof (int), tableInfo.TableAlias, "SubstitutedID", false),
                CreateColumn (typeof (int), tableInfo.TableAlias, "KitchenID", false),
                CreateColumn (typeof (int), tableInfo.TableAlias, "KnifeID", false),
                CreateColumn (typeof (int), tableInfo.TableAlias, "KnifeClassID", false),
                CreateColumn (typeof (CookRating), tableInfo.TableAlias, "CookRating", false),
                CreateColumn (typeof (string), tableInfo.TableAlias, "LetterOfRecommendation", false)
            });
      }
      else if (type == typeof (Company))
      {
        return new SqlEntityDefinitionExpression (
            tableInfo.ItemType,
            tableInfo.TableAlias, null,
            e => e.GetColumn (typeof (int), "ID", true),
            new[]
            {
                CreateColumn (typeof (int), tableInfo.TableAlias, "ID", true),
                CreateColumn (typeof (DateTime), tableInfo.TableAlias, "DateOfIncorporation", false)
            });
      }
      else if (type == typeof (Knife))
      {
        return new SqlEntityDefinitionExpression (
            tableInfo.ItemType,
            tableInfo.TableAlias, null,
            e => CreateMetaIDExpression (e.GetColumn (typeof (int), "ID", true), e.GetColumn (typeof (string), "ClassID", true)),
            new[]
            {
                CreateColumn (typeof (int), tableInfo.TableAlias, "ID", true),
                CreateColumn (typeof (string), tableInfo.TableAlias, "ClassID", true),
                CreateColumn (typeof (double), tableInfo.TableAlias, "Sharpness", false)
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

      // Prepare a join, then check if the foreign key column is on the left side => this is the identity. (Otherwise, return null.)

      var joinInfo =
          ResolveJoinInfo (
              new UnresolvedJoinInfo (entityRefMemberExpression.OriginatingEntity, entityRefMemberExpression.MemberInfo, JoinCardinality.One),
              new UniqueIdentifierGenerator());

      var rightKey = ((BinaryExpression) joinInfo.JoinCondition).Right;
      while (rightKey.NodeType == ExpressionType.Convert)
        rightKey = ((UnaryExpression) rightKey).Operand;

      if (((SqlColumnExpression) rightKey).IsPrimaryKey)
        return ((BinaryExpression) joinInfo.JoinCondition).Left;

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

    private ResolvedJoinInfo CreateResolvedJoinInfo (
        SqlEntityExpression originatingEntity,
        string leftColumnName,
        Type leftColumnType,
        bool leftColumnIsPrimaryKey,
        IResolvedTableInfo joinedTableInfo,
        string rightColumnName,
        Type rightColumnType,
        bool rightColumnIsPrimaryKey)
    {
      var leftColumn = originatingEntity.GetColumn (leftColumnType, leftColumnName, leftColumnIsPrimaryKey);
      var rightColumn = CreateColumn (rightColumnType, joinedTableInfo.TableAlias, rightColumnName, rightColumnIsPrimaryKey);

      return new ResolvedJoinInfo (
          joinedTableInfo, ConversionUtility.MakeBinaryWithOperandConversion (ExpressionType.Equal, leftColumn, rightColumn, false, null));
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