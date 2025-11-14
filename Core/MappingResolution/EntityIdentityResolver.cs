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
using System.Linq.Expressions;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;
using Remotion.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Utilities;

namespace Remotion.Linq.SqlBackend.MappingResolution
{
  /// <summary>
  /// Resolves expressions representing entities (<see cref="SqlEntityExpression"/>, <see cref="SqlEntityRefMemberExpression"/>, 
  /// <see cref="SqlSubStatementExpression"/> selecting entities) to their respective identity expressions.
  /// </summary>
  public class EntityIdentityResolver : IEntityIdentityResolver
  {
    private readonly IMappingResolutionStage _stage;
    private readonly IMappingResolver _resolver;
    private readonly IMappingResolutionContext _context;

    public EntityIdentityResolver (IMappingResolutionStage stage, IMappingResolver resolver, IMappingResolutionContext context)
    {
      _stage = stage;
      _resolver = resolver;
      _context = context;
    }

    public Expression ResolvePotentialEntity (Expression expression)
    {
      ArgumentUtility.CheckNotNull (nameof(expression), expression);

      var strippedExpression = StripConversions (expression);

      var namedExpression = strippedExpression as NamedExpression;
      if (namedExpression != null)
      {
        var result = ResolvePotentialEntity (namedExpression.Expression);
        if (result != namedExpression.Expression)
          return new NamedExpression (namedExpression.Name, result);

        return expression;
      }

      var entityExpression = strippedExpression as SqlEntityExpression;
      if (entityExpression != null)
      {
        var result = entityExpression.GetIdentityExpression();
        Assertion.IsTrue (result != entityExpression, "SqlEntityExpression cannot be the same instance as its identity Expression.");
        return result;
      }

      var entityConstantExpression = strippedExpression as SqlEntityConstantExpression;
      if (entityConstantExpression != null)
      {
        var result = entityConstantExpression.IdentityExpression;
        Assertion.IsTrue (result != entityConstantExpression, "SqlEntityConstantExpression cannot be the same instance as its identity Expression.");
        return result;
      }

      var entityRefMemberExpression = strippedExpression as SqlEntityRefMemberExpression;
      if (entityRefMemberExpression != null)
      {
        var result = GetIdentityExpressionForReferencedEntity (entityRefMemberExpression);
        if (result != entityRefMemberExpression)
          return result;
      }

      var sqlSubStatementExpression = strippedExpression as SqlSubStatementExpression;
      if (sqlSubStatementExpression != null)
      {
        var result = CheckAndSimplifyEntityWithinSubStatement (sqlSubStatementExpression);
        if (result != sqlSubStatementExpression)
          return result;
      }

      return expression;
    }

    public BinaryExpression ResolvePotentialEntityComparison (BinaryExpression binaryExpression)
    {
      ArgumentUtility.CheckNotNull (nameof(binaryExpression), binaryExpression);

      var newLeft = ResolvePotentialEntity (binaryExpression.Left);
      var newRight = ResolvePotentialEntity (binaryExpression.Right);

      if (newLeft != binaryExpression.Left || newRight != binaryExpression.Right)
      {
        // Note: Method is stripped because when an entity is reduced to its identity, the method can no longer work.
        return ConversionUtility.MakeBinaryWithOperandConversion (
            binaryExpression.NodeType,
            newLeft,
            newRight,
            false,
            null);
      }

      return binaryExpression;
    }

    public SqlInExpression ResolvePotentialEntityComparison (SqlInExpression inExpression)
    {
      ArgumentUtility.CheckNotNull (nameof(inExpression), inExpression);

      var newLeft = ResolvePotentialEntity (inExpression.LeftExpression);
      var newRight = ResolvePotentialEntity (inExpression.RightExpression);

      if (newLeft != inExpression.LeftExpression || newRight != inExpression.RightExpression)
        return new SqlInExpression (newLeft, newRight);

      return inExpression;
    }

    public SqlIsNullExpression ResolvePotentialEntityComparison (SqlIsNullExpression isNullExpression)
    {
      var newExpression = ResolvePotentialEntity (isNullExpression.Expression);

      if (newExpression != isNullExpression.Expression)
        return new SqlIsNullExpression (newExpression);

      return isNullExpression;
    }

    public SqlIsNotNullExpression ResolvePotentialEntityComparison (SqlIsNotNullExpression isNotNullExpression)
    {
      var newExpression = ResolvePotentialEntity (isNotNullExpression.Expression);

      if (newExpression != isNotNullExpression.Expression)
        return new SqlIsNotNullExpression (newExpression);

      return isNotNullExpression;
    }

    private Expression GetIdentityExpressionForReferencedEntity (SqlEntityRefMemberExpression expression)
    {
      var optimizedIdentity = _resolver.TryResolveOptimizedIdentity (expression);
      if (optimizedIdentity != null)
        return optimizedIdentity;

      var unresolvedJoinInfo = new UnresolvedJoinInfo (expression.OriginatingEntity, expression.MemberInfo, JoinCardinality.One);
      return _stage.ResolveEntityRefMemberExpression (expression, unresolvedJoinInfo, _context).GetIdentityExpression ();
    }

    private Expression CheckAndSimplifyEntityWithinSubStatement (SqlSubStatementExpression sqlSubStatementExpression)
    {
      var newSelectProjection = ResolvePotentialEntity (sqlSubStatementExpression.SqlStatement.SelectProjection);
      if (newSelectProjection != sqlSubStatementExpression.SqlStatement.SelectProjection)
      {
        var newSubStatement = new SqlStatementBuilder (sqlSubStatementExpression.SqlStatement) { SelectProjection = newSelectProjection };
        newSubStatement.RecalculateDataInfo (sqlSubStatementExpression.SqlStatement.SelectProjection);

        return newSubStatement.GetSqlStatement ().CreateExpression ();
      }

      return sqlSubStatementExpression;
    }

    private Expression StripConversions (Expression expression)
    {
      while (expression.NodeType == ExpressionType.Convert || expression.NodeType == ExpressionType.ConvertChecked)
        expression = ((UnaryExpression) expression).Operand;
      return expression;
    }
  }
}