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
  public class ResolvingTableSourceVisitorTest
  {
    private ISqlStatementResolver _resolverMock;
    private ConstantTableSource _constantTableSource;
    private JoinedTableSource _joinedTableSource;

    [SetUp]
    public void SetUp ()
    {
      _resolverMock = MockRepository.GenerateMock<ISqlStatementResolver>();
      _constantTableSource = SqlStatementModelObjectMother.CreateConstantTableSource_TypeIsCook ();
      _joinedTableSource = SqlStatementModelObjectMother.CreateJoinedTableSource_KitchenCook();
    }

    [Test]
    public void ResolveConstantTableSource ()
    {
      var tableSource = new SqlTableSource (typeof (int), "Table", "t");
      _resolverMock.Expect (mock => mock.ResolveConstantTableSource (_constantTableSource)).Return (tableSource);
      _resolverMock.Replay ();

      var result = ResolvingTableSourceVisitor.ResolveTableSource (tableSource, _resolverMock);

      Assert.That (result, Is.SameAs (tableSource));
    }

    [Test]
    public void ResolveConstantTableSource_AndRevisitsResult ()
    {
      var unresolvedResult = new ConstantTableSource (Expression.Constant (0), typeof (int));
      var resolvedResult = new SqlTableSource (typeof (int), "Table", "t");

      using (_resolverMock.GetMockRepository().Ordered())
      {
        _resolverMock
            .Expect (mock => mock.ResolveConstantTableSource (_constantTableSource))
            .Return (unresolvedResult);
        _resolverMock
            .Expect (mock => mock.ResolveConstantTableSource (unresolvedResult))
            .Return (resolvedResult);
      }
      _resolverMock.Replay ();

      var result = ResolvingTableSourceVisitor.ResolveTableSource (_constantTableSource, _resolverMock);

      Assert.That (result, Is.SameAs (resolvedResult));
      _resolverMock.VerifyAllExpectations();
    }

    [Test]
    public void ResolveConstantTableSource_AndRevisitsResult_OnlyIfDifferent ()
    {
      _resolverMock
          .Expect (mock => mock.ResolveConstantTableSource (_constantTableSource))
          .Return (_constantTableSource);
      _resolverMock.Replay ();

      var result = ResolvingTableSourceVisitor.ResolveTableSource (_constantTableSource, _resolverMock);

      Assert.That (result, Is.SameAs (_constantTableSource));
      _resolverMock.VerifyAllExpectations();
    }

    [Test]
    [ExpectedException (typeof (NotImplementedException))]
    public void ResolveConstantTableSource_WithUnknownTableSource ()
    {
      var sqlTable = new SqlTable (new UnknownTableSource());
      ResolvingTableSourceVisitor.ResolveTableSource (sqlTable.TableSource, _resolverMock);
    }

    [Test]
    public void ResolveJoinInfo_ResolvesJoinedTableSource ()
    {
      var cookSource = new SqlTableSource (typeof (string), "Cook", "c");
      var primaryColumn = new SqlColumnExpression (typeof (int), "k", "ID");
      var foreignColumn = new SqlColumnExpression (typeof (int), "c", "KitchenID");

      var sqlJoinedTableSource = new SqlJoinedTableSource (cookSource, primaryColumn, foreignColumn);

      _resolverMock
          .Expect (mock => mock.ResolveJoinedTableSource (Arg<JoinedTableSource>.Is.Anything))
          .Return (sqlJoinedTableSource);
      _resolverMock.Replay();

      var result = ResolvingTableSourceVisitor.ResolveJoinInfo (_joinedTableSource, _resolverMock);

      Assert.That (result, Is.SameAs (sqlJoinedTableSource));
      _resolverMock.VerifyAllExpectations();
    }

    [Test]
    public void ResolveJoinInfo_ResolvesJoinedTableSource_AndRevisitsResult ()
    {
      var memberInfo = typeof (Cook).GetProperty ("Substitution");
      var unresolvedResult = new JoinedTableSource (memberInfo);

      var foreignTableSource = new SqlTableSource (typeof (Cook), "CookTable", "s");
      var resolvedResult = new SqlJoinedTableSource (
          foreignTableSource, new SqlColumnExpression (typeof (int), "c", "ID"), new SqlColumnExpression (typeof (int), "s", "ID"));

      using (_resolverMock.GetMockRepository().Ordered())
      {
        _resolverMock
            .Expect (mock => mock.ResolveJoinedTableSource (_joinedTableSource))
            .Return (unresolvedResult);
        _resolverMock
            .Expect (mock => mock.ResolveJoinedTableSource (unresolvedResult))
            .Return (resolvedResult);
      }
      _resolverMock.Replay ();

      var result = ResolvingTableSourceVisitor.ResolveJoinInfo (_joinedTableSource, _resolverMock);

      Assert.That (result, Is.SameAs (resolvedResult));
      _resolverMock.VerifyAllExpectations();
    }

    [Test]
    public void ResolveJoinInfo_ResolvesJoinedTableSource_AndRevisitsResult_OnlyIfDifferent ()
    {
      _resolverMock
          .Expect (mock => mock.ResolveJoinedTableSource (_joinedTableSource))
          .Return (_joinedTableSource);
      _resolverMock.Replay ();
     
      var result = ResolvingTableSourceVisitor.ResolveJoinInfo (_joinedTableSource, _resolverMock);

      Assert.That (result, Is.SameAs (_joinedTableSource));
      _resolverMock.VerifyAllExpectations ();
    }
  }
}