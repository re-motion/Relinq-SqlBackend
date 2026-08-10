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
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Utilities;

namespace Remotion.Linq.SqlBackend.MappingResolution
{
  /// <summary>
  /// Implementation of <see cref="ISetOperationReconciliationContext"/> that comes with a builder to create a
  /// reconciled column view based on the name of the columns.
  /// </summary>
  /// <remarks>
  /// Override this type if you want to customize the creation of the reconciled column view.
  /// To ensure the builder works with your new type, create your own <see cref="CreateBuilder"/> method and pass a factory.
  /// </remarks>
  public class DefaultSetOperationReconciliationContext : ISetOperationReconciliationContext
  {
    public sealed class Builder
    {
      private readonly Func<IEnumerable<Column>, ISetOperationReconciliationContext> _factory;

      private readonly List<Column> _columns = new();
      private readonly Dictionary<string, int> _columnIndexLookup = new();

      public IReadOnlyList<Column> Columns => _columns;

      public Builder (Func<IEnumerable<Column>, ISetOperationReconciliationContext> factory)
      {
        ArgumentUtility.CheckNotNull (nameof(factory), factory);

        _factory = factory;
      }

      public void AddSqlColumn (SqlEntityExpression entity, SqlColumnExpression column)
      {
        ArgumentUtility.CheckNotNull (nameof(entity), entity);
        ArgumentUtility.CheckNotNull (nameof(column), column);

        var columnEntry = new ColumnEntry (entity, column);

        if (_columnIndexLookup.TryGetValue (column.ColumnName, out var columnIndex))
        {
          _columns[columnIndex] = _columns[columnIndex].AddEntry (columnEntry);
        }
        else
        {
          _columnIndexLookup.Add (column.ColumnName, _columns.Count);
          _columns.Add (new Column (column.ColumnName, column.Type, new[] { columnEntry }));
        }
      }

      public ISetOperationReconciliationContext Build ()
      {
        return _factory (_columns);
      }
    }

    public sealed class Column
    {
      public string Name { get; }

      public Type Type { get; }

      public IReadOnlyList<ColumnEntry> Entries { get; }

      public Column (string name, Type type, IEnumerable<ColumnEntry> entries)
      {
        ArgumentUtility.CheckNotNull (nameof(name), name);
        ArgumentUtility.CheckNotNull (nameof(type), type);
        ArgumentUtility.CheckNotNull (nameof(entries), entries);

        Name = name;
        Type = type;
        Entries = entries.ToArray();

        foreach (var entry in Entries)
        {
          if (name != entry.Column.ColumnName)
          {
            throw new ArgumentException (
                $"The column name '{name}' does not match up with the name of previous columns '{entry.Column.ColumnName}'.",
                nameof(name));
          }
          if (type != entry.Column.Type)
          {
            throw new ArgumentException (
                $"The column type '{type}' does not match up with the type of previous columns '{entry.Column.Type}'.",
                nameof(type));
          }
        }
      }

      public Column AddEntry (ColumnEntry entry)
      {
        ArgumentUtility.CheckNotNull (nameof(entry), entry);

        return new Column (Name, Type, Entries.Concat (new[] { entry }));
      }
    }

    public sealed class ColumnEntry
    {
      public SqlEntityExpression Entity { get; }

      public SqlColumnExpression Column { get; }

      public ColumnEntry (SqlEntityExpression entity, SqlColumnExpression column)
      {
        ArgumentUtility.CheckNotNull (nameof(entity), entity);
        ArgumentUtility.CheckNotNull (nameof(column), column);
        if (!entity.Columns.Contains (column))
          throw new ArgumentException ($"The column '{column}' does not belong to the entity '{entity.Name}'.", nameof(column));

        Entity = entity;
        Column = column;
      }
    }

    public static Builder CreateBuilder ()
    {
      return new Builder(e => new DefaultSetOperationReconciliationContext (e));
    }

    protected readonly IReadOnlyList<Column> Columns;

    protected readonly HashSet<SqlEntityExpression> EntitiesRequiringReconciliation;
    protected readonly IReadOnlyDictionary<SqlColumnExpression, int> ColumnIndexLookup;

    public DefaultSetOperationReconciliationContext (IEnumerable<Column> columns)
    {
      ArgumentUtility.CheckNotNull (nameof(columns), columns);

      Columns = columns.ToArray();

      var entitiesRequiringReconciliation = new HashSet<SqlEntityExpression>();
      var columnIndexLookup = new Dictionary<SqlColumnExpression, int>();

      var columnIndex = 0;
      foreach (var column in Columns)
      {
        foreach (var columnEntry in column.Entries)
        {
          entitiesRequiringReconciliation.Add (columnEntry.Entity);

          if (columnIndexLookup.ContainsKey (columnEntry.Column))
          {
            throw new InvalidOperationException (
                $"The column '{columnEntry.Column}' is used in multiple times in the same reconciliation context.");
          }

          columnIndexLookup.Add (columnEntry.Column, columnIndex);
        }

        columnIndex += 1;
      }

      EntitiesRequiringReconciliation = entitiesRequiringReconciliation;
      ColumnIndexLookup = columnIndexLookup;
    }

    public virtual bool IsReconciliationRequired (SqlEntityExpression entityExpression)
    {
      ArgumentUtility.CheckNotNull (nameof(entityExpression), entityExpression);

      return EntitiesRequiringReconciliation.Contains (entityExpression);
    }

    public virtual SqlColumnExpression[] GetReconciledColumns (SqlEntityExpression entityExpression)
    {
      ArgumentUtility.CheckNotNull (nameof(entityExpression), entityExpression);

      var result = new SqlColumnExpression[Columns.Count];
      for (var i = 0; i < result.Length; i++)
      {
        var column = Columns[i];
        result[i] = SqlComputedColumnExpression.CreateConstant (
            null,
            column.Type,
            entityExpression.TableAlias,
            column.Name,
            false);
      }

      foreach (var column in entityExpression.Columns)
      {
        if (!ColumnIndexLookup.TryGetValue (column, out var columnIndex))
        {
          throw new InvalidOperationException (
              $"Column '{column}' is not supported in set operation reconciliation context"
              + $" that claims to support the entity '{entityExpression.Name}'.");
        }

        result[columnIndex] = column;
      }

      return result;
    }
  }
}