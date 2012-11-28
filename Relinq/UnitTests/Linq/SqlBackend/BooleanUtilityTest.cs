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
using NUnit.Framework;
using Remotion.Linq.SqlBackend;

namespace Remotion.Linq.UnitTests.Linq.SqlBackend
{
  [TestFixture]
  public class BooleanUtilityTest
  {
    [Test]
    public void IsBooleanType ()
    {
      Assert.That (BooleanUtility.IsBooleanType (typeof (bool)), Is.True);
      Assert.That (BooleanUtility.IsBooleanType (typeof (bool?)), Is.True);
      Assert.That (BooleanUtility.IsBooleanType (typeof (int)), Is.False);
      Assert.That (BooleanUtility.IsBooleanType (typeof (int?)), Is.False);
    }

    [Test]
    public void GetMatchingIntType ()
    {
      Assert.That (BooleanUtility.GetMatchingIntType (typeof (bool)), Is.EqualTo (typeof (int)));
      Assert.That (BooleanUtility.GetMatchingIntType (typeof (bool?)), Is.EqualTo (typeof (int?)));
      Assert.That (
          () => BooleanUtility.GetMatchingIntType (typeof (int)),
          Throws.ArgumentException.With.Message.EqualTo ("Type must be Boolean or Nullable<Boolean>.\r\nParameter name: type"));
    }

    [Test]
    public void GetMatchingBoolType ()
    {
      Assert.That (BooleanUtility.GetMatchingBoolType (typeof (int)), Is.EqualTo (typeof (bool)));
      Assert.That (BooleanUtility.GetMatchingBoolType (typeof (int?)), Is.EqualTo (typeof (bool?)));
      Assert.That (
          () => BooleanUtility.GetMatchingBoolType (typeof (bool)),
          Throws.ArgumentException.With.Message.EqualTo ("Type must be Int32 or Nullable<Int32>.\r\nParameter name: type"));
    }
  }
}