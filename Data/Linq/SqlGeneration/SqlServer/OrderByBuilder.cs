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
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Utilities;

namespace Remotion.Data.Linq.SqlGeneration.SqlServer
{
  public class OrderByBuilder : IOrderByBuilder
  {
    private static string GetOrderedDirectionString (OrderDirection direction)
    {
      switch (direction)
      {
        case OrderDirection.Asc:
          return "ASC";
        case OrderDirection.Desc:
          return "DESC";
        default:
          throw new NotSupportedException ("OrderDirection " + direction + " is not supported.");
      }
    }

    private readonly CommandBuilder _commandBuilder;

    public OrderByBuilder (CommandBuilder commandBuilder)
    {
      ArgumentUtility.CheckNotNull ("commandBuilder", commandBuilder);
      _commandBuilder = commandBuilder;
    }

    public void BuildOrderByPart (List<OrderingField> orderingFields)
    {
      if (orderingFields.Count != 0)
      {
        _commandBuilder.Append (" ORDER BY ");
        _commandBuilder.AppendSeparatedItems (orderingFields, AppendOrderingField);
      }
    }

    private void AppendOrderingField (OrderingField orderingField)
    {
      _commandBuilder.AppendEvaluation (orderingField.Column);
      _commandBuilder.Append (" ");
      _commandBuilder.Append (GetOrderedDirectionString (orderingField.Direction));
    }
  }
}
