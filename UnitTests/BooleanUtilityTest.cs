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
using Remotion.Linq.SqlBackend.UnitTests.NUnit;

namespace Remotion.Linq.SqlBackend.UnitTests
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
          Throws.ArgumentException.With.ArgumentExceptionMessageEqualTo ("Type must be Boolean or Nullable<Boolean>.", "type"));
    }

    [Test]
    public void GetMatchingBoolType ()
    {
      Assert.That (BooleanUtility.GetMatchingBoolType (typeof (int)), Is.EqualTo (typeof (bool)));
      Assert.That (BooleanUtility.GetMatchingBoolType (typeof (int?)), Is.EqualTo (typeof (bool?)));
      Assert.That (
          () => BooleanUtility.GetMatchingBoolType (typeof (bool)),
          Throws.ArgumentException.With.ArgumentExceptionMessageEqualTo ("Type must be Int32 or Nullable<Int32>.", "type"));
    }

    [Test]
    public void ConvertNullableIntToNullableBool ()
    {
      Assert.That (BooleanUtility.ConvertNullableIntToNullableBool (null), Is.EqualTo (null));
      Assert.That (BooleanUtility.ConvertNullableIntToNullableBool (0), Is.EqualTo (false));
      Assert.That (BooleanUtility.ConvertNullableIntToNullableBool (1), Is.EqualTo (true));
    }

    [Test]
    public void GetIntToBoolConversionMethod ()
    {
      Assert.That (
          BooleanUtility.GetIntToBoolConversionMethod (typeof (int)),
          Is.Not.Null.And.EqualTo (typeof (Convert).GetMethod ("ToBoolean", new[] { typeof (int) })));
      Assert.That (
          BooleanUtility.GetIntToBoolConversionMethod (typeof (int?)),
          Is.Not.Null.And.EqualTo (typeof (BooleanUtility).GetMethod ("ConvertNullableIntToNullableBool", new[] { typeof (int?) })));
      Assert.That (
          () => BooleanUtility.GetIntToBoolConversionMethod (typeof (bool)),
          Throws.ArgumentException.With.ArgumentExceptionMessageEqualTo ("Type must be Int32 or Nullable<Int32>.", "intType"));
    }
  }
}