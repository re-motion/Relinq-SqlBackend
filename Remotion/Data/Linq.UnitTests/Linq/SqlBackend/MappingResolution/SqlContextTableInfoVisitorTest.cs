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
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Data.Linq.UnitTests.Linq.Core.TestDomain;
using Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlStatementModel;
using Rhino.Mocks;

namespace Remotion.Data.Linq.UnitTests.Linq.SqlBackend.MappingResolution
{
  [TestFixture]
  public class SqlContextTableInfoVisitorTest
  {
    private IMappingResolutionStage _stageMock;
    private IMappingResolutionContext _mappingresolutionContext;

    [SetUp]
    public void SetUp ()
    {
      _stageMock = MockRepository.GenerateMock<IMappingResolutionStage> ();
      _mappingresolutionContext = new MappingResolutionContext();
    }

    [Test]
    public void ApplyContext_ResolvedSimpleTableInfo ()
    {
      var tableInfo = new ResolvedSimpleTableInfo (typeof (Cook), "CookTable", "c");

      var result = SqlContextTableInfoVisitor.ApplyContext (tableInfo, SqlExpressionContext.ValueRequired, _stageMock, _mappingresolutionContext);

      Assert.That (result, Is.SameAs (tableInfo));
    }

    [Test]
    public void ApplyContext_UnresolvedTableInfo ()
    {
      var tableInfo = new UnresolvedTableInfo (typeof (Cook));

      var result = SqlContextTableInfoVisitor.ApplyContext (tableInfo, SqlExpressionContext.ValueRequired, _stageMock, _mappingresolutionContext);

      Assert.That (result, Is.SameAs (tableInfo));
    }

    [Test]
    public void ApplyContext_ResolvedSubStatementTableInfo_SameTableInfo ()
    {
      var subStatement = new SqlStatementBuilder (SqlStatementModelObjectMother.CreateSqlStatement_Resolved (typeof (Cook[])))
      {
        DataInfo = new StreamedSequenceInfo (typeof (IQueryable<Cook>), Expression.Constant (new Cook ()))
      }.GetSqlStatement ();
      var subStatementTableInfo = new ResolvedSubStatementTableInfo ("c", subStatement);
     
      _stageMock
          .Expect (mock => mock.ApplySelectionContext (subStatement, SqlExpressionContext.ValueRequired, _mappingresolutionContext))
          .Return (subStatement);
      _stageMock.Replay ();

      var result = SqlContextTableInfoVisitor.ApplyContext (subStatementTableInfo, SqlExpressionContext.ValueRequired, _stageMock, _mappingresolutionContext);

      Assert.That (result, Is.SameAs (subStatementTableInfo));
      _stageMock.VerifyAllExpectations ();
    }

    [Test]
    public void ApplyContext_ResolvedSubStatementTableInfo_NewTableInfo ()
    {
      var subStatement = new SqlStatementBuilder (SqlStatementModelObjectMother.CreateSqlStatement_Resolved (typeof (Cook[])))
      {
        DataInfo = new StreamedSequenceInfo (typeof (IQueryable<Cook>), Expression.Constant (new Cook ()))
      }.GetSqlStatement ();
      var subStatementTableInfo = new ResolvedSubStatementTableInfo ("c", subStatement);
      var returnedStatement = new SqlStatementBuilder (SqlStatementModelObjectMother.CreateSqlStatement_Resolved (typeof (Cook[])))
      {
        DataInfo = new StreamedSequenceInfo (typeof (IQueryable<Cook>), Expression.Constant (new Cook ()))
      }.GetSqlStatement ();

      _stageMock
          .Expect (mock => mock.ApplySelectionContext (subStatement, SqlExpressionContext.ValueRequired, _mappingresolutionContext))
          .Return (returnedStatement);
      _stageMock.Replay ();

      var result = SqlContextTableInfoVisitor.ApplyContext (subStatementTableInfo, SqlExpressionContext.ValueRequired, _stageMock, _mappingresolutionContext);

      _stageMock.VerifyAllExpectations ();
      Assert.That (result, Is.Not.SameAs (subStatementTableInfo));
    }

    [Test]
    public void ApplyContext_SqlJoinedTable_SameJoinInfo ()
    {
      var tableInfo = new ResolvedSimpleTableInfo (typeof (Cook), "CookTable", "c");
      var resolvedJoinInfo = new ResolvedJoinInfo (tableInfo, new SqlColumnDefinitionExpression (typeof (int), "c", "ID", false), new SqlColumnDefinitionExpression (typeof (int), "r", "CookID", false));
      var sqlJoinedTable = new SqlJoinedTable (resolvedJoinInfo, JoinSemantics.Left);

      _stageMock
          .Expect (mock => mock.ApplyContext (resolvedJoinInfo, SqlExpressionContext.ValueRequired, _mappingresolutionContext))
          .Return (resolvedJoinInfo);
      _stageMock.Replay ();

      var result = SqlContextTableInfoVisitor.ApplyContext (sqlJoinedTable, SqlExpressionContext.ValueRequired, _stageMock, _mappingresolutionContext);

      _stageMock.VerifyAllExpectations ();
      Assert.That (result, Is.SameAs (sqlJoinedTable));
    }

    [Test]
    public void ApplyContext_SqlJoinedTable_NewJoinInfo ()
    {
      var tableInfo = new ResolvedSimpleTableInfo (typeof (Cook), "CookTable", "c");
      var resolvedJoinInfo = new ResolvedJoinInfo (tableInfo, new SqlColumnDefinitionExpression (typeof (int), "c", "ID", false), new SqlColumnDefinitionExpression (typeof (int), "r", "CookID", false));
      var sqlJoinedTable = new SqlJoinedTable (resolvedJoinInfo, JoinSemantics.Left);
      var fakeJoinInfo = new ResolvedJoinInfo (tableInfo, new SqlLiteralExpression (1), new SqlLiteralExpression (1));

      _stageMock
          .Expect (mock => mock.ApplyContext (resolvedJoinInfo, SqlExpressionContext.ValueRequired, _mappingresolutionContext))
          .Return (fakeJoinInfo);
      _stageMock.Replay ();

      var result = SqlContextTableInfoVisitor.ApplyContext (sqlJoinedTable, SqlExpressionContext.ValueRequired, _stageMock, _mappingresolutionContext);

      _stageMock.VerifyAllExpectations ();
      Assert.That (((SqlJoinedTable) result).JoinInfo, Is.SameAs (fakeJoinInfo));
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "UnresolvedGroupReferenceTableInfo is not valid at this point.")]
    public void ApplyContext_UnresolvedGroupReferenceTableInfo ()
    {
      var sqlStatement = new SqlStatementBuilder (SqlStatementModelObjectMother.CreateSqlStatement_Resolved (typeof (Cook[])))
      {
        DataInfo = new StreamedSequenceInfo (typeof (IQueryable<Cook>), Expression.Constant (new Cook ()))
      }.GetSqlStatement ();
      var resolvedSubStatmentTableInfo = new ResolvedSubStatementTableInfo ("cook", sqlStatement);
      var tableInfo = new UnresolvedGroupReferenceTableInfo (resolvedSubStatmentTableInfo);

      SqlContextTableInfoVisitor.ApplyContext (tableInfo, SqlExpressionContext.ValueRequired, _stageMock, _mappingresolutionContext);
    }

  }
}