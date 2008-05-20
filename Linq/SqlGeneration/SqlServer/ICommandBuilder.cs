using System;
using System.Collections.Generic;
using System.Text;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.SqlGeneration;

namespace Remotion.Data.Linq.SqlGeneration.SqlServer
{
  public interface ICommandBuilder
  {
    string GetCommandText();
    CommandParameter[] GetCommandParameters();
    void Append (string text);
    void AppendEvaluation (IEvaluation evaluation);
    void AppendSeparatedItems<T> (IEnumerable<T> items, Action<T> appendAction);
    void AppendEvaluations (IEnumerable<IEvaluation> evaluations);
    void AppendConstant (Constant constant);
    CommandParameter AddParameter (object value);
    StringBuilder CommandText { get; }
    List<CommandParameter> CommandParameters { get; }
  }
}