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
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.SqlBackend.SqlPreparation;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Data.Linq.UnitTests.Linq.Core;
using Remotion.Data.Linq.UnitTests.Linq.Core.Parsing;
using Remotion.Data.Linq.UnitTests.Linq.Core.TestDomain;

namespace Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlPreparation
{
  [TestFixture]
  public class DefaultSqlPreparationStageTest
  {
    private ISqlPreparationContext _context;
    private SqlTable _sqlTable;
    private QuerySourceReferenceExpression _querySourceReferenceExpression;
    private DefaultSqlPreparationStage _stage;

    [SetUp]
    public void SetUp ()
    {
      _context = new SqlPreparationContext();

      var querySource = ExpressionHelper.CreateMainFromClause_Cook();
      _sqlTable = new SqlTable (new UnresolvedTableInfo (typeof (Cook)));

      _context.AddQuerySourceMapping (querySource, _sqlTable);

      _querySourceReferenceExpression = new QuerySourceReferenceExpression (querySource);

      _stage = new DefaultSqlPreparationStage (
          MethodCallTransformerRegistry.CreateDefault(), ResultOperatorHandlerRegistry.CreateDefault(), _context, new UniqueIdentifierGenerator());
    }

    [Test]
    public void PrepareSelectExpression ()
    {
      var result = _stage.PrepareSelectExpression (_querySourceReferenceExpression);

      var expectedExpression = new SqlTableReferenceExpression (_sqlTable);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }

    [Test]
    public void PrepareWhereExpression ()
    {
      var result = _stage.PrepareWhereExpression (_querySourceReferenceExpression);

      var expectedExpression = new SqlTableReferenceExpression (_sqlTable);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }

    [Test]
    public void PrepareTopExpression ()
    {
      var result = _stage.PrepareTopExpression (_querySourceReferenceExpression);

      var expectedExpression = new SqlTableReferenceExpression (_sqlTable);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }

    [Test]
    public void PrepareFromExpression ()
    {
      var result = _stage.PrepareFromExpression (_querySourceReferenceExpression);

      var expectedExpression = new SqlTableReferenceExpression (_sqlTable);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }

    [Test]
    public void PrepareItemExpression ()
    {
      var result = _stage.PrepareFromExpression (_querySourceReferenceExpression);

      var expectedExpression = new SqlTableReferenceExpression (_sqlTable);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }

    [Test]
    public void GetTableForFromExpression ()
    {
      var fromExpression = Expression.Constant (new Cook[0]);
      var result = _stage.PrepareSqlTable (fromExpression, typeof (Cook));

      Assert.That (result, Is.TypeOf (typeof (SqlTable)));
    }

    [Test]
    public void PrepareSqlStatement ()
    {
      var queryModel = ExpressionHelper.CreateQueryModel_Cook();

      var result = _stage.PrepareSqlStatement (queryModel);

      Assert.That (result, Is.Not.Null);
    }
  }
}