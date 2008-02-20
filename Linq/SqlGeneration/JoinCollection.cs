using Rubicon.Collections;
using Rubicon.Data.Linq.DataObjectModel;

namespace Rubicon.Data.Linq.SqlGeneration
{
#warning TODO: Refactor to use composition instead of inheritance
  public class JoinCollection : MultiDictionary<Table, SingleJoin>
  {
    public void AddTree (JoinTree joinTree)
    {
        Table startingTable = joinTree.GetStartingTable ();
        foreach (SingleJoin singleJoin in joinTree.GetAllSingleJoins ())
          AddSingleJoin (startingTable, singleJoin);
    }

    private void AddSingleJoin (Table startingTable, SingleJoin join)
    {
      if (!this[startingTable].Contains (join))
        Add (startingTable, join);
    }
  }
}