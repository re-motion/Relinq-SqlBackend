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
using Remotion.Data.Linq.Parsing;
using Remotion.Utilities;

namespace Remotion.Data.Linq.SqlGeneration.SqlServer
{
  // If a fixedCommandBuilder is specified, the SqlServerGenerator can only be used to create one query from one thread. Otherwise, it is
  // stateless and can be used for multiple queries from multiple threads.
  public class InlineSqlServerGenerator : SqlServerGenerator
  {
    private readonly CommandBuilder _fixedCommandBuilder;

    public InlineSqlServerGenerator (IDatabaseInfo databaseInfo, CommandBuilder fixedCommandBuilder, ParseMode parseMode)
      : base (databaseInfo, parseMode)
    {
      ArgumentUtility.CheckNotNull ("fixedCommandBuilder", fixedCommandBuilder);
      _fixedCommandBuilder = fixedCommandBuilder;
    }

    protected override SqlServerGenerationContext CreateContext ()
    {
      return new SqlServerGenerationContext (_fixedCommandBuilder);
    }
  }
}
