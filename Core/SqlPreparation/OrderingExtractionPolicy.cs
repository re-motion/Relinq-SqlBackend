using Remotion.Linq.SqlBackend.SqlStatementModel;

namespace Remotion.Linq.SqlBackend.SqlPreparation
{
  /// <summary>
  /// Defines how to deal with <see cref="SqlStatement.Orderings"/> when moving a <see cref="SqlStatement"/> into a substatement using
  /// <see cref="FromExpressionInfo"/>.
  /// </summary>
  public enum OrderingExtractionPolicy
  {
    /// <summary>
    /// The ordering expressions should be moved to the projection so that the outer statement can sort by the same criteria as the inner statement.
    /// <see cref="FromExpressionInfo.ExtractedOrderings"/> will contain the extracted orderings, <see cref="FromExpressionInfo.ItemSelector"/>
    /// contains an expression to select the original projection of the inner statement.
    /// </summary>
    ExtractOrderingsIntoProjection,
    /// <summary>
    /// The ordering expressions are not moved to the projection.
    /// <see cref="FromExpressionInfo.ExtractedOrderings"/> will be empty and <see cref="FromExpressionInfo.ItemSelector"/> simply select the 
    /// full projection of the inner statement.
    /// </summary>
    DoNotExtractOrderings
  }
}