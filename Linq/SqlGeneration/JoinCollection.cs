using Rubicon.Collections;
using Rubicon.Data.Linq.DataObjectModel;

namespace Rubicon.Data.Linq.SqlGeneration
{
  public class JoinCollection : MultiDictionary<Table, JoinTree>
  {
    public void Add (JoinTree joinTree)
    {
        Table startingTable = joinTree.GetStartingTable ();
        if (!this[startingTable].Contains (joinTree))
          Add (startingTable, joinTree);
    }
  }
}