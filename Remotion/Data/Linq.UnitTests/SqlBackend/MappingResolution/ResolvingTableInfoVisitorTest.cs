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
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.SqlBackend.MappingResolution;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Data.Linq.UnitTests.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.UnitTests.TestDomain;
using Rhino.Mocks;

namespace Remotion.Data.Linq.UnitTests.SqlBackend.MappingResolution
{
  [TestFixture]
  public class ResolvingTableInfoVisitorTest
  {
    private ISqlStatementResolver _resolverMock;
    private UnresolvedTableInfo _unresolvedTableInfo;
    private UniqueIdentifierGenerator _generator;

    [SetUp]
    public void SetUp ()
    {
      _resolverMock = MockRepository.GenerateMock<ISqlStatementResolver>();
      _unresolvedTableInfo = SqlStatementModelObjectMother.CreateUnresolvedTableInfo (typeof (Cook));
      _generator = new UniqueIdentifierGenerator();
    }

    [Test]
    public void ResolveTableInfo ()
    {
      var resolvedTableInfo = new ResolvedTableInfo (typeof (int), "Table", "t");
      _resolverMock.Expect (mock => mock.ResolveTableInfo (_unresolvedTableInfo, _generator)).Return (resolvedTableInfo);
      _resolverMock.Replay ();

      var result = ResolvingTableInfoVisitor.ResolveTableInfo (resolvedTableInfo, _resolverMock, _generator);

      Assert.That (result, Is.SameAs (resolvedTableInfo));
    }

    [Test]
    public void ResolveTableInfo_AndRevisitsResult ()
    {
      var unresolvedResult = new UnresolvedTableInfo (Expression.Constant (0), typeof (int));
      var resolvedResult = new ResolvedTableInfo (typeof (int), "Table", "t");

      using (_resolverMock.GetMockRepository().Ordered())
      {
        _resolverMock
            .Expect (mock => mock.ResolveTableInfo (_unresolvedTableInfo, _generator))
            .Return (unresolvedResult);
        _resolverMock
            .Expect (mock => mock.ResolveTableInfo (unresolvedResult, _generator))
            .Return (resolvedResult);
      }
      _resolverMock.Replay ();

      var result = ResolvingTableInfoVisitor.ResolveTableInfo (_unresolvedTableInfo, _resolverMock, _generator);

      Assert.That (result, Is.SameAs (resolvedResult));
      _resolverMock.VerifyAllExpectations();
    }

    [Test]
    public void ResolveTableInfo_AndRevisitsResult_OnlyIfDifferent ()
    {
      _resolverMock
          .Expect (mock => mock.ResolveTableInfo (_unresolvedTableInfo, _generator))
          .Return (_unresolvedTableInfo);
      _resolverMock.Replay ();

      var result = ResolvingTableInfoVisitor.ResolveTableInfo (_unresolvedTableInfo, _resolverMock, _generator);

      Assert.That (result, Is.SameAs (_unresolvedTableInfo));
      _resolverMock.VerifyAllExpectations();
    }
  }
}