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
using Remotion.Linq.LinqToSqlAdapter.UnitTests.TestDomain;
using Remotion.Linq.SqlBackend.SqlGeneration;
using Moq;

namespace Remotion.Linq.LinqToSqlAdapter.UnitTests
{
  [TestFixture]
  public class RowWrapperTest
  {
    private Mock<IDataReader> _readerMock;
    private Mock<IReverseMappingResolver> _reverseMappingResolverMock;
    private MetaModel _metaModel;

    [SetUp]
    public void SetUp ()
    {
      _readerMock = new Mock<IDataReader>();
      _reverseMappingResolverMock = new Mock<IReverseMappingResolver>();

      _metaModel = new AttributeMappingSource().GetModel (typeof (DataContextTestClass));
    }

    [Test]
    public void SimpleGetValue_ShouldReturnValue ()
    {
      var columnID = new ColumnID ("Name", 1);
      var rowWrapper = new RowWrapper (_readerMock.Object, _reverseMappingResolverMock.Object);
      _readerMock
         .Setup (mock => mock.GetValue (columnID.Position))
         .Returns ("Peter")
         .Verifiable();

      var value = rowWrapper.GetValue<string> (columnID);

      _readerMock.Verify();
      Assert.That ("Peter", Is.EqualTo (value));
    }

    [Test]
    public void GetEntity_TypeWithoutDiscriminator ()
    {
      _readerMock
         .Setup (mock => mock.GetValue (1))
         .Returns ("Peter")
         .Verifiable();
      _readerMock
         .Setup (mock => mock.GetValue (2))
         .Returns (21)
         .Verifiable();
      _reverseMappingResolverMock
         .Setup (mock => mock.GetMetaDataMembers (typeof (PersonTestClass)))
         .Returns (_metaModel.GetTable (typeof (PersonTestClass)).RowType.DataMembers.Where (dataMember => !dataMember.IsAssociation).ToArray())
         .Verifiable();

      var columnIDs = new[]
                      {
                          new ColumnID ("FirstName", 1),
                          new ColumnID ("Age", 2)
                      };

      var rowWrapper = new RowWrapper (_readerMock.Object, _reverseMappingResolverMock.Object);

      var instance = rowWrapper.GetEntity<PersonTestClass> (columnIDs);


      _readerMock.Verify();
      Assert.That (new PersonTestClass ("Peter", 21), Is.EqualTo (instance));
    }

    [Test]
    public void GetEntity_CreatesInstanceAccordingToDiscriminatorColumn ()
    {
      _readerMock
         .Setup (mock => mock.GetValue (2))
         .Returns ("Customer") //return value of discriminator column
         .Verifiable();
      _reverseMappingResolverMock
         .Setup (mock => mock.GetMetaDataMembers (typeof (ContactWithInheritanceHierarchy)))
         .Returns (_metaModel.GetTable (typeof (ContactWithInheritanceHierarchy)).RowType.DataMembers.Where (dataMember => !dataMember.IsAssociation).ToArray())
         .Verifiable();

      var rowWrapper = new RowWrapper (_readerMock.Object, _reverseMappingResolverMock.Object);

      var columnIDs = new[]
                      {
                          new ColumnID ("ContactID", 1),
                          new ColumnID ("ContactType", 2)
                      };

      var customer = rowWrapper.GetEntity<ContactWithInheritanceHierarchy> (columnIDs);

      _readerMock.Verify();
      _reverseMappingResolverMock.Verify();
      Assert.IsInstanceOf (typeof (ContactWithInheritanceHierarchy.CustomerContact), customer);
    }

    [Test]
    public void GetEntity_CreateDefaultInstanceIfDiscriminatorIsNull ()
    {
      _readerMock
         .Setup (mock => mock.GetValue (2))
         .Returns (null) //return value of discriminator column
         .Verifiable();
      _reverseMappingResolverMock
         .Setup (mock => mock.GetMetaDataMembers (typeof (ContactWithInheritanceHierarchy)))
         .Returns (_metaModel.GetTable (typeof (ContactWithInheritanceHierarchy)).RowType.DataMembers.Where (dataMember => !dataMember.IsAssociation).ToArray())
         .Verifiable();

      var rowWrapper = new RowWrapper (_readerMock.Object, _reverseMappingResolverMock.Object);

      var columnIDs = new[]
                      {
                          new ColumnID ("ContactID", 1),
                          new ColumnID ("ContactType", 2)
                      };

      var contact = rowWrapper.GetEntity<ContactWithInheritanceHierarchy> (columnIDs);

      _readerMock.Verify();
      _reverseMappingResolverMock.Verify();
      Assert.IsInstanceOf (typeof (ContactWithInheritanceHierarchy), contact);
    }

    [Test]
    public void GetEntity_AdditionalMappedMembersInSubTypesDontCauseException ()
    {
      _readerMock
         .Setup (mock => mock.GetValue (2))
         .Returns (1)
         .Verifiable();
      _readerMock
         .Setup (mock => mock.GetValue (3))
         .Returns ("Supplier") //return value of discriminator column
         .Verifiable();
      _readerMock
         .Setup (mock => mock.GetValue (1))
         .Returns ("www.homepage.com")
         .Verifiable();

      _reverseMappingResolverMock
         .Setup (mock => mock.GetMetaDataMembers (typeof (ContactWithInheritanceHierarchy)))
         .Returns (_metaModel.GetMetaType (typeof (ContactWithInheritanceHierarchy.SupplierContact)).DataMembers.ToArray())
         .Verifiable();

      var rowWrapper = new RowWrapper (_readerMock.Object, _reverseMappingResolverMock.Object);

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

      _readerMock.Verify();
      _reverseMappingResolverMock.Verify();

      Assert.That (contact.Equals (expectedContact), Is.True);
    }

    [Test]
    public void GetEntity_ByteArraysCanBeUsed ()
    {
      var encoding = new System.Text.UTF8Encoding();
      var pw = encoding.GetBytes ("passwordtext");

      var photo = encoding.GetBytes ("myPhoto");

      _readerMock
         .Setup (mock => mock.GetValue (1))
         .Returns (pw)
         .Verifiable();
      _readerMock
         .Setup (mock => mock.GetValue (2))
         .Returns (photo)
         .Verifiable();
      _readerMock
         .Setup (mock => mock.GetValue (3))
         .Returns (1) //return value of discriminator column
         .Verifiable();
      _readerMock
         .Setup (mock => mock.GetValue (4))
         .Returns ("Employee")
         .Verifiable();

      _reverseMappingResolverMock
         .Setup (mock => mock.GetMetaDataMembers (typeof (ContactWithInheritanceHierarchy)))
         .Returns (_metaModel.GetMetaType (typeof (ContactWithInheritanceHierarchy.EmployeeContact)).DataMembers.ToArray())
         .Verifiable();

      var rowWrapper = new RowWrapper (_readerMock.Object, _reverseMappingResolverMock.Object);

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

      _readerMock.Verify();
      _reverseMappingResolverMock.Verify();

      Assert.That (contact.Equals (expectedContact), Is.True);
    }
  }
}