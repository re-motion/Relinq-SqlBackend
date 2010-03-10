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
    private ISqlStatementResolver _resolverMock;
    private UniqueIdentifierGenerator _uniqueIdentifierGenerator;
    
    private TestableResolvingSqlStatementVisitor _visitor;

    private UnresolvedTableInfo _unresolvedTableInfo;
    private SqlTable _sqlTable;
    private ResolvedTableInfo _fakeResolvedTableInfo;

    [SetUp]
    public void SetUp ()
    {
      _resolverMock = MockRepository.GenerateMock<ISqlStatementResolver>();
      _uniqueIdentifierGenerator = new UniqueIdentifierGenerator ();

      _visitor = new TestableResolvingSqlStatementVisitor (_resolverMock, _uniqueIdentifierGenerator);

      _unresolvedTableInfo = SqlStatementModelObjectMother.CreateUnresolvedTableInfo_TypeIsCook ();
      _sqlTable = SqlStatementModelObjectMother.CreateSqlTable (_unresolvedTableInfo);
      _fakeResolvedTableInfo = SqlStatementModelObjectMother.CreateResolvedTableInfo_TypeIsCook ();
    }

    [Test]
    public void VisitSqlTable_ResolvesTableInfo ()
    {
      _resolverMock
          .Expect (mock => mock.ResolveTableInfo (_unresolvedTableInfo, _uniqueIdentifierGenerator))
          .Return (_fakeResolvedTableInfo);
      _resolverMock.Replay();
      
      _visitor.VisitSqlTable (_sqlTable);

      _resolverMock.VerifyAllExpectations ();
      Assert.That (_sqlTable.TableInfo, Is.SameAs (_fakeResolvedTableInfo));
    }

    [Test]
    public void VisitSqlTable_ResolvesJoinInfo ()
    {
      var memberInfo = typeof (Kitchen).GetProperty ("Cook");
      var join = _sqlTable.GetOrAddJoin (memberInfo);

      var fakeResolvedJoinInfo = SqlStatementModelObjectMother.CreateResolvedJoinInfo_TypeIsCook();

      using (_resolverMock.GetMockRepository ().Ordered ())
      {
        _resolverMock
            .Expect (mock => mock.ResolveTableInfo (_unresolvedTableInfo, _uniqueIdentifierGenerator))
            .Return (_fakeResolvedTableInfo);
        _resolverMock
            .Expect (mock => mock.ResolveJoinInfo (_sqlTable, (UnresolvedJoinInfo) join.JoinInfo, _uniqueIdentifierGenerator))
            .Return (fakeResolvedJoinInfo);
      }
      _resolverMock.Replay ();

      _visitor.VisitSqlTable (_sqlTable);

      _resolverMock.VerifyAllExpectations ();
      Assert.That (join.JoinInfo, Is.SameAs (fakeResolvedJoinInfo));
    }

    [Test]
    public void VisitSqlTable_ResolvesJoinInfo_Multiple ()
    {
      var join1 = _sqlTable.GetOrAddJoin (typeof (Kitchen).GetProperty ("Cook"));
      var join2 = _sqlTable.GetOrAddJoin (typeof (Kitchen).GetProperty ("Restaurant"));

      var fakeResolvedJoinInfo1 = SqlStatementModelObjectMother.CreateResolvedJoinInfo (typeof (Cook));
      var fakeResolvedJoinInfo2 = SqlStatementModelObjectMother.CreateResolvedJoinInfo (typeof (Restaurant));

      using (_resolverMock.GetMockRepository ().Ordered ())
      {
        _resolverMock
            .Expect (mock => mock.ResolveTableInfo (_unresolvedTableInfo, _uniqueIdentifierGenerator))
            .Return (_fakeResolvedTableInfo);
        _resolverMock
            .Expect (mock => mock.ResolveJoinInfo (_sqlTable, (UnresolvedJoinInfo) join1.JoinInfo, _uniqueIdentifierGenerator))
            .Return (fakeResolvedJoinInfo1);
        _resolverMock
            .Expect (mock => mock.ResolveJoinInfo (_sqlTable, (UnresolvedJoinInfo) join2.JoinInfo, _uniqueIdentifierGenerator))
            .Return (fakeResolvedJoinInfo2);
      }
      _resolverMock.Replay ();

      _visitor.VisitSqlTable (_sqlTable);

      _resolverMock.VerifyAllExpectations ();
      Assert.That (join1.JoinInfo, Is.SameAs (fakeResolvedJoinInfo1));
      Assert.That (join2.JoinInfo, Is.SameAs (fakeResolvedJoinInfo2));
    }

    [Test]
    public void VisitSqlTable_ResolvesJoinInfo_Recursive ()
    {
      var memberInfo = typeof (Kitchen).GetProperty ("Cook");
      var join1 = _sqlTable.GetOrAddJoin (memberInfo);
      var join2 = join1.GetOrAddJoin (typeof (Cook).GetProperty ("Substitution"));
      var join3 = join1.GetOrAddJoin (typeof (Cook).GetProperty ("Name"));

      var fakeResolvedJoinInfo1 = SqlStatementModelObjectMother.CreateResolvedJoinInfo_TypeIsCook ();
      var fakeResolvedJoinInfo2 = SqlStatementModelObjectMother.CreateResolvedJoinInfo_TypeIsCook ();
      var fakeResolvedJoinInfo3 = SqlStatementModelObjectMother.CreateResolvedJoinInfo (typeof (string));

      using (_resolverMock.GetMockRepository ().Ordered ())
      {
        _resolverMock
            .Expect (mock => mock.ResolveTableInfo (_unresolvedTableInfo, _uniqueIdentifierGenerator))
            .Return (_fakeResolvedTableInfo);
        _resolverMock
            .Expect (mock => mock.ResolveJoinInfo (_sqlTable, (UnresolvedJoinInfo) join1.JoinInfo, _uniqueIdentifierGenerator))
            .Return (fakeResolvedJoinInfo1);
        _resolverMock
            .Expect (mock => mock.ResolveJoinInfo (join1, (UnresolvedJoinInfo) join2.JoinInfo, _uniqueIdentifierGenerator))
            .Return (fakeResolvedJoinInfo2);
        _resolverMock
            .Expect (mock => mock.ResolveJoinInfo (join1, (UnresolvedJoinInfo) join3.JoinInfo, _uniqueIdentifierGenerator))
            .Return (fakeResolvedJoinInfo3);
      }
      _resolverMock.Replay ();

      _visitor.VisitSqlTable (_sqlTable);

      _resolverMock.VerifyAllExpectations ();
      Assert.That (join1.JoinInfo, Is.SameAs (fakeResolvedJoinInfo1));
      Assert.That (join2.JoinInfo, Is.SameAs (fakeResolvedJoinInfo2));
      Assert.That (join3.JoinInfo, Is.SameAs (fakeResolvedJoinInfo3));
    }

    [Test]
    public void VisitSelectProjection_ResolvesExpression ()
    {
      var expression = new SqlTableReferenceExpression(_sqlTable);
      var fakeResult = Expression.Constant (0);

      _resolverMock
          .Expect (mock => mock.ResolveTableReferenceExpression (expression, _uniqueIdentifierGenerator))
          .Return (fakeResult);
      _resolverMock.Replay();

      var result = _visitor.VisitSelectProjection(expression);

      _resolverMock.VerifyAllExpectations();

      Assert.That (result, Is.SameAs (fakeResult));
    }

    [Test]
    public void VisitTopExpression_ResolvesExpression ()
    {
      var expression = new SqlTableReferenceExpression (_sqlTable);
      var fakeResult = Expression.Constant (0);

      _resolverMock
          .Expect (mock => mock.ResolveTableReferenceExpression (expression, _uniqueIdentifierGenerator))
          .Return (fakeResult);
      _resolverMock.Replay ();

      var result = _visitor.VisitTopExpression(expression);

      _resolverMock.VerifyAllExpectations ();

      Assert.That (result, Is.SameAs (fakeResult));
    }

    [Test]
    public void VisitWhereCondition_ResolvesExpression ()
    {
      var expression = new SqlTableReferenceExpression (_sqlTable);
      var fakeResult = Expression.Constant (0);

      _resolverMock
          .Expect (mock => mock.ResolveTableReferenceExpression (expression, _uniqueIdentifierGenerator))
          .Return (fakeResult);
      _resolverMock.Replay ();

      var result = _visitor.VisitWhereCondition (expression);

      _resolverMock.VerifyAllExpectations ();

      Assert.That (result, Is.SameAs (fakeResult));
    }
  }
}