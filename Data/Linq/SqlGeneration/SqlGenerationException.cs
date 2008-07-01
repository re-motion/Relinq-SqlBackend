using System;

namespace Remotion.Data.Linq.SqlGeneration
{
  public class SqlGenerationException : Exception
  {
    public SqlGenerationException (string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public SqlGenerationException (string message)
        : base(message)
    {
    }
  }
}