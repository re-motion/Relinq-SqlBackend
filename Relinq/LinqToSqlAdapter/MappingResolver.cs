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
using System.Data.Linq.Mapping;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Remotion.Linq.SqlBackend.MappingResolution;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Linq.Utilities;

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

    public ResolvedJoinInfo ResolveJoinInfo (UnresolvedJoinInfo joinInfo, UniqueIdentifierGenerator generator)
    {
      ArgumentUtility.CheckNotNull ("joinInfo", joinInfo);
      ArgumentUtility.CheckNotNull ("generator", generator);

      var metaType = GetMetaType (joinInfo.OriginatingEntity.Type);
      var metaAssociation = GetDataMember (metaType, joinInfo.MemberInfo).Association;
      Debug.Assert (metaAssociation != null);

      IResolvedTableInfo resolvedTable = ResolveTableInfo (new UnresolvedTableInfo (joinInfo.ItemType), generator);
      return CreateResolvedJoinInfo (joinInfo.OriginatingEntity, metaAssociation, resolvedTable);
    }

    public SqlEntityDefinitionExpression ResolveSimpleTableInfo (IResolvedTableInfo tableInfo, UniqueIdentifierGenerator generator)
    {
      ArgumentUtility.CheckNotNull ("tableInfo", tableInfo);
      ArgumentUtility.CheckNotNull ("generator", generator);

      Type type = tableInfo.ItemType;
      var primaryKeyMember = GetPrimaryKeyMember(GetMetaType (type));

      var columnMembers = GetMetaDataMembers (tableInfo.ItemType);

      var columns = columnMembers.Select (metaDataMember => CreateSqlColumnExpression (tableInfo, metaDataMember)).ToArray();
      return new SqlEntityDefinitionExpression (
          tableInfo.ItemType,
          tableInfo.TableAlias,
          null,
          e => e.GetColumn (primaryKeyMember.Type, primaryKeyMember.MappedName, primaryKeyMember.IsPrimaryKey),
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

      var memberType = ReflectionUtility.GetMemberReturnType (memberInfo);
      return originatingEntity.GetColumn (memberType, dataMember.MappedName, dataMember.IsPrimaryKey);
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
        var primaryKey = GetPrimaryKeyMember (metaType);
        var primaryKeyValue = primaryKey.MemberAccessor.GetBoxedValue (constantExpression.Value);
        return new SqlEntityConstantExpression (constantExpression.Type, constantExpression.Value, Expression.Constant (primaryKeyValue, primaryKey.Type));
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
      Debug.Assert (discriminatorDataMember != null);

      // ReSharper disable PossibleNullReferenceException
      return Expression.Equal (
          Expression.MakeMemberAccess (expression, discriminatorDataMember.Member),
          Expression.Constant (desiredDiscriminatorValue));
      // ReSharper restore PossibleNullReferenceException
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

    private MetaDataMember GetPrimaryKeyMember (MetaType metaType)
    {
      var identityMembers = metaType.IdentityMembers;

      // TODO RM-3110: Refactor when re-linq supports compound keys
      if (identityMembers.Count > 1)
        throw new NotSupportedException ("Entities with more than one identity member are currently not supported by re-linq. (" + metaType.Name + ")");

      if (identityMembers.Count == 0)
        throw new NotSupportedException ("Entities without identity members are not supported by re-linq. (" + metaType.Name + ")");

      return identityMembers.Single ();
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

    private static ResolvedJoinInfo CreateResolvedJoinInfo (
        SqlEntityExpression originatingEntity, MetaAssociation metaAssociation, IResolvedTableInfo joinedTableInfo)
    {
      // TODO RM-3110: Refactor when re-linq supports compound keys

      Debug.Assert (metaAssociation.ThisKey.Count == 1);
      Debug.Assert (metaAssociation.OtherKey.Count == 1);

      var thisKey = metaAssociation.ThisKey[0];
      var otherKey = metaAssociation.OtherKey[0];

      var leftColumn = originatingEntity.GetColumn (thisKey.Type, thisKey.MappedName, thisKey.IsPrimaryKey);
      var rightColumn = new SqlColumnDefinitionExpression (
        otherKey.Type,
        joinedTableInfo.TableAlias,
        otherKey.MappedName,
        otherKey.IsPrimaryKey);

      return new ResolvedJoinInfo (joinedTableInfo, Expression.Equal (leftColumn, rightColumn));
    }

    private static SqlColumnExpression CreateSqlColumnExpression (IResolvedTableInfo tableInfo, MetaDataMember metaDataMember)
    {
      return new SqlColumnDefinitionExpression (
          metaDataMember.Type,
          tableInfo.TableAlias,
          metaDataMember.MappedName,
          metaDataMember.IsPrimaryKey);
    }
  }
}