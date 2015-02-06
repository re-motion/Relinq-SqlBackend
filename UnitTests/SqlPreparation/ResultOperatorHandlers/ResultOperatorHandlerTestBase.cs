using NUnit.Framework;
using Remotion.Linq.SqlBackend.SqlPreparation;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;

namespace Remotion.Linq.SqlBackend.UnitTests.SqlPreparation.ResultOperatorHandlers
{
  public class ResultOperatorHandlerTestBase
  {
    private UniqueIdentifierGenerator _uniqueIdentifierGenerator;

    protected UniqueIdentifierGenerator UniqueIdentifierGenerator
    {
      get { return _uniqueIdentifierGenerator; }
    }

    [SetUp]
    public virtual void SetUp ()
    {
      _uniqueIdentifierGenerator = new UniqueIdentifierGenerator ();
    }

    protected void AssertStatementWasMovedToSubStatement (SqlStatementBuilder sqlStatementBuilder)
    {
      AssertStatementWasMovedToSubStatement (sqlStatementBuilder.GetSqlStatement());
    }

    protected void AssertStatementWasMovedToSubStatement (SqlStatement sqlStatement)
    {
      Assert.That (sqlStatement.SqlTables.Count, Is.EqualTo (1));
      Assert.That (sqlStatement.SqlTables[0].SqlTable.TableInfo, Is.TypeOf (typeof (ResolvedSubStatementTableInfo)));
    }

    protected DefaultSqlPreparationStage CreateDefaultSqlPreparationStage ()
    {
      return new DefaultSqlPreparationStage (
          CompoundMethodCallTransformerProvider.CreateDefault(),
          ResultOperatorHandlerRegistry.CreateDefault(),
          UniqueIdentifierGenerator);
    }
  }
}