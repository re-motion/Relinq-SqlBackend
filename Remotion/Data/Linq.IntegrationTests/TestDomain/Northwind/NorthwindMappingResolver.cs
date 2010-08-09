using System;
using System.Collections.Generic;
using System.Data.Linq.Mapping;
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
      MetaTable table = _metaModel.GetTable (tableInfo.ItemType);

      return new ResolvedSimpleTableInfo (tableInfo.ItemType, table.TableName, generator.GetUniqueIdentifier("t"));
    }

    public ResolvedJoinInfo ResolveJoinInfo (UnresolvedJoinInfo joinInfo, UniqueIdentifierGenerator generator)
    {
      throw new NotImplementedException();
    }

    //TODO: Better Solution For IResolveTableInfo? (Current: Cast to SimpleTableInfo) 
    public SqlEntityDefinitionExpression ResolveSimpleTableInfo (IResolvedTableInfo tableInfo, UniqueIdentifierGenerator generator)
    {
      ArgumentUtility.CheckNotNull ("simpleTableInfo", tableInfo);
      ResolvedSimpleTableInfo simpleTableInfo = (ResolvedSimpleTableInfo) tableInfo;
      

      SqlColumnExpression primaryColumn=null;
      List<SqlColumnExpression> otherColumns=new List<SqlColumnExpression>();

      MetaTable table = _metaModel.GetTable (tableInfo.ItemType);
      foreach (var metaDataMember in table.RowType.DataMembers)
      {
        SqlColumnExpression sqlColumnExpression = new SqlColumnDefinitionExpression (
            metaDataMember.Type, simpleTableInfo.TableAlias, metaDataMember.MappedName, metaDataMember.IsPrimaryKey);

        if (metaDataMember.IsPrimaryKey)
          primaryColumn = sqlColumnExpression;
        else
          otherColumns.Add (sqlColumnExpression);
      }

      return new SqlEntityDefinitionExpression (
          simpleTableInfo.ItemType, simpleTableInfo.TableAlias, simpleTableInfo.TableName, primaryColumn, otherColumns.ToArray());
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
  }
}
