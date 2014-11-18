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
using System.Linq.Expressions;
using Remotion.Linq.SqlBackend.MappingResolution;
using Remotion.Utilities;

namespace Remotion.Linq.SqlBackend.SqlStatementModel.Resolved
{
  /// <summary>
  /// <see cref="ResolvedSimpleTableInfo"/> represents the data source defined by a table in a relational database.
  /// </summary>
  public class ResolvedSimpleTableInfo : IResolvedTableInfo
  {
    private readonly Type _itemType;
    private readonly string _tableName;
    private readonly string _tableAlias;

    public ResolvedSimpleTableInfo (Type itemType, string tableName, string tableAlias)
    {
      ArgumentUtility.CheckNotNull ("itemType", itemType);
      ArgumentUtility.CheckNotNullOrEmpty ("tableName", tableName);
      ArgumentUtility.CheckNotNullOrEmpty ("tableAlias", tableAlias);

      _itemType = itemType;
      _tableName = tableName;
      _tableAlias = tableAlias;
    }

    public string TableName
    {
      get { return _tableName; }
    }

    public string TableAlias
    {
      get { return _tableAlias; }
    }

    public Expression ResolveReference (SqlTableBase sqlTable, IMappingResolver mappingResolver, IMappingResolutionContext context, UniqueIdentifierGenerator generator)
    {
      ArgumentUtility.CheckNotNull ("sqlTable", sqlTable);
      ArgumentUtility.CheckNotNull ("mappingResolver", mappingResolver);
      ArgumentUtility.CheckNotNull ("context", context);
      ArgumentUtility.CheckNotNull ("generator", generator);

      var entity = (SqlEntityExpression) mappingResolver.ResolveSimpleTableInfo (this);
      context.AddSqlEntityMapping (entity, sqlTable);
      return entity;
    }

    public virtual Type ItemType
    {
      get { return _itemType; }
    }

    public virtual ITableInfo Accept (ITableInfoVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);
      return visitor.VisitSimpleTableInfo (this);
    }

    public virtual IResolvedTableInfo GetResolvedTableInfo ()
    {
      return this;
    }

    public override string ToString ()
    {
      return string.Format ("[{0}] [{1}]", TableName, TableAlias);
    }
  }
}