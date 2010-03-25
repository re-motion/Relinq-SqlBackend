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
using Remotion.Data.Linq.Backend;
using Remotion.Data.Linq.Backend.DataObjectModel;
using Remotion.Data.Linq.Backend.DetailParsing;
using Remotion.Data.Linq.Backend.DetailParsing.WhereConditionParsing;
using Remotion.Data.Linq.Backend.FieldResolving;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.Clauses.ResultOperators;
using Remotion.Data.Linq.UnitTests.Linq.Core.Backend.DetailParsing;
using Remotion.Data.Linq.UnitTests.Linq.Core.TestDomain;

namespace Remotion.Data.Linq.UnitTests.Linq.Core.Backend.DetailParsing.WhereConditionParsing
{
  [TestFixture]
  public class SubQueryExpressionParserTest : DetailParserTestBase
  {
    private WhereConditionParserRegistry _whereConditionParserRegistry;
    private SubQueryExpressionParser _subQueryExpressionParser;

    public override void SetUp ()
    {
      base.SetUp ();

      var resolver = new FieldResolver (StubDatabaseInfo.Instance, new WhereFieldAccessPolicy (StubDatabaseInfo.Instance));
      _whereConditionParserRegistry = new WhereConditionParserRegistry (StubDatabaseInfo.Instance);
      _whereConditionParserRegistry.RegisterParser (typeof (ConstantExpression), new ConstantExpressionParser (StubDatabaseInfo.Instance));
      _whereConditionParserRegistry.RegisterParser (typeof (MemberExpression), new MemberExpressionParser (resolver));

      _subQueryExpressionParser = new SubQueryExpressionParser (_whereConditionParserRegistry);
      _subQueryExpressionParser = new SubQueryExpressionParser (_whereConditionParserRegistry);
    }

    [Test]
    public void CanParse_SubQueryExpression ()
    {
      var subQueryExpression = new SubQueryExpression (ExpressionHelper.CreateQueryModel_Cook());
      Assert.That (_subQueryExpressionParser.CanParse (subQueryExpression), Is.True);
    }

    [Test]
    public void ParseSubQuery ()
    {
      QueryModel subQueryModel = ExpressionHelper.CreateQueryModel_Cook();
      var subQueryExpression = new SubQueryExpression (subQueryModel);

      var expectedSubQuery = new SubQuery (subQueryModel, ParseMode.SubQueryInWhere, null);

      ICriterion actualCriterion = _subQueryExpressionParser.Parse (subQueryExpression, ParseContext);

      Assert.That (actualCriterion, Is.EqualTo (expectedSubQuery));
    }

    [Test]
    public void ParseSubQuery_WithContains ()
    {
      QueryModel subQueryModel = ExpressionHelper.CreateQueryModel (ExpressionHelper.CreateMainFromClause_Int());
      subQueryModel.ResultOperators.Add (new ContainsResultOperator (Expression.Constant (20)));

      var subQueryExpression = new SubQueryExpression (subQueryModel);

      ICriterion actualCriterion = _subQueryExpressionParser.Parse (subQueryExpression, ParseContext);

      Assert.That (actualCriterion, Is.InstanceOfType (typeof (BinaryCondition)));
      var containsCriterion = (BinaryCondition) actualCriterion;
      Assert.That (containsCriterion.Kind, Is.EqualTo (BinaryCondition.ConditionKind.Contains));
      Assert.That (containsCriterion.Right, Is.EqualTo (new Constant (20)));

      var subQueryModelWithoutResultOperator = subQueryModel.Clone();
      subQueryModelWithoutResultOperator.ResultOperators.RemoveAt (0);

      var subQuery = ((SubQuery) containsCriterion.Left);
      Assert.That (subQuery.QueryModel.ToString (), Is.EqualTo (subQueryModelWithoutResultOperator.ToString ()));
      Assert.That (subQuery.ParseMode, Is.EqualTo (ParseMode.SubQueryInWhere));
      Assert.That (subQuery.Alias, Is.Null);
    }

    [Test]
    public void ParseSubQuery_WithContains_OnConstantLeftSide ()
    {
      var constantLeftSide = new[] { 1, 2, 3 };
      
      var mainFromClause = new MainFromClause ("i", typeof (int), Expression.Constant (constantLeftSide));
      QueryModel subQueryModel = new QueryModel(mainFromClause, new SelectClause (new QuerySourceReferenceExpression (mainFromClause)));
      subQueryModel.ResultOperators.Add (new ContainsResultOperator (Expression.Constant (20)));

      var subQueryExpression = new SubQueryExpression (subQueryModel);

      ICriterion actualCriterion = _subQueryExpressionParser.Parse (subQueryExpression, ParseContext);

      Assert.That (actualCriterion, Is.InstanceOfType (typeof (BinaryCondition)));
      var containsCriterion = (BinaryCondition) actualCriterion;
      Assert.That (containsCriterion.Kind, Is.EqualTo (BinaryCondition.ConditionKind.Contains));
      Assert.That (containsCriterion.Right, Is.EqualTo (new Constant (20)));
      Assert.That (containsCriterion.Left, Is.EqualTo (new Constant (constantLeftSide)));
    }
  }
}