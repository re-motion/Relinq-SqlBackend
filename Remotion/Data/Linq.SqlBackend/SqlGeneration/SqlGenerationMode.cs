namespace Remotion.Data.Linq.SqlBackend.SqlGeneration
{
  /// <summary>
  /// Defines the mode for <see cref="SqlGeneratingExpressionVisitor"/>.
  /// </summary>
  public enum SqlGenerationMode
  {
    /// <summary>
    /// Generate SQL for a SELECT expression. The generator will append "AS ..." aliases if necessary.
    /// </summary>
    SelectExpression,
    /// <summary>
    /// Generate SQL for a non-SELECT expression. The generator will not append "AS ..." aliases.
    /// </summary>
    NonSelectExpression
  }
}