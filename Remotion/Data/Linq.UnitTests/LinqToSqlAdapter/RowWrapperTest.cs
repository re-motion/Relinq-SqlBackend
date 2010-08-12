// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System.Data;
using System.Data.Linq.Mapping;
using System.Linq;
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

      ColumnID[] columnIDs = new[]
                             {
                                 new ColumnID ("FirstName", 1),
                                 new ColumnID ("Age", 2)
                             };

      var rowWrapper = new RowWrapper (_readerMock, _reverseMappingResolverMock);

      var instance = rowWrapper.GetEntity<PersonTestClass> (columnIDs);
      Assert.AreEqual (
          instance,
          new PersonTestClass ("Peter", 21));

      _readerMock.VerifyAllExpectations ();
    }
  }
}