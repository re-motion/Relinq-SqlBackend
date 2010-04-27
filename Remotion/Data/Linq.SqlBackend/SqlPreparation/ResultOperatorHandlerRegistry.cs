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
using System.Linq;
using Remotion.Data.Linq.SqlBackend.SqlPreparation.ResultOperatorHandlers;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.SqlBackend.SqlPreparation
{
  /// <summary>
  /// The <see cref="ResultOperatorHandlerRegistry"/> is used to register <see cref="ResultOperatorHandler{T}"/>s.
  /// </summary>
  public class ResultOperatorHandlerRegistry
  {
    private readonly Dictionary<Type, IResultOperatorHandler> _handlers;

    public static ResultOperatorHandlerRegistry CreateDefault (UniqueIdentifierGenerator generator, ISqlPreparationStage stage)
    {
      var handlerTypes = from t in typeof (ResultOperatorHandlerRegistry).Assembly.GetTypes ()
                         where typeof (IResultOperatorHandler).IsAssignableFrom (t) && t != typeof (IResultOperatorHandler) && t != typeof (ResultOperatorHandler<>)
                         select t;

      var registry = new ResultOperatorHandlerRegistry ();

      foreach (var handlerType in handlerTypes)
      {
        var handler = (IResultOperatorHandler) Activator.CreateInstance (handlerType, generator, stage);
        registry.Register (handler.GetType ().BaseType.GetGenericArguments ()[0], handler);
      }

      return registry;
    }

    public ResultOperatorHandlerRegistry ()
    {
      _handlers = new Dictionary<Type, IResultOperatorHandler> ();
    }

    public void Register (Type resultOperatorType, IResultOperatorHandler handler)
    {
      ArgumentUtility.CheckNotNull ("resultOperatorType", resultOperatorType);
      ArgumentUtility.CheckNotNull ("handler", handler);

      _handlers[resultOperatorType] = handler;
    }

    public IResultOperatorHandler GetHandler (Type resultOperatorType)
    {
      ArgumentUtility.CheckNotNull ("resultOperatorType", resultOperatorType);

      if (_handlers.ContainsKey (resultOperatorType))
        return _handlers[resultOperatorType];

      string message =
          string.Format (
              "The handler type '{0}' is not supported by this registry and no custom result operator handler has been registered.",
              resultOperatorType.FullName);
      throw new NotSupportedException (message);
    }

  }
}