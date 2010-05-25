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
using System.Collections;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.SqlBackend.MappingResolution
{
  /// <summary>
  /// <see cref="ResolvingExpressionVisitor"/> implements <see cref="IUnresolvedSqlExpressionVisitor"/> and <see cref="ThrowingExpressionTreeVisitor"/>.
  /// </summary>
  public class ResolvingExpressionVisitor : ExpressionTreeVisitor, IUnresolvedSqlExpressionVisitor, ISqlSubStatementVisitor, INamedExpressionVisitor
  {
    private readonly IMappingResolver _resolver;
    private readonly UniqueIdentifierGenerator _generator;
    private readonly IMappingResolutionStage _stage;
    private readonly IMappingResolutionContext _context;

    public static Expression ResolveExpression (Expression expression, IMappingResolver resolver, UniqueIdentifierGenerator generator, IMappingResolutionStage stage, IMappingResolutionContext context)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      ArgumentUtility.CheckNotNull ("resolver", resolver);
      ArgumentUtility.CheckNotNull ("generator", generator);
      ArgumentUtility.CheckNotNull ("stage", stage);
      ArgumentUtility.CheckNotNull ("context", context);

      var visitor = new ResolvingExpressionVisitor (resolver, generator, stage, context);
      var result = visitor.VisitExpression (expression);
      return result;
    }

    protected ResolvingExpressionVisitor (IMappingResolver resolver, UniqueIdentifierGenerator generator, IMappingResolutionStage stage, IMappingResolutionContext context)
    {
      ArgumentUtility.CheckNotNull ("resolver", resolver);
      ArgumentUtility.CheckNotNull ("generator", generator);
      ArgumentUtility.CheckNotNull ("stage", stage);
      ArgumentUtility.CheckNotNull ("context", context);

      _resolver = resolver;
      _generator = generator;
      _stage = stage;
      _context = context;
    }

    public Expression VisitSqlTableReferenceExpression (SqlTableReferenceExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      
      var resolvedExpression = _stage.ResolveTableReferenceExpression (expression, _context);
      return VisitExpression (resolvedExpression);
    }

    protected override Expression VisitConstantExpression (ConstantExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      return _resolver.ResolveConstantExpression (expression);
    }

    protected override Expression VisitMemberExpression (MemberExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      // First process any nested expressions
      // E.g, for (kitchen.Cook).FirstName, first process kitchen => newExpression1 (SqlEntity)
      // then newExpression1.Cook => newExpression2 (SqlEntityRef/SqlEntity)
      // then newExpression2.FirstName => result (SqlColumn)
      var newExpression = VisitExpression (expression.Expression);

      //member withe a cast?
      var newExpressionAsUnaryExpression = newExpression as UnaryExpression;
      if (newExpressionAsUnaryExpression != null && newExpressionAsUnaryExpression.NodeType == ExpressionType.Convert)
      {
        newExpression = newExpressionAsUnaryExpression.Operand;
      }

      var newExpressionAsSqlEntityRefMemberExpression = newExpression as SqlEntityRefMemberExpression;
      if (newExpressionAsSqlEntityRefMemberExpression != null)
      {
        var unresolvedJoinInfo = new UnresolvedJoinInfo (
            newExpressionAsSqlEntityRefMemberExpression.EntityExpression, newExpressionAsSqlEntityRefMemberExpression.MemberInfo, JoinCardinality.One);
        newExpression = _stage.ResolveEntityRefMemberExpression (newExpressionAsSqlEntityRefMemberExpression, unresolvedJoinInfo, _context);
      }

      // member applied to an entity?
      var newExpressionAsEntityExpression = newExpression as SqlEntityExpression;
      if (newExpressionAsEntityExpression != null)
      {
        var propertyInfoType = ((PropertyInfo) expression.Member).PropertyType;
        if (typeof (IEnumerable).IsAssignableFrom (propertyInfoType) && propertyInfoType!=typeof(string))
          throw new NotSupportedException (
              "The member 'Cook.Assistants' describes a collection and can only be used in places where collections are allowed.");
        
        var resolvedMemberExpression = _resolver.ResolveMemberExpression (newExpressionAsEntityExpression, expression.Member, _generator);
        return VisitExpression (resolvedMemberExpression);
      }

      // member applied to a column?
      var newExpressionAsColumnExpression = newExpression as SqlColumnExpression;
      if (newExpressionAsColumnExpression != null)
      {
        var resolvedMemberExpression = _resolver.ResolveMemberExpression (newExpressionAsColumnExpression, expression.Member);
        return VisitExpression (resolvedMemberExpression);
      }

      // member applied to a compound expression?
      var newExpressionAsCompoundReferenceExpression = newExpression as SqlCompoundReferenceExpression;
      if (newExpressionAsCompoundReferenceExpression != null)
      {
        var property = (PropertyInfo) expression.Member;
        var getterMethod = property.GetGetMethod (true);

        var membersAndAssignedExpressions = newExpressionAsCompoundReferenceExpression.ReferencedNewExpression.Members
            .Select ((m, i) => new { Member = m, Argument = newExpressionAsCompoundReferenceExpression.ReferencedNewExpression.Arguments[i] });
        var expressionOfMatchingMember = membersAndAssignedExpressions.Single (c => c.Member == getterMethod).Argument;

        var referenceExpression = SqlTableReferenceResolver.CreateReferenceExpression (
            expressionOfMatchingMember,
            newExpressionAsCompoundReferenceExpression.SubStatementTableInfo,
            newExpressionAsCompoundReferenceExpression.ReferencedTable);
        return VisitExpression (referenceExpression);
      }

      throw new NotSupportedException (string.Format ("Resolved inner expression of type {0} is not supported.", newExpression.Type.Name));
    }

    protected override Expression VisitTypeBinaryExpression (TypeBinaryExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      var newExpression = VisitExpression(expression.Expression);
      var resolvedTypeExpression = _resolver.ResolveTypeCheck (newExpression, expression.TypeOperand);
      return VisitExpression (resolvedTypeExpression);
    }

    public Expression VisitSqlSubStatementExpression (SqlSubStatementExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      var newSqlStatement = _stage.ResolveSqlStatement (expression.SqlStatement, _context);
      return new SqlSubStatementExpression (newSqlStatement);
    }

    public Expression VisitNamedExpression (NamedExpression expression)
    {
      var newExpression = VisitExpression (expression.Expression);

      if (newExpression is SqlEntityExpression)
        return ((SqlEntityExpression) newExpression).Update (newExpression.Type, ((SqlEntityExpression) newExpression).TableAlias, expression.Name);

      if (newExpression != expression.Expression)
        return new NamedExpression (expression.Name, newExpression);
      return expression;
    }

    Expression IUnresolvedSqlExpressionVisitor.VisitSqlEntityRefMemberExpression (SqlEntityRefMemberExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      return base.VisitUnknownExpression (expression);
    }

    Expression IUnresolvedSqlExpressionVisitor.VisitSqlEntityConstantExpression (SqlEntityConstantExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      return base.VisitUnknownExpression (expression);
    }
   
  }
}