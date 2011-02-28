using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Remotion.Data.Linq.SqlBackend.SqlGeneration
{
  /// <summary>
  /// Used by the classes in the SQL generation stage to build a SQL command. Use <see cref="GetCommand"/> to access the command when the stage
  /// has finished.
  /// </summary>
  public interface ISqlCommandBuilder
  {
    ParameterExpression InMemoryProjectionRowParameter { get; }
    
    CommandParameter CreateParameter (object value);

    void Append (string stringToAppend);
    void AppendSeparated<T> (string separator, IEnumerable<T> values, Action<ISqlCommandBuilder, T> appender);
    void AppendIdentifier (string identifier);
    void AppendStringLiteral (string value);
    void AppendFormat (string stringToAppend, params object[] parameters);
    CommandParameter AppendParameter (object value);

    void SetInMemoryProjectionBody (Expression body);
    Expression GetInMemoryProjectionBody ();
 
    string GetCommandText ();
    CommandParameter[] GetCommandParameters ();
    SqlCommandData GetCommand ();
  }
}