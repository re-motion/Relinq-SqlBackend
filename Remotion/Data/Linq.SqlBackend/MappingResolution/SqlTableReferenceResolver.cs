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

    public static Expression CreateReferenceExpression (Expression referencedExpression, ResolvedSubStatementTableInfo subStatementTableInfo, SqlTableBase sqlTable)
    {
      var innerSqlEntityExpression = referencedExpression as SqlEntityExpression;
      var innerNamedExpression = referencedExpression as NamedExpression;
      var innerNewExpression = referencedExpression as NewExpression;
      var innerUnaryExpression = referencedExpression as UnaryExpression;

      if (innerSqlEntityExpression != null)
        return innerSqlEntityExpression.CreateReference (sqlTable.GetResolvedTableInfo ().TableAlias);
      else if (innerNamedExpression != null)
        return new SqlValueReferenceExpression (referencedExpression.Type, innerNamedExpression.Name, subStatementTableInfo.TableAlias);
      else if (innerNewExpression != null)
        return new SqlCompoundReferenceExpression (referencedExpression.Type, null, sqlTable, subStatementTableInfo, innerNewExpression);
      else if (innerUnaryExpression != null)
        return CreateReferenceExpression (innerUnaryExpression.Operand, subStatementTableInfo, sqlTable);
      else
        throw new InvalidOperationException ("The table projection for a referenced sub-statement must be a new-expression, named or an entity.");
    }

    protected SqlTableReferenceResolver (SqlTableReferenceExpression expression, IMappingResolver resolver, UniqueIdentifierGenerator generator, IMappingResolutionContext context)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      ArgumentUtility.CheckNotNull ("resolver", resolver);
      ArgumentUtility.CheckNotNull ("generator", generator);
      ArgumentUtility.CheckNotNull ("context", context);

      // TODO 2719: don't store the expression, only store the SqlTable
      // TODO 2719: As soon as SqlEntity is decoupled from SqlTable, don't store the SqlTable(Expression) any longer
      _expression = expression;
      _generator = generator;
      _resolver = resolver;
      _context = context;
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
      _context.AddSqlEntityMapping ((SqlEntityExpression)_result, _expression.SqlTable);
      return tableInfo;
    }

    public ITableInfo VisitSubStatementTableInfo (ResolvedSubStatementTableInfo subStatementTableInfo)
    {
      ArgumentUtility.CheckNotNull ("subStatementTableInfo", subStatementTableInfo);

      var selectProjection = subStatementTableInfo.SqlStatement.SelectProjection;
      var sqlTable = _expression.SqlTable;

      _result = CreateReferenceExpression (selectProjection, subStatementTableInfo, sqlTable);
      if(_result is SqlEntityExpression)
        _context.AddSqlEntityMapping ((SqlEntityExpression) _result, sqlTable);

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