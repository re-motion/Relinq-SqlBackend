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
using NUnit.Framework;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Linq.SqlBackend.UnitTests.TestDomain;
using Rhino.Mocks;

namespace Remotion.Linq.SqlBackend.UnitTests.SqlStatementModel.Unresolved
{
  [TestFixture]
  public class UnresolvedJoinInfoTest
  {
    private SqlEntityExpression _entityExpression;

    [SetUp]
    public void SetUp ()
    {
      _entityExpression = SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression (typeof (Cook));
    }

    [Test]
    public void Initialization_CardinalityMany_NonEnumerable_Throws ()
    {
      Assert.That (
          () => new UnresolvedJoinInfo (_entityExpression, typeof (Cook).GetProperty ("Substitution"), JoinCardinality.Many),
          Throws.ArgumentException
              .With.Message.EqualTo (
                  "Expected a closed generic type implementing IEnumerable<T>, but found 'Remotion.Linq.SqlBackend.UnitTests.TestDomain.Cook'."
                  + "\r\nParameter name: memberInfo"));
    }

    [Test]
    public void ItemType_CardinalityOne ()
    {
      var joinInfo = new UnresolvedJoinInfo (_entityExpression, typeof (Cook).GetProperty ("Substitution"), JoinCardinality.One);
      Assert.That (joinInfo.ItemType, Is.SameAs (typeof (Cook)));
    }

    [Test]
    public void ItemType_CardinalityMany ()
    {
      var entityExpression = SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression (typeof (Restaurant));
      var joinInfo = new UnresolvedJoinInfo (entityExpression, typeof (Restaurant).GetProperty ("Cooks"), JoinCardinality.Many);
      Assert.That (joinInfo.ItemType, Is.SameAs (typeof (Cook)));
    }

    [Test]
    public void Accept ()
    {
      var joinInfo = SqlStatementModelObjectMother.CreateUnresolvedJoinInfo_KitchenCook();

      var joinInfoVisitorMock = MockRepository.GenerateMock<IJoinInfoVisitor>();
      joinInfoVisitorMock.Expect (mock => mock.VisitUnresolvedJoinInfo (joinInfo));

      joinInfoVisitorMock.Replay();

      joinInfo.Accept (joinInfoVisitorMock);
      joinInfoVisitorMock.VerifyAllExpectations();
    }

    [Test]
    public void GetResolvedTableInfo_Throws ()
    {
      var joinInfo = SqlStatementModelObjectMother.CreateUnresolvedJoinInfo_KitchenCook();
      Assert.That (
          () => joinInfo.GetResolvedJoinInfo(),
          Throws.InvalidOperationException
              .With.Message.EqualTo (
                  "This join has not yet been resolved; call the resolution step first."));
    }

    [Test]
    public new void ToString ()
    {
      var joinInfo = SqlStatementModelObjectMother.CreateUnresolvedJoinInfo_KitchenCook ();
      var result = joinInfo.ToString ();

      Assert.That (result, Is.EqualTo ("Kitchen.Cook"));
    }
  }
}