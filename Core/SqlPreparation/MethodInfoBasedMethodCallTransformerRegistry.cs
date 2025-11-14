// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (c) rubicon IT GmbH, www.rubicon.eu
// 
// re-linq is free software; you can redistribute it and/or modify it under 
// the terms of the GNU Lesser General Public License as published by the 
// Free Software Foundation; either version 2.1 of the License, 
// or (at your option) any later version.
// 
// re-linq is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-linq; if not, see http://www.gnu.org/licenses.
// 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Remotion.Linq.SqlBackend.Utilities;
using Remotion.Utilities;

namespace Remotion.Linq.SqlBackend.SqlPreparation
{
  /// <summary>
  /// <see cref="CompoundMethodCallTransformerProvider"/> is used to register methods and get <see cref="IMethodCallTransformer"/> instances.
  /// </summary>
  public class MethodInfoBasedMethodCallTransformerRegistry : RegistryBase<MethodInfoBasedMethodCallTransformerRegistry, MethodInfo, IMethodCallTransformer>, IMethodCallTransformerProvider
  {
    public IMethodCallTransformer GetTransformer (MethodCallExpression methodCallExpression)
    {
      ArgumentUtility.CheckNotNull (nameof(methodCallExpression), methodCallExpression);

      var key = methodCallExpression.Method;

      return GetItem (key);
    }
    
    public override IMethodCallTransformer GetItem (MethodInfo key)
    {
      ArgumentUtility.CheckNotNull (nameof(key), key);

      var transformer = GetItemExact (key);
      if (transformer != null)
        return transformer;

      if (key.IsGenericMethod && !key.IsGenericMethodDefinition)
        return GetItem (key.GetGenericMethodDefinition ());

      var baseMethod = key.GetBaseDefinition ();
      if (baseMethod != key)
        return GetItem (baseMethod);

      return null;
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