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
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;

namespace Remotion.Linq.SqlBackend.MappingResolution
{
  /// <summary>
  /// <see cref="SetOperationSelectProjectionReconciler"/> aligns the SELECT column lists of the sides of a set operation (e.g. UNION) whenever
  /// they select whole entities of related-but-different CLR types (typically types within the same inheritance hierarchy). Without this,
  /// selecting a base type on one side of a UNION and a derived type (with its own additional mapped columns) on the other side would produce
  /// SELECT lists with different column counts, which is invalid SQL.
  /// </summary>
  /// <remarks>
  /// Only whole-entity projections whose CLR types share a common base type (other than <see cref="object"/>) are reconciled; anything else is
  /// left completely unchanged, on the assumption that a query which doesn't fit this shape either already produces matching column lists or is
  /// an existing, independently-tracked limitation. Columns are matched purely by name, never by their original relative position, since
  /// sibling/derived types are not guaranteed by any particular <see cref="IMappingResolver"/> to declare shared columns in a consistent order.
  ///
  /// The statement that is being resolved (<see cref="SqlStatementBuilder.SelectProjection"/> itself, as opposed to any of its
  /// <see cref="SqlStatementBuilder.SetOperationCombinedStatements"/>) is the one whose projection ultimately gets materialized into an object
  /// once the whole query executes - regardless of which side of the UNION a given result row actually came from. Its entity's own
  /// <see cref="SqlEntityExpression.Columns"/> collection therefore must never be reordered or have foreign columns spliced into it: doing so
  /// would desynchronize the column order from whatever order an <see cref="IMappingResolver"/> implementation independently uses to map columns
  /// back onto CLR members during materialization. Any columns the other side(s) introduce that this statement lacks are instead appended,
  /// strictly after its own real columns, as separate "NULL AS ..." slots that a materializing visitor can render but ignore.
  ///
  /// The other (combined-statement) sides of the set operation are never materialized directly - the final result is always materialized
  /// according to this statement's own projection - so their entities can be freely reordered, decomposed, and padded as needed to line up with
  /// this statement's column order.
  /// </remarks>
  public static class SetOperationSelectProjectionReconciler
  {
    public static void ReconcileIfPossible (SqlStatementBuilder sqlStatementBuilder)
    {
      if (sqlStatementBuilder.SetOperationCombinedStatements.Count == 0)
        return;

      // TODO: we unwrap but we never re-wrap. As such, some of the tree is lost (the unary convert operations) -> this should not happen and instead be done using a visitor
      var primaryEntity = UnwrapEntity (sqlStatementBuilder.SelectProjection);
      if (primaryEntity == null)
        return;

      var applicableSecondaryIndices = new List<int>();
      var secondaryEntities = new SqlEntityExpression[sqlStatementBuilder.SetOperationCombinedStatements.Count];
      for (int i = 0; i < sqlStatementBuilder.SetOperationCombinedStatements.Count; i++)
      {
        var secondaryEntity = UnwrapEntity (sqlStatementBuilder.SetOperationCombinedStatements[i].SqlStatement.SelectProjection);
        if (secondaryEntity == null)
          continue;

        if (!HasCommonBaseTypeOtherThanObject (primaryEntity.Type, secondaryEntity.Type))
          continue;

        secondaryEntities[i] = secondaryEntity;
        applicableSecondaryIndices.Add (i);
      }

      if (applicableSecondaryIndices.Count == 0)
        return;

      var primaryNames = primaryEntity.Columns.Select (c => c.ColumnName).ToList();

      var needsReconciliation = applicableSecondaryIndices
          .Select (i => secondaryEntities[i].Columns.Select (c => c.ColumnName))
          .Any (secondaryNames => !primaryNames.SequenceEqual (secondaryNames));
      if (!needsReconciliation)
        return;

      var masterColumnOrder = BuildMasterColumnOrder (primaryNames, applicableSecondaryIndices, secondaryEntities);

      var allApplicableEntities = new[] { primaryEntity }.Concat (applicableSecondaryIndices.Select (i => secondaryEntities[i])).ToList();
      if (!TryBuildPaddingColumnTypeMap (masterColumnOrder, allApplicableEntities, out var columnNameToType))
        return;

      var primaryPaddingNames = masterColumnOrder.Skip (primaryNames.Count).ToList();
      if (primaryPaddingNames.Count > 0)
      {
        var primaryColumnsByName = primaryEntity.Columns.ToDictionary (c => c.ColumnName);
        var primarySlots = masterColumnOrder
            .Select (name => primaryColumnsByName.TryGetValue (name, out var column)
                ? column
                : new SqlNullColumnExpression (typeof(int?), primaryEntity.TableAlias, name, false));

        sqlStatementBuilder.SelectProjection = primaryEntity.UpdateColumns (primarySlots);
      }

      foreach (var i in applicableSecondaryIndices)
      {
        var secondaryEntity = secondaryEntities[i];
        var secondaryNames = secondaryEntity.Columns.Select (c => c.ColumnName).ToList();
        if (secondaryNames.SequenceEqual (masterColumnOrder))
          continue;

        var secondaryColumnsByName = secondaryEntity.Columns.ToDictionary (c => c.ColumnName);
        var secondarySlots = masterColumnOrder
            .Select (name => secondaryColumnsByName.TryGetValue (name, out var column)
                ? column
                : new SqlNullColumnExpression (typeof(int?), secondaryEntity.TableAlias, name, false));

        var newProjection = secondaryEntity.UpdateColumns (secondarySlots);

        var oldCombinedStatement = sqlStatementBuilder.SetOperationCombinedStatements[i];
        var newInnerStatementBuilder = new SqlStatementBuilder (oldCombinedStatement.SqlStatement) { SelectProjection = newProjection };
        sqlStatementBuilder.SetOperationCombinedStatements[i] = new SetOperationCombinedStatement (
            newInnerStatementBuilder.GetSqlStatement(), oldCombinedStatement.SetOperation);
      }
    }

    private static List<string> BuildMasterColumnOrder (
        List<string> primaryNames, List<int> applicableSecondaryIndices, SqlEntityExpression[] secondaryEntities)
    {
      var masterColumnOrder = new List<string> (primaryNames);
      var seenNames = new HashSet<string> (primaryNames);
      foreach (var i in applicableSecondaryIndices)
      {
        foreach (var name in secondaryEntities[i].Columns.Select (c => c.ColumnName))
        {
          if (seenNames.Add (name))
            masterColumnOrder.Add (name);
        }
      }

      return masterColumnOrder;
    }

    /// <summary>
    /// For every column name that at least one, but not all, of <paramref name="allApplicableEntities"/> declares (i.e., a name for which some
    /// side will need a synthesized "NULL AS ..." placeholder), determines the CLR type to use for that placeholder from whichever side(s)
    /// actually declare the column. If those sides disagree on the type, bails out entirely (returns false, no partial result) rather than
    /// guessing - this is a conservative, rare edge case (e.g. a mapping quirk where a same-named column has different types across sibling
    /// types) that isn't worth risking a wrong padding type for. Names declared by every applicable entity never need a placeholder and are
    /// therefore never type-checked here, even if their declared types happen to differ across sides - that's a pre-existing, unrelated
    /// situation this reconciliation doesn't touch or need to fix.
    /// </summary>
    private static bool TryBuildPaddingColumnTypeMap (
        List<string> masterColumnOrder, List<SqlEntityExpression> allApplicableEntities, out Dictionary<string, Type> columnNameToType)
    {
      columnNameToType = new Dictionary<string, Type>();

      foreach (var name in masterColumnOrder)
      {
        var declaringTypes = allApplicableEntities
            .Select (entity => entity.Columns.FirstOrDefault (c => c.ColumnName == name))
            .Where (column => column != null)
            .Select (column => column.Type)
            .ToList();

        if (declaringTypes.Count == allApplicableEntities.Count)
          continue; // declared by every side - no placeholder is ever needed for this name, so any type disagreement is irrelevant here.

        if (declaringTypes.Distinct().Count() > 1)
        {
          columnNameToType = null;
          return false;
        }

        columnNameToType[name] = declaringTypes[0];
      }

      return true;
    }

    private static Expression CreateNullNamedSlot (string name, Type type)
    {
      var nullableFriendlyType = type.IsValueType && Nullable.GetUnderlyingType (type) == null
          ? typeof (Nullable<>).MakeGenericType (type)
          : type;

      return new NamedExpression (name, Expression.Constant (null, nullableFriendlyType));
    }

    private static SqlEntityExpression UnwrapEntity (Expression expression)
    {
      while (expression is UnaryExpression unaryExpression)
        expression = unaryExpression.Operand;

      if (expression is SqlEntityExpression sqlEntityExpression)
        return sqlEntityExpression;

      return null;
    }

    private static bool HasCommonBaseTypeOtherThanObject (Type first, Type second)
    {
      for (var currentFirst = first; currentFirst != null && currentFirst != typeof (object); currentFirst = currentFirst.BaseType)
      {
        for (var currentSecond = second; currentSecond != null && currentSecond != typeof (object); currentSecond = currentSecond.BaseType)
        {
          if (currentFirst == currentSecond)
            return true;
        }
      }

      return false;
    }
  }
}
