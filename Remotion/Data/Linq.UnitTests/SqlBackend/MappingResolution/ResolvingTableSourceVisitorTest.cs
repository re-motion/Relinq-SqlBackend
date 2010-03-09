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
    private SqlTable _constantSqlTable;
    private SqlTable _joinedSqlTable;

    [SetUp]
    public void SetUp ()
    {
      _resolverMock = MockRepository.GenerateMock<ISqlStatementResolver>();
      _constantSqlTable = SqlStatementModelObjectMother.CreateSqlTableWithConstantTableSource();
      _joinedSqlTable = SqlStatementModelObjectMother.CreateSqlTableWithJoinedTableSource();
    }

    [Test]
    public void ResolveConstantTableSource ()
    {
      var tableSource = new SqlTableSource (typeof (int), "Table", "t");
      _resolverMock.Expect (mock => mock.ResolveConstantTableSource ((ConstantTableSource) _constantSqlTable.TableSource)).Return (tableSource);

      var result = ResolvingTableSourceVisitor.ResolveTableSource (tableSource, _resolverMock);

      Assert.That (result, Is.TypeOf (typeof (SqlTableSource)));
    }

    [Test]
    public void ResolveConstantTableSource_AndRevisitsResult ()
    {
      var unresolvedResult = new ConstantTableSource (Expression.Constant (0), typeof (int));
      var resolvedResult = new SqlTableSource (typeof (int), "Table", "t");

      using (_resolverMock.GetMockRepository().Ordered())
      {
        _resolverMock
            .Expect (mock => mock.ResolveConstantTableSource ((ConstantTableSource) _constantSqlTable.TableSource))
            .Return (unresolvedResult);
        _resolverMock
            .Expect (mock => mock.ResolveConstantTableSource (unresolvedResult))
            .Return (resolvedResult);
      }

      var result = ResolvingTableSourceVisitor.ResolveTableSource (_constantSqlTable.TableSource, _resolverMock);

      Assert.That (result, Is.SameAs (resolvedResult));
      _resolverMock.VerifyAllExpectations();
    }

    [Test]
    public void ResolveConstantTableSource_AndRevisitsResult_OnlyIfDifferent ()
    {
      _resolverMock
          .Expect (mock => mock.ResolveConstantTableSource ((ConstantTableSource) _constantSqlTable.TableSource))
          .Return (_constantSqlTable.TableSource);

      var result = ResolvingTableSourceVisitor.ResolveTableSource (_constantSqlTable.TableSource, _resolverMock);

      Assert.That (result, Is.SameAs (_constantSqlTable.TableSource));
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
    public void ResolveJoinedTableSource ()
    {
      var kitchenSource = new SqlTableSource (typeof (Kitchen), "Kitchen", "k");
      var cookSource = new SqlTableSource (typeof (string), "Cook", "c");
      var primaryColumn = new SqlColumnExpression (typeof (int), "k", "ID");
      var foreignColumn = new SqlColumnExpression (typeof (int), "c", "KitchenID");

      var sqlJoinedTableSource = new SqlJoinedTableSource (cookSource, primaryColumn, foreignColumn);

      _resolverMock.Expect (mock => mock.ResolveJoinedTableSource (Arg<JoinedTableSource>.Is.Anything)).Return (sqlJoinedTableSource);

      var result = ResolvingTableSourceVisitor.ResolveTableSource (_joinedSqlTable.TableSource, _resolverMock);

      Assert.That (result, Is.TypeOf (typeof (SqlJoinedTableSource)));
    }

    [Test]
    public void ResolveJoinedTableSource_AndRevisitsResult ()
    {
      var memberInfo = typeof (Cook).GetProperty ("Substitution");
      var unresolvedResult = new JoinedTableSource (memberInfo);

      var foreignTableSource = new SqlTableSource (typeof (Cook), "CookTable", "s");
      var resolvedResult = new SqlJoinedTableSource (
          foreignTableSource, new SqlColumnExpression (typeof (int), "c", "ID"), new SqlColumnExpression (typeof (int), "s", "ID"));

      using (_resolverMock.GetMockRepository().Ordered())
      {
        _resolverMock
            .Expect (mock => mock.ResolveJoinedTableSource ((JoinedTableSource) _joinedSqlTable.TableSource))
            .Return (unresolvedResult);
        _resolverMock
            .Expect (mock => mock.ResolveJoinedTableSource (unresolvedResult))
            .Return (resolvedResult);
      }

      var result = ResolvingTableSourceVisitor.ResolveTableSource (_joinedSqlTable.TableSource, _resolverMock);

      Assert.That (result, Is.SameAs (resolvedResult));
      _resolverMock.VerifyAllExpectations();
    }

    [Test]
    public void ResolveJoinedTableSource_AndRevisitsResult_OnlyIfDifferent ()
    {
      _resolverMock
          .Expect (mock => mock.ResolveJoinedTableSource ((JoinedTableSource) _joinedSqlTable.TableSource))
          .Return (_joinedSqlTable.TableSource);
     
      var result = ResolvingTableSourceVisitor.ResolveTableSource (_joinedSqlTable.TableSource, _resolverMock);

      Assert.That (result, Is.SameAs (_joinedSqlTable.TableSource));
      _resolverMock.VerifyAllExpectations ();
    }


    private class UnknownTableSource : AbstractTableSource
    {
      public override Type ItemType
      {
        get { return typeof (string); }
      }

      public override AbstractTableSource Accept (ITableSourceVisitor visitor)
      {
        throw new NotImplementedException();
      }
    }
  }
}