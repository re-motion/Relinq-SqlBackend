using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Remotion.Data.Linq.SqlBackend.SqlGeneration
{
  // TODO Review 2977: Missing docs
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
    Expression<Func<IDatabaseResultRow, object>> GetInMemoryProjection ();

    string GetCommandText ();
    CommandParameter[] GetCommandParameters ();
    SqlCommandData GetCommand ();

  }
}