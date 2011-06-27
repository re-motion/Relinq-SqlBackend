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
using System;
using System.Data.Linq.Mapping;

namespace Remotion.Linq.UnitTests.LinqToSqlAdapter.TestDomain
{
  [Table (Name = "Person")]
  internal class PersonTestClass
  {
    public PersonTestClass ()
    {
    }

    public PersonTestClass (string first, int age)
    {
      First = first;
      Age = age;
    }

    [Column (Name = "FirstName", IsPrimaryKey = true)]
    public string First { get; set; }

    [Column (Name = "Age")]
    public int Age { get; set; }

    public override bool Equals (object obj)
    {
      if (obj == null || GetType() != obj.GetType())
        return false;

      if (!((PersonTestClass) obj).First.Equals (First))
        return false;
      if (!((PersonTestClass) obj).Age.Equals (Age))
        return false;
      return true;
    }

    public override int GetHashCode ()
    {
      throw new NotImplementedException();
    }
  }
}