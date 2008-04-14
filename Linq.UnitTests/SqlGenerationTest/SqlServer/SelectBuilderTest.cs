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
      CommandBuilder commandBuilder = new CommandBuilder (new StringBuilder (), new List<CommandParameter> ());
      SelectBuilder selectBuilder = new SelectBuilder (commandBuilder);

      List<IEvaluation> evaluations = new List<IEvaluation> {
        new Column (new Table ("s1", "s1"), "c1"),
        new Column (new Table ("s2", "s2"), "c2"),
        new Column (new Table ("s3", "s3"), "c3")
      };

      selectBuilder.BuildSelectPart (evaluations, false);
      Assert.AreEqual ("SELECT [s1].[c1], [s2].[c2], [s3].[c3] ", commandBuilder.GetCommandText());
    }

    [Test]
    [ExpectedException (typeof (System.InvalidOperationException), ExpectedMessage = "The query does not select any fields from the data source.")]
    public void NoColumns()
    {
      CommandBuilder commandBuilder = new CommandBuilder (new StringBuilder (), new List<CommandParameter> ());
      SelectBuilder selectBuilder = new SelectBuilder (commandBuilder);
      List<IEvaluation> evaluations = new List<IEvaluation>();

      selectBuilder.BuildSelectPart (evaluations,false);
    }

    [Test]
    public void DistinctSelect ()
    {
      CommandBuilder commandBuilder = new CommandBuilder (new StringBuilder (), new List<CommandParameter> ());
      SelectBuilder selectBuilder = new SelectBuilder (commandBuilder);

      List<IEvaluation> evaluations = new List<IEvaluation> {
        new Column (new Table ("s1", "s1"), "c1"),
        new Column (new Table ("s2", "s2"), "c2")
      };

      selectBuilder.BuildSelectPart (evaluations, true);

      Assert.AreEqual ("SELECT DISTINCT [s1].[c1], [s2].[c2] ", commandBuilder.GetCommandText ());

    }
  }
}