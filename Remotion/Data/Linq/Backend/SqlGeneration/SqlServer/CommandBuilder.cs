// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
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
using System;
using System.Collections.Generic;
using System.Text;
using Remotion.Data.Linq.Backend.DataObjectModel;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Backend.SqlGeneration.SqlServer
{
  public class CommandBuilder : ICommandBuilder
  {
    public CommandBuilder (
        StringBuilder commandText,
        List<CommandParameter> commandParameters,
        IDatabaseInfo databaseInfo,
        MethodCallSqlGeneratorRegistry methodCallRegistry)
    {
      ArgumentUtility.CheckNotNull ("commandText", commandText);
      ArgumentUtility.CheckNotNull ("commandParameters", commandParameters);
      ArgumentUtility.CheckNotNull ("databaseInfo", databaseInfo);
      ArgumentUtility.CheckNotNull ("methodCallRegistry", methodCallRegistry);

      CommandText = commandText;
      CommandParameters = commandParameters;
      DatabaseInfo = databaseInfo;
      MethodCallRegistry = methodCallRegistry;
    }

    public StringBuilder CommandText { get; private set; }
    public List<CommandParameter> CommandParameters { get; private set; }
    public IDatabaseInfo DatabaseInfo { get; private set; }
    public MethodCallSqlGeneratorRegistry MethodCallRegistry { get; private set; }

    public string GetCommandText ()
    {
      return CommandText.ToString();
    }

    public CommandParameter[] GetCommandParameters ()
    {
      return CommandParameters.ToArray();
    }

    public void Append (string text)
    {
      CommandText.Append (text);
    }

    public void AppendEvaluation (IEvaluation evaluation)
    {
      SqlServerEvaluationVisitor visitor = new SqlServerEvaluationVisitor (this, DatabaseInfo, MethodCallRegistry);
      evaluation.Accept (visitor);
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

    public CommandParameter AddParameter (object value)
    {
      CommandParameter parameter = new CommandParameter ("@" + (CommandParameters.Count + 1), value);
      CommandParameters.Add (parameter);
      return parameter;
    }
  }
}