using System;
using NUnit.Framework;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;

namespace Remotion.Linq.SqlBackend.UnitTests.SqlStatementModel
{
  [TestFixture]
  public class SqlAppendedTableTest
  {
    [Test]
    public void To_String_WithLeftJoin ()
    {
      var sqlTable = SqlStatementModelObjectMother.CreateSqlTable (new ResolvedSimpleTableInfo (typeof (int), "CookTable", "t0"));
      var apply = SqlStatementModelObjectMother.CreateSqlAppendedTable(sqlTable, JoinSemantics.Left);

      var result = apply.ToString();

      Assert.That (result, Is.EqualTo ("OUTER APPLY [CookTable] [t0]"));
    }

    [Test]
    public void To_String_WithLeftJoin_AndJoinCondition ()
    {
      var sqlTable = SqlStatementModelObjectMother.CreateSqlTable (new ResolvedSimpleTableInfo (typeof (int), "CookTable", "t0"));
      sqlTable.AddJoinForExplicitQuerySource (SqlStatementModelObjectMother.CreateSqlJoin());
      var apply = SqlStatementModelObjectMother.CreateSqlAppendedTable(sqlTable, JoinSemantics.Left);

      var result = apply.ToString();

      Assert.That (result, Is.EqualTo ("OUTER APPLY [CookTable] [t0] INNER JOIN TABLE(Cook) ON False"));
    }

    [Test]
    public void To_String_WithInnerJoin_AndSimpleTable ()
    {
      var sqlTable = SqlStatementModelObjectMother.CreateSqlTable (new ResolvedSimpleTableInfo (typeof (int), "CookTable", "t0"));
      var apply = SqlStatementModelObjectMother.CreateSqlAppendedTable(sqlTable, JoinSemantics.Inner);

      var result = apply.ToString();

      Assert.That (result, Is.EqualTo ("CROSS APPLY [CookTable] [t0]"));
    }

    [Test]
    public void To_String_WithInnerJoin_AndJoinedGrouping ()
    {
      var joinedTableInfo = new ResolvedJoinedGroupingTableInfo (
          "t0",
          SqlStatementModelObjectMother.CreateSqlStatement_Resolved (typeof (int)),
          SqlStatementModelObjectMother.CreateSqlGroupingSelectExpression(),
          "gs");
      var sqlTable = SqlStatementModelObjectMother.CreateSqlTable (joinedTableInfo);
      var apply = SqlStatementModelObjectMother.CreateSqlAppendedTable(sqlTable, JoinSemantics.Inner);

      var result = apply.ToString();

      Assert.That (result, Is.EqualTo ("CROSS JOIN JOINED-GROUPING([gs], (SELECT [t0] FROM CROSS APPLY [Table] [t]) [t0])"));
    }

    [Test]
    public void To_String_WithInnerJoin_AndSubStatement ()
    {
      var joinedTableInfo = new ResolvedSubStatementTableInfo ("t0", SqlStatementModelObjectMother.CreateSqlStatement_Resolved (typeof (int)));
      var sqlTable = SqlStatementModelObjectMother.CreateSqlTable (joinedTableInfo);
      var apply = SqlStatementModelObjectMother.CreateSqlAppendedTable(sqlTable, JoinSemantics.Inner);

      var result = apply.ToString();

      Assert.That (result, Is.EqualTo ("CROSS JOIN (SELECT [t0] FROM CROSS APPLY [Table] [t]) [t0]"));
    }

    [Test]
    public void To_String_WithInnerJoin_AndJoinCondition ()
    {
      var sqlTable = SqlStatementModelObjectMother.CreateSqlTable (new ResolvedSimpleTableInfo (typeof (int), "CookTable", "t0"));
      sqlTable.AddJoinForExplicitQuerySource (SqlStatementModelObjectMother.CreateSqlJoin());
      var apply = SqlStatementModelObjectMother.CreateSqlAppendedTable(sqlTable, JoinSemantics.Inner);

      var result = apply.ToString();

      Assert.That (result, Is.EqualTo ("CROSS APPLY [CookTable] [t0] INNER JOIN TABLE(Cook) ON False"));
    }
  }
}