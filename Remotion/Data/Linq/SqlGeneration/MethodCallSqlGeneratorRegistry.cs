// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2008 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// version 3.0 as published by the Free Software Foundation.
// 
// re-motion is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-motion; if not, see http://www.gnu.org/licenses.
// 
using System;
using System.Collections.Generic;
using System.Linq;
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

      //var baseMethods = (from m in _generators.Keys
      //                   where m.GetBaseDefinition() == methodInfo.GetBaseDefinition() // => override the same method
      //                   where m.DeclaringType.IsAssignableFrom (methodInfo.DeclaringType)
      //                   select m);
      //var orderedMethods = baseMethods.OrderBy (mi => mi.DeclaringType, new TypeComparer());
      //var baseMethod = orderedMethods.FirstOrDefault ();

      //if (baseMethod != null)
      //  return _generators[baseMethod];

    //class TypeComparer : IComparer<Type>
    //{
    //  public int Compare (Type x, Type y)
    //  {
    //    if (x == y)
    //      return 0;
    //    else if (x.IsAssignableFrom (y))
    //      return -1;
    //    else
    //      return 1;
    //  }
    //}

      throw new SqlGenerationException (message);
    }
  }
}
