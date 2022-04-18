// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (c) rubicon IT GmbH, www.rubicon.eu
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

using System;

namespace Remotion.Linq.SqlBackend.UnitTests.TestDomain
{
  public class MetaID : IEquatable<MetaID>
  {
    public MetaID (object value, string classID)
    {
      Value = value;
      ClassID = classID;
    }

    public object Value { get; set; }
    public string ClassID { get; set; }

    public override bool Equals (object obj)
    {
      return base.Equals (obj);
    }

    public override int GetHashCode ()
    {
      return base.GetHashCode ();
    }

    public bool Equals (MetaID other)
    {
      return Value == other.Value && string.Equals (ClassID, other.ClassID);
    }

    public static bool operator == (MetaID left, MetaID right)
    {
      return left.Equals (right);
    }

    public static bool operator != (MetaID left, MetaID right)
    {
      return !left.Equals (right);
    }
  }
}