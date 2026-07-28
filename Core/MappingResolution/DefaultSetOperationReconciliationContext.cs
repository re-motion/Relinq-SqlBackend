using System;
using System.Collections.Generic;
using System.Linq;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Utilities;

namespace Remotion.Linq.SqlBackend.MappingResolution
{
    public class DefaultSetOperationReconciliationContext : ISetOperationReconciliationContext
    {
        public class Builder
        {
            private readonly List<Column> _columns = new();
            private readonly Dictionary<string, int> _columnIndexLookup = new();

            public IReadOnlyList<Column> Columns => _columns;

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

            public DefaultSetOperationReconciliationContext Build ()
            {
                return new DefaultSetOperationReconciliationContext (_columns.ToArray());
            }
        }

        public class Column
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
                        throw new ArgumentException ("The specified column name does not match up with the entries.", nameof(name));
                    if (type != entry.Column.Type)
                        throw new ArgumentException ("The specified column type does not match up with the entries.", nameof(type));
                }
            }

            public Column AddEntry (ColumnEntry entry)
            {
                return new Column (Name, Type, Entries.Concat (new[] { entry }));
            }
        }

        public class ColumnEntry
        {
            public SqlEntityExpression Entity { get; }

            public SqlColumnExpression Column { get; }

            public ColumnEntry (SqlEntityExpression entity, SqlColumnExpression column)
            {
                ArgumentUtility.CheckNotNull (nameof(entity), entity);
                ArgumentUtility.CheckNotNull (nameof(column), column);
                if (!entity.Columns.Contains (column))
                    throw new ArgumentException ("The specified column does not belong to the specified entity.", nameof(column));

                Entity = entity;
                Column = column;
            }
        }

        public static Builder CreateBuilder ()
        {
            return new Builder();
        }

        private readonly IReadOnlyList<Column> _columns;

        private readonly HashSet<SqlEntityExpression> _entitiesRequiringReconciliation;
        private readonly IReadOnlyDictionary<SqlColumnExpression, int> _columnIndexLookup;

        public DefaultSetOperationReconciliationContext (IEnumerable<Column> columns)
        {
            ArgumentUtility.CheckNotNull (nameof(columns), columns);

            _columns = columns.ToArray();

            var entitiesRequiringReconciliation = new HashSet<SqlEntityExpression>();
            var columnIndexLookup = new Dictionary<SqlColumnExpression, int>();

            var columnIndex = 0;
            foreach (var column in _columns)
            {
                foreach (var columnEntry in column.Entries)
                {
                    entitiesRequiringReconciliation.Add (columnEntry.Entity);

                    if (columnIndexLookup.TryGetValue (columnEntry.Column, out _))
                    {
                        throw new InvalidOperationException (
                                $"The column '{columnEntry.Column}' is used in multiple times in the same reconciliation context.");
                    }

                    columnIndexLookup.Add (columnEntry.Column, columnIndex);
                }

                columnIndex += 1;
            }

            _entitiesRequiringReconciliation = entitiesRequiringReconciliation;
            _columnIndexLookup = columnIndexLookup;
        }

        public bool RequiresReconciliation (SqlEntityExpression entityExpression)
        {
            ArgumentUtility.CheckNotNull (nameof(entityExpression), entityExpression);

            return _entitiesRequiringReconciliation.Contains (entityExpression);
        }

        public SqlColumnExpression[] CreateNullColumnArray (SqlEntityExpression entityExpression)
        {
            ArgumentUtility.CheckNotNull (nameof(entityExpression), entityExpression);

            var result = new SqlColumnExpression[_columns.Count];
            for (var i = 0; i < result.Length; i++)
            {
                var column = _columns[i];
                result[i] = new SqlNullColumnExpression (
                        column.Type,
                        entityExpression.TableAlias,
                        column.Name,
                        false);
            }

            return result;
        }

        public bool TryGetColumnIndex (SqlEntityExpression entityExpression, SqlColumnExpression column, out int columnIndex)
        {
            ArgumentUtility.CheckNotNull (nameof(entityExpression), entityExpression);
            ArgumentUtility.CheckNotNull (nameof(column), column);

            // We don't need entity expression here as we do the lookup straight of the column.
            // The entity is passed to provide context if multi-level unions should be resolved.
            return _columnIndexLookup.TryGetValue (column, out columnIndex);
        }
    }
}