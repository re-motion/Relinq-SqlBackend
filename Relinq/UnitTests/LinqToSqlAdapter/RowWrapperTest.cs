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
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Linq;
using NUnit.Framework;
using Remotion.Linq.LinqToSqlAdapter;
using Remotion.Linq.SqlBackend.SqlGeneration;
using Remotion.Linq.UnitTests.LinqToSqlAdapter.TestDomain;
using Rhino.Mocks;

namespace Remotion.Linq.UnitTests.LinqToSqlAdapter
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
      _readerMock = MockRepository.GenerateMock<IDataReader>();
      _reverseMappingResolverMock = MockRepository.GenerateMock<IReverseMappingResolver>();

      _metaModel = new AttributeMappingSource().GetModel (typeof (DataContextTestClass));
    }

    [Test]
    public void SimpleGetValue_ShouldReturnValue ()
    {
      var columnID = new ColumnID ("Name", 1);
      var rowWrapper = new RowWrapper (_readerMock, _reverseMappingResolverMock);
      _readerMock
          .Expect (mock => mock.GetValue (columnID.Position))
          .Return ("Peter");

      var value = rowWrapper.GetValue<string> (columnID);

      _readerMock.VerifyAllExpectations();
      Assert.That ("Peter", Is.EqualTo (value));
    }

    [Test]
    public void GetEntity_TypeWithoutDiscriminator ()
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


      _readerMock.VerifyAllExpectations();
      Assert.That (new PersonTestClass ("Peter", 21), Is.EqualTo (instance));
    }

    [Test]
    public void GetEntity_CreatesInstanceAccordingToDiscriminatorColumn ()
    {
      _readerMock
          .Expect (mock => mock.GetValue (2))
          .Return ("Customer"); //return value of discriminator column
      _reverseMappingResolverMock
          .Expect (mock => mock.GetMetaDataMembers (typeof (ContactWithInheritanceHierarchy)))
          .Return (
              _metaModel.GetTable (typeof (ContactWithInheritanceHierarchy)).RowType.DataMembers.Where (dataMember => !dataMember.IsAssociation).
                  ToArray(
                      
                  ));

      var rowWrapper = new RowWrapper (_readerMock, _reverseMappingResolverMock);

      var columnIDs = new[]
                      {
                          new ColumnID ("ContactID", 1),
                          new ColumnID ("ContactType", 2)
                      };

      var customer = rowWrapper.GetEntity<ContactWithInheritanceHierarchy> (columnIDs);

      _readerMock.VerifyAllExpectations();
      _reverseMappingResolverMock.VerifyAllExpectations();
      Assert.IsInstanceOf (typeof (ContactWithInheritanceHierarchy.CustomerContact), customer);
    }

    [Test]
    public void GetEntity_CreateDefaultInstanceIfDiscriminatorIsNull ()
    {
      _readerMock
          .Expect (mock => mock.GetValue (2))
          .Return (null); //return value of discriminator column
      _reverseMappingResolverMock
          .Expect (mock => mock.GetMetaDataMembers (typeof (ContactWithInheritanceHierarchy)))
          .Return (
              _metaModel.GetTable (typeof (ContactWithInheritanceHierarchy)).RowType.DataMembers.Where (dataMember => !dataMember.IsAssociation).
                  ToArray()
          );

      var rowWrapper = new RowWrapper (_readerMock, _reverseMappingResolverMock);

      var columnIDs = new[]
                      {
                          new ColumnID ("ContactID", 1),
                          new ColumnID ("ContactType", 2)
                      };

      var contact = rowWrapper.GetEntity<ContactWithInheritanceHierarchy> (columnIDs);

      _readerMock.VerifyAllExpectations();
      _reverseMappingResolverMock.VerifyAllExpectations();
      Assert.IsInstanceOf (typeof (ContactWithInheritanceHierarchy), contact);
    }

    [Test]
    public void GetEntity_AdditionalMappedMembersInSubTypesDontCauseException ()
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
          .Expect (mock => mock.GetMetaDataMembers (typeof (ContactWithInheritanceHierarchy)))
          .Return (
              _metaModel.GetMetaType (typeof (ContactWithInheritanceHierarchy.SupplierContact)).DataMembers.ToArray()
          );

      var rowWrapper = new RowWrapper (_readerMock, _reverseMappingResolverMock);

      var columnIDs = new[]
                      {
                          new ColumnID ("HomePage", 1),
                          new ColumnID ("ContactID", 2),
                          new ColumnID ("ContactType", 3)
                      };

      var contact = rowWrapper.GetEntity<ContactWithInheritanceHierarchy> (columnIDs);

      var expectedContact = new ContactWithInheritanceHierarchy.SupplierContact();
      expectedContact.ContactID = 1;
      expectedContact.ContactType = "Supplier";
      expectedContact.HomePage = "www.homepage.com";

      _readerMock.VerifyAllExpectations();
      _reverseMappingResolverMock.VerifyAllExpectations();

      Assert.That (contact.Equals (expectedContact), Is.True);
    }

    [Test]
    public void GetEntity_ByteArraysCanBeUsed ()
    {
      var encoding = new System.Text.UTF8Encoding();
      var pw = encoding.GetBytes ("passwordtext");

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
          .Expect (mock => mock.GetMetaDataMembers (typeof (ContactWithInheritanceHierarchy)))
          .Return (
              _metaModel.GetMetaType (typeof (ContactWithInheritanceHierarchy.EmployeeContact)).DataMembers.ToArray()
          );

      var rowWrapper = new RowWrapper (_readerMock, _reverseMappingResolverMock);

      var columnIDs = new[]
                      {
                          new ColumnID ("Password", 1),
                          new ColumnID ("Photo", 2),
                          new ColumnID ("ContactID", 3),
                          new ColumnID ("ContactType", 4)
                      };

      var contact = rowWrapper.GetEntity<ContactWithInheritanceHierarchy> (columnIDs);

      var expectedContact = new ContactWithInheritanceHierarchy.EmployeeContact();
      expectedContact.ContactID = 1;
      expectedContact.ContactType = "Employee";
      expectedContact.Password = pw;
      expectedContact.Photo = new Binary (photo);

      _readerMock.VerifyAllExpectations();
      _reverseMappingResolverMock.VerifyAllExpectations();

      Assert.That (contact.Equals (expectedContact), Is.True);
    }
  }
}