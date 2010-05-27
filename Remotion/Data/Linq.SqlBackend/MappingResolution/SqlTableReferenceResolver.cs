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

    // TODO Review 2716: Move this method to another class: SubStatementReferenceResolver; refactor to work as an ExpressionVisitor instead of casting; move the tests from SqlTableReferenceResolverTest to SubStatementReferenceResolverTest; only leave two tests for VisitSubStatementTableInfo: one where an entity is returned (check that the mapping is added to the context), one where another expression is returned
    public static Expression CreateReferenceExpression (
        Expression referencedExpression, 
        ResolvedSubStatementTableInfo containingSubStatementTableInfo, 
        SqlTableBase containingSqlTable)
    {
      ArgumentUtility.CheckNotNull ("referencedExpression", referencedExpression);
      ArgumentUtility.CheckNotNull ("containingSubStatementTableInfo", containingSubStatementTableInfo);
      ArgumentUtility.CheckNotNull ("sqlTable", containingSqlTable);

      var innerSqlEntityExpression = referencedExpression as SqlEntityExpression;
      var innerNamedExpression = referencedExpression as NamedExpression;
      var innerNewExpression = referencedExpression as NewExpression;
      var innerUnaryExpression = referencedExpression as UnaryExpression;

      if (innerSqlEntityExpression != null)
        return innerSqlEntityExpression.CreateReference (containingSqlTable.GetResolvedTableInfo ().TableAlias); // TODO Review 2788: Use containingSubStatementTableInfo.TableAlias
      else if (innerNamedExpression != null)
        return new SqlValueReferenceExpression (referencedExpression.Type, innerNamedExpression.Name, containingSubStatementTableInfo.TableAlias);
      else if (innerNewExpression != null)
        return new SqlCompoundReferenceExpression (referencedExpression.Type, null, containingSqlTable, containingSubStatementTableInfo, innerNewExpression);
      else if (innerUnaryExpression != null)
        return CreateReferenceExpression (innerUnaryExpression.Operand, containingSubStatementTableInfo, containingSqlTable);
      else
        throw new InvalidOperationException ("The table projection for a referenced sub-statement must be a NewExpression, named or an entity.");
    }

    protected SqlTableReferenceResolver (SqlTableReferenceExpression expression, IMappingResolver resolver, UniqueIdentifierGenerator generator, IMappingResolutionContext context)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      ArgumentUtility.CheckNotNull ("resolver", resolver);
      ArgumentUtility.CheckNotNull ("generator", generator);
      ArgumentUtility.CheckNotNull ("context", context);

      // TODO Review 2716: don't store the expression, only store the SqlTable
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
      var entity = (SqlEntityExpression) _resolver.ResolveTableReferenceExpression(_expression, _generator);
      _context.AddSqlEntityMapping (entity, _expression.SqlTable);
      _result = entity;
      return tableInfo;
    }

    public ITableInfo VisitSubStatementTableInfo (ResolvedSubStatementTableInfo subStatementTableInfo)
    {
      ArgumentUtility.CheckNotNull ("subStatementTableInfo", subStatementTableInfo);

      var selectProjection = subStatementTableInfo.SqlStatement.SelectProjection;
      var sqlTable = _expression.SqlTable;

      _result = CreateReferenceExpression (selectProjection, subStatementTableInfo, sqlTable);

      // TODO Review 2788: Move this to CreateReferenceExpression (to the innerSqlEntityExpression case, cast is not required there); the mapping must be added whenever a new entity is created, not just in this one case (otherwise code that accesses a member of a compound reference won't work correctly -  see NestedSelectProjection_WithJoinOnCompoundReferenceMember integration test); add a test for CreateReference showing that the mapping is added for entities
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