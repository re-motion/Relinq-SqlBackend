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
using System.Data.Linq.Mapping;
using System.Diagnostics;
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

      // TODO: check for db-schema
      var tableName = table.TableName.StartsWith ("dbo.") ? table.TableName.Substring (4) : table.TableName;

      return new ResolvedSimpleTableInfo (tableInfo.ItemType, tableName, generator.GetUniqueIdentifier ("t"));
    }

    public ResolvedJoinInfo ResolveJoinInfo (UnresolvedJoinInfo joinInfo, UniqueIdentifierGenerator generator)
    {
      ArgumentUtility.CheckNotNull ("joinInfo", joinInfo);
      ArgumentUtility.CheckNotNull ("generator", generator);

      IResolvedTableInfo resolvedTable = ResolveTableInfo (new UnresolvedTableInfo (joinInfo.ItemType), generator);

      var metaAssociation = _metaModel.GetTable (joinInfo.OriginatingEntity.Type).RowType.GetDataMember (joinInfo.MemberInfo).Association;
      ArgumentUtility.CheckNotNull ("metaAssociation", metaAssociation);

      return CreateResolvedJoinInfo (
          joinInfo.OriginatingEntity,
          metaAssociation,
          resolvedTable
          );
    }

    public SqlEntityDefinitionExpression ResolveSimpleTableInfo (IResolvedTableInfo tableInfo, UniqueIdentifierGenerator generator)
    {
      ArgumentUtility.CheckNotNull ("tableInfo", tableInfo);
      ArgumentUtility.CheckNotNull ("generator", generator);

      SqlColumnExpression primaryColumn = null;
      List<SqlColumnExpression> otherColumns = new List<SqlColumnExpression> ();

      MetaDataMember[] sortedMembers = GetMetaDataMembers (tableInfo.ItemType);

      foreach (var metaDataMember in sortedMembers)
      {
        SqlColumnExpression sqlColumnExpression = new SqlColumnDefinitionExpression (
            metaDataMember.Type, tableInfo.TableAlias, metaDataMember.MappedName, metaDataMember.IsPrimaryKey);

        if (metaDataMember.IsPrimaryKey)
          primaryColumn = sqlColumnExpression;

        otherColumns.Add (sqlColumnExpression);
      }

      return new SqlEntityDefinitionExpression (
          tableInfo.ItemType, tableInfo.TableAlias, null, primaryColumn, otherColumns.ToArray ());
    }

    public Expression ResolveMemberExpression (SqlEntityExpression originatingEntity, MemberInfo memberInfo)
    {
      ArgumentUtility.CheckNotNull ("originatingEntity", originatingEntity);
      ArgumentUtility.CheckNotNull ("memberInfo", memberInfo);

      var memberType = ReflectionUtility.GetFieldOrPropertyType (memberInfo);

      var dataTable = _metaModel.GetMetaType (memberInfo.DeclaringType);

      if (dataTable == null)
        throw new UnmappedItemException ("Cannot resolve member: " + memberInfo);

      var dataMember = dataTable.GetDataMember (memberInfo);

      if (dataMember == null)
        throw new UnmappedItemException ("Cannot resolve member: " + memberInfo);

      if (dataMember.IsAssociation)
      {
        return new SqlEntityRefMemberExpression (originatingEntity, memberInfo);
      }
      else
      {
        return originatingEntity.GetColumn (memberType, dataMember.MappedName, dataMember.IsPrimaryKey);
      }
    }


    public Expression ResolveMemberExpression (SqlColumnExpression sqlColumnExpression, MemberInfo memberInfo)
    {
      //TODO implement if needed by integration tests
      throw new NotImplementedException ("Implement if needed by integration tests");

      //ArgumentUtility.CheckNotNull ("sqlColumnExpression", sqlColumnExpression);
      //ArgumentUtility.CheckNotNull ("memberInfo", memberInfo);

      //if (sqlColumnExpression is SqlColumnReferenceExpression)
      //{
      //  return new SqlColumnReferenceExpression (
      //    ReflectionUtility.GetFieldOrPropertyType(memberInfo),
      //    sqlColumnExpression.OwningTableAlias,
      //    memberInfo.Name,
      //    sqlColumnExpression.IsPrimaryKey,
      //    ((SqlColumnReferenceExpression) sqlColumnExpression).ReferencedEntity);
      //}
      //return new SqlColumnDefinitionExpression (
      //  ReflectionUtility.GetFieldOrPropertyType (memberInfo),
      //  sqlColumnExpression.OwningTableAlias, 
      //  memberInfo.Name, 
      //  sqlColumnExpression.IsPrimaryKey);
    }

    public Expression ResolveConstantExpression (ConstantExpression constantExpression)
    {
      ArgumentUtility.CheckNotNull ("constantExpression", constantExpression);

      var valueType = constantExpression.Value.GetType ();
      var table = _metaModel.GetTable (valueType);
      if (table != null)
      {
        var dataMembers = table.RowType.DataMembers;
        var primaryKeys = new List<MetaDataMember> ();

        foreach (var member in dataMembers)
        {
          if (member.IsPrimaryKey)
            primaryKeys.Add (member);
        }

        if (primaryKeys.Count > 1)
          throw new NotImplementedException ("Multiple primary keys currently not supported");

        return new SqlEntityConstantExpression (valueType, constantExpression.Value, primaryKeys[0]);
      }
      return constantExpression;
    }

    public Expression ResolveTypeCheck (Expression expression, Type desiredType)
    {
      //TODO check if column supports more than one type and if type is one of those types

      throw new NotImplementedException (("Type check currently not supported"));

      //ArgumentUtility.CheckNotNull ("expression", expression);
      //ArgumentUtility.CheckNotNull ("desiredType", desiredType);

      //if (desiredType.IsAssignableFrom (expression.Type))
      //  return Expression.Constant (true);

      //throw new UnmappedItemException ("Cannot resolve type for checkedExpression: " + expression.Type.Name);
    }

    public MetaDataMember[] GetMetaDataMembers (Type entityType)
    {
      ArgumentUtility.CheckNotNull ("entityType", entityType);
      return MemberSortUtility.SortDataMembers (_metaModel.GetTable (entityType).RowType.DataMembers);
    }

    #region privateMethods

    private ResolvedJoinInfo CreateResolvedJoinInfo (
        SqlEntityExpression originatingEntity, MetaAssociation metaAssociation, IResolvedTableInfo joinedTableInfo)
    {
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

    #endregion
  }
}