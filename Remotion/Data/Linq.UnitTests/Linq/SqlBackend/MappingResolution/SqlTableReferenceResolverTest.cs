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
  public class SqlTableReferenceResolverTest
  {
    private IMappingResolver _resolverMock;
    private UniqueIdentifierGenerator _generator;

    [SetUp]
    public void SetUp ()
    {
      _resolverMock = MockRepository.GenerateStrictMock<IMappingResolver>();
      _generator = new UniqueIdentifierGenerator();
    }

    [Test]
    public void ResolveSqlTableReferenceExpression_WithResolvedSimpleTableInfo ()
    {
      var sqlTable = SqlStatementModelObjectMother.CreateSqlTable_WithResolvedTableInfo (typeof (Cook));
      var expression = new SqlTableReferenceExpression (sqlTable);
      var fakeResult = new SqlEntityExpression (sqlTable, new SqlColumnExpression (typeof (string), "c", "Name", false));

      _resolverMock
          .Expect (mock => mock.ResolveTableReferenceExpression (expression, _generator))
          .Return (fakeResult);
      _resolverMock.Replay();

      var result = SqlTableReferenceResolver.ResolveTableReference (expression, _resolverMock, _generator);

      _resolverMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (fakeResult));
    }

    [Test]
    public void ResolveSqlTableReferenceExpression_WithResolvedSubStatementTableInfo_NamedExpression ()
    {
      var sqlStatement = new SqlStatementBuilder (SqlStatementModelObjectMother.CreateSqlStatement_Resolved (typeof (Cook)))
                         {
                             SelectProjection = new NamedExpression("test", Expression.Constant (5)),
                             DataInfo = new StreamedSequenceInfo (typeof (Cook[]), Expression.Constant (new Cook()))
                         }.GetSqlStatement();
      var tableInfo = new ResolvedSubStatementTableInfo ("q0", sqlStatement);
      var sqlTable = new SqlTable (tableInfo);
      var expression = new SqlTableReferenceExpression (sqlTable);

      var result = SqlTableReferenceResolver.ResolveTableReference (expression, _resolverMock, _generator);

      Assert.That (result, Is.TypeOf (typeof (SqlValueReferenceExpression)));
      Assert.That (((SqlValueReferenceExpression) result).Name, Is.EqualTo ("test"));
      Assert.That (((SqlValueReferenceExpression) result).Alias, Is.EqualTo (tableInfo.TableAlias));
      Assert.That (result.Type, Is.EqualTo (sqlTable.ItemType));
    }
    
    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "The table projection for a referenced sub-statement must be named or an entity.")]
    public void ResolveSqlTableReferenceExpression_WithResolvedSubStatementTableInfo_NotSupportedExpression ()
    {
      var sqlStatement = new SqlStatementBuilder (SqlStatementModelObjectMother.CreateSqlStatement_Resolved (typeof (Cook)))
      {
        SelectProjection = Expression.Constant(0),
        DataInfo = new StreamedSequenceInfo (typeof (Cook[]), Expression.Constant (new Cook ()))
      }.GetSqlStatement ();
      var tableInfo = new ResolvedSubStatementTableInfo ("q0", sqlStatement);
      var sqlTable = new SqlTable (tableInfo);
      var expression = new SqlTableReferenceExpression (sqlTable);

      SqlTableReferenceResolver.ResolveTableReference (expression, _resolverMock, _generator);
    }

    [Test]
    public void ResolveSqlTableReferenceExpression_WithResolvedSubStatementTableInfo_SqlEntityExpression ()
    {
      var subStatementTable = new SqlTable (new ResolvedSimpleTableInfo (typeof (Cook), "CookTable", "c"));
      var sqlStatement = new SqlStatementBuilder (SqlStatementModelObjectMother.CreateSqlStatement_Resolved (typeof (Cook)))
                         {
                             SelectProjection =
                                 new SqlEntityExpression (subStatementTable, new SqlColumnExpression (typeof (string), "c", "Name", false)),
                             DataInfo = new StreamedSequenceInfo (typeof (Cook[]), Expression.Constant (new Cook()))
                         }.GetSqlStatement();
      var tableInfo = new ResolvedSubStatementTableInfo ("q0", sqlStatement);
      var sqlTable = new SqlTable (tableInfo);
      var expression = new SqlTableReferenceExpression (sqlTable);

      var result = SqlTableReferenceResolver.ResolveTableReference (expression, _resolverMock, _generator);

      Assert.That (result, Is.TypeOf (typeof (SqlEntityExpression)));
      Assert.That (((SqlEntityExpression) result).SqlTable, Is.SameAs (sqlTable));
      // TODO Review 2718: check column
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = 
        "This table has not yet been resolved; call the resolution step first.")]
    public void ResolveSqlTableReferenceExpression_VisitUnresolvedTableInfo ()
    {
      var sqlTable = SqlStatementModelObjectMother.CreateSqlTable_WithUnresolvedTableInfo (typeof (Cook));
      var expression = new SqlTableReferenceExpression (sqlTable);

      SqlTableReferenceResolver.ResolveTableReference (expression, _resolverMock, _generator);
    }
  }
}