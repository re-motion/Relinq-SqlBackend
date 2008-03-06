using System;
using System.Collections.Generic;
using System.Text;
using Rubicon.Data.Linq.Clauses;
using Rubicon.Data.Linq.DataObjectModel;
using Rubicon.Text;
using Rubicon.Utilities;

namespace Rubicon.Data.Linq.SqlGeneration.SqlServer
{
  public class OrderByBuilder : IOrderByBuilder
  {
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
        IEnumerable<string> orderingFieldStrings = CombineOrderedFields (orderingFields);
        _commandBuilder.Append (SeparatedStringBuilder.Build (", ", orderingFieldStrings));
      }
    }

    private IEnumerable<string> CombineOrderedFields (IEnumerable<OrderingField> orderingFields)
    {
      foreach (OrderingField orderingField in orderingFields)
        yield return SqlServerUtility.GetColumnString (orderingField.Column) + " " + GetOrderedDirectionString (orderingField.Direction);
    }

    private string GetOrderedDirectionString (OrderDirection direction)
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
  }
}