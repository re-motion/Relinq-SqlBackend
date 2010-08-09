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
using System.Collections.Generic;
using System.Data.Linq.Mapping;

namespace Remotion.Data.Linq.IntegrationTests.Utilities
{
  internal static class MemberSortUtility
  {
    internal static MetaDataMember[] SortDataMembers (ICollection<MetaDataMember> unsortedMembers)
    {
      MetaDataMember[] sortedMembers = new MetaDataMember[unsortedMembers.Count];
      unsortedMembers.CopyTo (sortedMembers, 0);
      Array.Sort (sortedMembers, new DataMemberComparer());

      return sortedMembers;
    }

    private class DataMemberComparer : IComparer<MetaDataMember>
    {
      public int Compare (MetaDataMember x, MetaDataMember y)
      {
        if (x.Ordinal.Equals (y.Ordinal))
          return 0;

        if (x.IsPrimaryKey)
          return -1;

        if (y.IsPrimaryKey)
          return 1;

        return x.Ordinal.CompareTo (y.Ordinal);
      }
    }
  }
}