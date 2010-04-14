using System;
using System.Collections.Generic;

namespace Remotion.Data.Linq.SqlBackend.SqlGeneration
{
  public interface ISqlCommandBuilder
  {
    CommandParameter CreateParameter (object value);
    void Append (string stringToAppend);
    void AppendSeparated<T> (string separator, IEnumerable<T> values, Action<ISqlCommandBuilder, T> appender);
    void AppendIdentifier (string identifier);
    void AppendStringLiteral (string value);
    void AppendFormat (string stringToAppend, params object[] parameters);
    CommandParameter AppendParameter (object value);
    string GetCommandText ();
    CommandParameter[] GetCommandParameters ();
    SqlCommandData GetCommand ();
  }
}