using System;
using System.Collections.Generic;
using Rubicon.Data.Linq.DataObjectModel;
using Rubicon.Data.Linq.SqlGeneration;

namespace Rubicon.Data.Linq.SqlGeneration.SqlServer
{
  public interface ICommandBuilder
  {
    string GetCommandText();
    CommandParameter[] GetCommandParameters();
    void Append (string text);
    void AppendColumn (Column column);
    void AppendSeparatedItems<T> (IEnumerable<T> items, Action<T> appendAction);
    void AppendColumns (IEnumerable<Column> columns);
    void AppendConstant (Constant constant);
    CommandParameter AddParameter (object value);
  }
}