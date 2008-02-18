using Rubicon.Data.Linq.DataObjectModel;

namespace Rubicon.Data.Linq.SqlGeneration
{
  public interface IWhereBuilder
  {
    void BuildWherePart (ICriterion criterion);
  }
}