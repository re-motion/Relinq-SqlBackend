using System;
using System.Collections;
using System.Collections.Generic;
using Rubicon.Collections;
using Rubicon.Data.Linq.DataObjectModel;

namespace Rubicon.Data.Linq.SqlGeneration
{
  public class JoinCollection : IEnumerable<KeyValuePair<IFromSource, List<SingleJoin>>>
  {
    private readonly MultiDictionary<IFromSource, SingleJoin> _innerDictionary = new MultiDictionary<IFromSource, SingleJoin>();

    public void AddPath (FieldSourcePath fieldSourcePath)
    {
      foreach (var join in fieldSourcePath.Joins)
        AddSingleJoin (fieldSourcePath.FirstSource, join);
    }

    public List<SingleJoin> this[IFromSource fromSource]
    {
      get { return _innerDictionary[fromSource]; }
    }

    public int Count
    {
      get { return _innerDictionary.Count; }
    }

    private void AddSingleJoin (IFromSource firstSource, SingleJoin join)
    {
      if (!this[firstSource].Contains (join))
        _innerDictionary.Add (firstSource, join);
    }

    public IEnumerator<KeyValuePair<IFromSource, List<SingleJoin>>> GetEnumerator ()
    {
      return ((IEnumerable<KeyValuePair<IFromSource, List<SingleJoin>>>)_innerDictionary).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator ()
    {
      return GetEnumerator();
    }
  }
}