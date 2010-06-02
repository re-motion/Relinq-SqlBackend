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
using System.Linq.Expressions;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.SqlBackend.MappingResolution
{
  public class SqlTableReferenceResolver : ITableInfoVisitor
  {
    private Expression _result;
    private readonly SqlTableBase _sqlTable;
    private readonly IMappingResolver _resolver;
    private readonly UniqueIdentifierGenerator _generator;
    private readonly IMappingResolutionContext _context;

    public static Expression ResolveTableReference (
        SqlTableReferenceExpression expression, IMappingResolver resolver, UniqueIdentifierGenerator generator, IMappingResolutionContext context)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      ArgumentUtility.CheckNotNull ("resolver", resolver);
      ArgumentUtility.CheckNotNull ("generator", generator);
      ArgumentUtility.CheckNotNull ("context", context);

      var visitor = new SqlTableReferenceResolver (expression, resolver, generator, context);
      return visitor.ResolveSqlTableReferenceExpression (expression);
    }

    protected SqlTableReferenceResolver (
        SqlTableReferenceExpression expression, IMappingResolver resolver, UniqueIdentifierGenerator generator, IMappingResolutionContext context)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      ArgumentUtility.CheckNotNull ("resolver", resolver);
      ArgumentUtility.CheckNotNull ("generator", generator);
      ArgumentUtility.CheckNotNull ("context", context);

      _sqlTable = expression.SqlTable;
      _generator = generator;
      _resolver = resolver;
      _context = context;
    }

    public Expression ResolveSqlTableReferenceExpression (SqlTableReferenceExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      expression.SqlTable.GetResolvedTableInfo().Accept (this);

      return _result;
    }

    public ITableInfo VisitSimpleTableInfo (ResolvedSimpleTableInfo tableInfo)
    {
      var entity = (SqlEntityExpression) _resolver.ResolveSimpleTableInfo (_sqlTable.GetResolvedTableInfo(), _generator);
      _context.AddSqlEntityMapping (entity, _sqlTable);
      _result = entity;
      return tableInfo;
    }

    public ITableInfo VisitSubStatementTableInfo (ResolvedSubStatementTableInfo subStatementTableInfo)
    {
      ArgumentUtility.CheckNotNull ("subStatementTableInfo", subStatementTableInfo);

      var selectProjection = subStatementTableInfo.SqlStatement.SelectProjection;

      _result = SubStatementReferenceResolver.ResolveSubStatementReferenceExpression (
          selectProjection, subStatementTableInfo, _sqlTable, selectProjection.Type, _context);
      
      return subStatementTableInfo;
    }

    ITableInfo ITableInfoVisitor.VisitUnresolvedTableInfo (UnresolvedTableInfo tableInfo)
    {
      //method should never be called because 'expression.SqlTable.GetResolvedTableInfo' throws an exception before 

      throw new InvalidOperationException ("UnresolvedTableInfo is not valid at this point.");
    }

    ITableInfo ITableInfoVisitor.VisitSqlJoinedTable (SqlJoinedTable joinedTable)
    {
      //method should never be called because 'expression.SqlTable.GetResolvedTableInfo' throws an exception before 

      throw new InvalidOperationException ("SqlJoinedTable is not valid at this point.");
    }
  }
}