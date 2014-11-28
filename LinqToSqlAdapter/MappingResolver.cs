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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Remotion.Linq.SqlBackend;
using Remotion.Linq.SqlBackend.MappingResolution;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Utilities;

namespace Remotion.Linq.LinqToSqlAdapter
{
  /// <summary>
  /// Implements <see cref="IMappingResolver"/> to resolve expressions from linq2sql to relinq
  /// Implements <see cref="IReverseMappingResolver"/> to get all metaDataMembers of a type mapped by linq2sql
  /// </summary>
  public class MappingResolver : IMappingResolver, IReverseMappingResolver
  {
    private readonly MetaModel _metaModel;

    public MappingResolver (MetaModel metaModel)
    {
      _metaModel = metaModel;
    }

    public IResolvedTableInfo ResolveTableInfo (UnresolvedTableInfo tableInfo, UniqueIdentifierGenerator generator)
    {
      ArgumentUtility.CheckNotNull ("tableInfo", tableInfo);
      ArgumentUtility.CheckNotNull ("generator", generator);

      MetaTable table = GetMetaTable (tableInfo.ItemType);

      // TODO RM-3127: Refactor when re-linq supports schema names
      var tableName = table.TableName.StartsWith ("dbo.") ? table.TableName.Substring (4) : table.TableName;

      return new ResolvedSimpleTableInfo (tableInfo.ItemType, tableName, generator.GetUniqueIdentifier ("t"));
    }

    public ITableInfo ResolveJoinTableInfo (UnresolvedJoinTableInfo tableInfo, UniqueIdentifierGenerator generator)
    {
      ArgumentUtility.CheckNotNull ("tableInfo", tableInfo);
      ArgumentUtility.CheckNotNull ("generator", generator);

      return new UnresolvedTableInfo (tableInfo.ItemType);
    }

    public Expression ResolveJoinCondition (SqlEntityExpression originatingEntity, MemberInfo memberInfo, IResolvedTableInfo joinedTableInfo)
    {
      ArgumentUtility.CheckNotNull ("originatingEntity", originatingEntity);
      ArgumentUtility.CheckNotNull ("memberInfo", memberInfo);
      ArgumentUtility.CheckNotNull ("joinedTableInfo", joinedTableInfo);

      var metaType = GetMetaType (originatingEntity.Type);
      var metaAssociation = GetDataMember (metaType, memberInfo).Association;
      Assertion.DebugAssert (metaAssociation != null);

      return CreateResolvedJoinCondition (originatingEntity, metaAssociation, joinedTableInfo);
    }

    public SqlEntityDefinitionExpression ResolveSimpleTableInfo (ResolvedSimpleTableInfo tableInfo)
    {
      ArgumentUtility.CheckNotNull ("tableInfo", tableInfo);

      Type type = tableInfo.ItemType;
      var primaryKeyMembers = GetMetaType (type).IdentityMembers;

      var columnMembers = GetMetaDataMembers (tableInfo.ItemType);

      var columns = columnMembers.Select (metaDataMember => CreateSqlColumnExpression (tableInfo, metaDataMember)).ToArray();
      return new SqlEntityDefinitionExpression (
          tableInfo.ItemType,
          tableInfo.TableAlias,
          null,
          e => CreateIdentityExpression (type, primaryKeyMembers.Select (m => ResolveDataMember (e, m)).ToArray()),
          columns);
    }

    public Expression ResolveMemberExpression (SqlEntityExpression originatingEntity, MemberInfo memberInfo)
    {
      ArgumentUtility.CheckNotNull ("originatingEntity", originatingEntity);
      ArgumentUtility.CheckNotNull ("memberInfo", memberInfo);

      var dataTable = GetMetaType (memberInfo.DeclaringType);
      var dataMember = GetDataMember (dataTable, memberInfo);

      if (dataMember.IsAssociation)
        return new SqlEntityRefMemberExpression (originatingEntity, memberInfo);
      else
        return ResolveDataMember (originatingEntity, dataMember);
    }

    public Expression ResolveMemberExpression (SqlColumnExpression sqlColumnExpression, MemberInfo memberInfo)
    {
      var message = string.Format (
          "Cannot resolve members appplied to expressions representing columns. (Member: {0}, Column: {1})", 
          memberInfo.Name, 
          sqlColumnExpression);
      throw new UnmappedItemException (message);
    }

    public Expression ResolveConstantExpression (ConstantExpression constantExpression)
    {
      ArgumentUtility.CheckNotNull ("constantExpression", constantExpression);

      if (constantExpression.Value == null)
        return constantExpression;
      
      var metaType = _metaModel.GetMetaType (constantExpression.Type);
      if (metaType.Table != null)
      {
        var primaryKeyMembers = metaType.IdentityMembers;
        var primaryKeyValues = primaryKeyMembers.Select (member => Expression.Constant (member.MemberAccessor.GetBoxedValue (constantExpression.Value), member.Type)).ToArray();
        var primaryKeyExpression = CreateIdentityExpression (metaType.Type, primaryKeyValues);

        return new SqlEntityConstantExpression (constantExpression.Type, constantExpression.Value, primaryKeyExpression);
      }

      return constantExpression;
    }

    public Expression ResolveTypeCheck (Expression expression, Type desiredType)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      ArgumentUtility.CheckNotNull ("desiredType", desiredType);

      if (desiredType.IsAssignableFrom (expression.Type))
        return Expression.Constant (true);

      if (!expression.Type.IsAssignableFrom (desiredType))
        return Expression.Constant (false);

      var desiredDiscriminatorValue = GetMetaType (desiredType).InheritanceCode;
      if (desiredDiscriminatorValue == null)
        throw new UnmappedItemException ("Cannot perform a type check for type " + desiredType + " - there is no inheritance code for this type.");

      var discriminatorDataMember =  GetMetaType (expression.Type).Discriminator;
      Assertion.DebugAssert (discriminatorDataMember != null);

      // ReSharper disable PossibleNullReferenceException
      return Expression.Equal (
          Expression.MakeMemberAccess (expression, discriminatorDataMember.Member),
          Expression.Constant (desiredDiscriminatorValue));
      // ReSharper restore PossibleNullReferenceException
    }

    public Expression TryResolveOptimizedIdentity (SqlEntityRefMemberExpression entityRefMemberExpression)
    {
      ArgumentUtility.CheckNotNull ("entityRefMemberExpression", entityRefMemberExpression);

      var metaType = GetMetaType (entityRefMemberExpression.OriginatingEntity.Type);
      var metaAssociation = GetDataMember (metaType, entityRefMemberExpression.MemberInfo).Association;
      Assertion.DebugAssert (metaAssociation != null);

      if (metaAssociation.IsForeignKey)
        return ResolveMember (entityRefMemberExpression.OriginatingEntity, metaAssociation.ThisKey);

      return null;
    }

    public Expression TryResolveOptimizedMemberExpression (SqlEntityRefMemberExpression entityRefMemberExpression, MemberInfo memberInfo)
    {
      ArgumentUtility.CheckNotNull ("entityRefMemberExpression", entityRefMemberExpression);
      ArgumentUtility.CheckNotNull ("memberInfo", memberInfo);

      var metaType = GetMetaType (entityRefMemberExpression.OriginatingEntity.Type);
      var metaAssociation = GetDataMember (metaType, entityRefMemberExpression.MemberInfo).Association;
      var memberOnReferencedType = GetDataMember (metaAssociation.OtherType, memberInfo);

      if (memberOnReferencedType.IsPrimaryKey)
        return TryResolveOptimizedIdentity (entityRefMemberExpression);

      return null;
    }

    public MetaDataMember[] GetMetaDataMembers (Type entityType)
    {
      ArgumentUtility.CheckNotNull ("entityType", entityType);

      var metaType = _metaModel.GetMetaType (entityType).InheritanceRoot;
      return GetMetaDataMembersRecursive (metaType);
    }

    private static MetaDataMember GetDataMember (MetaType dataType, MemberInfo member)
    {
      MetaDataMember dataMember;
      try
      {
        dataMember = dataType.GetDataMember (member);
      }
      catch (InvalidOperationException)
      {
        throw new UnmappedItemException ("Cannot resolve member: " + member.DeclaringType.FullName + "." + member.Name + " is not a mapped member");
      }

      if (dataMember == null)
         throw new UnmappedItemException ("Cannot resolve member: " + member.DeclaringType.FullName + "." + member.Name + " is not a mapped member");
    
      return dataMember;
    }

    private MetaTable GetMetaTable (Type typeToRetrieve)
    {
      var metaTable = _metaModel.GetTable (typeToRetrieve);

      if (metaTable == null)
        throw new UnmappedItemException ("Cannot resolve table: " + typeToRetrieve + " is not a mapped table");

      return metaTable;
    }

    private MetaType GetMetaType (Type typeToRetrieve)
    {
      var metaType = _metaModel.GetMetaType (typeToRetrieve);

      if ( metaType.Table == null)
        throw new UnmappedItemException ("Cannot resolve type: " + typeToRetrieve + " is not a mapped type");

      return metaType;
    }

    private MetaDataMember[] GetMetaDataMembersRecursive (MetaType metaType)
    {
      var members = new HashSet<MetaDataMember> (new MetaDataMemberComparer ());
      foreach (var metaDataMember in metaType.PersistentDataMembers.Where (m => !m.IsAssociation))
        members.Add (metaDataMember);

      var derivedMembers = from derivedType in metaType.DerivedTypes
                           from derivedMember in GetMetaDataMembersRecursive (derivedType)
                           select derivedMember;
      foreach (var metaDataMember in derivedMembers)
        members.Add (metaDataMember);

      return members.ToArray();
    }

    private static Expression CreateResolvedJoinCondition (
        SqlEntityExpression originatingEntity,
        MetaAssociation metaAssociation,
        IResolvedTableInfo joinedTableInfo)
    {
      var leftColumn = ResolveMember (originatingEntity, metaAssociation.ThisKey);

      // If needed, implement by using compounds (NewExpressions with named arguments, see NamedExpression.CreateNewExpressionWithNamedArguments.)
      if (metaAssociation.OtherKey.Count > 1)
      {
        throw new NotSupportedException (
            string.Format (
                "Associations with more than one column are currently not supported. ({0}.{1})",
                originatingEntity.Type,
                metaAssociation.OtherMember.Name));
      }

      var otherKey = metaAssociation.OtherKey[0];
      var rightColumn = new SqlColumnDefinitionExpression (
        otherKey.Type,
        joinedTableInfo.TableAlias,
        otherKey.MappedName,
        otherKey.IsPrimaryKey);

      return ConversionUtility.MakeBinaryWithOperandConversion (ExpressionType.Equal, leftColumn, rightColumn, false, null);
    }

    private static SqlColumnExpression ResolveMember (SqlEntityExpression entity, ReadOnlyCollection<MetaDataMember> metaDataMembers)
    {
      // If needed, implement by using compounds (NewExpressions with named arguments, see NamedExpression.CreateNewExpressionWithNamedArguments.)
      if (metaDataMembers.Count > 1)
      {
        throw new NotSupportedException (
            string.Format (
                "Members mapped to more than one column are currently not supported. ({0}.{1})",
                entity.Type));
      }

      var thisKey = metaDataMembers[0];
      return ResolveDataMember (entity, thisKey);
    }

    private static SqlColumnExpression CreateSqlColumnExpression (IResolvedTableInfo tableInfo, MetaDataMember metaDataMember)
    {
      return new SqlColumnDefinitionExpression (
          metaDataMember.Type,
          tableInfo.TableAlias,
          metaDataMember.MappedName,
          metaDataMember.IsPrimaryKey);
    }

    private static SqlColumnExpression ResolveDataMember (SqlEntityExpression originatingEntity, MetaDataMember dataMember)
    {
      return originatingEntity.GetColumn (dataMember.Type, dataMember.MappedName, dataMember.IsPrimaryKey);
    }

    private Expression CreateIdentityExpression (Type entityType, Expression[] primaryKeyValues)
    {
      Type genericTupleType;
      switch (primaryKeyValues.Length)
      {
        case 0:
          throw new NotSupportedException (string.Format ("Entities without identity members are not supported by re-linq. ({0})", entityType));
        case 1:
          return primaryKeyValues.Single ();
        case 2:
          genericTupleType = typeof (CompoundIdentityTuple<,>);
          break;
        default:
          throw new NotSupportedException (string.Format ("Primary keys with more than 2 members are not supported. ({0})", entityType));
      }

      var ctor = genericTupleType.MakeGenericType (primaryKeyValues.Select (e => e.Type).ToArray ()).GetConstructors ().Single ();
      Assertion.DebugAssert (ctor != null);
      var tupleConstructionExpression = Expression.New (
          ctor, 
          primaryKeyValues, 
          ctor.GetParameters().Select ((pi, i) => (MemberInfo) ctor.DeclaringType.GetProperty ("Item" + (i + 1))));
      return NamedExpression.CreateNewExpressionWithNamedArguments (tupleConstructionExpression);
    }
    
    public class CompoundIdentityTuple<T1, T2>
    {
      private readonly T1 _item1;
      private readonly T2 _item2;

      public CompoundIdentityTuple (T1 item1, T2 item2)
      {
        _item1 = item1;
        _item2 = item2;
      }

      public T1 Item1
      {
        get { return _item1; }
      }

      public T2 Item2
      {
        get { return _item2; }
      }
    }

  }
}