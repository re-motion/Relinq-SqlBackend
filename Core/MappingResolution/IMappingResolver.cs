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
using System.Reflection;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Linq.SqlBackend.SqlStatementModel.Unresolved;

namespace Remotion.Linq.SqlBackend.MappingResolution
{
  /// <summary>
  /// <see cref="IMappingResolver"/> provides methods to resolve expressions with database-specific information delivered by an O/R mapper. This
  /// interface is implemented by LINQ providers making use of the re-linq SQL backend.
  /// </summary>
  public interface IMappingResolver
  {
    /// <summary>
    /// Takes an <see cref="UnresolvedTableInfo"/> and an <see cref="UniqueIdentifierGenerator"/> 
    /// to generate an <see cref="IResolvedTableInfo"/> that represents the table in the database.
    /// </summary>
    /// <param name="tableInfo">The <see cref="UnresolvedTableInfo"/> which is to be resolved.</param>
    /// <param name="generator">A <see cref="UniqueIdentifierGenerator"/> that can be used to generate unique identifiers such as table aliases.</param>
    /// <returns>An <see cref="IResolvedTableInfo"/> instance representing the  <paramref name="tableInfo"/> in the database.</returns>
    /// <exception cref="UnmappedItemException">The given <see cref="UnresolvedTableInfo"/> cannot be resolved to a mapped database item.</exception>
    IResolvedTableInfo ResolveTableInfo (UnresolvedTableInfo tableInfo, UniqueIdentifierGenerator generator);

    /// <summary>
    /// Takes an <see cref="UnresolvedJoinTableInfo"/> and an <see cref="UniqueIdentifierGenerator"/> to generate a 
    /// <see cref="ITableInfo"/> that represents the joined table in the database.
    /// </summary>
    /// <param name="tableInfo">The <see cref="UnresolvedJoinTableInfo"/> which is to be resolved.</param>
    /// <param name="generator">A <see cref="UniqueIdentifierGenerator"/> that can be used to generate unique identifiers such as table aliases.</param>
    /// <returns>
    /// An <see cref="ITableInfo"/> instance representing the <paramref name="tableInfo"/> in the database. Note that SQL does not allow this
    /// <see cref="ITableInfo"/> to be (or become) a <see cref="ResolvedSubStatementTableInfo"/> that references the 
    /// <see cref="UnresolvedJoinTableInfo.OriginatingEntity"/>. All such references must be moved to the join condition (see 
    /// <see cref="ResolveJoinCondition"/>).
    /// </returns>
    /// <exception cref="UnmappedItemException">The given <see cref="UnresolvedJoinTableInfo"/> cannot be resolved to a mapped database item.</exception>
    ITableInfo ResolveJoinTableInfo (UnresolvedJoinTableInfo tableInfo, UniqueIdentifierGenerator generator);

    /// <summary>
    /// Takes an <see cref="SqlEntityExpression"/> as the left side and a <see cref="IResolvedTableInfo"/> as the right side of a join as well as
    /// a <see cref="MemberInfo"/> identifying the member joining the two tables and returns an <see cref="Expression"/> that describes a join
    /// condition corresponding to that <see cref="MemberInfo"/>.
    /// </summary>
    /// <param name="originatingEntity">The entity constituting the left side of the join.</param>
    /// <param name="memberInfo">The <see cref="MemberInfo"/> used for joining the two sides.</param>
    /// <param name="joinedTableInfo">The table constituting the right side of the join.</param>
    /// <returns>The join condition <see cref="Expression"/> corresponding to <paramref name="memberInfo"/>.</returns>
    /// <exception cref="UnmappedItemException">The given <see cref="UnresolvedJoinConditionExpression"/> contains a reference that cannot be 
    /// resolved to a mapped database item.</exception>
    Expression ResolveJoinCondition (SqlEntityExpression originatingEntity, MemberInfo memberInfo, IResolvedTableInfo joinedTableInfo);

    ///  <summary>
    ///  Analyzes the given <see cref="ResolvedSimpleTableInfo"/> and returns a <see cref="SqlEntityDefinitionExpression"/>, i.e., an entity 
    ///  made of a row of the database table. If the item type of the <paramref name="tableInfo"/> is not a 
    ///  queryable entity, the resolver should throw an <see cref="UnmappedItemException"/>.
    ///  </summary>
    ///  <param name="tableInfo">The <see cref="ResolvedSimpleTableInfo"/> to be resolved.</param>
    /// <returns>A <see cref="SqlEntityDefinitionExpression"/> which contains all the columns of the referenced <paramref name="tableInfo"/> item type.
    /// </returns>
    ///  <remarks>
    ///  Note that compound expressions (<see cref="NewExpression"/> instances with named arguments) can be used to express compound entity identity. 
    ///  Use <see cref="NamedExpression.CreateNewExpressionWithNamedArguments(System.Linq.Expressions.NewExpression)"/> to create a compound
    ///  expression.
    ///  </remarks>
    SqlEntityDefinitionExpression ResolveSimpleTableInfo (ResolvedSimpleTableInfo tableInfo);

    /// <summary>
    /// Analyzes the given <see cref="MemberInfo"/> and returns an expression representing that member in the database. The resolved version will 
    /// usually be a <see cref="SqlColumnExpression"/> if the member represents a simple column, or a
    /// <see cref="SqlEntityRefMemberExpression"/> if the member references another entity.
    /// </summary>
    /// <param name="originatingEntity">The <see cref="SqlEntityExpression"/> representing the entity the member is accessed on.</param> 
    /// <param name="memberInfo">The <see cref="MemberInfo"/> to be resolved.</param>
    /// <returns>Usually a <see cref="SqlColumnExpression"/> if the member is resolved to a simple column, or a 
    /// <see cref="SqlEntityRefMemberExpression"/>  if the member references another entity.
    /// This method can return a partial result that itself again needs to be resolved.</returns>
    /// <exception cref="UnmappedItemException">The given <see cref="MemberInfo"/> cannot be resolved to a mapped database item.</exception>
    /// <remarks>
    /// Note that compound expressions (<see cref="NewExpression"/> instances with named arguments) can be used to express a compound member. 
    /// Use <see cref="NamedExpression.CreateNewExpressionWithNamedArguments(System.Linq.Expressions.NewExpression)"/> to create a compound
    /// expression.
    /// </remarks>
    Expression ResolveMemberExpression (SqlEntityExpression originatingEntity, MemberInfo memberInfo);

    /// <summary>
    /// Analyzes a <see cref="MemberInfo"/> that is applied to a column and returns an expression representing that member in the database. The 
    /// resolved version will usually be a <see cref="SqlColumnExpression"/> if the member represents a simple column access, but it can also be any
    /// other expression if more complex calculations are needed.
    /// </summary>
    /// <param name="sqlColumnExpression">The <see cref="SqlColumnExpression"/> the member is accessed on.</param>
    /// <param name="memberInfo">The <see cref="MemberInfo"/> which is needed to identify a mapped property.</param>
    /// <returns>Returns a modified Expression which is usually a <see cref="SqlColumnExpression"/>.</returns>
    /// <exception cref="UnmappedItemException">The given <see cref="MemberInfo"/> cannot be resolved.</exception>
    /// <remarks>
    /// Note that compound expressions (<see cref="NewExpression"/> instances with named arguments) can be used to express a compound member. 
    /// Use <see cref="NamedExpression.CreateNewExpressionWithNamedArguments(System.Linq.Expressions.NewExpression)"/> to create a compound
    /// expression.
    /// </remarks>
    Expression ResolveMemberExpression (SqlColumnExpression sqlColumnExpression, MemberInfo memberInfo);

    /// <summary>
    /// Analyses the given <see cref="ConstantExpression"/> and resolves it to a database-compatible expression if necessary. For example, if the 
    /// constant value is another entity, this method should return a <see cref="SqlEntityConstantExpression"/>.
    /// </summary>
    /// <param name="constantExpression">The <see cref="ConstantExpression"/> to be analyzed.</param>
    /// <returns>A resolved version of <paramref name="constantExpression"/>, usually a <see cref="SqlEntityConstantExpression"/>, or the
    /// <paramref name="constantExpression"/> itself.</returns>
    /// <exception cref="UnmappedItemException">The given <see cref="MemberInfo"/> cannot be resolved.</exception>
    /// <remarks>
    /// Note that compound expressions (<see cref="NewExpression"/> instances with named arguments) can be used to express compound entity identity. 
    /// Use <see cref="NamedExpression.CreateNewExpressionWithNamedArguments(System.Linq.Expressions.NewExpression)"/> to create a compound
    /// expression.
    /// </remarks>
    Expression ResolveConstantExpression (ConstantExpression constantExpression);

    /// <summary>
    /// Analyzes the given <see cref="Expression"/> and returns an expression that evaluates to <see langword="true" /> if it is of a desired 
    /// <see cref="Type"/>. This will usually be a comparison of a type identifier column with a constant value.
    /// </summary>
    /// <param name="expression">The <see cref="Expression"/> to be analyzed.</param>
    /// <param name="desiredType">The <see cref="Type"/> to check for.</param>
    /// <returns>An expression representing a type check of the given <paramref name="expression"/>. 
    /// This method can return a partial result that itself again needs to be resolved.</returns>
    /// <exception cref="UnmappedItemException">The given type check cannot be resolved because no database-level check can be constructed for it.</exception>
    Expression ResolveTypeCheck (Expression expression, Type desiredType);

    /// <summary>
    /// Analyzes the given <see cref="SqlEntityRefMemberExpression"/> and returns an expression that represents the optimized identity expression
    /// of the referenced entity if possible. The expression must be equivalent to the identity of the joined entity 
    /// (<see cref="SqlEntityExpression.GetIdentityExpression"/>). The purpose of this method is to avoid creating a join if the identity of the
    /// referenced entity can be inferred from the <see cref="SqlEntityRefMemberExpression.OriginatingEntity"/> (e.g., by analyzing a foreign key).
    /// </summary>
    /// <param name="entityRefMemberExpression">The <see cref="SqlEntityRefMemberExpression"/> representing the referenced entity whose identity
    /// is to be returned.</param>
    /// <returns>An expression equivalent to the identity of the references entity that can be deduced without creating a join, or 
    /// <see langword="null" /> if the identity cannot be deduced without a join.</returns>
    /// <exception cref="UnmappedItemException">The given <see cref="SqlEntityRefMemberExpression"/> cannot be resolved to a mapped database item.
    /// (Implementations can also return <see langword="null" /> instead.)</exception>
    /// <remarks>
    /// Note that compound expressions (<see cref="NewExpression"/> instances with named arguments) can be used to express compound entity identity. 
    /// Use <see cref="NamedExpression.CreateNewExpressionWithNamedArguments(System.Linq.Expressions.NewExpression)"/> to create a compound
    /// expression.
    /// </remarks>
    Expression TryResolveOptimizedIdentity (SqlEntityRefMemberExpression entityRefMemberExpression);

    /// <summary>
    /// Analyzes the given <see cref="SqlEntityRefMemberExpression"/> and <see cref="MemberInfo"/> and returns an expression that represents an
    /// optimized version of the member acces on the referenced entity if possible. The expression must be equivalent to the result of
    /// <see cref="ResolveMemberExpression(Remotion.Linq.SqlBackend.SqlStatementModel.Resolved.SqlEntityExpression,System.Reflection.MemberInfo)"/>
    /// when executed on the resolved result of <paramref name="entityRefMemberExpression"/>. The purpose of this method is to avoid creating a join
    /// if a the member can be inferred from the <see cref="SqlEntityRefMemberExpression.OriginatingEntity"/> (e.g., by analyzing a foreign key).
    /// </summary>
    /// <param name="entityRefMemberExpression">The <see cref="SqlEntityRefMemberExpression"/> representing the referenced entity whose member
    /// is to be returned.</param>
    /// <param name="memberInfo">The <see cref="MemberInfo"/> that is to be resolved.</param>
    /// <returns>An expression equivalent to the result of 
    /// <see cref="ResolveMemberExpression(Remotion.Linq.SqlBackend.SqlStatementModel.Resolved.SqlEntityExpression,System.Reflection.MemberInfo)"/>
    /// deduced without a join, or <see langword="null" /> is the <paramref name="memberInfo"/> cannot be deduced without a join.
    /// </returns>
    /// <exception cref="UnmappedItemException">The given <see cref="SqlEntityRefMemberExpression"/> of <see cref="MemberExpression"/> cannot be 
    /// resolved to a mapped database item.
    /// (Implementations can also return <see langword="null" /> instead.)</exception>
    /// <remarks>
    /// Note that compound expressions (<see cref="NewExpression"/> instances with named arguments) can be used to express a compound member. 
    /// Use <see cref="NamedExpression.CreateNewExpressionWithNamedArguments(System.Linq.Expressions.NewExpression)"/> to create a compound
    /// expression.
    /// </remarks>   
    Expression TryResolveOptimizedMemberExpression (SqlEntityRefMemberExpression entityRefMemberExpression, MemberInfo memberInfo);
  }
}