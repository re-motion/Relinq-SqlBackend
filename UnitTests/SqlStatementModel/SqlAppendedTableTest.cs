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
    public new void ToString ()
    {
      var sqlTable = SqlStatementModelObjectMother.CreateSqlTable (new ResolvedSimpleTableInfo (typeof (int), "CookTable", "t0"));
      var apply = SqlStatementModelObjectMother.CreateSqlAppendedTable(sqlTable, JoinSemantics.Left);

      var result = apply.ToString();

      Assert.That (result, Is.EqualTo ("OUTER APPLY [CookTable] [t0]"));
    }
  }
}