using System;
using System.Collections;
using System.Collections.Generic;
using Rubicon.Collections;
using Rubicon.Data.Linq.DataObjectModel;

namespace Rubicon.Data.Linq.SqlGeneration
{
  public class JoinCollection : IEnumerable<KeyValuePair<IColumnSource, List<SingleJoin>>>
  {
    private readonly MultiDictionary<IColumnSource, SingleJoin> _innerDictionary = new MultiDictionary<IColumnSource, SingleJoin>();

    public void AddPath (FieldSourcePath fieldSourcePath)
    {
      foreach (var join in fieldSourcePath.Joins)
        AddSingleJoin (fieldSourcePath.FirstSource, join);
    }

    public List<SingleJoin> this[IColumnSource columnSource]
    {
      get { return _innerDictionary[columnSource]; }
    }

    public int Count
    {
      get { return _innerDictionary.Count; }
    }

    private void AddSingleJoin (IColumnSource firstSource, SingleJoin join)
    {
      if (!this[firstSource].Contains (join))
        _innerDictionary.Add (firstSource, join);
    }

    public IEnumerator<KeyValuePair<IColumnSource, List<SingleJoin>>> GetEnumerator ()
    {
      return ((IEnumerable<KeyValuePair<IColumnSource, List<SingleJoin>>>)_innerDictionary).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator ()
    {
      return GetEnumerator();
    }
  }
}