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
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Clauses.StreamedData;
using Remotion.Data.Linq.SqlBackend.SqlPreparation;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Data.Linq.UnitTests.Linq.Core.TestDomain;
using Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlStatementModel;
using Rhino.Mocks;

namespace Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlPreparation
{
  [TestFixture]
  public class SqlPreparationSubStatementTableFactoryTest
  {
    private ISqlPreparationStage _stageMock;
    private SqlPreparationContext _context;
    private UniqueIdentifierGenerator _generator;

    [SetUp]
    public void SetUp ()
    {
      _stageMock = MockRepository.GenerateStrictMock<ISqlPreparationStage>();
      _context = new SqlPreparationContext();
      _generator = new UniqueIdentifierGenerator();
    }

    [Test]
    public void CreateSqlTableForSubStatement_NoTopExpression ()
    {
      var builder = new SqlStatementBuilder (SqlStatementModelObjectMother.CreateSqlStatement_Resolved (typeof (Cook[])))
      {
        SelectProjection = Expression.Constant (new Cook ()),
        TopExpression = null,
        DataInfo = new StreamedSequenceInfo (typeof (IQueryable<Cook>), Expression.Constant (new Cook ()))
      };
      builder.Orderings.Add (new Ordering (Expression.Constant ("order1"), OrderingDirection.Asc));
      var statement = builder.GetSqlStatement ();
      var fakeSelectProjection = Expression.Constant (new KeyValuePair<Cook, KeyValuePair<string, object>>());

      _stageMock
          .Expect (mock => mock.PrepareSelectExpression (Arg<Expression>.Is.Anything, Arg<ISqlPreparationContext>.Matches (c => c == _context)))
          .Return (fakeSelectProjection);
      _stageMock.Replay ();

      var result = SqlPreparationSubStatementTableFactory.CreateSqlTableForSubStatement (statement, _stageMock, _context, _generator, info => new SqlTable (info));

      _stageMock.VerifyAllExpectations ();
      Assert.That (result.ItemSelector, Is.TypeOf (typeof (MemberExpression)));
      Assert.That (((MemberExpression) result.ItemSelector).Expression, Is.TypeOf (typeof (SqlTableReferenceExpression)));
      var sqlTable = (SqlTable) ((SqlTableReferenceExpression) ((MemberExpression) result.ItemSelector).Expression).SqlTable;
      Assert.That (sqlTable.TableInfo, Is.TypeOf (typeof (ResolvedSubStatementTableInfo)));
      Assert.That (((ResolvedSubStatementTableInfo) sqlTable.TableInfo).SqlStatement.Orderings.Count, Is.EqualTo (0));
      Assert.That (result.ExtractedOrderings.Count, Is.EqualTo (1));
    }

    [Test]
    public void CreateSqlTableForSubStatement_WithTopExpression ()
    {
      var builder = new SqlStatementBuilder (SqlStatementModelObjectMother.CreateSqlStatement_Resolved (typeof (Cook[])))
      {
        SelectProjection = Expression.Constant (new Cook ()),
        TopExpression = Expression.Constant ("top"),
        DataInfo = new StreamedSequenceInfo (typeof (IQueryable<Cook>), Expression.Constant (new Cook ()))
      };
      builder.Orderings.Add (new Ordering (Expression.Constant ("order1"), OrderingDirection.Asc));
      var statement = builder.GetSqlStatement ();
      var fakeSelectProjection = Expression.Constant (new KeyValuePair<Cook, KeyValuePair<string, object>> ());

      _stageMock
          .Expect (mock => mock.PrepareSelectExpression (Arg<Expression>.Is.Anything, Arg<ISqlPreparationContext>.Matches (c => c == _context)))
          .Return (fakeSelectProjection);
      _stageMock.Replay ();

      var result = SqlPreparationSubStatementTableFactory.CreateSqlTableForSubStatement (statement, _stageMock, _context, _generator, info => new SqlTable (info));

      _stageMock.VerifyAllExpectations ();
      Assert.That (result.ItemSelector, Is.TypeOf (typeof (MemberExpression)));
      Assert.That (((MemberExpression) result.ItemSelector).Expression, Is.TypeOf (typeof (SqlTableReferenceExpression)));
      var sqlTable = (SqlTable) ((SqlTableReferenceExpression) ((MemberExpression) result.ItemSelector).Expression).SqlTable;
      Assert.That (sqlTable.TableInfo, Is.TypeOf (typeof (ResolvedSubStatementTableInfo)));
      Assert.That (((ResolvedSubStatementTableInfo) sqlTable.TableInfo).SqlStatement.Orderings.Count, Is.EqualTo (1));
      Assert.That (result.ExtractedOrderings.Count, Is.EqualTo (1));
    }
  }
}