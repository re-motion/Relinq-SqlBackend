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
using NUnit.Framework;
using Remotion.Linq.SqlBackend.MappingResolution;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Linq.SqlBackend.UnitTests.NUnit;
using Remotion.Linq.SqlBackend.UnitTests.SqlStatementModel;
using Remotion.Linq.SqlBackend.UnitTests.TestDomain;

namespace Remotion.Linq.SqlBackend.UnitTests.MappingResolution
{
  [TestFixture]
  public class DefaultSetOperationReconciliationContextTest
  {
    private SqlEntityDefinitionExpression _baseEntity;
    private SqlEntityDefinitionExpression _subEntity;

    private SqlColumnExpression _baseIdColumn;
    private SqlColumnExpression _baseNameColumn;
    private SqlColumnExpression _baseAgeColumn;

    private SqlColumnExpression _subIdColumn;
    private SqlColumnExpression _subNameColumn;
    private SqlColumnExpression _subAgeColumn;
    private SqlColumnExpression _subLastEducationColumn;

    [SetUp]
    public void SetUp ()
    {
      _baseEntity = SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression (
          typeof (Cook),
          name: "cook",
          owningTableAlias: "c0",
          dataColumns: new[]
                       {
                           new SqlColumnDefinitionExpression (typeof (string), "c0", "Name", false),
                           new SqlColumnDefinitionExpression (typeof (int), "c0", "Age", false)
                       });

      _subEntity = SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression (
          typeof (Chef),
          name: "chef",
          owningTableAlias: "c1",
          dataColumns: new[]
                       {
                           new SqlColumnDefinitionExpression (typeof (string), "c1", "Name", false),
                           new SqlColumnDefinitionExpression (typeof (int), "c1", "Age", false),
                           new SqlColumnDefinitionExpression (typeof (DateTime), "c1", "LastEducation", false)
                       });

      _baseIdColumn = _baseEntity.Columns[0];
      _baseNameColumn = _baseEntity.Columns[1];
      _baseAgeColumn = _baseEntity.Columns[2];

      _subIdColumn = _subEntity.Columns[0];
      _subNameColumn = _subEntity.Columns[1];
      _subAgeColumn = _subEntity.Columns[2];
      _subLastEducationColumn = _subEntity.Columns[3];
    }

    [Test]
    public void Builder_AddSqlColumn_GroupsEntriesByColumnName ()
    {
      var builder = CreateBuilderForInheritanceScenario();

      Assert.That (builder.Columns.Count, Is.EqualTo (4));

      Assert.That (builder.Columns[0].Name, Is.EqualTo ("ID"));
      Assert.That (builder.Columns[0].Entries.Count, Is.EqualTo (2));
      Assert.That (builder.Columns[0].Entries[0].Entity, Is.SameAs (_baseEntity));
      Assert.That (builder.Columns[0].Entries[0].Column, Is.SameAs (_baseIdColumn));
      Assert.That (builder.Columns[0].Entries[1].Entity, Is.SameAs (_subEntity));
      Assert.That (builder.Columns[0].Entries[1].Column, Is.SameAs (_subIdColumn));

      Assert.That (builder.Columns[1].Name, Is.EqualTo ("Name"));
      Assert.That (builder.Columns[1].Entries.Count, Is.EqualTo (2));

      Assert.That (builder.Columns[2].Name, Is.EqualTo ("Age"));
      Assert.That (builder.Columns[2].Entries.Count, Is.EqualTo (2));

      Assert.That (builder.Columns[3].Name, Is.EqualTo ("LastEducation"));
      Assert.That (builder.Columns[3].Entries.Count, Is.EqualTo (1));
      Assert.That (builder.Columns[3].Entries[0].Entity, Is.SameAs (_subEntity));
      Assert.That (builder.Columns[3].Entries[0].Column, Is.SameAs (_subLastEducationColumn));
    }

    [Test]
    public void Builder_Build_CreatesWorkingContext ()
    {
      var builder = DefaultSetOperationReconciliationContext.CreateBuilder();
      builder.AddSqlColumn (_baseEntity, _baseIdColumn);

      var result = builder.Build();

      Assert.That (result.IsReconciliationRequired (_baseEntity), Is.True);
    }

    [Test]
    public void Column_Constructor_EntryWithNonMatchingColumnName_ThrowsArgumentException ()
    {
      var columnEntry = new DefaultSetOperationReconciliationContext.ColumnEntry (_baseEntity, _baseNameColumn);

      Assert.That (
          () => new DefaultSetOperationReconciliationContext.Column ("SomeOtherName", _baseNameColumn.Type, new[] { columnEntry }),
          Throws.ArgumentException.With.ArgumentExceptionMessageEqualTo (
              "The column name 'SomeOtherName' does not match up with the name of previous columns 'Name'.",
              "name"));
    }

    [Test]
    public void Column_Constructor_EntryWithNonMatchingType_ThrowsArgumentException ()
    {
      var columnEntry = new DefaultSetOperationReconciliationContext.ColumnEntry (_baseEntity, _baseNameColumn);

      Assert.That (
          () => new DefaultSetOperationReconciliationContext.Column (_baseNameColumn.ColumnName, typeof (Chef), new[] { columnEntry }),
          Throws.ArgumentException.With.ArgumentExceptionMessageEqualTo (
              "The column type 'Remotion.Linq.SqlBackend.UnitTests.TestDomain.Chef' does not match up"
              + " with the type of previous columns 'System.String'.",
              "type"));
    }

    [Test]
    public void Column_AddEntry_ReturnsNewColumnWithAppendedEntry_AndLeavesOriginalUnchanged ()
    {
      var baseEntry = new DefaultSetOperationReconciliationContext.ColumnEntry (_baseEntity, _baseNameColumn);
      var subEntry = new DefaultSetOperationReconciliationContext.ColumnEntry (_subEntity, _subNameColumn);
      var column = new DefaultSetOperationReconciliationContext.Column (
          _baseNameColumn.ColumnName,
          _baseNameColumn.Type,
          new[] { baseEntry });

      var result = column.AddEntry (subEntry);

      Assert.That (column.Entries, Is.EqualTo (new[] { baseEntry }));
      Assert.That (result.Entries, Is.EqualTo (new[] { baseEntry, subEntry }));
    }

    [Test]
    public void ColumnEntry_Constructor_ColumnNotBelongingToEntity_ThrowsArgumentException ()
    {
      Assert.That (
          () => new DefaultSetOperationReconciliationContext.ColumnEntry (_baseEntity, _subLastEducationColumn),
          Throws.ArgumentException.With.ArgumentExceptionMessageEqualTo (
              "The column '[c1].[LastEducation]' does not belong to the entity 'cook'.",
              "column"));
    }

    [Test]
    public void Constructor_SameColumnUsedInMultipleEntries_ThrowsInvalidOperationException ()
    {
      var columns = new[]
                    {
                        new DefaultSetOperationReconciliationContext.Column (
                            _baseNameColumn.ColumnName,
                            _baseNameColumn.Type,
                            new[] { new DefaultSetOperationReconciliationContext.ColumnEntry (_baseEntity, _baseNameColumn) }),
                        new DefaultSetOperationReconciliationContext.Column (
                            _baseNameColumn.ColumnName,
                            _baseNameColumn.Type,
                            new[] { new DefaultSetOperationReconciliationContext.ColumnEntry (_baseEntity, _baseNameColumn) })
                    };

      Assert.That (
          () => new DefaultSetOperationReconciliationContext (columns),
          Throws.InvalidOperationException.With.Message.EqualTo (
              $"The column '{_baseNameColumn}' is used in multiple times in the same reconciliation context."));
    }

    [Test]
    public void IsReconciliationRequired_EntityContributingToReconciledColumns_ReturnsTrue ()
    {
      var context = CreateBuilderForInheritanceScenario().Build();

      Assert.That (context.IsReconciliationRequired (_baseEntity), Is.True);
      Assert.That (context.IsReconciliationRequired (_subEntity), Is.True);
    }

    [Test]
    public void IsReconciliationRequired_EntityNotPartOfTheSetOperation_ReturnsFalse ()
    {
      var unrelatedEntity = SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression (typeof (Cook), owningTableAlias: "c2");

      var context = CreateBuilderForInheritanceScenario().Build();

      Assert.That (context.IsReconciliationRequired (unrelatedEntity), Is.False);
    }

    [Test]
    public void GetReconciledColumns_ReturnsOneNullColumnPerReconciledColumn_UsingEntityTableAlias ()
    {
      var context = CreateBuilderForInheritanceScenario().Build();

      var result = context.GetReconciledColumns (_baseEntity);

      Assert.That (result.Length, Is.EqualTo (4));

      Assert.That (result[0], Is.EqualTo(_baseIdColumn));
      Assert.That (result[1], Is.EqualTo(_baseNameColumn));
      Assert.That (result[2], Is.EqualTo(_baseAgeColumn));
      AssertIsNullColumn (result[3], typeof (DateTime), "LastEducation", _baseEntity.TableAlias);
    }

    private DefaultSetOperationReconciliationContext.Builder CreateBuilderForInheritanceScenario ()
    {
      var builder = DefaultSetOperationReconciliationContext.CreateBuilder();

      builder.AddSqlColumn (_baseEntity, _baseIdColumn);
      builder.AddSqlColumn (_subEntity, _subIdColumn);
      builder.AddSqlColumn (_baseEntity, _baseNameColumn);
      builder.AddSqlColumn (_subEntity, _subNameColumn);
      builder.AddSqlColumn (_baseEntity, _baseAgeColumn);
      builder.AddSqlColumn (_subEntity, _subAgeColumn);
      builder.AddSqlColumn (_subEntity, _subLastEducationColumn);

      return builder;
    }

    private static void AssertIsNullColumn (SqlColumnExpression column, Type type, string columnName, string owningTableAlias)
    {
      Assert.That (column, Is.TypeOf<SqlComputedColumnExpression>());
      Assert.That (column.Type, Is.EqualTo (type));
      Assert.That (column.ColumnName, Is.EqualTo (columnName));
      Assert.That (column.OwningTableAlias, Is.EqualTo (owningTableAlias));
      Assert.That (column.IsPrimaryKey, Is.False);
    }
  }
}
