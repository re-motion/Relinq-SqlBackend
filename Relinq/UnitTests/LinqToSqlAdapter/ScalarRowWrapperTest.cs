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
using System.Data;
using NUnit.Framework;
using Remotion.Linq.UnitTests.LinqToSqlAdapter.TestDomain;
using Remotion.Linq.LinqToSqlAdapter;
using Remotion.Linq.SqlBackend.SqlGeneration;
using Rhino.Mocks;
using System;

namespace Remotion.Linq.UnitTests.LinqToSqlAdapter
{
  [TestFixture]
  public class ScalarRowWrapperTest
  {
    private IDataReader _readerMock;

    [SetUp]
    public void SetUp ()
    {
      _readerMock = MockRepository.GenerateMock<IDataReader>();
    }

    [Test]
    public void GetValue_ShouldReturnValue ()
    {
      var columnID = new ColumnID ("Name", 0);
      var scalarRowWrapper = new ScalarRowWrapper (_readerMock);
      _readerMock
          .Expect (mock => mock.GetValue (columnID.Position))
          .Return ("Peter");

      var value = scalarRowWrapper.GetValue<string> (columnID);

      _readerMock.VerifyAllExpectations();
      Assert.That ("Peter", Is.EqualTo (value));
    }

    [Test]
    [ExpectedException (typeof (ArgumentException))]
    public void GetValue_ShouldThrowException ()
    {
      var scalarRowWrapper = new ScalarRowWrapper (_readerMock);

      scalarRowWrapper.GetValue<string> (new ColumnID ("Name", 1));
    }

    [Test]
    [ExpectedException (typeof (ArgumentException))]
    public void GetEntity_ShouldThrowException ()
    {
      var scalarRowWrapper = new ScalarRowWrapper (_readerMock);

      scalarRowWrapper.GetEntity<PersonTestClass> (null);
    }

    [Test]
    public void GetEntity_WithSingleColumn ()
    {
      var columnID = new ColumnID ("Name", 0);
      _readerMock
          .Expect (mock => mock.GetValue (columnID.Position))
          .Return ("Peter");

      var scalarRowWrapper = new ScalarRowWrapper (_readerMock);

      var value = scalarRowWrapper.GetEntity<string> (columnID);
      Assert.That ("Peter", Is.EqualTo (value));
    }

    [Test]
    [ExpectedException (typeof (ArgumentException))]
    public void GetEntity_WithMultipleColumns ()
    {
      var columnIDs = new[]
                      {
                          new ColumnID ("FirstName", 1),
                          new ColumnID ("Age", 2)
                      };

      var scalarRowWrapper = new ScalarRowWrapper (_readerMock);

      scalarRowWrapper.GetEntity<PersonTestClass> (columnIDs);
    }
  }
}