using System.Collections.Generic;
using Remotion.Data.Linq.DataObjectModel;

namespace Remotion.Data.Linq.SqlGeneration
{
  public interface IOrderByBuilder
  {
    void BuildOrderByPart (List<OrderingField> orderingFields);
  }
}