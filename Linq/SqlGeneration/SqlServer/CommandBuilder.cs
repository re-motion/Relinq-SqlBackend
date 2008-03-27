using System;
using System.Collections.Generic;
using System.Text;
using Rubicon.Data.Linq.DataObjectModel;
using Rubicon.Utilities;

namespace Rubicon.Data.Linq.SqlGeneration.SqlServer
{
  public class CommandBuilder : ICommandBuilder
  {
    public CommandBuilder (StringBuilder commandText, List<CommandParameter> commandParameters)
    {
      ArgumentUtility.CheckNotNull ("commandText", commandText);
      ArgumentUtility.CheckNotNull ("commandParameters", commandParameters);

      CommandText = commandText;
      CommandParameters = commandParameters;
    }

    public StringBuilder CommandText { get; private set; }
    public List<CommandParameter> CommandParameters { get; private set; }
    
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

    public void AppendColumn (Column column)
    {
      CommandText.Append (SqlServerUtility.GetColumnString (column));
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

    public void AppendColumns (IEnumerable<Column> columns)
    {
      AppendSeparatedItems (columns, AppendColumn);
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