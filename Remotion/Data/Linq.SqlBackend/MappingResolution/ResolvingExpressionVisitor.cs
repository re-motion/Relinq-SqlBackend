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
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.SqlBackend.MappingResolution
{
  /// <summary>
  /// <see cref="ResolvingExpressionVisitor"/> implements <see cref="IUnresolvedSqlExpressionVisitor"/> and <see cref="ThrowingExpressionTreeVisitor"/>.
  /// </summary>
  public class ResolvingExpressionVisitor : ExpressionTreeVisitor, IUnresolvedSqlExpressionVisitor, ISqlResultExpressionVisitor
  {
    private readonly IMappingResolver _resolver;
    private readonly UniqueIdentifierGenerator _generator;
    private readonly IMappingResolutionStage _stage;

    public static Expression ResolveExpression (
        Expression expression, IMappingResolver resolver, UniqueIdentifierGenerator generator, IMappingResolutionStage stage)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      ArgumentUtility.CheckNotNull ("resolver", resolver);
      ArgumentUtility.CheckNotNull ("generator", generator);
      ArgumentUtility.CheckNotNull ("stage", stage);

      var visitor = new ResolvingExpressionVisitor (resolver, generator, stage);
      var result = visitor.VisitExpression (expression);
      return result;
    }

    protected ResolvingExpressionVisitor (IMappingResolver resolver, UniqueIdentifierGenerator generator, IMappingResolutionStage stage)
    {
      ArgumentUtility.CheckNotNull ("resolver", resolver);
      ArgumentUtility.CheckNotNull ("generator", generator);
      ArgumentUtility.CheckNotNull ("stage", stage);

      _resolver = resolver;
      _generator = generator;
      _stage = stage;
    }

    public Expression VisitSqlTableReferenceExpression (SqlTableReferenceExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      var newExpression = _resolver.ResolveTableReferenceExpression (expression, _generator);
      if (newExpression == expression)
        return expression;
      else
        return VisitExpression (newExpression);
    }

    public Expression VisitSqlMemberExpression (SqlMemberExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      var newExpression = _resolver.ResolveMemberExpression (expression, _generator);
      if (newExpression == expression)
        return expression;
      else
        return VisitExpression (newExpression);
    }

    protected override Expression VisitConstantExpression (ConstantExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      return _resolver.ResolveConstantExpression (expression);
    }

    public Expression VisitSqlEntityRefMemberExpression (SqlEntityRefMemberExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      var join = expression.SqlTable.GetOrAddJoin (expression.MemberInfo, JoinCardinality.One);
      join.JoinInfo = ResolvingJoinInfoVisitor.ResolveJoinInfo (join.JoinInfo, _resolver, _generator);

      var sqlTableReferenceExpression = new SqlTableReferenceExpression (join);
      return VisitExpression (sqlTableReferenceExpression);
    }

    public Expression VisitSqlSubStatementExpression (SqlSubStatementExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      _stage.ResolveSqlStatement (expression.SqlStatement);
      return expression;
    }

    public Expression VisitSqlFunctionExpression (SqlFunctionExpression expression)
    {
      // TODO: return VisitUnknownExpression (expression);

      ArgumentUtility.CheckNotNull ("expression", expression);

      var newPrefixExpression = ResolveExpression (expression.Prefix, _resolver, _generator, _stage);
      List<Expression> newArguments = new List<Expression>();

      foreach (var arg in expression.Args)
        newArguments.Add (ResolveExpression (arg, _resolver, _generator, _stage));
      
      if ((expression.Prefix != newPrefixExpression) || (expression.Args.ToList() != newArguments))
        return new SqlFunctionExpression (expression.Type, expression.SqlFunctioName, newPrefixExpression, newArguments.ToArray());

      return expression;
    }

    public Expression VisitSqlConvertExpression (SqlConvertExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      var newExpression = ResolveExpression (expression.Source, _resolver, _generator, _stage);

      if (expression.Source != newExpression)
        return new SqlConvertExpression (expression.Type, newExpression);

      return expression;
    }
  }
}