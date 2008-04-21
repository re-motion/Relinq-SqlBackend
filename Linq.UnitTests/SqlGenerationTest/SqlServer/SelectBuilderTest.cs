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
    private CommandBuilder _commandBuilder;
    private SelectBuilder _selectBuilder;

    [SetUp]
    public void SetUp ()
    {
      _commandBuilder = new CommandBuilder (new StringBuilder (), new List<CommandParameter> (), StubDatabaseInfo.Instance);
      _selectBuilder = new SelectBuilder (_commandBuilder);
    }

    [Test]
    public void CombineColumnItems()
    {
      
      List<IEvaluation> evaluations = new List<IEvaluation> {
        new Column (new Table ("s1", "s1"), "c1"),
        new Column (new Table ("s2", "s2"), "c2"),
        new Column (new Table ("s3", "s3"), "c3")
      };

      _selectBuilder.BuildSelectPart (evaluations, false);
      Assert.AreEqual ("SELECT [s1].[c1], [s2].[c2], [s3].[c3] ", _commandBuilder.GetCommandText());
    }

    [Test]
    [ExpectedException (typeof (System.InvalidOperationException), ExpectedMessage = "The query does not select any fields from the data source.")]
    public void NoColumns()
    {

      List<IEvaluation> evaluations = new List<IEvaluation>();

      _selectBuilder.BuildSelectPart (evaluations,false);
    }

    [Test]
    public void DistinctSelect ()
    {

      List<IEvaluation> evaluations = new List<IEvaluation> {
        new Column (new Table ("s1", "s1"), "c1"),
        new Column (new Table ("s2", "s2"), "c2")
      };

      _selectBuilder.BuildSelectPart (evaluations, true);

      Assert.AreEqual ("SELECT DISTINCT [s1].[c1], [s2].[c2] ", _commandBuilder.GetCommandText ());
    }

    [Test]
    [Ignore]
    public void BinaryEvaluations_Add ()
    {
      Column c1 = new Column (new Table ("s1", "s1"), "c1");
      Column c2 = new Column (new Table ("s2", "s2"), "c2");
      
      BinaryEvaluation binaryEvaluation = new BinaryEvaluation(c1,c2,BinaryEvaluation.EvaluationKind.Add);

      List<IEvaluation> evaluations = new List<IEvaluation> { binaryEvaluation };

      _selectBuilder.BuildSelectPart (evaluations, true);
    }
  }
}