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
using NUnit.Framework;
using Remotion.Linq.SqlBackend.MappingResolution;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Linq.SqlBackend.UnitTests.SqlStatementModel;
using Remotion.Linq.SqlBackend.UnitTests.TestDomain;

namespace Remotion.Linq.SqlBackend.UnitTests.MappingResolution
{
  [TestFixture]
  public class SetOperationSelectProjectionReconcilerTest
  {
    [Test]
    public void ReconcileIfPossible_NoSetOperations_IsANoOp ()
    {
      var primaryEntity = CreateEntity (typeof (Cook), "t0", ("ID", typeof (int), true));
      var builder = CreateBuilder (primaryEntity);

      SetOperationSelectProjectionReconciler.ReconcileIfPossible (builder);

      Assert.That (builder.SelectProjection, Is.SameAs (primaryEntity));
    }

    [Test]
    public void ReconcileIfPossible_IdenticalColumnLists_IsANoOp ()
    {
      var primaryEntity = CreateEntity (typeof (Cook), "t0", ("ID", typeof (int), true), ("Name", typeof (string), false));
      var secondaryEntity = CreateEntity (typeof (Chef), "t1", ("ID", typeof (int), true), ("Name", typeof (string), false));

      var builder = CreateBuilder (primaryEntity);
      var secondaryStatement = SqlStatementModelObjectMother.CreateSqlStatement (secondaryEntity);
      builder.SetOperationCombinedStatements.Add (new SetOperationCombinedStatement (secondaryStatement, SetOperation.Union));

      SetOperationSelectProjectionReconciler.ReconcileIfPossible (builder);

      Assert.That (builder.SelectProjection, Is.SameAs (primaryEntity));
      Assert.That (builder.SetOperationCombinedStatements.Single().SqlStatement, Is.SameAs (secondaryStatement));
    }

    [Test]
    public void ReconcileIfPossible_SecondaryHasExtraColumn_PadsPrimaryWithTrailingNull_LeavesSecondaryUnchanged ()
    {
      var primaryEntity = CreateEntity (typeof (Cook), "t0", ("ID", typeof (int), true), ("Name", typeof (string), false));
      var secondaryEntity = CreateEntity (
          typeof (Chef), "t1", ("ID", typeof (int), true), ("Name", typeof (string), false), ("LetterOfRecommendation", typeof (string), false));

      var builder = CreateBuilder (primaryEntity);
      var secondaryStatement = SqlStatementModelObjectMother.CreateSqlStatement (secondaryEntity);
      builder.SetOperationCombinedStatements.Add (new SetOperationCombinedStatement (secondaryStatement, SetOperation.Union));

      SetOperationSelectProjectionReconciler.ReconcileIfPossible (builder);

      var paddedPrimary = builder.SelectProjection as SqlSetOperationPaddedProjectionExpression;
      Assert.That (paddedPrimary, Is.Not.Null);
      Assert.That (paddedPrimary.PreservedEntitySlotIndex, Is.EqualTo (0));
      // 1 slot for the whole (untouched) entity plus 1 trailing padding slot for the one missing column - the entity's own columns are never
      // split into separate slots.
      Assert.That (paddedPrimary.Slots.Count, Is.EqualTo (2));
      Assert.That (paddedPrimary.Slots[0], Is.SameAs (primaryEntity));

      var paddingSlot = (NamedExpression) paddedPrimary.Slots[1];
      Assert.That (paddingSlot.Name, Is.EqualTo ("LetterOfRecommendation"));
      var paddingConstant = (ConstantExpression) paddingSlot.Expression;
      Assert.That (paddingConstant.Value, Is.Null);
      Assert.That (paddingConstant.Type, Is.EqualTo (typeof (string)));

      // The secondary's own column order already extends the primary's exactly, so it needs no changes at all.
      Assert.That (builder.SetOperationCombinedStatements.Single().SqlStatement, Is.SameAs (secondaryStatement));
    }

    [Test]
    public void ReconcileIfPossible_MissingNonNullableValueTypeColumn_UsesNullableTypeForPlaceholder ()
    {
      var primaryEntity = CreateEntity (typeof (Cook), "t0", ("ID", typeof (int), true));
      var secondaryEntity = CreateEntity (typeof (Chef), "t1", ("ID", typeof (int), true), ("IsActive", typeof (bool), false));

      var builder = CreateBuilder (primaryEntity);
      var secondaryStatement = SqlStatementModelObjectMother.CreateSqlStatement (secondaryEntity);
      builder.SetOperationCombinedStatements.Add (new SetOperationCombinedStatement (secondaryStatement, SetOperation.Union));

      Assert.That (
          () => SetOperationSelectProjectionReconciler.ReconcileIfPossible (builder),
          Throws.Nothing);

      var paddedPrimary = (SqlSetOperationPaddedProjectionExpression) builder.SelectProjection;
      var paddingSlot = (NamedExpression) paddedPrimary.Slots[1];
      var paddingConstant = (ConstantExpression) paddingSlot.Expression;
      Assert.That (paddingConstant.Type, Is.EqualTo (typeof (bool?)));
      Assert.That (paddingConstant.Value, Is.Null);
    }

    [Test]
    public void ReconcileIfPossible_ColumnsDeclaredInDifferentPositions_ReordersBothSidesByName ()
    {
      // primary has its own extra column (KnifeID) that the secondary lacks; the secondary has its own extra column (Rating) that the primary
      // lacks. Neither side's own columns are simply a prefix/suffix of the other's, so both sides need reconciling, matching by name only.
      var primaryEntity = CreateEntity (
          typeof (Cook), "t0", ("ID", typeof (int), true), ("Name", typeof (string), false), ("KnifeID", typeof (int), false));
      var secondaryEntity = CreateEntity (
          typeof (Cook), "t1", ("ID", typeof (int), true), ("Name", typeof (string), false), ("Rating", typeof (int), false));

      var builder = CreateBuilder (primaryEntity);
      var secondaryStatement = SqlStatementModelObjectMother.CreateSqlStatement (secondaryEntity);
      builder.SetOperationCombinedStatements.Add (new SetOperationCombinedStatement (secondaryStatement, SetOperation.Union));

      SetOperationSelectProjectionReconciler.ReconcileIfPossible (builder);

      var paddedPrimary = (SqlSetOperationPaddedProjectionExpression) builder.SelectProjection;
      Assert.That (paddedPrimary.Slots.Count, Is.EqualTo (2));
      Assert.That (paddedPrimary.Slots[0], Is.SameAs (primaryEntity));
      Assert.That (((NamedExpression) paddedPrimary.Slots[1]).Name, Is.EqualTo ("Rating"));
      Assert.That (((ConstantExpression) ((NamedExpression) paddedPrimary.Slots[1]).Expression).Value, Is.Null);

      var newSecondaryStatement = builder.SetOperationCombinedStatements.Single().SqlStatement;
      Assert.That (newSecondaryStatement, Is.Not.SameAs (secondaryStatement));
      var paddedSecondary = (SqlSetOperationPaddedProjectionExpression) newSecondaryStatement.SelectProjection;
      Assert.That (paddedSecondary.Slots.Count, Is.EqualTo (4));
      Assert.That (((NamedExpression) paddedSecondary.Slots[0]).Name, Is.EqualTo ("ID"));
      Assert.That (((NamedExpression) paddedSecondary.Slots[1]).Name, Is.EqualTo ("Name"));

      var reorderedKnifeIdSlot = (NamedExpression) paddedSecondary.Slots[2];
      Assert.That (reorderedKnifeIdSlot.Name, Is.EqualTo ("KnifeID"));
      Assert.That (((ConstantExpression) reorderedKnifeIdSlot.Expression).Value, Is.Null);

      var reorderedRatingSlot = (NamedExpression) paddedSecondary.Slots[3];
      Assert.That (reorderedRatingSlot.Name, Is.EqualTo ("Rating"));
      Assert.That (reorderedRatingSlot.Expression, Is.SameAs (secondaryEntity.Columns.Single (c => c.ColumnName == "Rating")));
    }

    [Test]
    public void ReconcileIfPossible_NoCommonBaseType_IsANoOp ()
    {
      var primaryEntity = CreateEntity (typeof (Cook), "t0", ("ID", typeof (int), true));
      var secondaryEntity = CreateEntity (typeof (Kitchen), "t1", ("ID", typeof (int), true), ("Name", typeof (string), false));

      var builder = CreateBuilder (primaryEntity);
      var secondaryStatement = SqlStatementModelObjectMother.CreateSqlStatement (secondaryEntity);
      builder.SetOperationCombinedStatements.Add (new SetOperationCombinedStatement (secondaryStatement, SetOperation.Union));

      SetOperationSelectProjectionReconciler.ReconcileIfPossible (builder);

      Assert.That (builder.SelectProjection, Is.SameAs (primaryEntity));
      Assert.That (builder.SetOperationCombinedStatements.Single().SqlStatement, Is.SameAs (secondaryStatement));
    }

    [Test]
    public void ReconcileIfPossible_ProjectionIsNotAWholeEntity_IsANoOp ()
    {
      var primaryProjection = Expression.Constant (0);
      var secondaryEntity = CreateEntity (typeof (Cook), "t1", ("ID", typeof (int), true), ("Name", typeof (string), false));

      var builder = CreateBuilder (primaryProjection);
      var secondaryStatement = SqlStatementModelObjectMother.CreateSqlStatement (secondaryEntity);
      builder.SetOperationCombinedStatements.Add (new SetOperationCombinedStatement (secondaryStatement, SetOperation.Union));

      SetOperationSelectProjectionReconciler.ReconcileIfPossible (builder);

      Assert.That (builder.SelectProjection, Is.SameAs (primaryProjection));
      Assert.That (builder.SetOperationCombinedStatements.Single().SqlStatement, Is.SameAs (secondaryStatement));
    }

    [Test]
    public void ReconcileIfPossible_AmbiguousTypeForAMissingColumn_BailsOutCompletely ()
    {
      // "Z" is missing on the primary (so some side would need a NULL placeholder for it), but the two secondaries that do declare "Z"
      // disagree on its type - there is no single correct type to synthesize a placeholder with, so nothing should be changed at all.
      var primaryEntity = CreateEntity (typeof (Cook), "t0", ("ID", typeof (int), true));
      var secondaryEntity1 = CreateEntity (typeof (Cook), "t1", ("ID", typeof (int), true), ("Z", typeof (string), false));
      var secondaryEntity2 = CreateEntity (typeof (Cook), "t2", ("ID", typeof (int), true), ("Z", typeof (int), false));

      var builder = CreateBuilder (primaryEntity);
      var secondaryStatement1 = SqlStatementModelObjectMother.CreateSqlStatement (secondaryEntity1);
      var secondaryStatement2 = SqlStatementModelObjectMother.CreateSqlStatement (secondaryEntity2);
      builder.SetOperationCombinedStatements.Add (new SetOperationCombinedStatement (secondaryStatement1, SetOperation.Union));
      builder.SetOperationCombinedStatements.Add (new SetOperationCombinedStatement (secondaryStatement2, SetOperation.Union));

      SetOperationSelectProjectionReconciler.ReconcileIfPossible (builder);

      Assert.That (builder.SelectProjection, Is.SameAs (primaryEntity));
      Assert.That (builder.SetOperationCombinedStatements[0].SqlStatement, Is.SameAs (secondaryStatement1));
      Assert.That (builder.SetOperationCombinedStatements[1].SqlStatement, Is.SameAs (secondaryStatement2));
    }

    [Test]
    public void ReconcileIfPossible_SameColumnPresentOnBothSidesWithDifferentTypes_IsIgnored ()
    {
      // "KnifeClassID"-style pre-existing quirk: a column present on every applicable side never needs a placeholder, so a type mismatch for it
      // must not block reconciliation of the columns that actually do need one.
      var primaryEntity = CreateEntity (
          typeof (Cook), "t0", ("ID", typeof (int), true), ("Mismatched", typeof (string), false));
      var secondaryEntity = CreateEntity (
          typeof (Chef),
          "t1",
          ("ID", typeof (int), true),
          ("Mismatched", typeof (int), false),
          ("LetterOfRecommendation", typeof (string), false));

      var builder = CreateBuilder (primaryEntity);
      var secondaryStatement = SqlStatementModelObjectMother.CreateSqlStatement (secondaryEntity);
      builder.SetOperationCombinedStatements.Add (new SetOperationCombinedStatement (secondaryStatement, SetOperation.Union));

      SetOperationSelectProjectionReconciler.ReconcileIfPossible (builder);

      var paddedPrimary = (SqlSetOperationPaddedProjectionExpression) builder.SelectProjection;
      Assert.That (paddedPrimary.Slots.Count, Is.EqualTo (2));
      Assert.That (paddedPrimary.Slots[0], Is.SameAs (primaryEntity));
      Assert.That (((NamedExpression) paddedPrimary.Slots[1]).Name, Is.EqualTo ("LetterOfRecommendation"));
    }

    private static SqlEntityDefinitionExpression CreateEntity (Type entityType, string tableAlias, params (string Name, Type Type, bool IsPrimaryKey)[] columns)
    {
      return new SqlEntityDefinitionExpression (
          entityType,
          tableAlias,
          null,
          e => e.GetColumn (typeof (int), "ID", true),
          columns.Select (c => (SqlColumnExpression) new SqlColumnDefinitionExpression (c.Type, tableAlias, c.Name, c.IsPrimaryKey)).ToArray());
    }

    private static SqlStatementBuilder CreateBuilder (Expression primaryProjection)
    {
      return new SqlStatementBuilder
             {
                 SelectProjection = primaryProjection,
                 DataInfo = SqlStatementModelObjectMother.CreateSqlStatement (primaryProjection).DataInfo
             };
    }
  }
}
