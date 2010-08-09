// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;
using System.Data;
using System.Data.Linq.Mapping;
using System.Data.SqlClient;
using NUnit.Framework;
using Remotion.Data.Linq.IntegrationTests.TestDomain.Northwind;
using Remotion.Data.Linq.SqlBackend.MappingResolution;
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

  [Table (Name = "Person")]
  class Person
  {
    public Person ()
    {
    }

    public Person (string first, int age)
    {
      First = first;
      Age = age;
      //this.p_3 = p_3;
      //this.p_4 = p_4;
    }

    [Column (Name = "First")]
    public string First { get; set; }

    [Column (Name = "Age")]
    public int Age { get; set; }

    public override bool Equals (object obj)
    {
      if (obj == null || GetType () != obj.GetType ())
      {
        return false;
      }

      if (!((Person) obj).First.Equals (First))
        return false;
      if (!((Person) obj).Age.Equals (Age))
        return false;
      return true;
    }

    // override object.GetHashCode
    public override int GetHashCode ()
    {
      // TODO: write your implementation of GetHashCode() here
      throw new NotImplementedException ();
      return base.GetHashCode ();
    }
  }

}