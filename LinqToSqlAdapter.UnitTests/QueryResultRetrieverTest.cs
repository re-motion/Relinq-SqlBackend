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
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using Remotion.Linq.SqlBackend.SqlGeneration;

namespace Remotion.Linq.LinqToSqlAdapter.UnitTests
{
  [TestFixture]
  public class QueryResultRetrieverTest
  {
    private Mock<DbDataReader> _dataReaderMock;
    private Mock<DbCommand> _commandMock;
    private Mock<DbConnection> _connectionMock;
    private Mock<IConnectionManager> _connectionManagerStub;
    private Mock<IReverseMappingResolver> _resolverStub;
    private Mock<DbParameter> _dataParameter;
    private Func<IDatabaseResultRow, string> _projection;
    private Func<IDatabaseResultRow, int> _scalarProjection;

    [SetUp]
    public void SetUp ()
    {
      _dataReaderMock = new Mock<DbDataReader>();

      _dataParameter = new Mock<DbParameter>();

      _commandMock = new Mock<DbCommand>();
      _commandMock.Protected().Setup<DbDataReader>("ExecuteDbDataReader", CommandBehavior.Default).Returns (_dataReaderMock.Object);
      _commandMock.Protected().Setup<DbParameter>("CreateDbParameter").Returns (_dataParameter.Object);

      _connectionMock = new Mock<DbConnection>();
      _connectionMock.Protected().Setup<DbCommand>("CreateDbCommand").Returns (_commandMock.Object);

      _connectionManagerStub = new Mock<IConnectionManager>();
      _connectionManagerStub.Setup (stub => stub.Open()).Returns (_connectionMock.Object);
      _resolverStub = new Mock<IReverseMappingResolver>();

      _projection = row => row.GetValue<string> (new ColumnID ("test", 0));
      _scalarProjection = row => row.GetValue<int> (new ColumnID ("test", 0));
    }

    [Test]
    public void GetResults_CreatesCommandAndReadsData ()
    {
      var readResults = new Queue<bool>();
      var getValueResults = new Queue<string>();
      var sequence = new VerifiableSequence();

      readResults.Enqueue (true);
      _dataReaderMock.InVerifiableSequence (sequence).Setup (stub => stub.Read()).Returns (() => readResults.Dequeue());
      
      getValueResults.Enqueue ("testColumnValue1");
      _dataReaderMock.InVerifiableSequence (sequence).Setup (stub => stub.GetValue (0)).Returns (() => getValueResults.Dequeue());

      readResults.Enqueue (true);
      _dataReaderMock.InVerifiableSequence (sequence).Setup (stub => stub.Read()).Returns (() => readResults.Dequeue());

      getValueResults.Enqueue ("testColumnValue2");
      _dataReaderMock.InVerifiableSequence (sequence).Setup (stub => stub.GetValue (0)).Returns (() => getValueResults.Dequeue());

      readResults.Enqueue (false);
      _dataReaderMock.InVerifiableSequence (sequence).Setup (stub => stub.Read()).Returns (() => readResults.Dequeue());

      var retriever = new QueryResultRetriever (_connectionManagerStub.Object, _resolverStub.Object);

      var result = retriever.GetResults (_projection, "Text", new CommandParameter[0]).ToArray();

      Assert.That (result, Is.EqualTo (new[] { "testColumnValue1", "testColumnValue2" }));

      sequence.Verify();
    }

    [Test]
    public void GetResults_DisposesAllObjects ()
    {
      _dataReaderMock.Setup (stub => stub.Read()).Returns (false);

      var retriever = new QueryResultRetriever (_connectionManagerStub.Object, _resolverStub.Object);

      var result = retriever.GetResults (_projection, "Text", new CommandParameter[0]).ToArray();

      Assert.That (result, Is.Empty);

      _dataReaderMock.Protected().Verify ("Dispose", Times.AtLeastOnce(), (object)true);
      _commandMock.Protected().Verify ("Dispose", Times.AtLeastOnce(), (object)true);
      _connectionMock.Protected().Verify ("Dispose", Times.AtLeastOnce(), (object)true);
    }

    [Test]
    public void GetResults_SetsCommandData ()
    {
      _dataReaderMock.Setup (stub => stub.Read()).Returns (false);

      var dataParameterCollectionMock = new Mock<DbParameterCollection> (MockBehavior.Strict);
      dataParameterCollectionMock
          .Setup (mock => mock.Add (_dataParameter.Object))
          .Returns (0);
      _commandMock.Protected().Setup<DbParameterCollection> ("DbParameterCollection").Returns (dataParameterCollectionMock.Object);

      var retriever = new QueryResultRetriever (_connectionManagerStub.Object, _resolverStub.Object);

      var result = retriever.GetResults (_projection, "Text", new[] { new CommandParameter ("p1", "value1") }).ToArray();

      Assert.That (result, Is.Empty);

      _dataParameter.VerifySet (mock => mock.ParameterName = "p1", Times.AtLeastOnce());
      _dataParameter.VerifySet (mock => mock.Value = "value1", Times.AtLeastOnce());
      _commandMock.VerifySet (mock => mock.CommandText = "Text", Times.AtLeastOnce());
      dataParameterCollectionMock.Verify();
    }

    [Test]
    public void GetResults_UsesProjection ()
    {
      var readResults = new Queue<bool>();
      var sequence = new VerifiableSequence();

      readResults.Enqueue (true);
      _dataReaderMock.InVerifiableSequence (sequence).Setup (stub => stub.Read()).Returns (() => readResults.Dequeue());

      readResults.Enqueue (false);
      _dataReaderMock.InVerifiableSequence (sequence).Setup (stub => stub.Read()).Returns (() => readResults.Dequeue());

      var projectionInvocationCount = 0;
      var projection = new Func<IDatabaseResultRow, string> (
          resultRow =>
          {
            var returnValue = "result_" + projectionInvocationCount;
            projectionInvocationCount++;
            return returnValue;
          });

      var retriever = new QueryResultRetriever (_connectionManagerStub.Object, _resolverStub.Object);
      var result = retriever.GetResults (projection, "Text", new CommandParameter[0]).ToArray();

      Assert.That (result, Is.EqualTo (new[] { "result_0" }));
      sequence.Verify();
    }

    [Test]
    public void GetScalar ()
    {
      var fakeResult = 10;
      _dataReaderMock.Setup (stub => stub.Read()).Returns (true);
      _dataReaderMock.Setup (stub => stub.GetValue (0)).Returns (fakeResult);

      var retriever = new QueryResultRetriever (_connectionManagerStub.Object, _resolverStub.Object);

      var result = retriever.GetScalar (_scalarProjection, "Text", new CommandParameter[0]);

      Assert.That (result, Is.EqualTo (fakeResult));
    }

    [Test]
    public void GetScalar_DisposesAllObjects ()
    {
      var retriever = new QueryResultRetriever (_connectionManagerStub.Object, _resolverStub.Object);

      retriever.GetScalar (_scalarProjection, "Text", new CommandParameter[0]);

      _commandMock.Protected().Verify ("Dispose", Times.AtLeastOnce(), (object)true);
      _connectionMock.Protected().Verify ("Dispose", Times.AtLeastOnce(), (object)true);
      _dataReaderMock.Protected().Verify ("Dispose", Times.AtLeastOnce(), (object)true);
    }

    [Test]
    public void GetScalar_SetsCommandData ()
    {
      var dataParameterCollectionMock = new Mock<DbParameterCollection> (MockBehavior.Strict);
      dataParameterCollectionMock
          .Setup (mock => mock.Add (_dataParameter.Object))
          .Returns (0);

      _commandMock.Protected().Setup<DbParameterCollection> ("DbParameterCollection").Returns (dataParameterCollectionMock.Object);
      _commandMock.Protected().Setup<DbParameter>("CreateDbParameter").Returns (_dataParameter.Object);

      var connectionMock = new Mock<DbConnection>();
      connectionMock.Protected().Setup<DbCommand>("CreateDbCommand").Returns (_commandMock.Object);

      var connectionManagerStub = new Mock<IConnectionManager>();
      connectionManagerStub.Setup (stub => stub.Open()).Returns (connectionMock.Object);

      var retriever = new QueryResultRetriever (connectionManagerStub.Object, _resolverStub.Object);

      retriever.GetScalar (_scalarProjection, "Text", new[] { new CommandParameter ("p1", "value1") });

      _dataParameter.VerifySet (mock => mock.ParameterName = "p1", Times.AtLeastOnce());
      _dataParameter.VerifySet (mock => mock.Value = "value1", Times.AtLeastOnce());
      _commandMock.VerifySet (mock => mock.CommandText = "Text", Times.AtLeastOnce());
      dataParameterCollectionMock.Verify();
    }
  }
}