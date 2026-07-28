using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;

namespace Remotion.Linq.SqlBackend.MappingResolution
{
    public interface ISetOperationReconciliationContext
    {
        bool RequiresReconciliation (SqlEntityExpression entityExpression);

        SqlColumnExpression[] CreateNullColumnArray (SqlEntityExpression entityExpression);

        bool TryGetColumnIndex (SqlEntityExpression entityExpression, SqlColumnExpression column, out int columnIndex);
    }
}