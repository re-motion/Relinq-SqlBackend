// Copyright (C) 2005 - 2008 rubicon informationstechnologie gmbh 
// All rights reserved.
//

using System;
using Remotion.Data.Linq.Backend.DataObjectModel;

namespace Remotion.Data.Linq.SqlGeneration.SqlServer.MethodCallGenerators
{
  public class MethodCallSubstring : IMethodCallSqlGenerator
  {
    public void GenerateSql (MethodCall methodCall, ICommandBuilder commandBuilder)
    {
      if (methodCall.Arguments.Count != 2)
        throw new ArgumentException ("wrong number of arguments");

      commandBuilder.Append ("SUBSTRING(");
      commandBuilder.AppendEvaluation (methodCall.TargetObject);
      commandBuilder.Append (",");
      commandBuilder.AppendEvaluation (methodCall.Arguments[0]);
      commandBuilder.Append (",");
      commandBuilder.AppendEvaluation (methodCall.Arguments[1]);
      commandBuilder.Append (")");
    }
  }
}