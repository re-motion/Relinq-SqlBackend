// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Remotion.Data.Linq.LinqToSqlAdapter;
using Remotion.Data.Linq.SqlBackend.MappingResolution;
using Remotion.Data.Linq.SqlBackend.SqlGeneration;
using Rhino.Mocks;

namespace Remotion.Data.Linq.UnitTests.LinqToSqlAdapter
{
  [TestFixture]
  public class RowWrapperTest
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
    public void SimpleGetValue_ShouldReturnValue ()
    {
      var columnID = new ColumnID("Name", 1);
      var rowWrapper = new RowWrapper (_readerMock, _reverseMappingResolverMock);
      _readerMock
          .Expect (mock => mock.GetValue(columnID.Position))
          .Return ("Peter");

      Assert.AreEqual (rowWrapper.GetValue<string> (columnID), "Peter");

      _readerMock.VerifyAllExpectations ();
    }

    //TODO: Also write a test showing that types without discriminator column can still be instantiated.
    [Test]
    public void SimpleGetEntity_ShouldReturnEntity ()
    {
      _readerMock
          .Expect (mock => mock.GetValue (1))
          .Return ("Peter");
      _readerMock
          .Expect (mock => mock.GetValue (2))
          .Return (21);
      _reverseMappingResolverMock
          .Expect (mock => mock.GetMetaDataMembers (typeof (PersonTestClass)))
          .Return (_metaModel.GetTable (typeof (PersonTestClass)).RowType.DataMembers.Where (dataMember => !dataMember.IsAssociation).ToArray());

      var columnIDs = new[]
                             {
                                 new ColumnID ("FirstName", 1),
                                 new ColumnID ("Age", 2)
                             };

      var rowWrapper = new RowWrapper (_readerMock, _reverseMappingResolverMock);

      var instance = rowWrapper.GetEntity<PersonTestClass> (columnIDs);


      _readerMock.VerifyAllExpectations ();
      Assert.AreEqual (
          instance,
          new PersonTestClass ("Peter", 21));
    }

    //TODO Use this implementation instead. Before doing so, write a test showing that GetEntity<Contact> will instantiate a Customer
    // TODO: if the CustomerType discriminator column contains the string "Customer".
    [Test]
    public void GetEntityMindingInheritance_Simple ()
    {
      _readerMock
          .Expect (mock => mock.GetValue (2))
          .Return ("Customer"); //return value of discriminator column
      _reverseMappingResolverMock
          .Expect (mock => mock.GetMetaDataMembers (typeof (ContactTestClass)))
          .Return (
              _metaModel.GetTable (typeof (ContactTestClass)).RowType.DataMembers.Where (dataMember => !dataMember.IsAssociation).ToArray(
                  ));

      var rowWrapper = new RowWrapper (_readerMock, _reverseMappingResolverMock);

      var columnIDs = new[]
                      {
                          new ColumnID ("ContactID", 1),
                          new ColumnID ("ContactType", 2)
                      };

      var customer = rowWrapper.GetEntity<ContactTestClass> (columnIDs);

      _readerMock.VerifyAllExpectations();
      _reverseMappingResolverMock.VerifyAllExpectations();
      Assert.IsInstanceOfType (typeof (ContactTestClass.CustomerContact), customer);
    }

    // TODO: Also write a test showing that if the discriminator column contains null, T (eg. Contact) is instantiated.
    [Test]
    public void GetEntityMindingInheritance_DiscriminatorColumnIsNull ()
    {
      _readerMock
          .Expect (mock => mock.GetValue (2))
          .Return (null); //return value of discriminator column
      _reverseMappingResolverMock
          .Expect (mock => mock.GetMetaDataMembers (typeof (ContactTestClass)))
          .Return (
              _metaModel.GetTable (typeof (ContactTestClass)).RowType.DataMembers.Where (dataMember => !dataMember.IsAssociation).ToArray()
              );

      var rowWrapper = new RowWrapper (_readerMock, _reverseMappingResolverMock);

      var columnIDs = new[]
                      {
                          new ColumnID ("ContactID", 1),
                          new ColumnID ("ContactType", 2)
                      };

      var contact = rowWrapper.GetEntity<ContactTestClass> (columnIDs);

      _readerMock.VerifyAllExpectations ();
      _reverseMappingResolverMock.VerifyAllExpectations ();
      Assert.IsInstanceOfType (typeof (ContactTestClass), contact);
    }

    //TODO: Also write a test showing that when entityMembers contains members of eg. Supplier, those members do not cause an exception 
    [Test]
    public void GetEntityMindingInheritance_AdditionalMappedMembersInSubTypes()
    {
      _readerMock
          .Expect (mock => mock.GetValue (2))
          .Return (1);
      _readerMock
          .Expect (mock => mock.GetValue (3))
          .Return ("Supplier"); //return value of discriminator column
      _readerMock
          .Expect (mock => mock.GetValue (1))
          .Return ("www.homepage.com");

      _reverseMappingResolverMock
          .Expect (mock => mock.GetMetaDataMembers (typeof (ContactTestClass)))
          .Return (
              _metaModel.GetMetaType (typeof (ContactTestClass.SupplierContact)).DataMembers.ToArray()
              );

      var rowWrapper = new RowWrapper (_readerMock, _reverseMappingResolverMock);

      var columnIDs = new[]
                      {
                          new ColumnID ("HomePage", 1),
                          new ColumnID ("ContactID", 2),
                          new ColumnID ("ContactType", 3)
                          
                      };

      var contact = rowWrapper.GetEntity<ContactTestClass> (columnIDs);

      var expectedContact = new ContactTestClass.SupplierContact();
      expectedContact.ContactID = 1;
      expectedContact.ContactType = "Supplier";
      expectedContact.HomePage = "www.homepage.com";

      _readerMock.VerifyAllExpectations ();
      _reverseMappingResolverMock.VerifyAllExpectations ();

      Assert.IsTrue (contact.Equals(expectedContact));
    }

    // TODO: Also write a test showing that byte[]s can be used.
    [Test]
    public void GetEntityMindingInheritance_ByteArraysCanBeUsed ()
    {
      System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding ();
      var pw = encoding.GetBytes("passwordtext");

      //var photo = new Binary (encoding.GetBytes ("myPhoto"));
      var photo = encoding.GetBytes ("myPhoto");

      _readerMock
          .Expect (mock => mock.GetValue (1))
          .Return (pw);
      _readerMock
          .Expect (mock => mock.GetValue (2))
          .Return (photo);
      _readerMock
          .Expect (mock => mock.GetValue (3))
          .Return (1); //return value of discriminator column
      _readerMock
          .Expect (mock => mock.GetValue (4))
          .Return ("Employee");

      _reverseMappingResolverMock
          .Expect (mock => mock.GetMetaDataMembers (typeof (ContactTestClass)))
          .Return (
              _metaModel.GetMetaType (typeof (ContactTestClass.EmployeeContact)).DataMembers.ToArray ()
              );

      var rowWrapper = new RowWrapper (_readerMock, _reverseMappingResolverMock);

      var columnIDs = new[]
                      {
                          new ColumnID ("Password", 1),
                          new ColumnID ("Photo", 2),
                          new ColumnID ("ContactID", 3),
                          new ColumnID ("ContactType", 4)
                      };

      var contact = rowWrapper.GetEntity<ContactTestClass> (columnIDs);

      var expectedContact = new ContactTestClass.EmployeeContact ();
      expectedContact.ContactID = 1;
      expectedContact.ContactType = "Employee";
      expectedContact.Password = pw;
      expectedContact.Photo = new Binary(photo);

      _readerMock.VerifyAllExpectations ();
      _reverseMappingResolverMock.VerifyAllExpectations ();

      Assert.IsTrue (contact.Equals (expectedContact));
    }
  }
}