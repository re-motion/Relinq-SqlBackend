// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// re-linq is free software; you can redistribute it and/or modify it under 
// the terms of the GNU Lesser General Public License as published by the 
// Free Software Foundation; either version 2.1 of the License, 
// or (at your option) any later version.
// 
// re-linq is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-linq; if not, see http://www.gnu.org/licenses.
// 
using System.Collections.Generic;
using System.Data.Linq.Mapping;

namespace Remotion.Linq.LinqToSqlAdapter
{
  /// <summary>
  /// Compares <see cref="MetaDataMember"/> instances based on their <see cref="MetaDataMember.MappedName"/> properties.
  /// </summary>
  public class MetaDataMemberComparer : IEqualityComparer<MetaDataMember>
  {
    public bool Equals (MetaDataMember x, MetaDataMember y)
    {
      return x.MappedName == y.MappedName;
    }

    public int GetHashCode (MetaDataMember obj)
    {
      return obj.MappedName.GetHashCode();
    }
  }
}