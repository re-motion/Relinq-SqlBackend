/* Copyright (C) 2005 - 2008 rubicon informationstechnologie gmbh
 *
 * This program is free software: you can redistribute it and/or modify it under 
 * the terms of the re:motion license agreement in license.txt. If you did not 
 * receive it, please visit http://www.re-motion.org/licensing.
 * 
 * Unless otherwise provided, this software is distributed on an "AS IS" basis, 
 * WITHOUT WARRANTY OF ANY KIND, either express or implied. 
 */

using Remotion.Data.Linq.DataObjectModel;

namespace Remotion.Data.Linq.SqlGeneration.SqlServer.MethodCallGenerators
{
  public class MethodCallRemove : IMethodCallSqlGenerator
  {
    public void GenerateSql (MethodCall methodCall, ICommandBuilder commandBuilder)
    {
      commandBuilder.Append ("STUFF(");
      commandBuilder.AppendEvaluation (methodCall.EvaluationParameter);
      commandBuilder.Append (",");
      foreach (var argument in methodCall.EvaluationArguments)
        commandBuilder.AppendEvaluation (argument);
      commandBuilder.Append (",CONVERT(Int,DATALENGTH(");
      commandBuilder.AppendEvaluation (methodCall.EvaluationParameter);
      commandBuilder.Append (") / 2), \"");
      commandBuilder.Append (")");
    }
  }
}