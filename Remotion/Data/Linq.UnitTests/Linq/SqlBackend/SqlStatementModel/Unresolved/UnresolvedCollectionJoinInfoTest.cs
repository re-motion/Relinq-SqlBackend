// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// as published by the Free Software Foundation; either version 2.1 of the 
// License, or (at your option) any later version.
// 
// re-motion is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-motion; if not, see http://www.gnu.org/licenses.
// 
using System;
using NUnit.Framework;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Rhino.Mocks;

namespace Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlStatementModel.Unresolved
{
  [TestFixture]
  public class UnresolvedCollectionJoinInfoTest
  {
    // TODO Review 2615: Add Initialization test showing that item type is calculated correctly.
    [Test]
    public void Accept ()
    {
      var joinInfo = SqlStatementModelObjectMother.CreateUnresolvedCollectionJoinInfo_RestaurantCooks ();

      var joinInfoVisitorMock = MockRepository.GenerateMock<IJoinInfoVisitor> ();
      joinInfoVisitorMock.Expect (mock => mock.VisitUnresolvedCollectionJoinInfo (joinInfo));

      joinInfoVisitorMock.Replay ();

      joinInfo.Accept (joinInfoVisitorMock);
      joinInfoVisitorMock.VerifyAllExpectations ();
    }
  }
}