// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// as published by the Free Software Foundation; either version 2.1 of the 
// License, or (at your option) any later version.
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
using System.Reflection;
using Remotion.Data.Linq.SqlBackend.SqlGeneration;
using Remotion.Data.Linq.Utilities;
using System.Linq;

namespace Remotion.Data.Linq.SqlBackend.SqlPreparation
{
  /// <summary>
  /// <see cref="MethodCallTransformerRegistry"/> is used to register and get <see cref="IMethodCallSqlGenerator"/> instances.
  /// </summary>
  public class MethodCallTransformerRegistry
  {
    private readonly Dictionary<MethodInfo, IMethodCallSqlGenerator> _generators;

    /// <summary>
    /// Creates a default <see cref="MethodCallTransformerRegistry"/>, which has all types implementing <see cref="IMethodCallSqlGenerator"/> from the
    /// re-linq assembly automatically registered, as long as they offer a public static <c>SupportedMethods</c> field.
    /// </summary>
    /// <returns>A default <see cref="MethodCallTransformerRegistry"/> with all <see cref="IMethodCallSqlGenerator"/>s with a <c>SupportedMethods</c>
    /// field registered.</returns>
    public static MethodCallTransformerRegistry CreateDefault ()
    {
      var methodGenerators = from t in typeof (MethodCallTransformerRegistry).Assembly.GetTypes ()
                             where typeof (IMethodCallSqlGenerator).IsAssignableFrom (t)
                             select t;

      var supportedMethodsForTypes = from t in methodGenerators
                                     let supportedMethodsField = t.GetField ("SupportedMethods", BindingFlags.Static | BindingFlags.Public)
                                     where supportedMethodsField != null
                                     select new { Generator = t, Methods = (IEnumerable<MethodInfo>) supportedMethodsField.GetValue (null) };

      var registry = new MethodCallTransformerRegistry();

      foreach (var supportedMethodsForType in supportedMethodsForTypes)
      {
        registry.Register (supportedMethodsForType.Methods, (IMethodCallSqlGenerator) Activator.CreateInstance (supportedMethodsForType.Generator));
      }

      return registry;
    }

    public MethodCallTransformerRegistry ()
    {
      _generators = new Dictionary<MethodInfo, IMethodCallSqlGenerator> ();
    }

    public void Register (MethodInfo methodInfo, IMethodCallSqlGenerator generator)
    {
      ArgumentUtility.CheckNotNull ("methodInfo", methodInfo);
      ArgumentUtility.CheckNotNull ("generator", generator);

      _generators[methodInfo] = generator;
    }

    public void Register (IEnumerable<MethodInfo> methodInfos, IMethodCallSqlGenerator generator)
    {
      ArgumentUtility.CheckNotNull ("generator", generator);
      ArgumentUtility.CheckNotNull ("methodInfos", methodInfos);

      foreach (var methodInfo in methodInfos)
      {
        _generators[methodInfo] = generator;
      }
    }

    public IMethodCallSqlGenerator GetGenerator (MethodInfo methodInfo)
    {
      ArgumentUtility.CheckNotNull ("methodInfo", methodInfo);

      if (_generators.ContainsKey (methodInfo))
        return _generators[methodInfo];

      if (methodInfo.IsGenericMethod && !methodInfo.IsGenericMethodDefinition)
        return GetGenerator (methodInfo.GetGenericMethodDefinition ());

      var baseMethod = methodInfo.GetBaseDefinition ();
      if (baseMethod != methodInfo)
        return GetGenerator (baseMethod);
      
      string message = string.Format (
          "The method '{0}.{1}' is not supported by this code generator, and no custom generator has been registered.",
          methodInfo.DeclaringType.FullName,
          methodInfo.Name);
      throw new NotSupportedException(message);
    }
  }
}