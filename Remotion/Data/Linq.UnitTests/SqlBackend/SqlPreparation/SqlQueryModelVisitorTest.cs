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
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.SqlBackend.SqlPreparation;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.UnitTests.SqlBackend.SqlStatementModel;

namespace Remotion.Data.Linq.UnitTests.SqlBackend.SqlPreparation
{
  [TestFixture]
  public class SqlQueryModelVisitorTest
  {
    private SelectClause _selectClause;
    private MainFromClause _mainFromClause;
    private QueryModel _queryModel;
    private SqlQueryModelVisitor _sqlQueryModelVisitor;
    
    [SetUp]
    public void SetUp ()
    {
      _mainFromClause = ClauseObjectMother.CreateMainFromClause();
      _selectClause = ClauseObjectMother.CreateSelectClause (_mainFromClause);

      _queryModel = new QueryModel (_mainFromClause, _selectClause);
      _sqlQueryModelVisitor = new SqlQueryModelVisitor ();
    }

    [Test]
    public void VisitFromClause_CreatesFromExpression ()
    {
      _sqlQueryModelVisitor.VisitMainFromClause (_mainFromClause, _queryModel);
      _sqlQueryModelVisitor.VisitSelectClause (_selectClause, _queryModel);
      
      var result = _sqlQueryModelVisitor.GetSqlStatement();

      Assert.That (result.FromExpression, Is.Not.Null);
      Assert.That (result.FromExpression.TableSource, Is.TypeOf (typeof (ConstantTableSource)));
      Assert.That (
          ((ConstantTableSource) result.FromExpression.TableSource).ConstantExpression,
          Is.SameAs (_mainFromClause.FromExpression));
    }

    [Test]
    public void VisitFromClause_AddMapping ()
    {
      _sqlQueryModelVisitor.VisitMainFromClause (_mainFromClause, _queryModel);
      _sqlQueryModelVisitor.VisitSelectClause (_selectClause, _queryModel);

      var result = _sqlQueryModelVisitor.GetSqlStatement();

      Assert.That (_sqlQueryModelVisitor.SqlPreparationContext.GetSqlTableForQuerySource (_mainFromClause), Is.Not.Null);
      var expected = result.FromExpression;
      Assert.That ((_sqlQueryModelVisitor.SqlPreparationContext.GetSqlTableForQuerySource(_mainFromClause)).TableSource, Is.SameAs (expected.TableSource));
    }

    [Test]
    public void VisitSelectClause_CreatesSelectProjection ()
    {
      _sqlQueryModelVisitor.VisitMainFromClause (_mainFromClause, _queryModel);
      _sqlQueryModelVisitor.VisitSelectClause (_selectClause, _queryModel);
      
      var result = _sqlQueryModelVisitor.GetSqlStatement ();

      Assert.That (result.SelectProjection, Is.Not.Null);
      Assert.That (result.SelectProjection, Is.TypeOf (typeof (SqlTableReferenceExpression)));
      Assert.That ((((SqlTableReferenceExpression) result.SelectProjection).SqlTable).TableSource, Is.EqualTo (result.FromExpression.TableSource));
    }
  }
}