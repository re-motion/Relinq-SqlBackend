// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System.Data;
using System.Data.SqlClient;
using NUnit.Framework;
using Remotion.Data.Linq.IntegrationTests.TestDomain.Northwind;
using Remotion.Data.Linq.SqlBackend.SqlGeneration;
using Rhino.Mocks;

namespace Remotion.Data.Linq.IntegrationTests.UnitTests
{
  [TestFixture]
  public class RowWrapperTest
  {
    private IDataReader _readerMock;
    private IReverseMappingResolver _reverseMappingResolverMock;
    [SetUp]
    public void SetUp ()
    {
      _readerMock = MockRepository.GenerateMock<IDataReader> ();
      _reverseMappingResolverMock = MockRepository.GenerateMock<IReverseMappingResolver> ();
    }

    /// <summary>
    /// Simple GetValue<T> Test
    /// </summary>
    [Test]
    public void SimpleGetValueShouldReturnValueTest ()
    {
      var columnID = new ColumnID("Name", 1);
      var rowWrapper = new RowWrapper (_readerMock, _reverseMappingResolverMock);
      _readerMock
          .Expect (mock => mock.GetValue(columnID.Position))
          .Return ("Peter");

      Assert.AreEqual (rowWrapper.GetValue<string> (columnID), "Peter");
    }



    ///// <summary>
    ///// Advanced GetValue<T> Test
    ///// </summary>
    //[Test]
    //public void AdvancedGetValueShouldReturnValueTest ()
    //{
    //  var rowWrapper = new RowWrapper (_readerMock);
    //}


    /// <summary>
    /// Simple GetEntity<T> Test
    /// </summary>
    [Test]
    public void SimpleGetEntityShouldReturnEntity ()
    {
      _readerMock
          .Expect (mock => mock.GetValue (1))
          .Return ("Peter");
      _readerMock
          .Expect (mock => mock.GetValue (2))
          .Return (21);
      _reverseMappingResolverMock
          .Expect (mock => mock.GetEntityMembers (typeof (Person)))
          .Return (typeof (Person).GetProperties ());

      ColumnID[] columnIDs = new ColumnID[]
                             {
                                 new ColumnID ("FirstName", 1),
                                 new ColumnID ("Age", 2)
                             };

      var rowWrapper = new RowWrapper (_readerMock, _reverseMappingResolverMock);

      var instance = rowWrapper.GetEntity<Person> (columnIDs);
      Assert.AreEqual (
          instance,
          new Person ("Peter", 21));
    }

    [TearDown]
    public void TearDown ()
    {
      _readerMock.VerifyAllExpectations();
    }
  }
}