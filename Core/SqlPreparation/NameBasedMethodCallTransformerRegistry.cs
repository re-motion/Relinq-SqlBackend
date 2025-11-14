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
  /// <see cref="NameBasedMethodCallTransformerRegistry"/> is used to register method names and get <see cref="IMethodCallTransformer"/> instances.
  /// </summary>
  public class NameBasedMethodCallTransformerRegistry : RegistryBase<NameBasedMethodCallTransformerRegistry, string, IMethodCallTransformer>, IMethodCallTransformerProvider
  {
    public IMethodCallTransformer GetTransformer (MethodCallExpression methodCallExpression)
    {
      ArgumentUtility.CheckNotNull (nameof(methodCallExpression), methodCallExpression);

      return GetItem (methodCallExpression.Method.Name);
    }
    
    public override IMethodCallTransformer GetItem (string key)
    {
      ArgumentUtility.CheckNotNullOrEmpty (nameof(key), key);

      return GetItemExact (key);
    }

    protected override void RegisterForTypes (IEnumerable<Type> itemTypes)
    {
      var supportedMethodsForTypes = from t in itemTypes
                                     let supportedMethodNamesField = t.GetField ("SupportedMethodNames", BindingFlags.Static | BindingFlags.Public)
                                     where supportedMethodNamesField != null
                                     select new { Generator = t, MethodNames = (IEnumerable<string>) supportedMethodNamesField.GetValue (null) };

      foreach (var supportedMethodsForType in supportedMethodsForTypes)
        Register (supportedMethodsForType.MethodNames, (IMethodCallTransformer) Activator.CreateInstance (supportedMethodsForType.Generator));
    }
    
  }
}