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
using Remotion.Data.Linq.Clauses.ResultOperators;
using Remotion.Data.Linq.Clauses.StreamedData;
using Remotion.Data.Linq.SqlBackend.SqlPreparation;
using Remotion.Data.Linq.SqlBackend.SqlPreparation.ResultOperatorHandlers;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Data.Linq.UnitTests.Linq.Core;
using Remotion.Data.Linq.UnitTests.Linq.Core.Parsing;
using Remotion.Data.Linq.UnitTests.Linq.Core.TestDomain;
using Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlStatementModel;
using Rhino.Mocks;

namespace Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlPreparation.ResultOperatorHandlers
{
  [TestFixture]
  public class SkipResultOperatorHandlerTest
  {
    private ISqlPreparationStage _stageMock;
    private UniqueIdentifierGenerator _generator;
    private SkipResultOperatorHandler _handler;
    private SqlStatementBuilder _sqlStatementBuilder;
    private QueryModel _queryModel;
    private SqlPreparationContext _context;

    [SetUp]
    public void SetUp ()
    {
      _stageMock = MockRepository.GenerateMock<ISqlPreparationStage> ();
      _generator = new UniqueIdentifierGenerator ();
      _handler = new SkipResultOperatorHandler ();
      _sqlStatementBuilder = new SqlStatementBuilder (SqlStatementModelObjectMother.CreateSqlStatement ())
      {
        DataInfo = new StreamedSequenceInfo (typeof (Cook[]), Expression.Constant (new Cook ()))
      };
      _sqlStatementBuilder.Orderings.Add (
         new Ordering (Expression.Constant ("order"), OrderingDirection.Asc));
      _queryModel = new QueryModel (ExpressionHelper.CreateMainFromClause_Cook (), ExpressionHelper.CreateSelectClause ());
      _context = new SqlPreparationContext ();
    }

    [Test]
    public void HandleResultOperator ()
    {
      var takeExpression = Expression.Constant (2);
      var resultOperator = new SkipResultOperator (takeExpression);
      var statement = _sqlStatementBuilder.GetSqlStatement();
      var fakeSelectProjection = GetFakeSekectProjectionFromSqlStatement (statement);

      _stageMock
          .Expect (mock => mock.PrepareSelectExpression (Arg<Expression>.Matches(e=>e is NewExpression), Arg<ISqlPreparationContext>.Matches(c=>c==_context)))
          .Return (fakeSelectProjection);

      _handler.HandleResultOperator (resultOperator, _sqlStatementBuilder, _generator, _stageMock, _context);

      Assert.That (_sqlStatementBuilder.DataInfo, Is.SameAs (statement.DataInfo));
      Assert.That (_sqlStatementBuilder.SelectProjection, Is.TypeOf (typeof (MemberExpression)));
      Assert.That (((SqlTable) _sqlStatementBuilder.SqlTables[0]).TableInfo, Is.TypeOf (typeof (ResolvedSubStatementTableInfo)));
      Assert.That (((ResolvedSubStatementTableInfo) ((SqlTable) _sqlStatementBuilder.SqlTables[0]).TableInfo).SqlStatement.SelectProjection, Is.SameAs(fakeSelectProjection));
      Assert.That (_sqlStatementBuilder.WhereCondition, Is.TypeOf (typeof (BinaryExpression)));

      var expectedKeySelector = Expression.MakeMemberAccess (new SqlTableReferenceExpression (_sqlStatementBuilder.SqlTables[0]), fakeSelectProjection.Type.GetProperty ("Key"));
      var expectedValueSelector = Expression.MakeMemberAccess (new SqlTableReferenceExpression (_sqlStatementBuilder.SqlTables[0]), fakeSelectProjection.Type.GetProperty ("Value"));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedKeySelector, _sqlStatementBuilder.SelectProjection);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedValueSelector, ((BinaryExpression) _sqlStatementBuilder.WhereCondition).Left);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedValueSelector, _sqlStatementBuilder.Orderings[0].Expression);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedKeySelector, _context.TryGetExpressionMapping (resultOperator.Count));
    }

    private Expression GetFakeSekectProjectionFromSqlStatement (SqlStatement sqlStatement)
    {
      Expression rowNumberExpression;
      if (sqlStatement.Orderings.Count > 0)
        rowNumberExpression = new SqlRowNumberExpression (sqlStatement.Orderings.ToArray ());
      else
        rowNumberExpression =
           new SqlRowNumberExpression (
               new[]
                {
                    new Ordering (
                    new SqlSubStatementExpression (
                        new SqlStatement (
                            new StreamedScalarValueInfo (typeof (int)), Expression.Constant (1), new SqlTable[0], new Ordering[0], null, null, false)),
                    OrderingDirection.Asc)});

      var tupleType = typeof (KeyValuePair<,>).MakeGenericType (sqlStatement.SelectProjection.Type, rowNumberExpression.Type);
      Expression newSelectProjection = Expression.New (
          tupleType.GetConstructors ()[0],
          new[] { sqlStatement.SelectProjection, rowNumberExpression },
          new[] { tupleType.GetMethod ("get_Key"), tupleType.GetMethod ("get_Value") });

      return newSelectProjection;
    }
  }
}