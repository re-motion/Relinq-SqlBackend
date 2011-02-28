// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// as published by the Free Software Foundation; either version 2.1 of the 
// License, or (at your option) any later version.
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
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Linq.LinqToSqlAdapter;
using Remotion.Linq.SqlBackend.SqlGeneration;
using Rhino.Mocks;
using System.Linq;

namespace Remotion.Linq.UnitTests.LinqToSqlAdapter
{
  [TestFixture]
  public class QueryResultRetrieverTest
  {
    private IDataReader _dataReaderMock;
    private IDbCommand _commandMock;
    private IDbConnection _connectionMock;
    private IConnectionManager _connectionManagerStub;
    private IReverseMappingResolver _resolverStub;
    private IDbDataParameter _dataParameter;
    private Func<IDatabaseResultRow, string> _projection;
    private Func<IDatabaseResultRow, int> _scalarProjection;

    [SetUp]
    public void SetUp ()
    {
      _dataReaderMock = MockRepository.GenerateMock<IDataReader>();

      _dataParameter = MockRepository.GenerateMock<IDbDataParameter>();

      _commandMock = MockRepository.GenerateMock<IDbCommand>();
      _commandMock.Stub (stub => stub.ExecuteReader()).Return (_dataReaderMock);
      _commandMock.Stub (stub => stub.CreateParameter()).Return (_dataParameter);

      _connectionMock = MockRepository.GenerateMock<IDbConnection>();
      _connectionMock.Stub (stub => stub.CreateCommand()).Return (_commandMock);

      _connectionManagerStub = MockRepository.GenerateStub<IConnectionManager>();
      _connectionManagerStub.Stub (stub => stub.Open()).Return (_connectionMock);
      _resolverStub = MockRepository.GenerateMock<IReverseMappingResolver>();

      _projection = row => row.GetValue<string> (new ColumnID ("test", 0));
      _scalarProjection = row => row.GetValue<int> (new ColumnID ("test", 0));
    }

    [Test]
    public void GetResults_CreatesCommandAndReadsData ()
    {
      _dataReaderMock.Stub (stub => stub.Read()).Return (true).Repeat.Once();
      _dataReaderMock.Stub (stub => stub.GetValue (0)).Return ("testColumnValue1").Repeat.Once();
      _dataReaderMock.Stub (stub => stub.Read()).Return (true).Repeat.Once();
      _dataReaderMock.Stub (stub => stub.GetValue (0)).Return ("testColumnValue2").Repeat.Once();
      _dataReaderMock.Stub (stub => stub.Read()).Return (false);

      var retriever = new QueryResultRetriever (_connectionManagerStub, _resolverStub);

      var result = retriever.GetResults (_projection, "Text", new CommandParameter[0]).ToArray();

      Assert.That (result, Is.EqualTo (new[] { "testColumnValue1", "testColumnValue2" }));
    }

    [Test]
    public void GetResults_DisposesAllObjects ()
    {
      _dataReaderMock.Stub (stub => stub.Read()).Return (false);

      var retriever = new QueryResultRetriever (_connectionManagerStub, _resolverStub);

      var result = retriever.GetResults (_projection, "Text", new CommandParameter[0]).ToArray();

      Assert.That (result, Is.Empty);

      _dataReaderMock.AssertWasCalled (mock => mock.Dispose());
      _commandMock.AssertWasCalled (mock => mock.Dispose());
      _connectionMock.AssertWasCalled (mock => mock.Dispose());
    }

    [Test]
    public void GetResults_SetsCommandData ()
    {
      _dataReaderMock.Stub (stub => stub.Read()).Return (false);

      var dataParameterCollectionMock = MockRepository.GenerateStrictMock<IDataParameterCollection>();
      dataParameterCollectionMock
          .Stub (mock => mock.Add (Arg<IDbDataParameter>.Is.Equal (_dataParameter)))
          .Return (0);
      dataParameterCollectionMock.Replay();
      _commandMock.Stub (stub => stub.Parameters).Return (dataParameterCollectionMock);

      var retriever = new QueryResultRetriever (_connectionManagerStub, _resolverStub);

      var result = retriever.GetResults (_projection, "Text", new[] { new CommandParameter ("p1", "value1") }).ToArray();

      Assert.That (result, Is.Empty);

      _dataParameter.AssertWasCalled (mock => mock.ParameterName = "p1");
      _dataParameter.AssertWasCalled (mock => mock.Value = "value1");
      _commandMock.AssertWasCalled (mock => mock.CommandText = "Text");
      dataParameterCollectionMock.VerifyAllExpectations();
    }

    [Test]
    public void GetResults_UsesProjection ()
    {
      _dataReaderMock.Stub (stub => stub.Read()).Return (true).Repeat.Once();
      _dataReaderMock.Stub (stub => stub.GetValue (0)).Return ("testColumnValue1").Repeat.Once();
      _dataReaderMock.Stub (stub => stub.Read()).Return (false);

      var projectionMock = MockRepository.GenerateMock<Func<IDatabaseResultRow, string>>();

      var retriever = new QueryResultRetriever (_connectionManagerStub, _resolverStub);
      var result = retriever.GetResults (projectionMock, "Text", new CommandParameter[0]).ToArray();

      Assert.That (result[0], Is.Null);
      projectionMock.AssertWasCalled (p => p.Invoke (Arg<IDatabaseResultRow>.Is.Anything));
      projectionMock.VerifyAllExpectations();
    }

    [Test]
    public void GetScalar ()
    {
      var fakeResult = 10;
      _dataReaderMock.Stub (stub => stub.Read()).Return (true).Repeat.Once();
      _dataReaderMock.Stub (stub => stub.GetValue (0)).Return (fakeResult).Repeat.Once();

      var retriever = new QueryResultRetriever (_connectionManagerStub, _resolverStub);

      var result = retriever.GetScalar (_scalarProjection, "Text", new CommandParameter[0]);

      Assert.That (result, Is.EqualTo (fakeResult));
    }

    [Test]
    public void GetScalar_DisposesAllObjects ()
    {
      var retriever = new QueryResultRetriever (_connectionManagerStub, _resolverStub);

      retriever.GetScalar (_scalarProjection, "Text", new CommandParameter[0]);

      _commandMock.AssertWasCalled (mock => mock.Dispose());
      _connectionMock.AssertWasCalled (mock => mock.Dispose());
      _dataReaderMock.AssertWasCalled (mock => mock.Dispose());
    }

    [Test]
    public void GetScalar_SetsCommandData ()
    {
      var dataParameterCollectionMock = MockRepository.GenerateStrictMock<IDataParameterCollection>();
      dataParameterCollectionMock
          .Stub (mock => mock.Add (Arg<IDbDataParameter>.Is.Equal (_dataParameter)))
          .Return (0);
      dataParameterCollectionMock.Replay();

      _commandMock.Stub (stub => stub.Parameters).Return (dataParameterCollectionMock);
      _commandMock.Stub (stub => stub.CreateParameter()).Return (_dataParameter);

      var connectionMock = MockRepository.GenerateMock<IDbConnection>();
      connectionMock.Stub (stub => stub.CreateCommand()).Return (_commandMock);

      var connectionManagerStub = MockRepository.GenerateStub<IConnectionManager>();
      connectionManagerStub.Stub (stub => stub.Open()).Return (connectionMock);

      var retriever = new QueryResultRetriever (connectionManagerStub, _resolverStub);

      retriever.GetScalar (_scalarProjection, "Text", new[] { new CommandParameter ("p1", "value1") });

      _dataParameter.AssertWasCalled (mock => mock.ParameterName = "p1");
      _dataParameter.AssertWasCalled (mock => mock.Value = "value1");
      _commandMock.AssertWasCalled (mock => mock.CommandText = "Text");
      dataParameterCollectionMock.VerifyAllExpectations();
    }
  }
}