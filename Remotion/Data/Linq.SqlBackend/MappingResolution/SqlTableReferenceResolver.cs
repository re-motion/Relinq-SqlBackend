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
    private Expression _expression;
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

      // TODO Review 2718: don't store the expression, only store the SqlTable
      // TODO 2719: As soon as SqlEntity is decoupled from SqlTable, don't store the SqlTable any longer
      _expression = expression;
      _generator = generator;
      _resolver = resolver;
    }

    public Expression ResolveSqlTableReferenceExpression (SqlTableReferenceExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      expression.SqlTable.GetResolvedTableInfo ().Accept (this);

      return _expression; // TODO Review 2718: rename field to _result
    }

    public ITableInfo VisitSimpleTableInfo (ResolvedSimpleTableInfo tableInfo)
    {
      // TODO Review 2718: Refactor ResolveTableReferenceExpression to take SimpleTableInfo instead of a SqlTableReferenceExpression, rename it to ResolveTableReference, don't forget to update the docs on IMappingResolver
      _expression = _resolver.ResolveTableReferenceExpression ((SqlTableReferenceExpression) _expression, _generator);
      return tableInfo;
    }

    public ITableInfo VisitSubStatementTableInfo (ResolvedSubStatementTableInfo subStatementTableInfo)
    {
      ArgumentUtility.CheckNotNull ("subStatementTableInfo", subStatementTableInfo);

      var selectProjection = subStatementTableInfo.SqlStatement.SelectProjection;
      var sqlTable = ((SqlTableReferenceExpression) _expression).SqlTable;

      SqlEntityExpression innerSqlEntityExpression;
      NamedExpression innerNamedExpression;

      if ((innerSqlEntityExpression = selectProjection as SqlEntityExpression) != null)
        _expression = innerSqlEntityExpression.Clone (sqlTable);
      else if ((innerNamedExpression = selectProjection as  NamedExpression) != null) // TODO Review 2718: Use as and check for null for symmetry with the if above
        _expression = new SqlValueReferenceExpression (sqlTable.ItemType, innerNamedExpression.Name, sqlTable.GetResolvedTableInfo ().TableAlias); // TODO Review 2718: use subStatementTableInfo.TableAlias
      else
        throw new NotSupportedException ("The table projection for a referenced sub-statement must be named or an entity."); // TODO Review 2718: change to InvalidOperationException; NotSupportedException is for an unsupported feature/usage, here we have an invalid operation due to unsupported input data
      
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