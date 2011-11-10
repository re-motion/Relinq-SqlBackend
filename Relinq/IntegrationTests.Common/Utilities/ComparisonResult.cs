// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (C) rubicon IT GmbH, www.rubicon.eu
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
using System.Text;

namespace Remotion.Linq.IntegrationTests.Common.Utilities
{
  /// <summary>
  /// Provides a visual command line comparison of to results
  /// </summary>
  public class ComparisonResult
  {
    private readonly bool _isEqual;
    private readonly string _expected;
    private readonly string _actual;

    public static readonly int ColumnLength = 70; // line length: pipe in the middle + 4 numbers + braces + space: 148
    public static readonly string NumberFormatString = "0000";

    private static string[] SplitString (string stringToSplit)
    {
      return stringToSplit.Split (new[] { Environment.NewLine }, StringSplitOptions.None);
    }

    private static void AppendRow (StringBuilder output, int lineNumber, string leftColumn, string rightColumn)
    {
      string lineNumberString = lineNumber.ToString (NumberFormatString);
      output.AppendLine ("(" + lineNumberString + ") " + PadAndCutColumn (leftColumn) + "|" + PadAndCutColumn (rightColumn));
    }

    private static string PadAndCutColumn (string stringToPad)
    {
      string paddedString = stringToPad.PadRight (ColumnLength);
      return paddedString.Substring (0, ColumnLength);
    }

    private static string GetTableHead ()
    {
      string tableHead = "(lines)";
      tableHead += PadAndCutColumn (" expected")  + "|";
      tableHead += PadAndCutColumn (" actual");
      return tableHead;
    }

    public ComparisonResult (string expected, string actual)
    {
      _isEqual = (expected == actual);
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
          AppendRow (output, i + 1, expectedLines[i], actualLines[i]);
      }

      for (int i = minLength; i < maxLength; i++)
      {
        if (expectedIsShorter)
          AppendRow (output, i + 1, string.Empty, actualLines[i]);
        else
          AppendRow (output, i + 1, expectedLines[i], string.Empty);
      }

      return output.ToString();
    }
  }
}