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
using System.Text;
using System.Linq;

namespace Remotion.Data.Linq.IntegrationTests.Utilities
{
  public class ComparisonResult
  {
    private readonly bool _isEqual;
    private readonly string _expected;
    private readonly string _actual;

    // TODO Review: Coding guidelines: public static fields should start with capital letters (ColumnLength); 
    // TODO Review: Coding guidelines: All static members (static fields, class constructors, static properties, static methods) should be at the top of the class
    public static readonly int columnLength = 70; // line length: pipe in the middle + 4 numbers + braces + space: 148
    public static readonly int numberWidth = 4; // TODO Review: Remove when NumberFormatString is used instead
    public static readonly int placeHolderWidth = 3; // braces + space // TODO Review: Remove this, it's not used by this class
    public static readonly char paddingChar = ' '; // TODO Review: ' ' is the default for the PadRight method, remove the constant and use the default overload

    public static readonly string NumberFormatString = new string ('0', numberWidth);

    public ComparisonResult (bool isEqual, string expected, string actual)
    {
      _isEqual = isEqual;
      _actual = actual;
      _expected = expected;
    }

    public string Actual
    {
      get { return _actual; }
    }

    public string Expected
    {
      get { return _expected; }
    }

    public bool IsEqual
    {
      get { return _isEqual; }
    }

    public string GetDiffSet ()
    {
      var output = new StringBuilder();
      output.AppendLine (GetTableHead());

      string[] expectedLines = SplitString (_expected);
      string[] actualLines = SplitString (_actual);

      bool expectedIsShorter = (expectedLines.Length < actualLines.Length);
      int minLength = expectedIsShorter ? expectedLines.Length : actualLines.Length;
      int maxLength = expectedIsShorter ? actualLines.Length : expectedLines.Length;

      for (int i = 0; i < minLength; i++)
      {
        if (!expectedLines[i].Equals (actualLines[i]))
          PadAndCutString (ref output, i + 1, expectedLines[i], actualLines[i]);
      }

      for (int i = minLength; i < maxLength; i++)
      {
        if (expectedIsShorter)
          PadAndCutString (ref output, i + 1, string.Empty, actualLines[i]);
        else
          PadAndCutString (ref output, i + 1, expectedLines[i], string.Empty);
      }

      return output.ToString();
    }

    private string[] SplitString (string stringToSplit)
    {
      return stringToSplit.Split (new[] { Environment.NewLine }, StringSplitOptions.None);
    }

    // TODO Review: ref is not needed
    // TODO Review: PadAndCutString sounds like a a method returning a (padded and cut) string, which this method does not do. Therefore, rename to "AppendRow" ("Append" is usually used in conjunction with string builders; "Row" because the method acts on a full diff set row, not a single string).
    private void PadAndCutString (ref StringBuilder output, int lineNumber, string leftColumn, string rightColumn)
    {
      string lineNumberString = GetNumberString (lineNumber);
      output.AppendLine ("(" + lineNumberString + ") " + PadAndCutColumn (leftColumn) + "|" + PadAndCutColumn (rightColumn));
    }

    private string GetNumberString (int number)
    {
      // TODO Review: Replace with:  number.ToString (NumberFormatString);
      // TODO Review: Then, inline this method
      return number.ToString ().PadLeft (numberWidth, '0');
    }

    private string PadAndCutColumn (string stringToPad)
    {
      string paddedString = stringToPad.PadRight (columnLength, paddingChar);

      // TODO Review: The check is not necessary, simply return "paddedString.Substring (0, columnLength)"
      return paddedString.Length > columnLength ? paddedString.Substring (0, columnLength) : paddedString;
    }

    // TODO Review: Make private and non-static - this is not really part of the public API of this class. The tests that use is can be adapted to use a helper method defined in the test fixture that returns "(lines) expected     | actual     "
    public static string GetTableHead ()
    {
      string tableHead = "(lines)";
      // TODO: Why not use PadAndCutColumn?
      tableHead += " expected".PadRight (columnLength, paddingChar) + "|";
      tableHead += " actual".PadRight (columnLength, paddingChar);
      return tableHead;
    }
  }
}