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
  /// <see cref="MethodCallTransformerRegistry"/> is used to register and get <see cref="IMethodCallTransformer"/> instances.
  /// </summary>
  public class MethodCallTransformerRegistry
  {
    private readonly Dictionary<MethodInfo, IMethodCallTransformer> _transformers;

    /// <summary>
    /// Creates a default <see cref="MethodCallTransformerRegistry"/>, which has all types implementing <see cref="IMethodCallTransformer"/> from the
    /// SQL backend assembly automatically registered, as long as they offer a public static <c>SupportedMethods</c> field.
    /// </summary>
    /// <returns>A default <see cref="MethodCallTransformerRegistry"/> with all <see cref="IMethodCallTransformer"/>s with a <c>SupportedMethods</c>
    /// field registered.</returns>
    public static MethodCallTransformerRegistry CreateDefault ()
    {
      var methodTransformers = from t in typeof (MethodCallTransformerRegistry).Assembly.GetTypes ()
                             where typeof (IMethodCallTransformer).IsAssignableFrom (t)
                             select t;

      var supportedMethodsForTypes = from t in methodTransformers
                                     let supportedMethodsField = t.GetField ("SupportedMethods", BindingFlags.Static | BindingFlags.Public)
                                     where supportedMethodsField != null
                                     select new { Generator = t, Methods = (IEnumerable<MethodInfo>) supportedMethodsField.GetValue (null) };

      var registry = new MethodCallTransformerRegistry ();

      foreach (var supportedMethodsForType in supportedMethodsForTypes)
      {
        registry.Register (supportedMethodsForType.Methods, (IMethodCallTransformer) Activator.CreateInstance (supportedMethodsForType.Generator));
      }

      return registry;
    }

    public MethodCallTransformerRegistry ()
    {
      _transformers = new Dictionary<MethodInfo, IMethodCallTransformer>();
    }

    public void Register (MethodInfo methodInfo, IMethodCallTransformer transformer)
    {
      ArgumentUtility.CheckNotNull ("methodInfo", methodInfo);
      ArgumentUtility.CheckNotNull ("transformer", transformer);

      _transformers[methodInfo] = transformer;
    }

    public void Register (IEnumerable<MethodInfo> methodInfos, IMethodCallTransformer transformer)
    {
      ArgumentUtility.CheckNotNull ("methodInfos", methodInfos);
      ArgumentUtility.CheckNotNull ("transformer", transformer);

      foreach (var methodInfo in methodInfos)
      {
        _transformers[methodInfo] = transformer;
      }
    }

    public IMethodCallTransformer GetTransformer (MethodInfo methodInfo)
    {
      ArgumentUtility.CheckNotNull ("methodInfo", methodInfo);

      if (_transformers.ContainsKey (methodInfo))
        return _transformers[methodInfo];

      if (methodInfo.IsGenericMethod && !methodInfo.IsGenericMethodDefinition)
        return GetTransformer (methodInfo.GetGenericMethodDefinition ());

      var baseMethod = methodInfo.GetBaseDefinition ();
      if (baseMethod != methodInfo)
        return GetTransformer (baseMethod);

      string message = string.Format (
          "The method '{0}.{1}' is not supported by this code generator, and no custom transformer has been registered.",
          methodInfo.DeclaringType.FullName,
          methodInfo.Name);
      throw new NotSupportedException (message);
    }
  }
}