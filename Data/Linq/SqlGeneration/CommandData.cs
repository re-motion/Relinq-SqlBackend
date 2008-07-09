/* Copyright (C) 2005 - 2008 rubicon informationstechnologie gmbh
 *
 * This program is free software: you can redistribute it and/or modify it under 
 * the terms of the re:motion license agreement in license.txt. If you did not 
 * receive it, please visit http://www.re-motion.org/licensing.
 * 
 * Unless otherwise provided, this software is distributed on an "AS IS" basis, 
 * WITHOUT WARRANTY OF ANY KIND, either express or implied. 
 */

using System.Reflection;
using Remotion.Utilities;

namespace Remotion.Data.Linq.SqlGeneration
{
  public struct CommandData
  {
    public CommandData (string statement, CommandParameter[] parameters, SqlGenerationData sqlGenerationData)
        : this()
    {
      ArgumentUtility.CheckNotNull ("statement", statement);
      ArgumentUtility.CheckNotNull ("parameters", parameters);
      ArgumentUtility.CheckNotNull ("sqlGenerationData", sqlGenerationData);
      
      Statement = statement;
      Parameters = parameters;
      SqlGenerationData = sqlGenerationData;
    }

    public string Statement { get; private set; }
    public CommandParameter[] Parameters { get; private set; }
    public SqlGenerationData SqlGenerationData { get; private set; }
  }
}
