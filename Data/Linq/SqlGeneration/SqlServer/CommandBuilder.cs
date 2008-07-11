/* Copyright (C) 2005 - 2008 rubicon informationstechnologie gmbh
 *
 * This program is free software: you can redistribute it and/or modify it under 
 * the terms of the re:motion license agreement in license.txt. If you did not 
 * receive it, please visit http://www.re-motion.org/licensing.
 * 
 * Unless otherwise provided, this software is distributed on an "AS IS" basis, 
 * WITHOUT WARRANTY OF ANY KIND, either express or implied. 
 */

using System;
using System.Collections.Generic;
using System.Text;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Utilities;

namespace Remotion.Data.Linq.SqlGeneration.SqlServer
{
  public class CommandBuilder : ICommandBuilder
  {
    public CommandBuilder (StringBuilder commandText, List<CommandParameter> commandParameters, IDatabaseInfo databaseInfo, MethodCallSqlGeneratorRegistry methodCallRegistry)
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
      SqlServerEvaluationVisitor visitor = new SqlServerEvaluationVisitor (this, DatabaseInfo, new MethodCallSqlGeneratorRegistry());
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
