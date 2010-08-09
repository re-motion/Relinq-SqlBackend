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

namespace Remotion.Data.Linq.IntegrationTests.TestDomain.Northwind
{
  public class NorthwindMappingResolver:IMappingResolver
  {
    private readonly MetaModel _metaModel;
    

    public NorthwindMappingResolver()
    {
        _metaModel = new AttributeMappingSource ().GetModel (typeof (Northwind));
    }

    public IResolvedTableInfo ResolveTableInfo (UnresolvedTableInfo tableInfo, UniqueIdentifierGenerator generator)
    {
      ArgumentUtility.CheckNotNull ("tableInfo", tableInfo);
      ArgumentUtility.CheckNotNull ("generator", generator);

      MetaTable table = _metaModel.GetTable (tableInfo.ItemType);

      return new ResolvedSimpleTableInfo (tableInfo.ItemType, table.TableName, generator.GetUniqueIdentifier("t"));
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

      SqlColumnExpression primaryColumn=null;
      List<SqlColumnExpression> otherColumns=new List<SqlColumnExpression>();

      MetaTable table = _metaModel.GetTable (tableInfo.ItemType);
      foreach (var metaDataMember in table.RowType.DataMembers)
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
      throw new NotImplementedException();
    }

    public Expression ResolveMemberExpression (SqlColumnExpression sqlColumnExpression, MemberInfo memberInfo)
    {
      throw new NotImplementedException();
    }

    public Expression ResolveConstantExpression (ConstantExpression constantExpression)
    {
      throw new NotImplementedException();
    }

    public Expression ResolveTypeCheck (Expression expression, Type desiredType)
    {
      throw new NotImplementedException();
    }

    #region privateMethods
    private ResolvedJoinInfo CreateResolvedJoinInfo (
        SqlEntityExpression originatingEntity, MetaAssociation metaAssociation, IResolvedTableInfo joinedTableInfo)
    {
      Debug.Assert (metaAssociation.ThisKey.Count==1);
      Debug.Assert (metaAssociation.OtherKey.Count==1);

      var thisKey = metaAssociation.ThisKey[0];
      var otherKey = metaAssociation.OtherKey[0];

      var leftColumn = originatingEntity.GetColumn (thisKey.Type, thisKey.MappedName, thisKey.IsPrimaryKey);
      var rightColumn = new SqlColumnDefinitionExpression (otherKey.Type, joinedTableInfo.TableAlias, otherKey.MappedName, otherKey.IsPrimaryKey);

      return new ResolvedJoinInfo (joinedTableInfo, leftColumn, rightColumn);
    }

    #endregion
  }
}
