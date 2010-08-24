using System;
using NUnit.Framework;
using Remotion.Data.Linq.IntegrationTests.Utilities;

namespace Remotion.Data.Linq.IntegrationTests.UnitTests.Utilities
{
  [TestFixture]
  public class ComparisonResultTest
  {
    // TODO Review: Use a helper method to make the tests more readable:
    //private string MakeDiffSet (params string[][] lines)
    // {
    //   var result = ComparisonResult.GetTableHead () + Environment.NewLine;
    //   foreach (var line in lines)
    //   {
    //     result += line[0];
    //     result += " ";
    //     result += line[1].PadRight (70);
    //     result += "|";
    //     result += line[2].PadRight (70);
    //     result += Environment.NewLine;
    //   }
    //   return result;
    // }

    // Single line: string expectedDiffSet = MakeDiffSet (new[] { "(0002)", "line is not same", "line is not the same" });
    // Multiple lines: 
    //   string expectedDiffSet = MakeDiffSet (
    //      new[] { "(0002)", "line is not same", "line is not the same" }, 
    //      new[] { "(0003)", "...", "...." });

    // TODO Review: Then remove these static members. In tests, it's often better to inline constants rather than to use the constants defined by the classes being tested. That way, mistakes are found more easily.
    private static readonly int _leftColumnPadding = ComparisonResult.columnLength + ComparisonResult.numberWidth + ComparisonResult.placeHolderWidth;
    private static readonly int _rightColumnPadding = ComparisonResult.columnLength;
    private static readonly char _paddingChar = ComparisonResult.paddingChar;

    [Test]
    public void GetDiffSet_OneLineNotEqual()
    {
      string expected = "line is same" + Environment.NewLine
                        + "line not same";
      string actual = "line is same" + Environment.NewLine
                      + "line is not the same";

      var comparisonResult = new ComparisonResult (false, expected, actual);

      string expectedDiffSet = ComparisonResult.GetTableHead() + Environment.NewLine;
      expectedDiffSet += "(0002) line not same".PadRight (_leftColumnPadding, _paddingChar) + "|"
                         + "line is not the same".PadRight (_rightColumnPadding, _paddingChar)
                         + Environment.NewLine;

      Assert.AreEqual (expectedDiffSet, comparisonResult.GetDiffSet());
    }

    [Test]
    public void GetDiffSet_ActualHasMoreLines()
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
    public void GetDiffSet_ExpectedHasMoreLines()
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
    public void GetDiffSet_BreakLongLines()
    {
      string actual = "long line test";
      string expected = "long line test".PadRight (_rightColumnPadding, _paddingChar) + "test";

      ComparisonResult comparisonResult = new ComparisonResult (false, expected, actual);

      string expectedDiffSet = ComparisonResult.GetTableHead() + Environment.NewLine;

      expectedDiffSet += "(0001) long line test".PadRight (_leftColumnPadding, _paddingChar) + "|"
                         + "long line test".PadRight (_rightColumnPadding, _paddingChar) + Environment.NewLine;

      Assert.AreEqual (expectedDiffSet, comparisonResult.GetDiffSet());
    }
  }
}