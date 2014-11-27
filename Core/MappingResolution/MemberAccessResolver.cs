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
using System.Collections;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Remotion.Linq.Parsing;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Linq.SqlBackend.Utilities;
using Remotion.Utilities;
using MemberBinding = Remotion.Linq.Parsing.ExpressionVisitors.MemberBindings.MemberBinding;

namespace Remotion.Linq.SqlBackend.MappingResolution
{
  /// <summary>
  /// <see cref="MemberAccessResolver"/> is used by <see cref="DefaultMappingResolutionStage"/> to resolve <see cref="MemberInfo"/>s applied to
  /// expressions. The <see cref="MemberAccessResolver"/> class assumes that its input expression has already been resolved, and it may return a
  /// result that itself needs to be resolved again.
  /// </summary>
  public class MemberAccessResolver
      : ThrowingExpressionVisitor,
        INamedExpressionVisitor,
        IResolvedSqlExpressionVisitor,
        ISqlGroupingSelectExpressionVisitor,
        ISqlEntityRefMemberExpressionVisitor
  {
    private readonly MemberInfo _memberInfo;
    private readonly IMappingResolver _mappingResolver;
    private readonly IMappingResolutionStage _stage;
    private readonly IMappingResolutionContext _context;

    public static Expression ResolveMemberAccess (
        Expression resolvedSourceExpression,
        MemberInfo memberInfo,
        IMappingResolver mappingResolver,
        IMappingResolutionStage mappingResolutionStage,
        IMappingResolutionContext mappingResolutionContext)
    {
      ArgumentUtility.CheckNotNull ("resolvedSourceExpression", resolvedSourceExpression);
      ArgumentUtility.CheckNotNull ("memberInfo", memberInfo);
      ArgumentUtility.CheckNotNull ("mappingResolver", mappingResolver);
      ArgumentUtility.CheckNotNull ("mappingResolutionStage", mappingResolutionStage);
      ArgumentUtility.CheckNotNull ("mappingResolutionContext", mappingResolutionContext);

      var resolver = new MemberAccessResolver (memberInfo, mappingResolver, mappingResolutionStage, mappingResolutionContext);
      return resolver.Visit (resolvedSourceExpression);
    }

    protected MemberAccessResolver (
        MemberInfo memberInfo, IMappingResolver mappingResolver, IMappingResolutionStage stage, IMappingResolutionContext context)
    {
      ArgumentUtility.CheckNotNull ("memberInfo", memberInfo);
      ArgumentUtility.CheckNotNull ("mappingResolver", mappingResolver);
      ArgumentUtility.CheckNotNull ("stage", stage);
      ArgumentUtility.CheckNotNull ("context", context);

      _memberInfo = memberInfo;
      _mappingResolver = mappingResolver;
      _stage = stage;
      _context = context;
    }

    protected override Exception CreateUnhandledItemException<T> (T unhandledItem, string visitMethod)
    {
      ArgumentUtility.CheckNotNull ("unhandledItem", unhandledItem);
      ArgumentUtility.CheckNotNull ("visitMethod", visitMethod);

      throw new NotSupportedException (
          string.Format (
              "Cannot resolve member '{0}' applied to expression '{1}'; the expression type '{2}' is not supported in member expressions.",
              _memberInfo.Name,
              unhandledItem,
              unhandledItem.GetType().Name));
    }

    protected override Expression VisitUnary (UnaryExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      if (expression.NodeType == ExpressionType.Convert)
      {
        // Scenario: ((SomeType) expr).Member
        // Strip away that cast, we don't care about it, we just care about the member in the context of the inner expression.
        return Visit (expression.Operand);
      }
      else
      {
        // Can't handle any other unary expression.
        return base.VisitUnary (expression);
      }
    }

    public Expression VisitSqlEntityRefMember (SqlEntityRefMemberExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      // Scenario: entityRef.Member

      var result = _mappingResolver.TryResolveOptimizedMemberExpression (expression, _memberInfo);
      if (result != null)
        return result;

      // Optimized member access didn't work, so resolve the entity reference (adding joins and such), then retry.
      var entityExpression = _stage.ResolveEntityRefMemberExpression (expression, _context);
      return Visit (entityExpression);
    }

    public Expression VisitNamed (NamedExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      // Scenario: (expr AS "Value")).Member
      // Just strip the name; we're resolving the Member and don't care about the name of the expression to which the member is applied.
      return Visit (expression.Expression);
    }

    protected override Expression VisitNew (NewExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      // This ReSharper warning is wrong, expression.Members can be null.
      // ReSharper disable ConditionIsAlwaysTrueOrFalse
      if (expression.Members != null && expression.Members.Count > 0)
      // ReSharper restore ConditionIsAlwaysTrueOrFalse
      {
        // Scenario: (new X (A = arg1, B = arg2, ...)).Member - we can resolve this if one of (A, B, ...) matches Member.
        // Use the MemberBinding classes to determine this. (This takes care of A, B, ... and Member being  of different member types, 
        // e.g., accessor MethodInfo and PropertyInfo.)
        var membersAndAssignedExpressions = expression.Members.Select ((m, i) => MemberBinding.Bind (m, expression.Arguments[i]));
        var result = membersAndAssignedExpressions.SingleOrDefault (c => c.MatchesReadAccess (_memberInfo));

        if (result != null)
        {
          // remove name if any - the name is only required at the definition, not at the reference
          return _context.RemoveNamesAndUpdateMapping (result.AssociatedExpression);
        }
      }

      // Scenario: (new X (A = arg1, B = arg2, ...)).Member - with a non-matching Member; or
      // Scenario: (new X (arg1, arg2, ...)).Member - we can't resolve this ATM


      throw new NotSupportedException (
            string.Format (
                "The member '{0}.{1}' cannot be translated to SQL. Expression: '{2}'",
                expression.Type.Name,
                _memberInfo.Name,
                expression));
    }

    public Expression VisitSqlEntity (SqlEntityExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      // Scenario: entity.Member
      // Member must not be a collection, since we don't support in-line usage of collection members for now (e.g., select c.Assistants)
      // Otherwise, we just ask the _mappingResolver to resolve this member for us.

      var type = ReflectionUtility.GetMemberReturnType (_memberInfo);
      if (typeof (IEnumerable).IsAssignableFrom (type) && type != typeof (string))
      {
        Assertion.DebugAssert (_memberInfo.DeclaringType != null, "Global members not supported.");
        var message = string.Format (
            "The member '{0}.{1}' describes a collection and can only be used in places where collections are allowed. Expression: '{2}'",
            _memberInfo.DeclaringType.Name,
            _memberInfo.Name,
            expression);
        throw new NotSupportedException (message);
      }

      return _mappingResolver.ResolveMemberExpression (expression, _memberInfo);
    }

    public Expression VisitSqlColumn (SqlColumnExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      // Scenario: column.Member (probably originally entity.ColumnMember.Member)
      // Handled by the _mappingResolver

      return _mappingResolver.ResolveMemberExpression (expression, _memberInfo);
    }

    public Expression VisitSqlGroupingSelect (SqlGroupingSelectExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      // Scenario: grouping.Key
      Assertion.DebugAssert (_memberInfo.Equals (expression.Type.GetProperty ("Key")));

      // No problem, just use the KeyExpression (without a name, we don't care about the original name of the expression when we resolve members).

      return _context.RemoveNamesAndUpdateMapping (expression.KeyExpression);
    }

    Expression IResolvedSqlExpressionVisitor.VisitSqlEntityConstant (SqlEntityConstantExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      // Not supported, required by IUnresolvedSqlExpressionVisitor.
      return VisitExtension (expression);
    }
  }
}