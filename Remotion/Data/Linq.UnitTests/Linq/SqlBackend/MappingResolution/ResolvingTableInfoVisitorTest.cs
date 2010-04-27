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
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Clauses.StreamedData;
using Remotion.Data.Linq.SqlBackend.MappingResolution;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Data.Linq.UnitTests.Linq.Core.TestDomain;
using Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlStatementModel;
using Rhino.Mocks;

namespace Remotion.Data.Linq.UnitTests.Linq.SqlBackend.MappingResolution
{
  [TestFixture]
  public class ResolvingTableInfoVisitorTest
  {
    private IMappingResolver _resolverMock;
    private UnresolvedTableInfo _unresolvedTableInfo;
    private UniqueIdentifierGenerator _generator;
    private IMappingResolutionStage _stageMock;
    private ResolvedSimpleTableInfo _resolvedTableInfo;
    private SqlStatement _sqlStatement;

    [SetUp]
    public void SetUp ()
    {
      _stageMock = MockRepository.GenerateMock<IMappingResolutionStage>();
      _resolverMock = MockRepository.GenerateMock<IMappingResolver>();
      _unresolvedTableInfo = SqlStatementModelObjectMother.CreateUnresolvedTableInfo (typeof (Cook));
      _resolvedTableInfo = SqlStatementModelObjectMother.CreateResolvedTableInfo (typeof (Cook));
      _generator = new UniqueIdentifierGenerator();
      _sqlStatement = SqlStatementModelObjectMother.CreateSqlStatement_Resolved (typeof (Cook[]));
    }

    [Test]
    public void ResolveTableInfo_Unresolved ()
    {
      var resolvedTableInfo = new ResolvedSimpleTableInfo (typeof (int), "Table", "t");
      _resolverMock.Expect (mock => mock.ResolveTableInfo (_unresolvedTableInfo, _generator)).Return (resolvedTableInfo);
      _resolverMock.Replay();

      var result = ResolvingTableInfoVisitor.ResolveTableInfo (resolvedTableInfo, _resolverMock, _generator, _stageMock);

      Assert.That (result, Is.SameAs (resolvedTableInfo));
    }

   [Test]
    public void ResolveTableInfo_Unresolved_RevisitsResult_OnlyIfDifferent ()
    {
      _resolverMock
          .Expect (mock => mock.ResolveTableInfo (_unresolvedTableInfo, _generator))
          .Return (_resolvedTableInfo);
      _resolverMock.Replay();

      var result = ResolvingTableInfoVisitor.ResolveTableInfo (_unresolvedTableInfo, _resolverMock, _generator, _stageMock);

      Assert.That (result, Is.SameAs (_resolvedTableInfo));
      _resolverMock.VerifyAllExpectations();
    }

    [Test]
    public void ResolveTableInfo_SubStatementTableInfo ()
    {
      _sqlStatement = new SqlStatementBuilder (_sqlStatement) { DataInfo = new StreamedSequenceInfo(typeof(IQueryable<Cook>), Expression.Constant(new Cook())) }.GetSqlStatement();
      
      var sqlSubStatementTableInfo = new ResolvedSubStatementTableInfo ("c", _sqlStatement);

      _stageMock
          .Expect (mock => mock.ResolveSqlSubStatement (_sqlStatement))
          .Return(_sqlStatement);
      _resolverMock.Replay();

      ResolvedSubStatementTableInfo result = (ResolvedSubStatementTableInfo) ResolvingTableInfoVisitor.ResolveTableInfo (sqlSubStatementTableInfo, _resolverMock, _generator, _stageMock);

      _stageMock.VerifyAllExpectations();
      Assert.That (result.SqlStatement, Is.EqualTo (sqlSubStatementTableInfo.SqlStatement));
    }

    [Test]
    public void ResolveTableInfo_SimpleTableInfo ()
    {
      var simpleTableInfo = new ResolvedSimpleTableInfo (typeof (Cook), "CookTable", "c");

      var result = ResolvingTableInfoVisitor.ResolveTableInfo (simpleTableInfo, _resolverMock, _generator, _stageMock);

      _stageMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (simpleTableInfo));
    }

  }
}