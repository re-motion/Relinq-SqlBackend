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
  public class ComparisonResultTest
  {
    private static readonly int _leftColumnPadding = ComparisonResult.columnLength + ComparisonResult.numberWidth + ComparisonResult.placeHolderWidth;
    private static readonly int _rightColumnPadding = ComparisonResult.columnLength;
    private static readonly char _paddingChar = ComparisonResult.paddingChar;

    [Test]
    public void GetDiffSet_OneLineNotEqual ()
    {
      string expected = "line is same" + Environment.NewLine
                        + "line not same";
      string actual = "line is same" + Environment.NewLine
                      + "line is not the same";

      ComparisonResult comparisonResult = new ComparisonResult (false, expected, actual);


      string expectedDiffSet = ComparisonResult.GetTableHead() + Environment.NewLine;
      expectedDiffSet += "(0002) line not same".PadRight (_leftColumnPadding, _paddingChar) + "|"
                         + "line is not the same".PadRight (_rightColumnPadding, _paddingChar)
                         + Environment.NewLine;

      Assert.AreEqual (expectedDiffSet, comparisonResult.GetDiffSet());
    }

    [Test]
    public void GetDiffSet_ActualHasMoreLines ()
    {
      string expected = "line is not same";
      string actual = "line is not the same" + Environment.NewLine
                      + "there is one line more here";

      ComparisonResult comparisonResult = new ComparisonResult (false, expected, actual);

      string expectedDiffSet = ComparisonResult.GetTableHead() + Environment.NewLine;


      expectedDiffSet += "(0001) line is not same".PadRight (_leftColumnPadding, _paddingChar) + "|"
                         + "line is not the same".PadRight (_rightColumnPadding, _paddingChar) + Environment.NewLine;
      expectedDiffSet += "(0002) ".PadRight (_leftColumnPadding, _paddingChar) + "|"
                         + "there is one line more here".PadRight (_rightColumnPadding, _paddingChar)
                         + Environment.NewLine;

      Assert.AreEqual (expectedDiffSet, comparisonResult.GetDiffSet());
    }

    [Test]
    public void GetDiffSet_ExpectedHasMoreLines ()
    {
      string actual = "line is not same";
      string expected = "line is not the same" + Environment.NewLine
                        + "there is one line more here";

      ComparisonResult comparisonResult = new ComparisonResult (false, expected, actual);

      string expectedDiffSet = ComparisonResult.GetTableHead() + Environment.NewLine;

      expectedDiffSet += "(0001) line is not the same".PadRight (_leftColumnPadding, _paddingChar) + "|"
                         + "line is not same".PadRight (_rightColumnPadding, _paddingChar) + Environment.NewLine;
      expectedDiffSet += "(0002) there is one line more here".PadRight (_leftColumnPadding, _paddingChar) + "|"
                         + String.Empty.PadRight (_rightColumnPadding, _paddingChar)
                         + Environment.NewLine;

      Assert.AreEqual (expectedDiffSet, comparisonResult.GetDiffSet());
    }

    [Test]
    public void GetDiffSet_BreakLongLines ()
    {
      string actual = "long line test";
      string expected = "long line test".PadRight (_rightColumnPadding, _paddingChar) + "test";

      ComparisonResult comparisonResult = new ComparisonResult (false, expected, actual);

      string expectedDiffSet = ComparisonResult.GetTableHead () + Environment.NewLine;

      expectedDiffSet += "(0001) long line test".PadRight (_leftColumnPadding, _paddingChar) + "|"
                         + "long line test".PadRight (_rightColumnPadding, _paddingChar) + Environment.NewLine;

      Assert.AreEqual (expectedDiffSet, comparisonResult.GetDiffSet ());
    }
  }
}