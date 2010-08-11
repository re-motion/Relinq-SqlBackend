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

namespace Remotion.Data.Linq.IntegrationTests.Utilities
{
  public class ComparisonResult
  {
    private readonly bool _isEqual;
    private readonly string _expected;
    private readonly string _actual;

    public static readonly int columnLength = 70; // line length: pipe in the middle + 4 numbers + braces + space: 148
    public static readonly int numberWidth = 4;
    public static readonly int placeHolderWidth = 3; // braces + space
    public static readonly char paddingChar = ' ';

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

    private static string GetNumberString (int number)
    {
      return number.ToString().PadLeft (numberWidth, '0');
    }

    private string[] SplitString (string stringToSplit)
    {
      return stringToSplit.Split (new[] { Environment.NewLine }, StringSplitOptions.None);
    }


    private void PadAndCutString (ref StringBuilder output, int lineNumber, string leftColumn, string rightColumn)
    {
      string numberString = GetNumberString (lineNumber);
      leftColumn = PadAndCutColumn (leftColumn);
      rightColumn = PadAndCutColumn (rightColumn);

      output.AppendLine ("(" + numberString + ") " + leftColumn + "|" + rightColumn);
    }

    private string PadAndCutColumn (string stringToPad)
    {
      string paddedString = stringToPad.PadRight (columnLength, paddingChar);

      int actualLength = paddedString.Length;
      int maxLength = ComparisonResult.columnLength;
      return actualLength > maxLength ? paddedString.Substring (0, maxLength) : paddedString;
    }

    public static string GetTableHead ()
    {
      string tableHead = "(lines)";
      tableHead += " expected".PadRight (columnLength, paddingChar) + "|";
      tableHead += " actual".PadRight (columnLength, paddingChar);
      return tableHead;
    }
  }
}