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
using Remotion.Data.Linq.IntegrationTests.Utilities;
using Remotion.Data.Linq.SqlBackend.MappingResolution;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.IntegrationTests.TestDomain.Northwind
{
  public class NorthwindMappingResolver : IMappingResolver, IReverseMappingResolver
  {
    private readonly MetaModel _metaModel;


    public NorthwindMappingResolver ()
    {
      _metaModel = new AttributeMappingSource ().GetModel (typeof (Northwind));
    }

    public IResolvedTableInfo ResolveTableInfo (UnresolvedTableInfo tableInfo, UniqueIdentifierGenerator generator)
    {
      ArgumentUtility.CheckNotNull ("tableInfo", tableInfo);
      ArgumentUtility.CheckNotNull ("generator", generator);

      MetaTable table = _metaModel.GetTable (tableInfo.ItemType);

      return new ResolvedSimpleTableInfo (tableInfo.ItemType, table.TableName, generator.GetUniqueIdentifier ("t"));
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
        else
          otherColumns.Add (sqlColumnExpression);
      }

      return new SqlEntityDefinitionExpression (
          tableInfo.ItemType, tableInfo.TableAlias, null, primaryColumn, otherColumns.ToArray ());
    }

    public Expression ResolveMemberExpression (SqlEntityExpression originatingEntity, MemberInfo memberInfo)
    {
      //var memberType = ReflectionUtility.GetFieldOrPropertyType (memberInfo);
      //if (memberInfo.DeclaringType == typeof (Cook))
      //{
      //  switch (memberInfo.Name)
      //  {
      //    case "ID":
      //      return originatingEntity.GetColumn (memberType, memberInfo.Name, true);
      //    case "FirstName":
      //    case "Name":
      //    case "IsFullTimeCook":
      //    case "IsStarredCook":
      //    case "Weight":
      //    case "MetaID":
      //      return originatingEntity.GetColumn (memberType, memberInfo.Name, false);
      //    case "Substitution":
      //      return new SqlEntityRefMemberExpression (originatingEntity, memberInfo);

      //  }
      var memberType = ReflectionUtility.GetFieldOrPropertyType (memberInfo);

      MetaTable table = _metaModel.GetTable (memberInfo.DeclaringType);

      if (originatingEntity is SqlEntityReferenceExpression)
      {
        return new SqlEntityRefMemberExpression (originatingEntity, memberInfo);
      }
      else
      {
        foreach (var dataMember in table.RowType.DataMembers)
        {
          return originatingEntity.GetColumn (memberType, memberInfo.Name, dataMember.IsPrimaryKey);
        }
      }

      /*
      foreach (var dataMember in table.RowType.DataMembers)
      {
        if (dataMember.MappedName.Equals (memberInfo.Name))
        {
          if (dataMember.IsAssociation) //ref
          {
            return new SqlEntityRefMemberExpression (originatingEntity, memberInfo);
          }
          else
          {
            return originatingEntity.GetColumn (memberType, memberInfo.Name, dataMember.IsPrimaryKey);
          }
        }
       
       }
       * */
  
      throw new UnmappedItemException ("Cannot resolve member: " + memberInfo);

      //tableCol.GetType().GetProperty(memberInfo.)
      //return new SqlColumnDefinitionExpression (typeof (Person).GetProperty ("First").GetType (), "p", "First", true);
    }

    public Expression ResolveMemberExpression (SqlColumnExpression sqlColumnExpression, MemberInfo memberInfo)
    {
      throw new NotImplementedException ();
    }

    public Expression ResolveConstantExpression (ConstantExpression constantExpression)
    {
      throw new NotImplementedException ();
    }

    public Expression ResolveTypeCheck (Expression expression, Type desiredType)
    {
      throw new NotImplementedException ();
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
      var rightColumn = new SqlColumnDefinitionExpression (otherKey.Type, joinedTableInfo.TableAlias, otherKey.MappedName, otherKey.IsPrimaryKey);

      return new ResolvedJoinInfo (joinedTableInfo, leftColumn, rightColumn);
    }

    #endregion
  }
}