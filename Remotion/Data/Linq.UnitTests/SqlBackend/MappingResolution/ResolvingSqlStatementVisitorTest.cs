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
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Data.Linq.UnitTests.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.UnitTests.TestDomain;
using Rhino.Mocks;

namespace Remotion.Data.Linq.UnitTests.SqlBackend.MappingResolution
{
  [TestFixture]
  public class ResolvingSqlStatementVisitorTest
  {
    private TestableResolvingSqlStatementVisitor _visitor;

    private UnresolvedTableInfo _unresolvedTableInfo;
    private SqlTable _sqlTable;
    private ResolvedSimpleTableInfo _fakeResolvedSimpleTableInfo;
    private IMappingResolutionStage _stageMock;

    [SetUp]
    public void SetUp ()
    {
      _stageMock = MockRepository.GenerateMock<IMappingResolutionStage>();
      
      _visitor = new TestableResolvingSqlStatementVisitor (_stageMock);

      _unresolvedTableInfo = SqlStatementModelObjectMother.CreateUnresolvedTableInfo (typeof (Cook));
      _sqlTable = SqlStatementModelObjectMother.CreateSqlTable (_unresolvedTableInfo);
      _fakeResolvedSimpleTableInfo = SqlStatementModelObjectMother.CreateResolvedTableInfo (typeof (Cook));
    }

    [Test]
    public void VisitSqlTable_ResolvesTableInfo ()
    {
      _stageMock
          .Expect (mock => mock.ResolveTableInfo (_unresolvedTableInfo))
          .Return (_fakeResolvedSimpleTableInfo);
      _stageMock.Replay();

      _visitor.VisitSqlTable (_sqlTable);

      _stageMock.VerifyAllExpectations();
      Assert.That (_sqlTable.TableInfo, Is.SameAs (_fakeResolvedSimpleTableInfo));
    }

    [Test]
    public void VisitSqlTable_ResolvesJoinInfo ()
    {
      var memberInfo = typeof (Kitchen).GetProperty ("Cook");
      var join = _sqlTable.GetOrAddJoin (memberInfo, JoinCardinality.One);

      var fakeResolvedJoinInfo = SqlStatementModelObjectMother.CreateResolvedJoinInfo (typeof (Cook));

      using (_stageMock.GetMockRepository().Ordered())
      {
        _stageMock
            .Expect (mock => mock.ResolveTableInfo (_unresolvedTableInfo))
            .Return (_fakeResolvedSimpleTableInfo);
        _stageMock
            .Expect (mock => mock.ResolveJoinInfo ((UnresolvedJoinInfo) join.JoinInfo))
            .Return (fakeResolvedJoinInfo);
      }
      _stageMock.Replay();

      _visitor.VisitSqlTable (_sqlTable);

      _stageMock.VerifyAllExpectations();
      Assert.That (join.JoinInfo, Is.SameAs (fakeResolvedJoinInfo));
    }

    [Test]
    public void VisitSqlTable_ResolvesJoinInfo_Multiple ()
    {
      var join1 = _sqlTable.GetOrAddJoin (typeof (Kitchen).GetProperty ("Cook"), JoinCardinality.One);
      var join2 = _sqlTable.GetOrAddJoin (typeof (Kitchen).GetProperty ("Restaurant"), JoinCardinality.One);

      var fakeResolvedJoinInfo1 = SqlStatementModelObjectMother.CreateResolvedJoinInfo (typeof (Cook));
      var fakeResolvedJoinInfo2 = SqlStatementModelObjectMother.CreateResolvedJoinInfo (typeof (Restaurant));

      using (_stageMock.GetMockRepository().Ordered())
      {
        _stageMock
            .Expect (mock => mock.ResolveTableInfo (_unresolvedTableInfo))
            .Return (_fakeResolvedSimpleTableInfo);
        _stageMock
            .Expect (mock => mock.ResolveJoinInfo ((UnresolvedJoinInfo) join1.JoinInfo))
            .Return (fakeResolvedJoinInfo1);
        _stageMock
            .Expect (mock => mock.ResolveJoinInfo ((UnresolvedJoinInfo) join2.JoinInfo))
            .Return (fakeResolvedJoinInfo2);
      }
      _stageMock.Replay();

      _visitor.VisitSqlTable (_sqlTable);

      _stageMock.VerifyAllExpectations();
      Assert.That (join1.JoinInfo, Is.SameAs (fakeResolvedJoinInfo1));
      Assert.That (join2.JoinInfo, Is.SameAs (fakeResolvedJoinInfo2));
    }

    [Test]
    public void VisitSqlTable_ResolvesJoinInfo_Recursive ()
    {
      var memberInfo = typeof (Kitchen).GetProperty ("Cook");
      var join1 = _sqlTable.GetOrAddJoin (memberInfo, JoinCardinality.One);
      var join2 = join1.GetOrAddJoin (typeof (Cook).GetProperty ("Substitution"), JoinCardinality.One);
      var join3 = join1.GetOrAddJoin (typeof (Cook).GetProperty ("Name"), JoinCardinality.One);

      var fakeResolvedJoinInfo1 = SqlStatementModelObjectMother.CreateResolvedJoinInfo (typeof (Cook));
      var fakeResolvedJoinInfo2 = SqlStatementModelObjectMother.CreateResolvedJoinInfo (typeof (Cook));
      var fakeResolvedJoinInfo3 = SqlStatementModelObjectMother.CreateResolvedJoinInfo (typeof (string));

      using (_stageMock.GetMockRepository().Ordered())
      {
        _stageMock
            .Expect (mock => mock.ResolveTableInfo (_unresolvedTableInfo))
            .Return (_fakeResolvedSimpleTableInfo);
        _stageMock
            .Expect (mock => mock.ResolveJoinInfo ((UnresolvedJoinInfo) join1.JoinInfo))
            .Return (fakeResolvedJoinInfo1);
        _stageMock
            .Expect (mock => mock.ResolveJoinInfo ((UnresolvedJoinInfo) join2.JoinInfo))
            .Return (fakeResolvedJoinInfo2);
        _stageMock
            .Expect (mock => mock.ResolveJoinInfo ((UnresolvedJoinInfo) join3.JoinInfo))
            .Return (fakeResolvedJoinInfo3);
      }
      _stageMock.Replay();

      _visitor.VisitSqlTable (_sqlTable);

      _stageMock.VerifyAllExpectations();
      Assert.That (join1.JoinInfo, Is.SameAs (fakeResolvedJoinInfo1));
      Assert.That (join2.JoinInfo, Is.SameAs (fakeResolvedJoinInfo2));
      Assert.That (join3.JoinInfo, Is.SameAs (fakeResolvedJoinInfo3));
    }

    [Test]
    public void VisitSelectProjection_ResolvesExpression ()
    {
      var expression = new SqlTableReferenceExpression (_sqlTable);
      var fakeResult = Expression.Constant (0);

      _stageMock
          .Expect (mock => mock.ResolveSelectExpression (expression))
          .Return (fakeResult);
      _stageMock.Replay();

      var result = _visitor.VisitSelectProjection (expression);

      _stageMock.VerifyAllExpectations();

      Assert.That (result, Is.SameAs (fakeResult));
    }

    [Test]
    public void VisitTopExpression_ResolvesExpression ()
    {
      var expression = new SqlTableReferenceExpression (_sqlTable);
      var fakeResult = Expression.Constant (0);

      _stageMock
          .Expect (mock => mock.ResolveTopExpression (expression))
          .Return (fakeResult);
      _stageMock.Replay();

      var result = _visitor.VisitTopExpression (expression);

      _stageMock.VerifyAllExpectations();

      Assert.That (result, Is.SameAs (fakeResult));
    }

    [Test]
    public void VisitWhereCondition_ResolvesExpression ()
    {
      var expression = new SqlTableReferenceExpression (_sqlTable);
      var fakeResult = Expression.Constant (0);

      _stageMock
          .Expect (mock => mock.ResolveWhereExpression (expression))
          .Return (fakeResult);
      _stageMock.Replay();

      var result = _visitor.VisitWhereCondition (expression);

      _stageMock.VerifyAllExpectations();

      Assert.That (result, Is.SameAs (fakeResult));
    }

    [Test]
    public void VisitOrderExpression_ResolvesExpression ()
    {
      var expression = new SqlTableReferenceExpression (_sqlTable);
      var fakeResult = Expression.Constant (0);

      _stageMock
          .Expect (mock => mock.ResolveOrderingExpression (expression))
          .Return (fakeResult);
      _stageMock.Replay();

      var result = _visitor.VisitOrderingExpression (expression);

      _stageMock.VerifyAllExpectations();

      Assert.That (result, Is.SameAs (fakeResult));
    }
  }
}