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
  public class MethodCallTransformerRegistry : RegistryBase<MethodCallTransformerRegistry, MethodInfo, IMethodCallTransformer>
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="MethodCallTransformerRegistry"/> class. Use 
    /// <see cref="RegistryBase{TRegistry,TKey,TItem}.CreateDefault"/> to create an instance pre-initialized with the default transformers instead.
    /// </summary>
    public MethodCallTransformerRegistry ()
    {
    }

    public override IMethodCallTransformer GetItem (MethodInfo key)
    {
      ArgumentUtility.CheckNotNull ("key", key);

      var transformer = GetItemExact (key);
      if (transformer != null)
        return transformer;

      if (key.IsGenericMethod && !key.IsGenericMethodDefinition)
        return GetItem (key.GetGenericMethodDefinition ());

      var baseMethod = key.GetBaseDefinition ();
      if (baseMethod != key)
        return GetItem (baseMethod);

      string message = string.Format (
          "The method '{0}.{1}' is not supported by this code generator, and no custom transformer has been registered.",
          key.DeclaringType.FullName,
          key.Name);
      throw new NotSupportedException (message);
    }

    protected override void RegisterForTypes (IEnumerable<Type> itemTypes)
    {
      var supportedMethodsForTypes = from t in itemTypes
                                     let supportedMethodsField = t.GetField ("SupportedMethods", BindingFlags.Static | BindingFlags.Public)
                                     where supportedMethodsField != null
                                     select new { Generator = t, Methods = (IEnumerable<MethodInfo>) supportedMethodsField.GetValue (null) };

      foreach (var supportedMethodsForType in supportedMethodsForTypes)
        Register (supportedMethodsForType.Methods, (IMethodCallTransformer) Activator.CreateInstance (supportedMethodsForType.Generator));
    }
  }
}