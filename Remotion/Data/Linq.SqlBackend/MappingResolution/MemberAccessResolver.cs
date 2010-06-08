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
using Remotion.Data.Linq.Clauses.ExpressionTreeVisitors;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.SqlBackend.MappingResolution
{
  /// <summary>
  /// <see cref="MemberAccessResolver"/> is used by <see cref="DefaultMappingResolutionStage"/> to resolve <see cref="MemberInfo"/>s applied to
  /// expressions. The <see cref="MemberAccessResolver"/> class assumes that its input expression has already been resolved, and it may return a
  /// result that itself needs to be resolved again.
  /// </summary>
  public class MemberAccessResolver
  {
    public static Expression ResolveMemberAccess (Expression resolvedSourceExpression, MemberInfo memberInfo, IMappingResolver mappingResolver, IMappingResolutionStage mappingResolutionStage, IMappingResolutionContext mappingResolutionContext)
    {
      ArgumentUtility.CheckNotNull ("resolvedSourceExpression", resolvedSourceExpression);
      ArgumentUtility.CheckNotNull ("memberInfo", memberInfo);
      ArgumentUtility.CheckNotNull ("mappingResolver", mappingResolver);
      ArgumentUtility.CheckNotNull ("mappingResolutionStage", mappingResolutionStage);
      ArgumentUtility.CheckNotNull ("mappingResolutionContext", mappingResolutionContext);

      var resolvedInnerExpression = resolvedSourceExpression;

      //member with a cast?
      UnaryExpression resolvedInnerAsUnaryExpression;
      while ((resolvedInnerAsUnaryExpression = resolvedInnerExpression as UnaryExpression) != null
             && resolvedInnerAsUnaryExpression.NodeType == ExpressionType.Convert)
        resolvedInnerExpression = resolvedInnerAsUnaryExpression.Operand;

      var resolvedInnerAsSqlEntityRefMemberExpression = resolvedInnerExpression as SqlEntityRefMemberExpression;
      if (resolvedInnerAsSqlEntityRefMemberExpression != null)
      {
        var unresolvedJoinInfo = new UnresolvedJoinInfo (
            resolvedInnerAsSqlEntityRefMemberExpression.OriginatingEntity, resolvedInnerAsSqlEntityRefMemberExpression.MemberInfo, JoinCardinality.One);
        resolvedInnerExpression = mappingResolutionStage.ResolveEntityRefMemberExpression (
            resolvedInnerAsSqlEntityRefMemberExpression, unresolvedJoinInfo, mappingResolutionContext);
      }

      // named expressions are ignored for member access
      while (resolvedInnerExpression is NamedExpression)
        resolvedInnerExpression = ((NamedExpression) resolvedInnerExpression).Expression;

      // member applied to an entity?
      var resolvedInnerAsEntityExpression = resolvedInnerExpression as SqlEntityExpression;
      if (resolvedInnerAsEntityExpression != null)
      {
        var propertyInfoType = ((PropertyInfo) memberInfo).PropertyType;
        if (typeof (IEnumerable).IsAssignableFrom (propertyInfoType) && propertyInfoType != typeof (string))
        {
          throw new NotSupportedException (
              "The member 'Cook.Assistants' describes a collection and can only be used in places where collections are allowed.");
        }

        return mappingResolver.ResolveMemberExpression (resolvedInnerAsEntityExpression, memberInfo);
      }

      // member applied to a column?
      var resolvedInnerAsColumnExpression = resolvedInnerExpression as SqlColumnExpression;
      if (resolvedInnerAsColumnExpression != null)
        return mappingResolver.ResolveMemberExpression (resolvedInnerAsColumnExpression, memberInfo);

      // member applied to a compound expression?
      var resolvedInnerAsNewExpression = resolvedInnerExpression as NewExpression;
      if (resolvedInnerAsNewExpression != null)
      {
        var property = (PropertyInfo) memberInfo;
        var getterMethod = property.GetGetMethod (true);

        var membersAndAssignedExpressions =
            resolvedInnerAsNewExpression.Members.Select ((m, i) => new { Member = m, Argument = resolvedInnerAsNewExpression.Arguments[i] });
        return membersAndAssignedExpressions.Single (c => c.Member == getterMethod).Argument;
      }

      throw new NotSupportedException (
          String.Format (
              "Resolved inner expression '{0}' of type '{1}' is not supported.",
              FormattingExpressionTreeVisitor.Format (resolvedInnerExpression),
              resolvedInnerExpression.GetType().Name));
    }
  }
}