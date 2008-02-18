using System;
using System.Data;
using System.Data.Common;
using NUnit.Framework;
using Rhino.Mocks;
using Rubicon.Data.Linq.SqlGeneration;

namespace Rubicon.Data.Linq.UnitTests.SqlGenerationTest
{
  [TestFixture]
  public class SqlUtilityTest
  {
    private IDatabaseInfo _databaseInfo;
    private IDbConnection _connection;

    [SetUp]
    public void SetUp()
    {
      _databaseInfo = StubDatabaseInfo.Instance;

      MockRepository repository = new MockRepository ();
      _connection = repository.CreateMock<IDbConnection> ();

      IDataParameterCollection parameterCollection = new StubParameterCollection ();

      IDbCommand command = repository.Stub<IDbCommand> ();
      SetupResult.For (command.Parameters).Return (parameterCollection);

      Expect.Call (_connection.CreateCommand ()).Return (command);
      repository.ReplayAll ();
    }

    [Test]
    public void CreateCommand_WithoutParameters()
    {
      IDbCommand command = SqlUtility.CreateCommand ("SELECT [s].* FROM [studentTable] [s]", new CommandParameter[0], _databaseInfo, _connection);
      Assert.IsNotNull (command);
      Assert.AreEqual ("SELECT [s].* FROM [studentTable] [s]", command.CommandText);
      Assert.AreEqual (CommandType.Text, command.CommandType);
      Assert.IsEmpty (command.Parameters);
    }

    [Test]
    public void CreateCommand_WithParameters ()
    {
      IDbCommand command = SqlUtility.CreateCommand ("SELECT [s].* FROM [studentTable] [s] WHERE @foo=@1",
          new CommandParameter[] { new CommandParameter("@1", "Garcia"), new CommandParameter("@foo", "bla")}, _databaseInfo, _connection);
      
      Assert.IsNotNull (command);
      Assert.AreEqual ("SELECT [s].* FROM [studentTable] [s] WHERE @foo=@1", command.CommandText);
      Assert.AreEqual (CommandType.Text, command.CommandType);
      Assert.AreEqual (2, command.Parameters.Count);
      Assert.AreEqual ("Garcia", ((DbParameter)command.Parameters[0]).Value);
      Assert.AreEqual ("bla", ((DbParameter)command.Parameters[1]).Value);
    }
  }
}