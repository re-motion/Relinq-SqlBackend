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
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.Utilities;

namespace Remotion.Data.Linq.SqlBackend.SqlPreparation
{
  /// <summary>
  /// The <see cref="ResultOperatorHandlerRegistry"/> holds the implementations of <see cref="IResultOperatorHandler"/> used by 
  /// <see cref="SqlPreparationQueryModelVisitor"/> for incorporating result operators into a <see cref="SqlStatement"/>.
  /// Use <see cref="RegistryBase{TRegistry,TKey,TItem}.CreateDefault"/> to create the default set of result operator handlers, 
  /// which can then be extended by custom handlers.
  /// </summary>
  public class ResultOperatorHandlerRegistry : RegistryBase<ResultOperatorHandlerRegistry, Type, IResultOperatorHandler>
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="ResultOperatorHandlerRegistry"/> class. Use 
    /// <see cref="RegistryBase{TRegistry,TKey,TItem}.CreateDefault"/> to create an instance pre-initialized with the default handlers instead.
    /// </summary>
    public ResultOperatorHandlerRegistry ()
    {
    }

    public override IResultOperatorHandler GetItem (Type key)
    {
      ArgumentUtility.CheckNotNull ("key", key);

      IResultOperatorHandler handler;

      var currentType = key;
      do
      {
        handler = GetItemExact (currentType);
        currentType = currentType.BaseType;
      } while (handler == null && currentType != null);

      return handler;
    }

    protected override void RegisterForTypes (IEnumerable<Type> itemTypes)
    {
      foreach (var handlerType in itemTypes)
      {
        var handler = (IResultOperatorHandler) Activator.CreateInstance (handlerType);
        Register (handler.SupportedResultOperatorType, handler);
      }
    }
  }
}