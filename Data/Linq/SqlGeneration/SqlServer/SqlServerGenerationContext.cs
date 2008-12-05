// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2008 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// version 3.0 as published by the Free Software Foundation.
// 
// re-motion is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-motion; if not, see http://www.gnu.org/licenses.
// 
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

      CommandBuilder = new CommandBuilder (new StringBuilder(), new List<CommandParameter>(), databaseInfo, methodCallRegistry);
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
