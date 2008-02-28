using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rubicon.Collections;
using Rubicon.Data.Linq.DataObjectModel;
using Rubicon.Data.Linq.SqlGeneration;
using Rubicon.Data.Linq.SqlGeneration.SqlServer;

namespace Rubicon.Data.Linq.UnitTests.SqlGenerationTest.SqlServer
{
  [TestFixture]
  public class SelectBuilderTest
  {
    [Test]
    public void CombineColumnItems()
    {
      StringBuilder commandText = new StringBuilder ();
      SelectBuilder selectBuilder = new SelectBuilder (commandText);

      List<Column> columns = new List<Column> {
        new Column (new Table ("s1", "s1"), "c1"),
        new Column (new Table ("s2", "s2"), "c2"),
        new Column (new Table ("s3", "s3"), "c3")
      };

      selectBuilder.BuildSelectPart (columns,false);
      Assert.AreEqual ("SELECT [s1].[c1], [s2].[c2], [s3].[c3] ", commandText.ToString ());
    }

    [Test]
    [ExpectedException (typeof (System.InvalidOperationException), ExpectedMessage = "The query does not select any fields from the data source.")]
    public void NoColumns()
    {
      StringBuilder commandText = new StringBuilder ();
      SelectBuilder selectBuilder = new SelectBuilder (commandText);
      List<Column> columns = new List<Column>();

      selectBuilder.BuildSelectPart (columns,false);
    }

    [Test]
    public void DistinctSelect ()
    {
      StringBuilder commandText = new StringBuilder ();
      SelectBuilder selectBuilder = new SelectBuilder(commandText);

      List<Column> columns = new List<Column> {
        new Column (new Table ("s1", "s1"), "c1"),
        new Column (new Table ("s2", "s2"), "c2")
      };

      selectBuilder.BuildSelectPart (columns, true);

      Assert.AreEqual ("SELECT DISTINCT [s1].[c1], [s2].[c2] ", commandText.ToString());

    }
  }
}