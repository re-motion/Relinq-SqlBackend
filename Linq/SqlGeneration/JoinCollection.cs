using System;
using System.Collections;
using System.Collections.Generic;
using Rubicon.Collections;
using Rubicon.Data.Linq.DataObjectModel;

namespace Rubicon.Data.Linq.SqlGeneration
{
  public class JoinCollection : IEnumerable<KeyValuePair<Table, List<SingleJoin>>>
  {
    private readonly MultiDictionary<Table, SingleJoin> _innerDictionary = new MultiDictionary<Table, SingleJoin>();

    public void AddPath (FieldSourcePath fieldSourcePath)
    {
      foreach (var join in fieldSourcePath.Joins)
        AddSingleJoin (fieldSourcePath.SourceTable, join);
    }

    public List<SingleJoin> this[Table table]
    {
      get { return _innerDictionary[table]; }
    }

    public int Count
    {
      get { return _innerDictionary.Count; }
    }

    private void AddSingleJoin (Table startingTable, SingleJoin join)
    {
      if (!this[startingTable].Contains (join))
        _innerDictionary.Add (startingTable, join);
    }

    public IEnumerator<KeyValuePair<Table, List<SingleJoin>>> GetEnumerator ()
    {
      return ((IEnumerable<KeyValuePair<Table, List<SingleJoin>>>)_innerDictionary).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator ()
    {
      return GetEnumerator();
    }
  }
}