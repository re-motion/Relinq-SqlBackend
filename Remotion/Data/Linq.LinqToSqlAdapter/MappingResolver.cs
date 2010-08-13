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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Linq.Mapping;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Remotion.Data.Linq.SqlBackend.MappingResolution;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.LinqToSqlAdapter
{
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

      var metaTable = GetMetaType (joinInfo.OriginatingEntity.Type);
      var metaAssociation=GetDataMember (metaTable, joinInfo.MemberInfo).Association;
      Debug.Assert (metaAssociation != null);

      IResolvedTableInfo resolvedTable = ResolveTableInfo (new UnresolvedTableInfo (joinInfo.ItemType), generator);
      return CreateResolvedJoinInfo (joinInfo.OriginatingEntity, metaAssociation, resolvedTable);
    }

    public SqlEntityDefinitionExpression ResolveSimpleTableInfo (IResolvedTableInfo tableInfo, UniqueIdentifierGenerator generator)
    {
      ArgumentUtility.CheckNotNull ("tableInfo", tableInfo);
      ArgumentUtility.CheckNotNull ("generator", generator);

      // TODO RM-3110: Refactor when re-linq supports compound keys
      
      var identityMembers = GetMetaType (tableInfo.ItemType).IdentityMembers;
      if(identityMembers.Count!=1)
        throw new ArgumentException("The table may not contain more or less than 1 identity member!");

      var primaryKeyMember = identityMembers.Single();
      var primaryKeyColumn = CreateSqlColumnExpression (tableInfo, primaryKeyMember);

      var columnMembers = GetMetaDataMembers (tableInfo.ItemType);

      var columns = columnMembers.Select (metaDataMember => CreateSqlColumnExpression (tableInfo, metaDataMember)).ToArray();
      return new SqlEntityDefinitionExpression (tableInfo.ItemType, tableInfo.TableAlias, null, primaryKeyColumn, columns);
    }

    public Expression ResolveMemberExpression (SqlEntityExpression originatingEntity, MemberInfo memberInfo)
    {
      ArgumentUtility.CheckNotNull ("originatingEntity", originatingEntity);
      ArgumentUtility.CheckNotNull ("memberInfo", memberInfo);

      var dataTable = GetMetaType (memberInfo.DeclaringType);
      var dataMember = GetDataMember (dataTable, memberInfo);

      if (dataMember.IsAssociation)
        return new SqlEntityRefMemberExpression (originatingEntity, memberInfo);

      var memberType = ReflectionUtility.GetFieldOrPropertyType (memberInfo);
      return originatingEntity.GetColumn (memberType, dataMember.MappedName, dataMember.IsPrimaryKey);
    }
    
    public Expression ResolveMemberExpression (SqlColumnExpression sqlColumnExpression, MemberInfo memberInfo)
    {
      throw new UnmappedItemException ("Cannot resolve member " + memberInfo.Name + " appplied to column " + sqlColumnExpression.ColumnName);
    }

    public Expression ResolveConstantExpression (ConstantExpression constantExpression)
    {
      ArgumentUtility.CheckNotNull ("constantExpression", constantExpression);

      var valueType = constantExpression.Type;

      var table = _metaModel.GetTable (valueType);
      if (table != null)
      {
        var primaryKeys = table.RowType.IdentityMembers;
        if (primaryKeys.Count != 1)
          throw new NotSupportedException ("Multiple primary keys or less than 1 currently not supported");

        var primaryKey = primaryKeys.Single();

        // TODO RM-3110: Refactor when re-linq supports compound keys

        return new SqlEntityConstantExpression (valueType, constantExpression.Value, primaryKey);
      }

      return constantExpression;
    }

    public Expression ResolveTypeCheck (Expression expression, Type desiredType)
    {
      // TODO: Use this implementation. Add a test showing that a check for eg. Contact on a Customer expression always returns Expression.Constant (true).
      // TODO: Also add tests for desiredTypes whose mapping types have no inheritance code.
      // TODO: Also add tests for expressions whose mapping types have no discriminator column.
      // TODO: Add a test showing that a type check for eg. Customer on a Contact expression returns an expression that compares the discriminator 
      // TODO: member with the inheritance code value.

      //ArgumentUtility.CheckNotNull ("expression", expression);
      //ArgumentUtility.CheckNotNull ("desiredType", desiredType);

      //if (desiredType.IsAssignableFrom (expression.Type))
      //  return Expression.Constant (true);

      // TODO if called, GetMetaType and similar methods must be tested

      //var desiredDiscriminatorValue = _metaModel.GetMetaType (desiredType).InheritanceCode;
      //if (desiredDiscriminatorValue == null)
      //  throw new UnmappedItemException ("Cannot perform a type check for type ... - there is no inheritance code for this type.");

      //var discriminatorDataMember = _metaModel.GetMetaType (expression.Type).Discriminator;
      //if (discriminatorDataMember == null)
      //  throw new UnmappedItemException ("Cannot perform a type check for type ... - there is no discriminator column.");

      //return Expression.Equal (
      //    Expression.MakeMemberAccess (expression, discriminatorDataMember.Member), 
      //    Expression.Constant (desiredDiscriminatorValue));

      throw new NotImplementedException ("Type check currently not supported");
    }

    public MetaDataMember[] GetMetaDataMembers (Type entityType)
    {
      // TODO if called, GetMetaType and similar methods must be tested
      ArgumentUtility.CheckNotNull ("entityType", entityType);

      ReadOnlyCollection<MetaDataMember> dataMembers = _metaModel.GetTable (entityType).RowType.DataMembers;
      var filteredMembers = dataMembers.Where (dataMember => !dataMember.IsAssociation);

      return filteredMembers.ToArray ();

      // TODO: Change to use this implementation. Before doing so, write a unit test for GetMetaDataMembers (typeof (Contact)) and expect
      // TODO: that members of Customer and Supplier are also returned. Members must not be duplicated, ContactID must be Contact.ContactID 
      // TODO: (not Customer.ContactID or Supplier.ContactID).
      //var metaType = _metaModel.GetMetaType (entityType).InheritanceRoot;
      //return GetMetaDataMembersRecursive (metaType);
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

    private static ResolvedJoinInfo CreateResolvedJoinInfo (
        SqlEntityExpression originatingEntity, MetaAssociation metaAssociation, IResolvedTableInfo joinedTableInfo)
    {
      // TODO RM-3110: Refactor when re-linq supports compound keys

      // TODO: Throw NotSupportedException if _metaModel.GetMetaType (tableInfo.ItemType).IdentityMembers.Count != 1;
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

      return new ResolvedJoinInfo (joinedTableInfo, leftColumn, rightColumn);
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