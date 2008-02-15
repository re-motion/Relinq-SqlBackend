using Rubicon.Collections;
using Rubicon.Data.Linq.DataObjectModel;

namespace Rubicon.Data.Linq.SqlGeneration
{
  public class JoinCollection : MultiDictionary<Table, Join>
  {
    public void Add (Join join)
    {
        Table startingTable = join.GetStartingTable ();
        if (!this[startingTable].Contains (join))
          Add (startingTable, join);
    }
  }
}