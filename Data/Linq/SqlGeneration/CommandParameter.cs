/* Copyright (C) 2005 - 2008 rubicon informationstechnologie gmbh
 *
 * This program is free software: you can redistribute it and/or modify it under 
 * the terms of the re:motion license agreement in license.txt. If you did not 
 * receive it, please visit http://www.re-motion.org/licensing.
 * 
 * Unless otherwise provided, this software is distributed on an "AS IS" basis, 
 * WITHOUT WARRANTY OF ANY KIND, either express or implied. 
 */

using Remotion.Utilities;

namespace Remotion.Data.Linq.SqlGeneration
{
  public struct CommandParameter
  {
    public readonly string Name;
    public readonly object Value;

    public CommandParameter (string name, object value)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("name", name);

      Name = name;
      Value = value;
    }
  }
}
