using System.Collections.Generic;
using Rubicon.Data.Linq.DataObjectModel;

namespace Rubicon.Data.Linq.SqlGeneration
{
  public interface IOrderByBuilder
  {
    void BuildOrderByPart (List<OrderingField> orderingFields);
  }
}