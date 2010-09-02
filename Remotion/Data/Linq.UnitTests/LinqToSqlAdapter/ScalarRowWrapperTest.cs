// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//

using System.Data;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Linq;
using NUnit.Framework;
using Remotion.Data.Linq.LinqToSqlAdapter;
using Remotion.Data.Linq.SqlBackend.MappingResolution;
using Remotion.Data.Linq.SqlBackend.SqlGeneration;
using Remotion.Data.Linq.UnitTests.LinqToSqlAdapter.TestDomain;
using Rhino.Mocks;
using System;

namespace Remotion.Data.Linq.UnitTests.LinqToSqlAdapter
{
  [TestFixture]
  public class ScalarRowWrapperTest
  {
    private IDataReader _readerMock;
    private IReverseMappingResolver _reverseMappingResolverMock;
    private MetaModel _metaModel;

    [SetUp]
    public void SetUp ()
    {
      _readerMock = MockRepository.GenerateMock<IDataReader> ();
      _reverseMappingResolverMock = MockRepository.GenerateMock<IReverseMappingResolver> ();

      _metaModel = new AttributeMappingSource ().GetModel (typeof (DataContextTestClass));
    }

    [Test]
    public void GetValue_ShouldReturnValue ()
    {
      var columnID = new ColumnID ("Name", 0);
      var scalarRowWrapper = new ScalarRowWrapper (_readerMock, _reverseMappingResolverMock);
      _readerMock
          .Expect (mock => mock.GetValue (columnID.Position))
          .Return ("Peter");

      var value = scalarRowWrapper.GetValue<string> (columnID);

      _readerMock.VerifyAllExpectations ();
      Assert.AreEqual (value, "Peter");
    }

    [Test]
    [ExpectedException (ExceptionType = typeof (ArgumentException))]
    public void GetValue_ShouldThrowException ()
    {
      var scalarRowWrapper = new ScalarRowWrapper (_readerMock, _reverseMappingResolverMock);

      scalarRowWrapper.GetValue<string> (new ColumnID ("Name", 1));
    }

    [Test]
    [ExpectedException(ExceptionType=typeof(ArgumentException))]
    public void GetEntity_ShouldThrowException ()
    {
      var scalarRowWrapper = new ScalarRowWrapper (_readerMock, _reverseMappingResolverMock);
      
      scalarRowWrapper.GetEntity<PersonTestClass> (null);
    }

    [Test]
    public void GetEntity_WithSingleColumn ()
    {
      var columnID = new ColumnID ("Name", 0);
      _readerMock
          .Expect (mock => mock.GetValue (columnID.Position))
          .Return ("Peter");

      var scalarRowWrapper = new ScalarRowWrapper (_readerMock, _reverseMappingResolverMock);

      var value=scalarRowWrapper.GetEntity<string> (columnID);
      Assert.AreEqual (value, "Peter");
    }

    [Test]
    [ExpectedException (ExceptionType = typeof (ArgumentException))]
    public void GetEntity_WithMultipleColumns ()
    {
      var columnIDs = new[]
                             {
                                 new ColumnID ("FirstName", 1),
                                 new ColumnID ("Age", 2)
                             };

      var scalarRowWrapper = new ScalarRowWrapper (_readerMock, _reverseMappingResolverMock);

      scalarRowWrapper.GetEntity<PersonTestClass> (columnIDs);
    }
  }
}
