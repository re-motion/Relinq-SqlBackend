// Copyright (C) 2005 - 2008 rubicon informationstechnologie gmbh 
// All rights reserved.
//

using Remotion.Data.Linq.Backend.DataObjectModel;
using Remotion.Data.Linq.Backend.SqlGeneration;

namespace Remotion.Data.Linq.Backend.SqlGeneration.SqlServer.MethodCallGenerators
{
  public class MethodCallFirst : IMethodCallSqlGenerator
  {
    public void GenerateSql (MethodCall methodCall, ICommandBuilder commandBuilder)
    {
      commandBuilder.Append ("TOP 1");
    }
  }
}