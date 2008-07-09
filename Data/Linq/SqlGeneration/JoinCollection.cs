/* Copyright (C) 2005 - 2008 rubicon informationstechnologie gmbh
 *
 * This program is free software: you can redistribute it and/or modify it under 
 * the terms of the re:motion license agreement in license.txt. If you did not 
 * receive it, please visit http://www.re-motion.org/licensing.
 * 
 * Unless otherwise provided, this software is distributed on an "AS IS" basis, 
 * WITHOUT WARRANTY OF ANY KIND, either express or implied. 
 */

using System.Collections;
using System.Collections.Generic;
using Remotion.Collections;
using Remotion.Data.Linq.DataObjectModel;

namespace Remotion.Data.Linq.SqlGeneration
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
