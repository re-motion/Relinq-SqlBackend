using System.Collections.Generic;
using System.Text;
using Rubicon.Data.Linq.DataObjectModel;
using Rubicon.Utilities;

namespace Rubicon.Data.Linq.SqlGeneration.SqlServer
{
  public class CommandBuilder
  {
    private readonly StringBuilder _commandText;
    private readonly List<CommandParameter> _commandParameters;

    public CommandBuilder (StringBuilder commandText, List<CommandParameter> commandParameters)
    {
      ArgumentUtility.CheckNotNull ("commandText", commandText);
      ArgumentUtility.CheckNotNull ("commandParameters", commandParameters);

      _commandText = commandText;
      _commandParameters = commandParameters;
    }

    public string GetCommandText()
    {
      return _commandText.ToString();
    }

    public CommandParameter[] GetCommandParameters()
    {
      return _commandParameters.ToArray();
    }

    public void Append (string text)
    {
      _commandText.Append (text);
    }

    public void AppendColumn (Column column)
    {
      _commandText.Append (SqlServerUtility.GetColumnString (column));
    }

    public void AppendConstant (Constant constant)
    {
      if (constant.Value == null)
        _commandText.Append ("NULL");
      else if (constant.Value.Equals (true))
        _commandText.Append ("(1=1)");
      else if (constant.Value.Equals (false))
        _commandText.Append ("(1<>1)");
      else
      {
        CommandParameter parameter = AddParameter (constant.Value);
        _commandText.Append (parameter.Name);
      }
    }

    public CommandParameter AddParameter (object value)
    {
      CommandParameter parameter = new CommandParameter ("@" + (_commandParameters.Count + 1), value);
      _commandParameters.Add (parameter);
      return parameter;
    }
  }
}