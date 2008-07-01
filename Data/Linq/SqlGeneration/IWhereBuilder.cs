using Remotion.Data.Linq.DataObjectModel;

namespace Remotion.Data.Linq.SqlGeneration
{
  public interface IWhereBuilder
  {
    void BuildWherePart (ICriterion criterion);
  }
}