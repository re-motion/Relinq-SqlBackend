using System;
using System.Collections.Generic;
using System.Text;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Utilities;

namespace Remotion.Data.Linq.SqlGeneration.SqlServer
{
  public class CommandBuilder : ICommandBuilder
  {
    public CommandBuilder (StringBuilder commandText, List<CommandParameter> commandParameters, IDatabaseInfo databaseInfo)
    {
      ArgumentUtility.CheckNotNull ("commandText", commandText);
      ArgumentUtility.CheckNotNull ("commandParameters", commandParameters);
      ArgumentUtility.CheckNotNull ("databaseInfo", databaseInfo);

      CommandText = commandText;
      CommandParameters = commandParameters;
      DatabaseInfo = databaseInfo;
    }

    public StringBuilder CommandText { get; private set; }
    public List<CommandParameter> CommandParameters { get; private set; }
    public IDatabaseInfo DatabaseInfo { get; private set; }
    
    public string GetCommandText()
    {
      return CommandText.ToString();
    }

    public CommandParameter[] GetCommandParameters()
    {
      return CommandParameters.ToArray();
    }

    public void Append (string text)
    {
      CommandText.Append (text);
    }

    public void AppendEvaluation (IEvaluation evaluation)
    {
      SqlServerEvaluationVisitor visitor = new SqlServerEvaluationVisitor (this, DatabaseInfo);
      evaluation.Accept (visitor);
      
      //if (evaluation.GetType() == typeof(Column))
      //  CommandText.Append(SqlServerUtility.GetColumnString ((Column) evaluation));
      //else
      //{
      //  throw new NotSupportedException("The Evaluation of type '" + evaluation.GetType().Name + "' is not supported.");
      //}
    }

    public void AppendSeparatedItems<T> (IEnumerable<T> items, Action<T> appendAction)
    {
      bool first = true;
      foreach (T item in items)
      {
        if (!first)
          Append (", ");
        appendAction (item);
        first = false;
      }
    }

    public void AppendEvaluations (IEnumerable<IEvaluation> evaluations)
    {
      AppendSeparatedItems (evaluations, AppendEvaluation);
    }

    public void AppendConstant (Constant constant)
    {
      if (constant.Value == null)
        CommandText.Append ("NULL");
      else if (constant.Value.Equals (true))
        CommandText.Append ("(1=1)");
      else if (constant.Value.Equals (false))
        CommandText.Append ("(1<>1)");
      else
      {
        CommandParameter parameter = AddParameter (constant.Value);
        CommandText.Append (parameter.Name);
      }
    }

    public CommandParameter AddParameter (object value)
    {
      CommandParameter parameter = new CommandParameter ("@" + (CommandParameters.Count + 1), value);
      CommandParameters.Add (parameter);
      return parameter;
    }
  }
}