using System.Collections.Generic;
using System.Linq.Expressions;
using Rubicon.Collections;
using Rubicon.Data.Linq.DataObjectModel;

namespace Rubicon.Data.Linq.SqlGeneration
{
  public interface IFromBuilder
  {
    void BuildFromPart (List<IColumnSource> fromSources, JoinCollection joins);
    void BuildLetPart (List<LetData> lets);
  }
}