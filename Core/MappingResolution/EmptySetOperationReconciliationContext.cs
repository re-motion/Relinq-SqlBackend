using System;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Utilities;

namespace Remotion.Linq.SqlBackend.MappingResolution
{
    public class EmptySetOperationReconciliationContext : ISetOperationReconciliationContext
    {
        public static readonly EmptySetOperationReconciliationContext Instance = new();

        private EmptySetOperationReconciliationContext ()
        {
        }

        public bool RequiresReconciliation (SqlEntityExpression entityExpression)
        {
            ArgumentUtility.CheckNotNull (nameof(entityExpression), entityExpression);

            return false;
        }

        public SqlColumnExpression[] CreateNullColumnArray (SqlEntityExpression entityExpression)
        {
            ArgumentUtility.CheckNotNull (nameof(entityExpression), entityExpression);

            return Array.Empty<SqlColumnExpression>();
        }

        public bool TryGetColumnIndex (SqlEntityExpression entityExpression, SqlColumnExpression column, out int columnIndex)
        {
            ArgumentUtility.CheckNotNull (nameof(entityExpression), entityExpression);
            ArgumentUtility.CheckNotNull (nameof(column), column);

            columnIndex = 0;
            return false;
        }
    }
}