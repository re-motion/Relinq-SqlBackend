using System;
using NUnit.Framework;
using Remotion.Data.Linq.IntegrationTests.Utilities;

namespace Remotion.Data.Linq.IntegrationTests.UnitTests.Utilities
{
  [TestFixture]
  public class ComparisonResultTest
  {
    private static string MakeDiffSet (params string[][] lines)
    {
      var result = GetTableHead () + Environment.NewLine;
      foreach (var line in lines)
      {
        result += line[0].PadRight (77);
        result += "|";
        result += line[1].PadRight (70);
        result += Environment.NewLine;
      }
      return result;
    }

    private static string GetTableHead ()
    {
      string tableHead = "(lines)";
      tableHead += " expected".PadRight (70) + "|";
      tableHead += " actual".PadRight (70);
      return tableHead;
    }

    [Test]
    public void GetDiffSet_OneLineNotEqual()
    {
      string expected = "line is same" + Environment.NewLine
                        + "line not same";
      string actual = "line is same" + Environment.NewLine
                      + "line is not the same";

      var comparisonResult = new ComparisonResult (expected, actual);

      string expectedDiffSet = MakeDiffSet (new[] { "(0002) line not same", "line is not the same" });

      Assert.AreEqual (expectedDiffSet, comparisonResult.GetDiffSet());
    }

    [Test]
    public void GetDiffSet_ActualHasMoreLines()
    {
      string expected = "line is not same";
      string actual = "line is not the same" + Environment.NewLine
                      + "there is one line more here";

      ComparisonResult comparisonResult = new ComparisonResult (expected, actual);

      string expectedDiffSet = MakeDiffSet (
          new[] { "(0001) line is not same", "line is not the same" }
          ,
          new[] { "(0002) ", "there is one line more here" });

      Assert.AreEqual (expectedDiffSet, comparisonResult.GetDiffSet());
    }

    [Test]
    public void GetDiffSet_ExpectedHasMoreLines()
    {
      string actual = "line is not same";
      string expected = "line is not the same" + Environment.NewLine
                        + "there is one line more here";

      ComparisonResult comparisonResult = new ComparisonResult (expected, actual);

      string expectedDiffSet =  MakeDiffSet (
          new[] { "(0001) line is not the same",  "line is not same" }
          ,
          new[] { "(0002) there is one line more here", string.Empty });

      Assert.AreEqual (expectedDiffSet, comparisonResult.GetDiffSet());
    }

    [Test]
    public void GetDiffSet_BreakLongLines()
    {
      string actual = "long line test";
      string expected = "long line test".PadRight (70) + "test";

      ComparisonResult comparisonResult = new ComparisonResult (expected, actual);

      string expectedDiffSet = MakeDiffSet (new[]{"(0001) long line test","long line test"});

      Assert.AreEqual (expectedDiffSet, comparisonResult.GetDiffSet());
    }
  }
}