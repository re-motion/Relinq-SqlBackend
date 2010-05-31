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
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.SqlBackend.MappingResolution
{
  public class SubStatementReferenceResolver : ExpressionTreeVisitor, IResolvedSqlExpressionVisitor, INamedExpressionVisitor
  {
    public static Expression ResolveSubStatementReferenceExpression (
        Expression referencedExpression,
        ResolvedSubStatementTableInfo containingSubStatementTableInfo,
        SqlTableBase containingSqlTable,
        Type type,
        IMappingResolutionContext context)
    {
      ArgumentUtility.CheckNotNull ("referencedExpression", referencedExpression);
      ArgumentUtility.CheckNotNull ("containingSubStatementTableInfo", containingSubStatementTableInfo);
      ArgumentUtility.CheckNotNull ("sqlTable", containingSqlTable);
      
      var visitor = new SubStatementReferenceResolver (containingSubStatementTableInfo, containingSqlTable, type);
      var result = visitor.VisitExpression (referencedExpression);

      if (result is SqlEntityExpression)
        context.AddSqlEntityMapping ((SqlEntityExpression) result, containingSqlTable);

      return result;
    }

    private readonly ResolvedSubStatementTableInfo _tableInfo;
    private readonly SqlTableBase _sqlTable;
    private Type _type;

    protected SubStatementReferenceResolver (ResolvedSubStatementTableInfo tableInfo, SqlTableBase sqlTable, Type type)
    {
      ArgumentUtility.CheckNotNull ("tableInfo", tableInfo);
      ArgumentUtility.CheckNotNull ("sqlTable", sqlTable);
      ArgumentUtility.CheckNotNull ("type", type);

      _tableInfo = tableInfo;
      _sqlTable = sqlTable;
      _type = type;
    }


    public Expression VisitSqlEntityExpression (SqlEntityExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      return expression.CreateReference (_tableInfo.TableAlias, _type);
    }
    
    public Expression VisitNamedExpression (NamedExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      return new SqlValueReferenceExpression (_type, expression.Name, _tableInfo.TableAlias);
    }

    protected override Expression VisitNewExpression (NewExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      return new SqlCompoundReferenceExpression (_type, null, _sqlTable, _tableInfo, expression);
    }

    protected override Expression VisitUnaryExpression (UnaryExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      _type = expression.Type;
      return VisitExpression (expression.Operand);
    }

    Expression IResolvedSqlExpressionVisitor.VisitSqlValueReferenceExpression (SqlValueReferenceExpression expression)
    {
      throw new InvalidOperationException ("SqlValueReferenceExpression is not valid at this point.");
    }

    Expression IResolvedSqlExpressionVisitor.VisitSqlColumnExpression (SqlColumnExpression expression)
    {
      throw new InvalidOperationException ("SqlColumnExpression is not valid at this point.");
    }
    
  }
}