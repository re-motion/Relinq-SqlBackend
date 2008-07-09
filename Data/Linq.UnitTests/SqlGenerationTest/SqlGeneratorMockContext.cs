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
using Remotion.Data.Linq.SqlGeneration;

namespace Remotion.Data.Linq.UnitTests.SqlGenerationTest
{
  public class SqlGeneratorMockContext : ISqlGenerationContext
  {
    public readonly StringBuilder CommandText = new StringBuilder ();
    public readonly List<CommandParameter> CommandParameters = new List<CommandParameter> ();

    string ISqlGenerationContext.CommandText
    {
      get { return CommandText.ToString(); }
    }

    CommandParameter[] ISqlGenerationContext.CommandParameters
    {
      get { return CommandParameters.ToArray(); }
    }
  }
}
