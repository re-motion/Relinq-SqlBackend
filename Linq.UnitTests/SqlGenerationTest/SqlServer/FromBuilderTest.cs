using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using Rhino.Mocks.Interfaces;
using Rubicon.Collections;
using Rubicon.Data.Linq.Parsing;
using Rubicon.Data.Linq.SqlGeneration;
using Rubicon.Data.Linq.SqlGeneration.SqlServer;
using Rubicon.Data.Linq.DataObjectModel;
using NUnit.Framework.SyntaxHelpers;
using Rubicon.Development.UnitTesting;

namespace Rubicon.Data.Linq.UnitTests.SqlGenerationTest.SqlServer
{
  [TestFixture]
  public class FromBuilderTest
  {
    [Test]
    public void CombineTables_SelectsJoinsPerTable()
    {
      CommandBuilder commandBuilder = new CommandBuilder (new StringBuilder (), new List<CommandParameter> ());
      FromBuilder fromBuilder = new FromBuilder (commandBuilder, StubDatabaseInfo.Instance);

      Table table1 = new Table ("s1", "s1_alias");
      Table table2 = new Table ("s2", "s2_alias");
      Column column1 = new Column (table1, "c1");
      Column column2 = new Column (table2, "c2");

      List<IFromSource> tables = new List<IFromSource> { table1 }; // this table does not have a join associated with it
      JoinCollection joins = new JoinCollection ();
      SingleJoin join = new SingleJoin (column2, column1);
      joins.AddPath (new FieldSourcePath(table2, new [] { join }));
      fromBuilder.BuildFromPart (tables, joins);

      Assert.AreEqual ("FROM [s1] [s1_alias]", commandBuilder.GetCommandText());
    }

    [Test]
    public void CombineTables_WithJoin ()
    {
      CommandBuilder commandBuilder = new CommandBuilder (new StringBuilder (), new List<CommandParameter> ());
      FromBuilder fromBuilder = new FromBuilder (commandBuilder, StubDatabaseInfo.Instance);

      Table table1 = new Table ("s1", "s1_alias");
      Table table2 = new Table ("s2", "s2_alias");
      Column column1 = new Column (table1, "c1");
      Column column2 = new Column (table2, "c2");

      List<IFromSource> tables = new List<IFromSource> { table2 };
      JoinCollection joins = new JoinCollection();
      SingleJoin join = new SingleJoin (column2, column1);
      joins.AddPath (new FieldSourcePath (table2, new[] { join }));
      fromBuilder.BuildFromPart (tables, joins);

      Assert.AreEqual ("FROM [s2] [s2_alias] LEFT OUTER JOIN [s1] [s1_alias] ON [s2_alias].[c2] = [s1_alias].[c1]", commandBuilder.GetCommandText ());
    }

    [Test]
    public void CombineTables_WithNestedJoin ()
    {
      CommandBuilder commandBuilder = new CommandBuilder (new StringBuilder (), new List<CommandParameter> ());
      FromBuilder fromBuilder = new FromBuilder (commandBuilder, StubDatabaseInfo.Instance);

      // table2.table1.table3

      Table table1 = new Table ("s1", "s1_alias");
      Table table2 = new Table ("s2", "s2_alias");
      Table table3 = new Table ("s3", "s3_alias");
      Column column1 = new Column (table1, "c1");
      Column column2 = new Column (table2, "c2");
      Column column3 = new Column (table1, "c1'");
      Column column4 = new Column (table3, "c3");

      List<IFromSource> tables = new List<IFromSource> { table2 };

      JoinCollection joins = new JoinCollection();

      SingleJoin join1 = new SingleJoin (column2, column1);
      SingleJoin join2 = new SingleJoin (column3, column4);
      joins.AddPath (new FieldSourcePath (table2, new[] { join1, join2 }));

      fromBuilder.BuildFromPart (tables, joins);

      Assert.AreEqual ("FROM [s2] [s2_alias] "
        + "LEFT OUTER JOIN [s1] [s1_alias] ON [s2_alias].[c2] = [s1_alias].[c1] "
        + "LEFT OUTER JOIN [s3] [s3_alias] ON [s1_alias].[c1'] = [s3_alias].[c3]", commandBuilder.GetCommandText());
    }

    [Test]
    public void CombineTables_WithSubqueries ()
    {
      MockRepository mockRepository = new MockRepository ();

      SubQuery subQuery = new SubQuery (ExpressionHelper.CreateQueryModel (), "sub_alias");
      Table table1 = new Table ("s1", "s1_alias");
      List<IFromSource> tables = new List<IFromSource> { table1, subQuery };

      CommandBuilder commandBuilder = new CommandBuilder (new StringBuilder (), new List<CommandParameter> ());
      commandBuilder.AddParameter (1);

      FromBuilder fromBuilderMock = mockRepository.CreateMock<FromBuilder> (commandBuilder, StubDatabaseInfo.Instance);
      SqlGeneratorBase subQueryGeneratorMock = mockRepository.CreateMock<SqlGeneratorBase> (subQuery.QueryModel, StubDatabaseInfo.Instance, ParseContext.SubQueryInFrom);

      Expect.Call (PrivateInvoke.InvokeNonPublicMethod (fromBuilderMock, "CreateSqlGeneratorForSubQuery", subQuery, StubDatabaseInfo.Instance,
          commandBuilder)).Return (subQueryGeneratorMock);
      Expect.Call (subQueryGeneratorMock.BuildCommandString ()).Do ((Func<Tuple<string, CommandParameter[]>>) delegate
      {
        commandBuilder.Append ("x");
        commandBuilder.AddParameter (0);
        return null;
      });

      mockRepository.ReplayAll ();
      fromBuilderMock.BuildFromPart (tables, new JoinCollection());
      mockRepository.VerifyAll ();

      Assert.AreEqual ("FROM [s1] [s1_alias] CROSS APPLY (x) [sub_alias]", commandBuilder.GetCommandText ());
      Assert.That (commandBuilder.GetCommandParameters(), Is.EqualTo (new[] {new CommandParameter ("@1", 1), new CommandParameter ("@2", 0)}));
    }

    [Test]
    public void CreateSqlGeneratorForSubQuery ()
    {
      SubQuery subQuery = new SubQuery (ExpressionHelper.CreateQueryModel (), "sub_alias");
      CommandBuilder commandBuilder = new CommandBuilder (new StringBuilder (), new List<CommandParameter> ());
      FromBuilder fromBuilder = new FromBuilder (commandBuilder, StubDatabaseInfo.Instance);
      SqlGeneratorBase subQueryGenerator = (SqlGeneratorBase) PrivateInvoke.InvokeNonPublicMethod (fromBuilder, "CreateSqlGeneratorForSubQuery", 
          subQuery, StubDatabaseInfo.Instance, commandBuilder);
      Assert.AreSame (subQuery.QueryModel, subQueryGenerator.QueryModel);
      Assert.AreSame (StubDatabaseInfo.Instance, subQueryGenerator.DatabaseInfo);
      Assert.AreEqual (ParseContext.SubQueryInFrom, subQueryGenerator.ParseContext);
    }
  }
}