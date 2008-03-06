using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rubicon.Collections;
using Rubicon.Data.Linq.SqlGeneration;
using Rubicon.Data.Linq.SqlGeneration.SqlServer;
using Rubicon.Data.Linq.DataObjectModel;

namespace Rubicon.Data.Linq.UnitTests.SqlGenerationTest.SqlServer
{
  [TestFixture]
  public class FromBuilderTest
  {
    [Test]
    public void CombineTables_SelectsJoinsPerTable()
    {
      CommandBuilder commandBuilder = new CommandBuilder (new StringBuilder (), new List<CommandParameter> ());
      FromBuilder fromBuilder = new FromBuilder (commandBuilder);

      Table table1 = new Table ("s1", "s1_alias");
      Table table2 = new Table ("s2", "s2_alias");
      Column column1 = new Column (table1, "c1");
      Column column2 = new Column (table2, "c2");

      List<Table> tables = new List<Table> { table1 }; // this table does not have a join associated with it
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
      FromBuilder fromBuilder = new FromBuilder (commandBuilder);

      Table table1 = new Table ("s1", "s1_alias");
      Table table2 = new Table ("s2", "s2_alias");
      Column column1 = new Column (table1, "c1");
      Column column2 = new Column (table2, "c2");

      List<Table> tables = new List<Table> { table2 };
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
      FromBuilder fromBuilder = new FromBuilder (commandBuilder);

      // table2.table1.table3

      Table table1 = new Table ("s1", "s1_alias");
      Table table2 = new Table ("s2", "s2_alias");
      Table table3 = new Table ("s3", "s3_alias");
      Column column1 = new Column (table1, "c1");
      Column column2 = new Column (table2, "c2");
      Column column3 = new Column (table1, "c1'");
      Column column4 = new Column (table3, "c3");

      List<Table> tables = new List<Table> { table2 };

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
    public void CombineTables_WithNestedJoin_Wrong () // remove after fixing previous test
    {
      CommandBuilder commandBuilder = new CommandBuilder (new StringBuilder (), new List<CommandParameter> ());
      FromBuilder fromBuilder = new FromBuilder (commandBuilder);

      // table2.table1.table3

      Table table1 = new Table ("s1", "s1_alias");
      Table table2 = new Table ("s2", "s2_alias");
      Table table3 = new Table ("s3", "s3_alias");
      Column column1 = new Column (table1, "c1");
      Column column2 = new Column (table2, "c2");
      Column column3 = new Column (table1, "c1'");
      Column column4 = new Column (table3, "c3");

      List<Table> tables = new List<Table> { table2 };

      JoinCollection joins = new JoinCollection ();

      SingleJoin join1 = new SingleJoin (column2, column1);
      SingleJoin join2 = new SingleJoin (column3, column4);
      joins.AddPath (new FieldSourcePath (table2, new[] { join1, join2 }));

      fromBuilder.BuildFromPart (tables, joins);

      Assert.AreEqual ("FROM [s2] [s2_alias] "
        + "LEFT OUTER JOIN [s1] [s1_alias] ON [s2_alias].[c2] = [s1_alias].[c1] "
        + "LEFT OUTER JOIN [s3] [s3_alias] ON [s1_alias].[c1'] = [s3_alias].[c3]", commandBuilder.GetCommandText());
    }
  }
}