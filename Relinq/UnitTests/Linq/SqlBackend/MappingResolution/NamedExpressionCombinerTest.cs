// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (c) rubicon IT GmbH, www.rubicon.eu
// 
// re-linq is free software; you can redistribute it and/or modify it under 
// the terms of the GNU Lesser General Public License as published by the 
// Free Software Foundation; either version 2.1 of the License, 
// or (at your option) any later version.
// 
// re-linq is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-linq; if not, see http://www.gnu.org/licenses.
// 
using System;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using Remotion.Linq.UnitTests.Linq.Core.Parsing;
using Remotion.Linq.UnitTests.Linq.Core.Parsing.ExpressionTreeVisitorTests;
using Remotion.Linq.UnitTests.Linq.Core.TestDomain;
using Remotion.Linq.UnitTests.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.SqlBackend.MappingResolution;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;

namespace Remotion.Linq.UnitTests.Linq.SqlBackend.MappingResolution
{
  [TestFixture]
  public class NamedExpressionCombinerTest
  {
    private IMappingResolutionContext _context;

    [SetUp]
    public void SetUp ()
    {
      _context = new MappingResolutionContext();
    }

    [Test]
    public void ProcessNames_NoSqlEntityExpression_SameExpression ()
    {
      var expression = new NamedExpression ("test", Expression.Constant ("test"));

      var result = NamedExpressionCombiner.ProcessNames (_context, expression);

      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    public void ProcessNames_NamedExpression_ReturnsCombinedExpression ()
    {
      var expression = new NamedExpression ("outer", new NamedExpression ("inner", new NamedExpression ("innermost", Expression.Constant ("test"))));

      var result = NamedExpressionCombiner.ProcessNames (_context, expression);

      Assert.That (result, Is.TypeOf (typeof (NamedExpression)));
      Assert.That (((NamedExpression) result).Name, Is.EqualTo ("outer_inner_innermost"));
    }

    [Test]
    public void ProcessNames_ReturnsNamedExpression_InnerNameIsNull ()
    {
      var expression = new NamedExpression ("outer", new NamedExpression (null, Expression.Constant ("test")));

      var result = NamedExpressionCombiner.ProcessNames (_context, expression);

      Assert.That (result, Is.TypeOf (typeof (NamedExpression)));
      Assert.That (((NamedExpression) result).Name, Is.EqualTo ("outer"));
    }

    [Test]
    public void ProcessNames_ReturnsNamedExpression_OuterNameIsNull ()
    {
      var expression = new NamedExpression (null, new NamedExpression ("inner", Expression.Constant ("test")));

      var result = NamedExpressionCombiner.ProcessNames (_context, expression);

      Assert.That (result, Is.TypeOf (typeof (NamedExpression)));
      Assert.That (((NamedExpression) result).Name, Is.EqualTo ("inner"));
    }

    [Test]
    public void ProcessNames_SqlEntityExpression ()
    {
      var entityExpression = SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression (typeof (Cook), "test2");
      var namedExpression = new NamedExpression ("test", entityExpression);

      var tableRegisteredForEntity = SqlStatementModelObjectMother.CreateSqlTable ();
      _context.AddSqlEntityMapping (entityExpression, tableRegisteredForEntity);

      var result = NamedExpressionCombiner.ProcessNames (_context, namedExpression);

      Assert.That (result, Is.Not.SameAs (namedExpression));
      Assert.That (result, Is.TypeOf (typeof (SqlEntityDefinitionExpression)));
      Assert.That (((SqlEntityDefinitionExpression) result).Name, Is.EqualTo ("test_test2"));
      Assert.That (_context.GetSqlTableForEntityExpression ((SqlEntityExpression) result), Is.SameAs (tableRegisteredForEntity));
    }

    [Test]
    public void ProcessNames_NewExpression ()
    {
      var expression = Expression.New (
          typeof (TypeForNewExpression).GetConstructor (new[] { typeof (int) }),
          new[] { Expression.Constant (0) },
          (MemberInfo) typeof (TypeForNewExpression).GetProperty ("A"));
      var namedExpression = new NamedExpression ("test", expression);

      var result = NamedExpressionCombiner.ProcessNames (_context, namedExpression);

      Assert.That (result, Is.TypeOf (typeof (NewExpression)));
      Assert.That (((NewExpression) result).Arguments.Count, Is.EqualTo (1));
      Assert.That (((NewExpression) result).Arguments[0], Is.TypeOf (typeof (NamedExpression)));
      Assert.That (((NewExpression) result).Members[0].Name, Is.EqualTo ("A"));
      Assert.That (((NewExpression) result).Members.Count, Is.EqualTo (1));
    }

    [Test]
    public void ProcessNames_NewExpression_NamedExpressionsInsideConstructorArgumentsCombined ()
    {
      var expression = Expression.New (
          typeof (TypeForNewExpression).GetConstructor (new[] { typeof (int) }),
          new[] { new NamedExpression ("inner", Expression.Constant (0)) },
          (MemberInfo) typeof (TypeForNewExpression).GetProperty ("A"));
      var namedExpression = new NamedExpression ("outer", expression);

      var result = NamedExpressionCombiner.ProcessNames (_context, namedExpression);

      Assert.That (result, Is.TypeOf (typeof (NewExpression)));
      Assert.That (((NewExpression) result).Arguments[0], Is.TypeOf (typeof (NamedExpression)));
      Assert.That (((NamedExpression) ((NewExpression) result).Arguments[0]).Name, Is.EqualTo ("outer_inner"));
    }

    [Test]
    public void ProcessNames_NewExpression_NoMembers ()
    {
      var expression = Expression.New (typeof (TypeForNewExpression).GetConstructor (new[] { typeof (int) }), new[] { Expression.Constant (0) });
      var namedExpression = new NamedExpression ("test", expression);

      var result = NamedExpressionCombiner.ProcessNames (_context, namedExpression);

      Assert.That (result, Is.TypeOf (typeof (NewExpression)));
      Assert.That (((NewExpression) result).Arguments.Count, Is.EqualTo (1));
      Assert.That (((NewExpression) result).Arguments[0], Is.TypeOf (typeof (NamedExpression)));
      Assert.That (((NewExpression) result).Members, Is.Null);
    }

    [Test]
    public void ProcessNames_SqlGroupingSelectExpression ()
    {
      var keyExpression = new NamedExpression ("key", Expression.Constant ("key"));
      var elementExpression = new NamedExpression ("element", Expression.Constant ("element"));
      var aggregationExpression = new NamedExpression ("a0", Expression.Constant ("aggregation"));
      var groupingSelectExpression = new SqlGroupingSelectExpression (keyExpression, elementExpression, new[]{aggregationExpression});
      var expression = new NamedExpression ("outer", groupingSelectExpression);
      var sqlTable = SqlStatementModelObjectMother.CreateSqlTable (typeof (Cook));
      _context.AddGroupReferenceMapping (groupingSelectExpression, sqlTable);

      var expectedResult = new SqlGroupingSelectExpression (
          new NamedExpression ("outer_key", Expression.Constant ("key")), 
          new NamedExpression ("outer_element", Expression.Constant ("element")),
          new[]{new NamedExpression("outer_a0", Expression.Constant("aggregation"))});

      var result = NamedExpressionCombiner.ProcessNames (_context, expression);

      ExpressionTreeComparer.CheckAreEqualTrees (result, expectedResult);
      Assert.That (_context.GetReferencedGroupSource (((SqlGroupingSelectExpression) result)), Is.SameAs (sqlTable));
    }

    [Test]
    public void ProcessNames_ConvertedBooleanExpression ()
    {
      var namedExpression = new NamedExpression ("outer", new ConvertedBooleanExpression (new NamedExpression ("inner", Expression.Constant (1))));

      var result = NamedExpressionCombiner.ProcessNames (_context, namedExpression);

      var expectedResult = new ConvertedBooleanExpression (new NamedExpression ("outer_inner", Expression.Constant (1)));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void ProcessNames_Convert ()
    {
      var namedExpression = new NamedExpression ("outer", Expression.Convert (new NamedExpression ("inner", Expression.Constant (1)), typeof (double)));

      var result = NamedExpressionCombiner.ProcessNames (_context, namedExpression);

      var expectedResult = Expression.Convert (new NamedExpression ("outer_inner", Expression.Constant (1)), typeof (double));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void ProcessNames_Convert_WithMethod ()
    {
      var convertMethod = typeof (Convert).GetMethod ("ToDouble", new[] { typeof (int) });
      var namedExpression = new NamedExpression (
          "outer", 
          Expression.Convert (
              new NamedExpression (
                  "inner", 
                  Expression.Constant (1)), 
              typeof (double), 
              convertMethod));

      var result = NamedExpressionCombiner.ProcessNames (_context, namedExpression);

      var expectedResult = Expression.Convert (new NamedExpression ("outer_inner", Expression.Constant (1)), typeof (double), convertMethod);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void ProcessNames_ConvertChecked ()
    {
      var namedExpression = new NamedExpression (
          "outer", 
          Expression.ConvertChecked (new NamedExpression ("inner", Expression.Constant (1)), 
          typeof (double)));

      var result = NamedExpressionCombiner.ProcessNames (_context, namedExpression);

      var expectedResult = Expression.ConvertChecked (new NamedExpression ("outer_inner", Expression.Constant (1)), typeof (double));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }
  }
}