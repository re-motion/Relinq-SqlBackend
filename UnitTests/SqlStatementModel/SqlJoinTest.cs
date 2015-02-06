using System;
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;

namespace Remotion.Linq.SqlBackend.UnitTests.SqlStatementModel
{
  [TestFixture]
  public class SqlJoinTest
  {
    [Test]
    public new void ToString ()
    {
      var joinedTable = SqlStatementModelObjectMother.CreateSqlTable (new ResolvedSimpleTableInfo (typeof (int), "CookTable", "t0"));
      var joinCondition = Expression.Constant (true);
      var apply = new SqlJoin (joinedTable, JoinSemantics.Inner, joinCondition);

      var result = apply.ToString();

      Assert.That (result, Is.EqualTo ("INNER JOIN [CookTable] [t0] ON True"));
    }
  }
}