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
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Remotion.Linq.SqlBackend.UnitTests.NUnit;
using Remotion.Linq.SqlBackend.Utilities;

namespace Remotion.Linq.SqlBackend.UnitTests.Utilities
{
  [TestFixture]
  public class ReflectionUtilityTest
  {
    [Test]
    public void GetItemTypeOfClosedGenericIEnumerable_ArgumentImplementsIEnumerable ()
    {
      Assert.That (ReflectionUtility.GetItemTypeOfClosedGenericIEnumerable (typeof (List<int>), "x"), Is.SameAs (typeof (int)));
    }

    [Test]
    public void GetItemTypeOfClosedGenericIEnumerable_ArgumentIsIEnumerable ()
    {
      Assert.That (ReflectionUtility.GetItemTypeOfClosedGenericIEnumerable (typeof (IEnumerable<int>), "x"), Is.SameAs (typeof (int)));
      Assert.That (ReflectionUtility.GetItemTypeOfClosedGenericIEnumerable (typeof (IEnumerable<IEnumerable<string>>), "x"), Is.SameAs (typeof (IEnumerable<string>)));
    }

    [Test]
    public void GetItemTypeOfClosedGenericIEnumerable_NonGenericIEnumerable_ThrowsArgumentException ()
    {
      Assert.That (
          () => ReflectionUtility.GetItemTypeOfClosedGenericIEnumerable (typeof (ArrayList), "x"),
          Throws.ArgumentException.With.ArgumentExceptionMessageEqualTo (
              "Expected a closed generic type implementing IEnumerable<T>, but found 'System.Collections.ArrayList'.", "x"));
    }

    [Test]
    public void GetItemTypeOfClosedGenericIEnumerable_InvalidType_ThrowsArgumentException ()
    {
      Assert.That (
          () => ReflectionUtility.GetItemTypeOfClosedGenericIEnumerable (typeof (int), "x"),
          Throws.ArgumentException.With.ArgumentExceptionMessageEqualTo (
              "Expected a closed generic type implementing IEnumerable<T>, but found 'System.Int32'.", "x"));
    }

    [Test]
    public void GetMemberReturnType_Field ()
    {
      var memberInfo = typeof (DateTime).GetField ("MinValue");

      var type = ReflectionUtility.GetMemberReturnType (memberInfo);
      Assert.That (type, Is.SameAs (typeof (DateTime)));
    }

    [Test]
    public void GetMemberReturnType_Property ()
    {
      var memberInfo = typeof (DateTime).GetProperty ("Now");

      var type = ReflectionUtility.GetMemberReturnType (memberInfo);
      Assert.That (type, Is.SameAs (typeof (DateTime)));
    }

    [Test]
    public void GetMemberReturnType_Method ()
    {
      var memberInfo = typeof (DateTime).GetMethod ("get_Now");

      var type = ReflectionUtility.GetMemberReturnType (memberInfo);
      Assert.That (type, Is.SameAs (typeof (DateTime)));
    }

    [Test]
    public void GetMemberReturnType_Other_Throws ()
    {
      var memberInfo = typeof (DateTime);

      Assert.That (
          () => ReflectionUtility.GetMemberReturnType (memberInfo),
          Throws.ArgumentException.With.ArgumentExceptionMessageEqualTo ("Argument must be FieldInfo, PropertyInfo, or MethodInfo.", "member"));
    }
  }
}
