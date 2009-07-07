// Copyright (C) 2005 - 2008 rubicon informationstechnologie gmbh 
// All rights reserved.
//

using Remotion.Data.Linq.Backend.DataObjectModel;
using Remotion.Data.Linq.Backend.SqlGeneration;

namespace Remotion.Data.Linq.Backend.SqlGeneration.SqlServer.MethodCallGenerators
{
  public class MethodCallTake : IMethodCallSqlGenerator
  {
    //s.th. like that TOP n

    public void GenerateSql (MethodCall methodCall, ICommandBuilder commandBuilder)
    {
      commandBuilder.Append ("TOP ");
      if (methodCall.Arguments.Count == 1)
        commandBuilder.Append (methodCall.Arguments[0].ToString());
    }
  }
}