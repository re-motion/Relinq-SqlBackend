// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (c) rubicon IT GmbH, www.rubicon.eu
// 
// re-linq is free software; you can redistribute it and/or modify it under 
// the terms of the GNU Lesser General Public License as published by the 
// Free Software Foundation; either version 2.1 of the License, 
// or (at your option) any later version.
// 
// re-linq is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-linq; if not, see http://www.gnu.org/licenses.
// 

using System;
using System.Data;
using System.Linq;
using NUnit.Framework;
using Remotion.Linq.SqlBackend.SqlGeneration;
using Moq;

namespace Remotion.Linq.LinqToSqlAdapter.UnitTests
{
  [TestFixture]
  public class QueryResultRetrieverTest
  {
    private Mock<IDataReader> _dataReaderMock;
    private Mock<IDbCommand> _commandMock;
    private Mock<IDbConnection> _connectionMock;
    private Mock<IConnectionManager> _connectionManagerStub;
    private Mock<IReverseMappingResolver> _resolverStub;
    private Mock<IDbDataParameter> _dataParameter;
    private Func<IDatabaseResultRow, string> _projection;
    private Func<IDatabaseResultRow, int> _scalarProjection;

    [SetUp]
    public void SetUp ()
    {
      _dataReaderMock = new Mock<IDataReader>();

      _dataParameter = new Mock<IDbDataParameter>();

      _commandMock = new Mock<IDbCommand>();
      _commandMock.Setup (stub => stub.ExecuteReader()).Returns (_dataReaderMock.Object);
      _commandMock.Setup (stub => stub.CreateParameter()).Returns (_dataParameter.Object);

      _connectionMock = new Mock<IDbConnection>();
      _connectionMock.Setup (stub => stub.CreateCommand()).Returns (_commandMock.Object);

      _connectionManagerStub = new Mock<IConnectionManager>();
      _connectionManagerStub.Setup (stub => stub.Open()).Returns (_connectionMock.Object);
      _resolverStub = new Mock<IReverseMappingResolver>();

      _projection = row => row.GetValue<string> (new ColumnID ("test", 0));
      _scalarProjection = row => row.GetValue<int> (new ColumnID ("test", 0));
    }

    [Test]
    public void GetResults_CreatesCommandAndReadsData ()
    {
      var sequence = new MockSequence();
      _dataReaderMock.InSequence (sequence).Setup (stub => stub.Read()).Returns (true);
      _dataReaderMock.InSequence (sequence).Setup (stub => stub.GetValue (0)).Returns ("testColumnValue1");
      _dataReaderMock.InSequence (sequence).Setup (stub => stub.Read()).Returns (true);
      _dataReaderMock.InSequence (sequence).Setup (stub => stub.GetValue (0)).Returns ("testColumnValue2");
      _dataReaderMock.InSequence (sequence).Setup (stub => stub.Read()).Returns (false);

      var retriever = new QueryResultRetriever (_connectionManagerStub.Object, _resolverStub.Object);

      var result = retriever.GetResults (_projection, "Text", new CommandParameter[0]).ToArray();

      Assert.That (result, Is.EqualTo (new[] { "testColumnValue1", "testColumnValue2" }));
    }

    [Test]
    public void GetResults_DisposesAllObjects ()
    {
      _dataReaderMock.Setup (stub => stub.Read()).Returns (false);

      var retriever = new QueryResultRetriever (_connectionManagerStub.Object, _resolverStub.Object);

      var result = retriever.GetResults (_projection, "Text", new CommandParameter[0]).ToArray();

      Assert.That (result, Is.Empty);

      _dataReaderMock.Verify (mock => mock.Dispose());
      _commandMock.Verify (mock => mock.Dispose());
      _connectionMock.Verify (mock => mock.Dispose());
    }

    [Test]
    public void GetResults_SetsCommandData ()
    {
      _dataReaderMock.Setup (stub => stub.Read()).Returns (false);

      var dataParameterCollectionMock = new Mock<IDataParameterCollection> (MockBehavior.Strict);
      dataParameterCollectionMock
          .Setup (mock => mock.Add (It.Is<IDbDataParameter> (d => d.Equals(_dataParameter.Object))))
          .Returns (0);
      _commandMock.SetupGet (stub => stub.Parameters).Returns (dataParameterCollectionMock.Object);

      var retriever = new QueryResultRetriever (_connectionManagerStub.Object, _resolverStub.Object);

      var result = retriever.GetResults (_projection, "Text", new[] { new CommandParameter ("p1", "value1") }).ToArray();

      Assert.That (result, Is.Empty);

      _dataParameter.VerifySet (mock => mock.ParameterName = "p1");
      _dataParameter.VerifySet (mock => mock.Value = "value1");
      _commandMock.VerifySet (mock => mock.CommandText = "Text");
      dataParameterCollectionMock.Verify();
    }

    [Test]
    public void GetResults_UsesProjection ()
    {
      var sequence = new MockSequence();
      _dataReaderMock.InSequence (sequence).Setup (stub => stub.Read()).Returns (true);
      _dataReaderMock.InSequence (sequence).Setup (stub => stub.GetValue (0)).Returns ("testColumnValue1");
      _dataReaderMock.InSequence (sequence).Setup (stub => stub.Read()).Returns (false);

      var projectionMock = new Mock<Func<IDatabaseResultRow, string>>();

      var retriever = new QueryResultRetriever (_connectionManagerStub.Object, _resolverStub.Object);
      var result = retriever.GetResults (projectionMock.Object, "Text", new CommandParameter[0]).ToArray();

      Assert.That (result[0], Is.Null);
      projectionMock.Verify (p => p.Invoke (It.IsAny<IDatabaseResultRow>()));
      projectionMock.Verify();
    }

    [Test]
    public void GetScalar ()
    {
      var fakeResult = 10;
      var sequence = new MockSequence();
      _dataReaderMock.InSequence (sequence).Setup (stub => stub.Read()).Returns (true);
      _dataReaderMock.InSequence (sequence).Setup (stub => stub.GetValue (0)).Returns (fakeResult);

      var retriever = new QueryResultRetriever (_connectionManagerStub.Object, _resolverStub.Object);

      var result = retriever.GetScalar (_scalarProjection, "Text", new CommandParameter[0]);

      Assert.That (result, Is.EqualTo (fakeResult));
    }

    [Test]
    public void GetScalar_DisposesAllObjects ()
    {
      var retriever = new QueryResultRetriever (_connectionManagerStub.Object, _resolverStub.Object);

      retriever.GetScalar (_scalarProjection, "Text", new CommandParameter[0]);

      _commandMock.Verify (mock => mock.Dispose());
      _connectionMock.Verify (mock => mock.Dispose());
      _dataReaderMock.Verify (mock => mock.Dispose());
    }

    [Test]
    public void GetScalar_SetsCommandData ()
    {
      var dataParameterCollectionMock = new Mock<IDataParameterCollection> (MockBehavior.Strict);
      dataParameterCollectionMock
          .Setup (mock => mock.Add (It.Is<IDbDataParameter> (d => d.Equals(_dataParameter.Object))))
          .Returns (0);

      _commandMock.SetupGet (stub => stub.Parameters).Returns (dataParameterCollectionMock.Object);
      _commandMock.Setup (stub => stub.CreateParameter()).Returns (_dataParameter.Object);

      var connectionMock = new Mock<IDbConnection>();
      connectionMock.Setup (stub => stub.CreateCommand()).Returns (_commandMock.Object);

      var connectionManagerStub = new Mock<IConnectionManager>();
      connectionManagerStub.Setup (stub => stub.Open()).Returns (connectionMock.Object);

      var retriever = new QueryResultRetriever (connectionManagerStub.Object, _resolverStub.Object);

      retriever.GetScalar (_scalarProjection, "Text", new[] { new CommandParameter ("p1", "value1") });

      _dataParameter.VerifySet (mock => mock.ParameterName = "p1");
      _dataParameter.VerifySet (mock => mock.Value = "value1");
      _commandMock.VerifySet (mock => mock.CommandText = "Text");
      dataParameterCollectionMock.Verify();
    }
  }
}