// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;
using System.Data;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.LinqToSqlAdapter;
using Remotion.Data.Linq.SqlBackend.MappingResolution;
using Remotion.Data.Linq.SqlBackend.SqlGeneration;
using Rhino.Mocks;
using System.Linq;

namespace Remotion.Data.Linq.UnitTests.LinqToSqlAdapter
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

    [SetUp]
    public void SetUp ()
    {
      _dataReaderMock = MockRepository.GenerateMock<IDataReader> ();

      _dataParameter = MockRepository.GenerateMock<IDbDataParameter> ();

      _commandMock = MockRepository.GenerateMock<IDbCommand> ();
      _commandMock.Stub (stub => stub.ExecuteReader ()).Return (_dataReaderMock);
      _commandMock.Stub (stub => stub.CreateParameter ()).Return (_dataParameter);

      _connectionMock = MockRepository.GenerateMock<IDbConnection> ();
      _connectionMock.Stub (stub => stub.CreateCommand ()).Return (_commandMock);

      _connectionManagerStub = MockRepository.GenerateStub<IConnectionManager> ();
      _connectionManagerStub.Stub (stub => stub.Open ()).Return (_connectionMock);
      _resolverStub = MockRepository.GenerateMock<IReverseMappingResolver> ();

      _projection = row => row.GetValue<string> (new ColumnID ("test", 0));
    }

    [Test]
    public void GetResults_CreatesCommandAndReadsData ()
    {
      _dataReaderMock.Stub (stub => stub.Read()).Return (true).Repeat.Once();
      _dataReaderMock.Stub (stub => stub.GetValue (0)).Return ("testColumnValue1").Repeat.Once ();
      _dataReaderMock.Stub (stub => stub.Read ()).Return (true).Repeat.Once ();
      _dataReaderMock.Stub (stub => stub.GetValue (0)).Return ("testColumnValue2").Repeat.Once ();
      _dataReaderMock.Stub (stub => stub.Read ()).Return (false);
      
      var retriever = new QueryResultRetriever (_connectionManagerStub, _resolverStub);

      var result = retriever.GetResults (_projection, "Text", new CommandParameter[0]).ToArray ();

      Assert.That (result, Is.EqualTo(new[] { "testColumnValue1", "testColumnValue2" }));
    }

    [Test]
    public void GetResults_DisposesAllObjects ()
    {
      _dataReaderMock.Stub (stub => stub.Read()).Return (false);
      
      var retriever = new QueryResultRetriever (_connectionManagerStub, _resolverStub);

      var result = retriever.GetResults (_projection, "Text", new CommandParameter[0]).ToArray ();

      Assert.That (result, Is.Empty);

      _dataReaderMock.AssertWasCalled (mock => mock.Dispose ());
      _commandMock.AssertWasCalled (mock => mock.Dispose ());
      _connectionMock.AssertWasCalled (mock => mock.Dispose ());
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
    public void GetScalar ()
    {
      var fakeResult = 10;

      var commandMock = MockRepository.GenerateMock<IDbCommand> ();
      commandMock.Stub (stub => stub.ExecuteScalar ()).Return (fakeResult);

      var connectionMock = MockRepository.GenerateMock<IDbConnection> ();
      connectionMock.Stub (stub => stub.CreateCommand ()).Return (commandMock);

      var connectionManagerStub = MockRepository.GenerateStub<IConnectionManager> ();
      connectionManagerStub.Stub (stub => stub.Open ()).Return (connectionMock);

      var retriever = new QueryResultRetriever (connectionManagerStub, _resolverStub);

      var result = retriever.GetScalar<int> ("Text", new CommandParameter[0]);

      Assert.That (result, Is.EqualTo (fakeResult));
    }

    // TODO Review: Tests for GetScalar are not complete - there is no tests for the Dispose calls, none that checks that the projection is used, and 
    // TODO Review: none that checks that the parameters and command text are correctly set
    // TODO Review: It would be best to just copy the tests for GetResults and change them to call GetScalar instead. Don't forget to stub the 
    // TODO Review: command mock's ExecuteScalar method.
  }
}