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
using System.Linq;
using System.Linq.Expressions;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.SqlBackend.SqlPreparation
{
  public class MethodCallTransformerRegistry : IMethodCallTransformerRegistry
  {
    private readonly IMethodCallTransformerRegistry[] _registries;

    public static MethodCallTransformerRegistry CreateDefault ()
    {
      var methodInfoBasedRegistry = MethodInfoBasedMethodCallTransformerRegistry.CreateDefault ();
      var nameBasedRegistry = NameBasedMethodCallTransformerRegistry.CreateDefault ();
      return new MethodCallTransformerRegistry (methodInfoBasedRegistry, nameBasedRegistry);
    }

    public IMethodCallTransformerRegistry[] Registries
    {
      get { return _registries; }
    }

    public MethodCallTransformerRegistry (params IMethodCallTransformerRegistry[] registries)
    {
      ArgumentUtility.CheckNotNull ("registries", registries);

      _registries = registries;
    }

    public IMethodCallTransformer GetTransformer (MethodCallExpression methodCallExpression)
    {
      ArgumentUtility.CheckNotNull ("methodCallExpression", methodCallExpression);

      return _registries
        .Select (methodCallTransformerRegistry => methodCallTransformerRegistry.GetTransformer (methodCallExpression))
        .FirstOrDefault (transformer => transformer != null);
    }
  }
}