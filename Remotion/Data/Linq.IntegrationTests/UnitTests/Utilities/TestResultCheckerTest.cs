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
using NUnit.Framework;
using Remotion.Data.Linq.IntegrationTests.Utilities;

namespace Remotion.Data.Linq.IntegrationTests.UnitTests.Utilities
{
  [TestFixture]
  public class TestResultCheckerTest
  {
    [Test]
    public void Check_OneLineValue ()
    {
      var test_expected = "should be";
      var test_actual = "should be";

      var result = TestResultChecker.Check (test_expected, test_actual);

      Assert.That (result, Is.EqualTo (true));
    }

    [Test]
    public void Check_OneLineValueFalse ()
    {
      var test_expected = "should be";
      var test_actual = "";

      var result = TestResultChecker.Check (test_expected, test_actual);

      Assert.That (result, Is.EqualTo (false));
    }

    [Test]
    public void Check_MultiLineValue()
    {
      var test_expected = "should be" + Environment.NewLine
        + "including second line";
      var test_actual = "should be" + Environment.NewLine
        + "including second line";

      var result = TestResultChecker.Check (test_expected, test_actual);

      Assert.That (result, Is.EqualTo (true));
    }
    
    [Test]
    public void Check_MultiLineValueFalse ()
    {
      var test_expected = "should be" + Environment.NewLine
        + "including second line";
      var test_actual = "should be" + Environment.NewLine;

      var result = TestResultChecker.Check (test_expected, test_actual);

      Assert.That (result, Is.EqualTo (false));
    }
  }
}