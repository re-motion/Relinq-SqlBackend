using System;
using Rubicon.Collections;
using Rubicon.Data.Linq.DataObjectModel;

namespace Rubicon.Data.Linq.SqlGeneration
{
#warning TODO: Refactor to use composition instead of inheritance
  public class JoinCollection : MultiDictionary<Table, SingleJoin>
  {
    private void AddSingleJoin (Table startingTable, SingleJoin join)
    {
      if (!this[startingTable].Contains (join))
        Add (startingTable, join);
    }

    public void AddPath (FieldSourcePath fieldSourcePath)
    {
      foreach (var join in fieldSourcePath.Joins)
        AddSingleJoin (fieldSourcePath.SourceTable, join);
    }
  }
}