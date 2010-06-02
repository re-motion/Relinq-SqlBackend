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
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
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
      
      var visitor = new SubStatementReferenceResolver (containingSubStatementTableInfo, containingSqlTable, type, context);
      var result = visitor.VisitExpression (referencedExpression);

      if (result is SqlEntityExpression)
        context.AddSqlEntityMapping ((SqlEntityExpression) result, containingSqlTable);

      return result;
    }

    private readonly ResolvedSubStatementTableInfo _tableInfo;
    private readonly SqlTableBase _sqlTable;
    private readonly IMappingResolutionContext _context;
    private Type _type;
    
    protected SubStatementReferenceResolver (ResolvedSubStatementTableInfo tableInfo, SqlTableBase sqlTable, Type type, IMappingResolutionContext context)
    {
      ArgumentUtility.CheckNotNull ("tableInfo", tableInfo);
      ArgumentUtility.CheckNotNull ("sqlTable", sqlTable);
      ArgumentUtility.CheckNotNull ("type", type);
      ArgumentUtility.CheckNotNull ("context", context);

      _tableInfo = tableInfo;
      _sqlTable = sqlTable;
      _type = type;
      _context = context;
    }
    
    public Expression VisitSqlEntityExpression (SqlEntityExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      return expression.CreateReference (_tableInfo.TableAlias, _type);
    }
    
    public Expression VisitNamedExpression (NamedExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      return new SqlColumnDefinitionExpression (_type, _tableInfo.TableAlias, expression.Name, false);
    }

    protected override Expression VisitNewExpression (NewExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      var innerReferenceExpressions = expression.Arguments.Select ((arg, i) => ResolveNewExpressionArgument(arg, expression.Members[i]));
      return Expression.New (expression.Constructor, innerReferenceExpressions, expression.Members);

    }

    protected override Expression VisitUnaryExpression (UnaryExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      _type = expression.Type;
      return VisitExpression (expression.Operand);
    }

    private Expression ResolveNewExpressionArgument (Expression argument, MemberInfo correspondingMember)
    {
      var referenceToArgument = ResolveSubStatementReferenceExpression (argument, _tableInfo, _sqlTable, argument.Type, _context);
      return new NamedExpression (correspondingMember.Name, referenceToArgument);
    }

    Expression IResolvedSqlExpressionVisitor.VisitSqlColumnExpression (SqlColumnExpression expression)
    {
      throw new InvalidOperationException ("SqlColumnExpression is not valid at this point.");
    }
    
  }
}