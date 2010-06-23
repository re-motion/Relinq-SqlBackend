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
using Remotion.Data.Linq.Clauses.ResultOperators;
using Remotion.Data.Linq.Clauses.StreamedData;
using Remotion.Data.Linq.SqlBackend.SqlPreparation;
using Remotion.Data.Linq.SqlBackend.SqlPreparation.ResultOperatorHandlers;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Data.Linq.UnitTests.Linq.Core;
using Remotion.Data.Linq.UnitTests.Linq.Core.Parsing;
using Remotion.Data.Linq.UnitTests.Linq.Core.TestDomain;
using Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlStatementModel;

namespace Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlPreparation.ResultOperatorHandlers
{
  [TestFixture]
  public class GroupResultOperatorHandlerTest
  {
    private ISqlPreparationStage _stage;
    private UniqueIdentifierGenerator _generator;
    private GroupResultOperatorHandler _handler;
    private SqlStatementBuilder _sqlStatementBuilder;
    // TODO Review 2904: Unused member. Please always check ReSharper warnings before committing files
    private QueryModel _queryModel;
    private SqlPreparationContext _context;

    [SetUp]
    public void SetUp ()
    {
      _generator = new UniqueIdentifierGenerator ();
      _stage = new DefaultSqlPreparationStage (
          MethodCallTransformerRegistry.CreateDefault (), ResultOperatorHandlerRegistry.CreateDefault (), _generator);
      _handler = new GroupResultOperatorHandler ();
      _sqlStatementBuilder = new SqlStatementBuilder (SqlStatementModelObjectMother.CreateSqlStatement ())
      {
        DataInfo = new StreamedSequenceInfo (typeof (Cook[]), Expression.Constant (new Cook ()))
      };
      _queryModel = new QueryModel (ExpressionHelper.CreateMainFromClause_Cook (), ExpressionHelper.CreateSelectClause ());
      _context = new SqlPreparationContext ();
    }


    [Test]
    public void HandleResultOperator ()
    {
      var keySelector = Expression.Constant("keySelector");
      var elementSelector = Expression.Constant("elementSelector");
      var resultOperator = new GroupResultOperator("itemName", keySelector, elementSelector);

      // TODO Review 2904: Use a mock for the stage to ensure the expressions are prepared
      _handler.HandleResultOperator (resultOperator, _sqlStatementBuilder, _generator, _stage, _context);

      Assert.That (_sqlStatementBuilder.GroupByExpression, Is.SameAs (keySelector));
      
      var expectedSelectProjection = new SqlGroupingSelectExpression (keySelector, elementSelector);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedSelectProjection, _sqlStatementBuilder.SelectProjection);

      // TODO Review 2904: This is not enough to check that UpdateDataInfo is called. For every line you add to a method, there should be an Assert in the test that fails.
      // TODO Review 2904: Comment the call to UpdateDataInfo in the implementation, the test will still work. Then, make the test fail (e.g., by checking the DataInfo.DataType for IGrouping<...,...>). Then, enable the call to UpdateDataInfo - the test should now work again.
      Assert.That (_sqlStatementBuilder.DataInfo, Is.TypeOf (typeof (StreamedSequenceInfo)));
    }

    [Test]
    public void HandleResultOperator_GroupByAfterTopExpression ()
    {
      var topExpression = Expression.Constant ("top");
      _sqlStatementBuilder.TopExpression = topExpression;

      var keySelector = Expression.Constant ("keySelector");
      var elementSelector = Expression.Constant ("elementSelector");
      var resultOperator = new GroupResultOperator ("itemName", keySelector, elementSelector);

      _handler.HandleResultOperator (resultOperator, _sqlStatementBuilder, _generator, _stage, _context);

      Assert.That (_sqlStatementBuilder.GroupByExpression, Is.SameAs(keySelector));
      Assert.That (_sqlStatementBuilder.SqlTables.Count, Is.EqualTo (1));
      Assert.That (((SqlTable) _sqlStatementBuilder.SqlTables[0]).TableInfo, Is.TypeOf (typeof (ResolvedSubStatementTableInfo)));
      var subStatement = ((ResolvedSubStatementTableInfo) ((SqlTable) _sqlStatementBuilder.SqlTables[0]).TableInfo).SqlStatement;
      Assert.That (subStatement.TopExpression, Is.SameAs (topExpression));
    }

    [Test]
    public void HandleResultOperator_GroupByAfterDistinct ()
    {
      _sqlStatementBuilder.IsDistinctQuery = true;

      var keySelector = Expression.Constant ("keySelector");
      var elementSelector = Expression.Constant ("elementSelector");
      var resultOperator = new GroupResultOperator ("itemName", keySelector, elementSelector);

      _handler.HandleResultOperator (resultOperator, _sqlStatementBuilder, _generator, _stage, _context);

      Assert.That (_sqlStatementBuilder.GroupByExpression, Is.SameAs (keySelector));
      Assert.That (_sqlStatementBuilder.SqlTables.Count, Is.EqualTo (1));
      Assert.That (((SqlTable) _sqlStatementBuilder.SqlTables[0]).TableInfo, Is.TypeOf (typeof (ResolvedSubStatementTableInfo)));
      var subStatement = ((ResolvedSubStatementTableInfo) ((SqlTable) _sqlStatementBuilder.SqlTables[0]).TableInfo).SqlStatement;
      Assert.That (subStatement.IsDistinctQuery, Is.True);
    }

    // TODO Review 2904: Add a test for GroupAfterGroup - ie., a GroupResultOperatorHandler applied to a QueryModel with the GroupByExpression already set. The original statement should be moved to a substatement
  }
}