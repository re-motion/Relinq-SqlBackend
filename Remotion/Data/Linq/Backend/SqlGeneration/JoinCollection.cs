// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// as published by the Free Software Foundation; either version 2.1 of the 
// License, or (at your option) any later version.
// 
// re-motion is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-motion; if not, see http://www.gnu.org/licenses.
// 
using System;
using System.Collections;
using System.Collections.Generic;
using Remotion.Data.Linq.Backend.DataObjectModel;
using Remotion.Data.Linq.Utilities;
using Remotion.Data.Linq.Collections;

namespace Remotion.Data.Linq.Backend.SqlGeneration
{
  public class JoinCollection : IEnumerable<KeyValuePair<IColumnSource, IList<SingleJoin>>>
  {
    private readonly MultiDictionary<IColumnSource, SingleJoin> _innerDictionary = new MultiDictionary<IColumnSource, SingleJoin>();

    public void AddPath (FieldSourcePath fieldSourcePath)
    {
      ArgumentUtility.CheckNotNull ("fieldSourcePath", fieldSourcePath);

      foreach (var join in fieldSourcePath.Joins)
        AddSingleJoin (fieldSourcePath.FirstSource, join);
    }

    public List<SingleJoin> this [IColumnSource columnSource]
    {
      get { return (List<SingleJoin>) _innerDictionary[columnSource]; }
    }

    public int Count
    {
      get { return _innerDictionary.KeyCount; }
    }

    private void AddSingleJoin (IColumnSource firstSource, SingleJoin join)
    {
      if (!this[firstSource].Contains (join))
        _innerDictionary.Add (firstSource, join);
    }

    public IEnumerator<KeyValuePair<IColumnSource, IList<SingleJoin>>> GetEnumerator ()
    {
      return _innerDictionary.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator ()
    {
      return GetEnumerator();
    }
  }
}
