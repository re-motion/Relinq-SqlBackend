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
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;
using Remotion.Utilities;

namespace Remotion.Linq.SqlBackend.SqlGeneration
{
  /// <summary>
  /// Extends <see cref="SqlGeneratingSelectExpressionVisitor"/> by building an in-memory projection. This should be used for the 
  /// <see cref="SqlStatement.SelectProjection"/> of the outermost <see cref="SqlStatement"/> in a query. For substatements, 
  /// <see cref="SqlGeneratingSelectExpressionVisitor"/> should be used instead.
  /// </summary>
  public class SqlGeneratingOuterSelectExpressionVisitor : SqlGeneratingSelectExpressionVisitor, ISqlConvertedBooleanExpressionVisitor
  {
    private static readonly MethodInfo s_getValueMethod = typeof (IDatabaseResultRow).GetMethod ("GetValue");
    private static readonly MethodInfo s_getEntityMethod = typeof (IDatabaseResultRow).GetMethod ("GetEntity");

    private readonly SetOperationsMode _setOperationsMode;

    public static void GenerateSql (Expression expression, ISqlCommandBuilder commandBuilder, ISqlGenerationStage stage, SetOperationsMode setOperationsMode)
    {
      ArgumentUtility.CheckNotNull (nameof(expression), expression);
      ArgumentUtility.CheckNotNull (nameof(commandBuilder), commandBuilder);
      ArgumentUtility.CheckNotNull (nameof(stage), stage);

      EnsureNoCollectionExpression (expression);

      var visitor = new SqlGeneratingOuterSelectExpressionVisitor (commandBuilder, stage, setOperationsMode);
      visitor.Visit (expression);
    }

    protected SqlGeneratingOuterSelectExpressionVisitor (ISqlCommandBuilder commandBuilder, ISqlGenerationStage stage, SetOperationsMode setOperationsMode)
        : base (commandBuilder, stage)
    {
      _setOperationsMode = setOperationsMode;
    }

    protected int ColumnPosition { get; set; }

    public override Expression VisitNamed (NamedExpression expression)
    {
      ArgumentUtility.CheckNotNull (nameof(expression), expression);

      var result = base.VisitNamed (expression);
      SetInMemoryProjectionForNamedExpression (expression.Type, Expression.Constant (GetNextColumnID (expression.Name ?? NamedExpression.DefaultName)));

      return result;
    }

    public virtual Expression VisitSqlConvertedBoolean (SqlConvertedBooleanExpression expression)
    {
      ArgumentUtility.CheckNotNull (nameof(expression), expression);

      var namedExpression = expression.Expression as NamedExpression;
      if (namedExpression != null)
      {
        // Since ADO.NET returns bit columns as actual boolean values (not as integer values), we need to convert the NamedExpression back to be
        // of type bool/bool? instead of int/int?.
        var conversionToBool = GetBitConversionExpression (
            sourceType: namedExpression.Type, targetType: expression.Type, convertedExpression: namedExpression.Expression);

        var newNamedExpression = new NamedExpression (namedExpression.Name, conversionToBool);
        return Visit (newNamedExpression);
      }
      else
      {
        Visit (expression.Expression);
        return expression;
      }
    }

    public override Expression VisitSqlEntity (SqlEntityExpression expression)
    {
      ArgumentUtility.CheckNotNull (nameof(expression), expression);

      var columnIds = expression.Columns
          .Select (e => GetNextColumnID (GetAliasForColumnOfEntity (e, expression) ?? e.ColumnName))
          .ToArray();

      var result = base.VisitSqlEntity (expression);

      var newInMemoryProjectionBody = Expression.Call (
          CommandBuilder.InMemoryProjectionRowParameter,
          s_getEntityMethod.MakeGenericMethod (expression.Type),
          Expression.Constant (columnIds));
      CommandBuilder.SetInMemoryProjectionBody (newInMemoryProjectionBody);

      return result;
    }

    protected override Expression VisitNew (NewExpression expression)
    {
      ArgumentUtility.CheckNotNull (nameof(expression), expression);

      var projectionExpressions = new List<Expression>();
      CommandBuilder.AppendSeparated (",", expression.Arguments, (cb, expr) => projectionExpressions.Add (VisitArgumentOfLocalEvaluation (expr)));

      // ReSharper disable ConditionIsAlwaysTrueOrFalse
      // ReSharper disable HeuristicUnreachableCode
      if (expression.Members == null)
        CommandBuilder.SetInMemoryProjectionBody (Expression.New (expression.Constructor, projectionExpressions));
      else
        CommandBuilder.SetInMemoryProjectionBody (Expression.New (expression.Constructor, projectionExpressions, expression.Members));
      // ReSharper restore HeuristicUnreachableCode
      // ReSharper restore ConditionIsAlwaysTrueOrFalse

      return expression;
    }

    protected override Expression VisitMethodCall (MethodCallExpression expression)
    {
      ArgumentUtility.CheckNotNull (nameof(expression), expression);

      if (_setOperationsMode == SetOperationsMode.StatementIsSetCombined)
      {
        var message = "In-memory method calls are not supported when a set operation (such as Union or Concat) is used. "
                      + "Rewrite the query to perform the in-memory operation after the set operation has been performed."
                      + Environment.NewLine
                      + "For example, instead of the following query:"
                      + Environment.NewLine
                      + "    SomeOrders.Select (o => SomeMethod (o.ID)).Concat (OtherOrders.Select (o => SomeMethod (o.ID)))"
                      + Environment.NewLine
                      + "Try the following query:"
                      + Environment.NewLine
                      + "    SomeOrders.Select (o => o.ID).Concat (OtherOrders.Select (o => o.ID)).Select (i => SomeMethod (i))";
        throw new NotSupportedException (message);
      }

      var allItems = expression.Object != null ? new[] { expression.Object }.Concat (expression.Arguments) : expression.Arguments;
      var projectionItems = new List<Expression>();
      CommandBuilder.AppendSeparated (",", allItems, (cb, expr) => projectionItems.Add (VisitArgumentOfLocalEvaluation (expr)));

      var instance = expression.Object != null ? projectionItems.First() : null;
      var arguments = expression.Object != null ? projectionItems.Skip (1) : projectionItems;

      var newInMemoryProjection = Expression.Call (instance, expression.Method, arguments);
      CommandBuilder.SetInMemoryProjectionBody (newInMemoryProjection);

      // If there are no projection items, we need to emit a NULL constant for the MethodCallExpression to avoid producing an empty SELECT list
      if (projectionItems.Count == 0)
      {
        Visit (Expression.Constant (null));
        // Don't forget to increment the column position!
        GetNextColumnID ("");
      }

      return expression;
    }

    protected override Expression VisitUnary (UnaryExpression expression)
    {
      var result = base.VisitUnary (expression);

      var oldInMemoryProjectionBody = CommandBuilder.GetInMemoryProjectionBody();
      if (oldInMemoryProjectionBody != null
          && (expression.NodeType == ExpressionType.Convert || expression.NodeType == ExpressionType.ConvertChecked))
      {
        var newInMemoryProjectionBody = Expression.MakeUnary (expression.NodeType, oldInMemoryProjectionBody, expression.Type, expression.Method);
        CommandBuilder.SetInMemoryProjectionBody (newInMemoryProjectionBody);
      }

      return result;
    }

    public override Expression VisitSqlGroupingSelect (SqlGroupingSelectExpression expression)
    {
      throw new NotSupportedException (
          "This SQL generator does not support queries returning groupings that result from a GroupBy operator because SQL is not suited to "
          + "efficiently return "
          + "LINQ groupings. Use 'group into' and either return the items of the groupings by feeding them into an additional from clause, or perform "
          + "an aggregation on the groupings. "
          + Environment.NewLine
          + Environment.NewLine
          + "Eg., instead of: "
          + Environment.NewLine + "'from c in Cooks group c.ID by c.Name', "
          + Environment.NewLine + "write: "
          + Environment.NewLine + "'from c in Cooks group c.ID by c.Name into groupedCooks "
          + Environment.NewLine + " from c in groupedCooks select new { Key = groupedCooks.Key, Item = c }', "
          + Environment.NewLine + "or: "
          + Environment.NewLine + "'from c in Cooks group c.ID by c.Name into groupedCooks "
          + Environment.NewLine + " select new { Key = groupedCooks.Key, Count = groupedCooks.Count() }'.");
    }

    protected ColumnID GetNextColumnID (string columnName)
    {
      return new ColumnID (columnName, ColumnPosition++);
    }

    private Expression VisitArgumentOfLocalEvaluation (Expression argumentExpression)
    {
      try
      {
        var namedExpression = argumentExpression as NamedExpression;
        if (_setOperationsMode == SetOperationsMode.StatementIsNotSetCombined 
            && namedExpression != null
            && namedExpression.Expression is ConstantExpression)
        {
          // Do not emit constants within a local evaluation; instead, emit "NULL", and directly use the constant expression as the in-memory
          // projection.
          // This enables us to use complex, local-only constants within a local expression.
          // However, we can only perform such tricks if there are no set operations. With set operations, constants are often used as discriminators,
          // e.g., Cooks.Select(c => new { Type = "Cook", ID = c.ID }).Union(Kitchens.Select(k => new { Type = "Kitchen", ID = k.ID })).
          // Since there is only _one_ in-memory projection here, we cannot do as much in memory as we wanted.
          Visit (new NamedExpression (namedExpression.Name, Expression.Constant (null)));
          return namedExpression.Expression;
        }
        else
        {
          Visit (argumentExpression);
          var argumentInMemoryProjectionBody = CommandBuilder.GetInMemoryProjectionBody();
          Assertion.DebugAssert (argumentInMemoryProjectionBody != null);

          return argumentInMemoryProjectionBody;
        }
      }
      finally
      {
        CommandBuilder.SetInMemoryProjectionBody (null);
      }
    }

    private void SetInMemoryProjectionForNamedExpression (Type typeOfValue, Expression columnID)
    {
      var newInMemoryProjectionBody = Expression.Call (
          CommandBuilder.InMemoryProjectionRowParameter,
          s_getValueMethod.MakeGenericMethod (typeOfValue),
          columnID);

      CommandBuilder.SetInMemoryProjectionBody (newInMemoryProjectionBody);
    }

    private Expression GetBitConversionExpression (Type sourceType, Type targetType, Expression convertedExpression)
    {
      // When selecting anything but a column, we also need to insert a "CONVERT (BIT, ...)" to convert the value to BIT.
      // For columns, we don't want that, as they already are of type BIT and we want to avoid the noise.

      if (convertedExpression is SqlColumnExpression)
        return Expression.Convert (convertedExpression, targetType, BooleanUtility.GetIntToBoolConversionMethod (sourceType));
      else
        return new SqlConvertExpression (targetType, convertedExpression);
    }
  }
}