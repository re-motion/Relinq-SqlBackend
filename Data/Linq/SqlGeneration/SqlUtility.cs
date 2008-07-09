/* Copyright (C) 2005 - 2008 rubicon informationstechnologie gmbh
 *
 * This program is free software: you can redistribute it and/or modify it under 
 * the terms of the re:motion license agreement in license.txt. If you did not 
 * receive it, please visit http://www.re-motion.org/licensing.
 * 
 * Unless otherwise provided, this software is distributed on an "AS IS" basis, 
 * WITHOUT WARRANTY OF ANY KIND, either express or implied. 
 */

using System.Data;
using Remotion.Utilities;

namespace Remotion.Data.Linq.SqlGeneration
{
  public static class SqlUtility
  {
    public static IDbCommand CreateCommand (string commandText, CommandParameter[] parameters, IDatabaseInfo databaseInfo, IDbConnection connection)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("commandText", commandText);
      ArgumentUtility.CheckNotNull ("parameters", parameters);
      ArgumentUtility.CheckNotNull ("databaseInfo", databaseInfo);
      ArgumentUtility.CheckNotNull ("connection", connection);

      IDbCommand command = connection.CreateCommand ();
      command.CommandText = commandText;
      command.CommandType = CommandType.Text;

      foreach (CommandParameter parameter in parameters)
        command.Parameters.Add (parameter.Value);

      return command;
    }
  }
}
