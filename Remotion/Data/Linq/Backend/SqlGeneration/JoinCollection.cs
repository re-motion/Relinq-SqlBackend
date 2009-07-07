// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// version 3.0 as published by the Free Software Foundation.
// 
// re-motion is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-motion; if not, see http://www.gnu.org/licenses.
// 
using System.Collections;
using System.Collections.Generic;
using Remotion.Collections;
using Remotion.Data.Linq.Backend.DataObjectModel;

namespace Remotion.Data.Linq.Backend.SqlGeneration
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
