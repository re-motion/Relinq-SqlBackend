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

      MetaTable table = _metaModel.GetTable (tableInfo.ItemType);

      // TODO RM-3127: Refactor when re-linq supports schema names
      var tableName = table.TableName.StartsWith ("dbo.") ? table.TableName.Substring (4) : table.TableName;

      return new ResolvedSimpleTableInfo (tableInfo.ItemType, tableName, generator.GetUniqueIdentifier ("t"));
    }

    public ResolvedJoinInfo ResolveJoinInfo (UnresolvedJoinInfo joinInfo, UniqueIdentifierGenerator generator)
    {
      ArgumentUtility.CheckNotNull ("joinInfo", joinInfo);
      ArgumentUtility.CheckNotNull ("generator", generator);

      var metaAssociation = _metaModel.GetMetaType (joinInfo.OriginatingEntity.Type).GetDataMember (joinInfo.MemberInfo).Association;
      Debug.Assert (metaAssociation != null);

      IResolvedTableInfo resolvedTable = ResolveTableInfo (new UnresolvedTableInfo (joinInfo.ItemType), generator);
      return CreateResolvedJoinInfo (joinInfo.OriginatingEntity, metaAssociation, resolvedTable);
    }

    public SqlEntityDefinitionExpression ResolveSimpleTableInfo (IResolvedTableInfo tableInfo, UniqueIdentifierGenerator generator)
    {
      ArgumentUtility.CheckNotNull ("tableInfo", tableInfo);
      ArgumentUtility.CheckNotNull ("generator", generator);

      // TODO RM-3110: Refactor when re-linq supports compound keys
      
      // TODO: Throw if _metaModel.GetMetaType (tableInfo.ItemType).IdentityMembers.Count != 1
      var primaryKeyMember = _metaModel.GetMetaType (tableInfo.ItemType).IdentityMembers.Single();
      var primaryKeyColumn = CreateSqlColumnExpression (tableInfo, primaryKeyMember);

      var columnMembers = GetMetaDataMembers (tableInfo.ItemType);

      var columns = columnMembers.Select (metaDataMember => CreateSqlColumnExpression (tableInfo, metaDataMember)).ToArray();
      return new SqlEntityDefinitionExpression (tableInfo.ItemType, tableInfo.TableAlias, null, primaryKeyColumn, columns);
    }

    public Expression ResolveMemberExpression (SqlEntityExpression originatingEntity, MemberInfo memberInfo)
    {
      ArgumentUtility.CheckNotNull ("originatingEntity", originatingEntity);
      ArgumentUtility.CheckNotNull ("memberInfo", memberInfo);

      var dataTable = _metaModel.GetMetaType (memberInfo.DeclaringType);

      // TODO: Change exception message to include reason (Type ... is not a mapped type)
      // TODO: Move this check to a method (GetMetaType) and use wherever a meta type is retrieved for a type
      if (dataTable == null)
        throw new UnmappedItemException ("Cannot resolve member: " + memberInfo);

      // TODO: Change exception message to include reason (Member ... is not a mapped member)
      // TODO: Move this check to a method (GetDataMember) and use wherever a data member is retrieved for a MemberInfo
      var dataMember = dataTable.GetDataMember (memberInfo);

      if (dataMember == null)
        throw new UnmappedItemException ("Cannot resolve member: " + memberInfo);

      if (dataMember.IsAssociation)
      {
        return new SqlEntityRefMemberExpression (originatingEntity, memberInfo);
      }
      else
      {
        var memberType = ReflectionUtility.GetFieldOrPropertyType (memberInfo);
        return originatingEntity.GetColumn (memberType, dataMember.MappedName, dataMember.IsPrimaryKey);
      }
    }
    
    public Expression ResolveMemberExpression (SqlColumnExpression sqlColumnExpression, MemberInfo memberInfo)
    {
      //TODO: Change to UnmappedItemException, changed message to: Cannot resolve member ... applied to column ...

      throw new NotImplementedException ("Implement if needed by integration tests");
    }

    public Expression ResolveConstantExpression (ConstantExpression constantExpression)
    {
      ArgumentUtility.CheckNotNull ("constantExpression", constantExpression);

      // TODO: Value can be null. Use constantExpression.Type instead.
      var valueType = constantExpression.Value.GetType ();
      
      var table = _metaModel.GetTable (valueType);
      if (table != null)
      {
        var dataMembers = table.RowType.DataMembers;
        var primaryKeys = dataMembers.Where (member => member.IsPrimaryKey).ToList(); // TODO: Use IdentityMembers instead?

        // TODO RM-3110: Refactor when re-linq supports compound keys

        // TODO: Throw NotSupportedException if primarykeys.Count != 1;
        if (primaryKeys.Count > 1)
          throw new NotImplementedException ("Multiple primary keys currently not supported");

        return new SqlEntityConstantExpression (valueType, constantExpression.Value, primaryKeys[0]);
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

    private ResolvedJoinInfo CreateResolvedJoinInfo (
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

    private SqlColumnExpression CreateSqlColumnExpression (IResolvedTableInfo tableInfo, MetaDataMember metaDataMember)
    {
      return new SqlColumnDefinitionExpression (
          metaDataMember.Type,
          tableInfo.TableAlias,
          metaDataMember.MappedName,
          metaDataMember.IsPrimaryKey);
    }
  }
}