/* Copyright (C) 2005 - 2008 rubicon informationstechnologie gmbh
 *
 * This program is free software: you can redistribute it and/or modify it under 
 * the terms of the re:motion license agreement in license.txt. If you did not 
 * receive it, please visit http://www.re-motion.org/licensing.
 * 
 * Unless otherwise provided, this software is distributed on an "AS IS" basis, 
 * WITHOUT WARRANTY OF ANY KIND, either express or implied. 
 */

using System.Collections.Generic;
using System.Text;
using Remotion.Utilities;

namespace Remotion.Data.Linq.SqlGeneration.SqlServer
{
  public class SqlServerGenerationContext : ISqlGenerationContext
  {
    public SqlServerGenerationContext (IDatabaseInfo databaseInfo, MethodCallSqlGeneratorRegistry methodCallRegistry)
    {
      ArgumentUtility.CheckNotNull ("databaseInfo", databaseInfo);
      ArgumentUtility.CheckNotNull ("methodCallRegistry", methodCallRegistry);

      CommandBuilder = new CommandBuilder (new StringBuilder(), new List<CommandParameter>(), databaseInfo, new MethodCallSqlGeneratorRegistry());
    }

    public SqlServerGenerationContext (CommandBuilder commandBuilder)
    {
      ArgumentUtility.CheckNotNull ("commandBuilder", commandBuilder);
      CommandBuilder = commandBuilder;
    }

    public CommandBuilder CommandBuilder { get; private set; }

    public string CommandText
    {
      get { return CommandBuilder.GetCommandText(); }
    }

    public CommandParameter[] CommandParameters
    {
      get { return CommandBuilder.GetCommandParameters (); }
    }
  }
}
