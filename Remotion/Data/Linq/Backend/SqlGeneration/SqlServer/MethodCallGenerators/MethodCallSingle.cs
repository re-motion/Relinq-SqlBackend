// Copyright (C) 2005 - 2008 rubicon informationstechnologie gmbh 
// All rights reserved.
//

using Remotion.Data.Linq.Backend.DataObjectModel;
using Remotion.Data.Linq.Backend.SqlGeneration;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Backend.SqlGeneration.SqlServer.MethodCallGenerators
{
  public class MethodCallSingle : IMethodCallSqlGenerator
  {
    public void GenerateSql (MethodCall methodCall, ICommandBuilder commandBuilder)
    {
      ArgumentUtility.CheckNotNull ("methodCall", methodCall);
      ArgumentUtility.CheckNotNull ("commandBuilder", commandBuilder);

      commandBuilder.Append ("TOP 2");

    }
  }
}