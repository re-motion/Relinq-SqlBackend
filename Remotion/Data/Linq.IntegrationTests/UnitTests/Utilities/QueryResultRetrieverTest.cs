// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;
using System.Collections;
using System.Data;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.IntegrationTests.Utilities;
using Remotion.Data.Linq.SqlBackend.MappingResolution;
using Remotion.Data.Linq.SqlBackend.SqlGeneration;
using Rhino.Mocks;
using System.Linq;

namespace Remotion.Data.Linq.IntegrationTests.UnitTests.Utilities
{
  [TestFixture]
  public class QueryResultRetrieverTest
  {
    private IDataReader _dataReaderMock;
    private IDbCommand _commandMock;
    private IDbConnection _connectionMock;
    private IConnectionManager _connectionManagerStub;
    private IReverseMappingResolver _resolverStub;

    [SetUp]
    public void SetUp ()
    {
      _dataReaderMock = MockRepository.GenerateMock<IDataReader> ();

      _commandMock = MockRepository.GenerateMock<IDbCommand> ();
      _commandMock.Stub (stub => stub.ExecuteReader ()).Return (_dataReaderMock);

      _connectionMock = MockRepository.GenerateMock<IDbConnection> ();
      _connectionMock.Stub (stub => stub.CreateCommand ()).Return (_commandMock);

      _connectionManagerStub = MockRepository.GenerateStub<IConnectionManager> ();
      _connectionManagerStub.Stub (stub => stub.Open ()).Return (_connectionMock);
      _resolverStub = MockRepository.GenerateMock<IReverseMappingResolver> ();
    }

    [Test]
    public void GetResults_CreatesCommandAndReadsData ()
    {
      _dataReaderMock.Stub (stub => stub.NextResult()).Return (true).Repeat.Once();
      _dataReaderMock.Stub (stub => stub.GetValue (0)).Return ("testColumnValue1").Repeat.Once ();
      _dataReaderMock.Stub (stub => stub.NextResult ()).Return (true).Repeat.Once ();
      _dataReaderMock.Stub (stub => stub.GetValue (0)).Return ("testColumnValue2").Repeat.Once ();
      _dataReaderMock.Stub (stub => stub.NextResult ()).Return (false);
      
      Func<IDatabaseResultRow, string> projection = row => row.GetValue<string> (new ColumnID ("test", 0));
      var retriever = new QueryResultRetriever (_connectionManagerStub, _resolverStub);

      var result = retriever.GetResults (projection, "Text", new CommandParameter[0]).ToArray ();

      Assert.That (result, Is.EqualTo(new[] { "testColumnValue1", "testColumnValue2" }));
    }

    [Test]
    public void GetResults_DisposesAllObjects ()
    {
      _dataReaderMock.Stub (stub => stub.NextResult ()).Return (false);
      
      Func<IDatabaseResultRow, string> projection = row => row.GetValue<string> (new ColumnID ("test", 0));
      var retriever = new QueryResultRetriever (_connectionManagerStub, _resolverStub);

      var result = retriever.GetResults (projection, "Text", new CommandParameter[0]).ToArray ();

      Assert.That (result, Is.Empty);

      _dataReaderMock.AssertWasCalled (mock => mock.Dispose ());
      _commandMock.AssertWasCalled (mock => mock.Dispose ());
      _connectionMock.AssertWasCalled (mock => mock.Dispose ());
    }

    [Test]
    public void GetResults_SetsCommandData ()
    {
      _dataReaderMock.Stub (stub => stub.NextResult()).Return (false);

      var dataParameterCollectionMock = MockRepository.GenerateStrictMock<IDataParameterCollection>();
      dataParameterCollectionMock
          .Stub (mock => mock.Add (Arg<CommandParameter>.Is.Equal (new CommandParameter ("p1", "value1"))))
          .Return (0);
      dataParameterCollectionMock.Replay();
      _commandMock.Stub (stub => stub.Parameters).Return (dataParameterCollectionMock);

      Func<IDatabaseResultRow, string> projection = row => row.GetValue<string> (new ColumnID ("test", 0));
      var retriever = new QueryResultRetriever (_connectionManagerStub, _resolverStub);


      var result = retriever.GetResults (projection, "Text", new[] { new CommandParameter ("p1", "value1") }).ToArray();

      Assert.That (result, Is.Empty);

      _commandMock.AssertWasCalled (mock => mock.CommandText = "Text");
      dataParameterCollectionMock.VerifyAllExpectations();
    }
  }
}