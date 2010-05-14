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
using Remotion.Data.Linq.SqlBackend.SqlPreparation.MethodCallTransformers;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.SqlBackend.MappingResolution
{
  public class SqlTableReferenceResolver : ITableInfoVisitor
  {
    private Expression _result;
    private readonly SqlTableReferenceExpression _expression;
    private readonly IMappingResolver _resolver;
    private readonly UniqueIdentifierGenerator _generator;

    public static Expression ResolveTableReference (
        SqlTableReferenceExpression expression, IMappingResolver resolver, UniqueIdentifierGenerator generator)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      ArgumentUtility.CheckNotNull ("resolver", resolver);
      ArgumentUtility.CheckNotNull ("generator", generator);

      var visitor = new SqlTableReferenceResolver (expression, resolver, generator);
      return visitor.ResolveSqlTableReferenceExpression (expression);
    }

    protected SqlTableReferenceResolver (
        SqlTableReferenceExpression expression, IMappingResolver resolver, UniqueIdentifierGenerator generator)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      ArgumentUtility.CheckNotNull ("resolver", resolver);
      ArgumentUtility.CheckNotNull ("generator", generator);

      // TODO 2719: don't store the expression, only store the SqlTable
      // TODO 2719: As soon as SqlEntity is decoupled from SqlTable, don't store the SqlTable(Expression) any longer
      _expression = expression;
      _generator = generator;
      _resolver = resolver;
    }

    public Expression ResolveSqlTableReferenceExpression (SqlTableReferenceExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      expression.SqlTable.GetResolvedTableInfo ().Accept (this);

      return _result;
    }

    public ITableInfo VisitSimpleTableInfo (ResolvedSimpleTableInfo tableInfo)
    {
      // TODO 2719: Refactor ResolveTableReferenceExpression to take SimpleTableInfo instead of a SqlTableReferenceExpression, rename it to ResolveTableReference, don't forget to update the docs on IMappingResolver
      _result = _resolver.ResolveTableReferenceExpression(_expression, _generator);
      return tableInfo;
    }

    public ITableInfo VisitSubStatementTableInfo (ResolvedSubStatementTableInfo subStatementTableInfo)
    {
      ArgumentUtility.CheckNotNull ("subStatementTableInfo", subStatementTableInfo);

      var selectProjection = subStatementTableInfo.SqlStatement.SelectProjection;
      var sqlTable = _expression.SqlTable;

      var innerSqlEntityExpression = selectProjection as SqlEntityExpression;
      var innerNamedExpression = selectProjection as NamedExpression;

      if (innerSqlEntityExpression != null)
        _result = innerSqlEntityExpression.Clone (sqlTable);
      else if (innerNamedExpression != null)
        _result = new SqlValueReferenceExpression (sqlTable.ItemType, innerNamedExpression.Name, subStatementTableInfo.TableAlias);
      else
        throw new InvalidOperationException ("The table projection for a referenced sub-statement must be named or an entity.");
      
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