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
using System.Linq;
using System.Linq.Expressions;
using Remotion.Linq.Parsing;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Utilities;

namespace Remotion.Linq.SqlBackend.MappingResolution
{
  /// <summary>
  /// Creates a reference to the given expression stemming from a <see cref="ResolvedSubStatementTableInfo"/>. References can only be made to 
  /// expressions with names. Expressions without names will be copied (but their child expressions resolved). For example, a 
  /// <see cref="NamedExpression"/> is referenced via a <see cref="SqlColumnExpression"/>; but a <see cref="NewExpression"/> is referenced by an
  /// equivalent <see cref="NewExpression"/> (whose arguments reference the arguments of the original <see cref="NewExpression"/>).
  /// </summary>
  public class SubStatementReferenceResolver : RelinqExpressionVisitor, IResolvedSqlExpressionVisitor, INamedExpressionVisitor, ISqlGroupingSelectExpressionVisitor
  {
    public static Expression ResolveSubStatementReferenceExpression (
        Expression referencedExpression,
        ResolvedSubStatementTableInfo containingSubStatementTableInfo,
        SqlTableBase containingSqlTable,
        IMappingResolutionContext context)
    {
      ArgumentUtility.CheckNotNull (nameof(referencedExpression), referencedExpression);
      ArgumentUtility.CheckNotNull (nameof(containingSubStatementTableInfo), containingSubStatementTableInfo);
      ArgumentUtility.CheckNotNull (nameof(containingSqlTable), containingSqlTable);
      
      var visitor = new SubStatementReferenceResolver (containingSubStatementTableInfo, containingSqlTable, context);
      var result = visitor.Visit (referencedExpression);

      return result;
    }

    private readonly ResolvedSubStatementTableInfo _tableInfo;
    private readonly SqlTableBase _sqlTable;
    private readonly IMappingResolutionContext _context;

    protected SubStatementReferenceResolver (ResolvedSubStatementTableInfo tableInfo, SqlTableBase sqlTable, IMappingResolutionContext context)
    {
      ArgumentUtility.CheckNotNull (nameof(tableInfo), tableInfo);
      ArgumentUtility.CheckNotNull (nameof(sqlTable), sqlTable);
      ArgumentUtility.CheckNotNull (nameof(context), context);

      _tableInfo = tableInfo;
      _sqlTable = sqlTable;
      _context = context;
    }
    
    public Expression VisitSqlEntity (SqlEntityExpression expression)
    {
      ArgumentUtility.CheckNotNull (nameof(expression), expression);

      var reference = expression.CreateReference (_tableInfo.TableAlias, expression.Type);
      _context.AddSqlEntityMapping (reference, _sqlTable);
      return reference;
    }

    public Expression VisitNamed (NamedExpression expression)
    {
      ArgumentUtility.CheckNotNull (nameof(expression), expression);

      return new SqlColumnDefinitionExpression (
          expression.Type,
          _tableInfo.TableAlias,
          expression.Name ?? NamedExpression.DefaultName,
          false);
    }

    // NewExpressions are referenced by creating a new NewExpression holding references to the original arguments. We need to explicitly name each 
    // argument reference, otherwise all of them would be called "value"...
    protected override Expression VisitNew (NewExpression expression)
    {
      ArgumentUtility.CheckNotNull (nameof(expression), expression);

      var resolvedArguments = expression.Arguments.Select (expr => ResolveChildExpression (expr));
      return NamedExpression.CreateNewExpressionWithNamedArguments (expression, resolvedArguments);
    }

    public Expression VisitSqlGroupingSelect (SqlGroupingSelectExpression expression)
    {
      ArgumentUtility.CheckNotNull (nameof(expression), expression);

      var referenceToKeyExpression = ResolveChildExpression (expression.KeyExpression);
      var referenceToElementExpression = ResolveChildExpression (expression.ElementExpression);
      var referenceToAggregationExpressions = expression.AggregationExpressions.Select (expr => ResolveChildExpression (expr));

      var newGroupingExpression = SqlGroupingSelectExpression.CreateWithNames (referenceToKeyExpression, referenceToElementExpression);
      foreach (var aggregationExpression in referenceToAggregationExpressions)
        newGroupingExpression.AddAggregationExpressionWithName (aggregationExpression);

      _context.AddGroupReferenceMapping (newGroupingExpression, _sqlTable);

      return newGroupingExpression;
    }

    private Expression ResolveChildExpression (Expression childExpression)
    {
      return ResolveSubStatementReferenceExpression (childExpression, _tableInfo, _sqlTable, _context);
    }

    Expression IResolvedSqlExpressionVisitor.VisitSqlColumn (SqlColumnExpression expression)
    {
      throw new InvalidOperationException ("SqlColumnExpression is not valid at this point. (Must be wrapped within a NamedExpression.)");
    }

    Expression IResolvedSqlExpressionVisitor.VisitSqlEntityConstant (SqlEntityConstantExpression expression)
    {
      throw new InvalidOperationException ("SqlEntityConstantExpression is not valid at this point. (Must be wrapped within a NamedExpression.)");
    }
  }
}