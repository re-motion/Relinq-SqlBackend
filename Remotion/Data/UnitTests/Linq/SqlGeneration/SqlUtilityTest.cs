// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// version 3.0 as published by the Free Software Foundation.
// 
// re-motion is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-motion; if not, see http://www.gnu.org/licenses.
// 
using System;
using System.Data;
using System.Data.Common;
using NUnit.Framework;
using Remotion.Data.Linq.Backend;
using Rhino.Mocks;
using Remotion.Data.Linq.Backend.SqlGeneration;

namespace Remotion.Data.UnitTests.Linq.SqlGeneration
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
      _connection = repository.StrictMock<IDbConnection> ();

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
