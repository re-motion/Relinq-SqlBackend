// Copyright (C) 2005 - 2008 rubicon informationstechnologie gmbh 
// All rights reserved.
//

using Remotion.Data.Linq.DataObjectModel;

namespace Remotion.Data.Linq.SqlGeneration.SqlServer.MethodCallGenerators
{
  public class MethodCallSubstring : IMethodCallSqlGenerator
  {
    public void GenerateSql (MethodCall methodCall, ICommandBuilder commandBuilder)
    {
      commandBuilder.Append ("SUBSTRING(");
      commandBuilder.AppendEvaluation (methodCall.EvaluationObject);
      commandBuilder.Append (",");
      int cnt = 0;
      int len = methodCall.EvaluationArguments.Count;
      foreach (var argument in methodCall.EvaluationArguments)
      {
        commandBuilder.AppendEvaluation (argument);
        cnt++;
        if (cnt != len)
          commandBuilder.Append (",");
      }
      commandBuilder.Append (")");
    }
  }
}