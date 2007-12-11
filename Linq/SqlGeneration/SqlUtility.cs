using System.Data;
using Rubicon.Utilities;

namespace Rubicon.Data.Linq.SqlGeneration
{
  public static class SqlUtility
  {
    public static IDbCommand CreateCommand (string commandText, QueryParameter[] parameters, IDatabaseInfo databaseInfo, IDbConnection connection)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("commandText", commandText);
      ArgumentUtility.CheckNotNull ("parameters", parameters);
      ArgumentUtility.CheckNotNull ("databaseInfo", databaseInfo);
      ArgumentUtility.CheckNotNull ("connection", connection);

      IDbCommand command = connection.CreateCommand ();
      command.CommandText = commandText;
      command.CommandType = CommandType.Text;

      foreach (QueryParameter parameter in parameters)
        command.Parameters.Add (parameter.Value);

      return command;
    }

  }
}