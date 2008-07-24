/* Copyright (C) 2005 - 2008 rubicon informationstechnologie gmbh
 *
 * This program is free software: you can redistribute it and/or modify it under 
 * the terms of the re:motion license agreement in license.txt. If you did not 
 * receive it, please visit http://www.re-motion.org/licensing.
 * 
 * Unless otherwise provided, this software is distributed on an "AS IS" basis, 
 * WITHOUT WARRANTY OF ANY KIND, either express or implied. 
 */

using System;
using System.Collections.Generic;
using System.Reflection;
using Remotion.Utilities;

namespace Remotion.Data.Linq.SqlGeneration
{
  public class MethodCallSqlGeneratorRegistry
  {
    private readonly Dictionary<MethodInfo, IMethodCallSqlGenerator> _generators;

    public MethodCallSqlGeneratorRegistry ()
    {
      _generators = new Dictionary<MethodInfo, IMethodCallSqlGenerator>();
    }

    public void Register (MethodInfo methodInfo, IMethodCallSqlGenerator generator)
    {
      ArgumentUtility.CheckNotNull ("methodInfo", methodInfo);
      ArgumentUtility.CheckNotNull ("generator", generator);

      if (!_generators.ContainsKey (methodInfo))
        _generators.Add (methodInfo, generator);
      else
        _generators[methodInfo] = generator;
    }

    public IMethodCallSqlGenerator GetGenerator (MethodInfo methodInfo)
    {
      ArgumentUtility.CheckNotNull ("methodInfo", methodInfo);

      string message = string.Format (
          "The method {0}.{1} is not supported by this code generator, and no custom generator has been registered.",
          methodInfo.DeclaringType.FullName,
          methodInfo.Name);

      if (_generators.ContainsKey(methodInfo))
        return _generators[methodInfo];
      throw new SqlGenerationException (message);
    }
  }
}