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
using System.Reflection;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.SqlBackend.MappingResolution;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Data.Linq.UnitTests.Linq.Core.Parsing.ExpressionTreeVisitorTests;
using Remotion.Data.Linq.UnitTests.Linq.Core.TestDomain;
using Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlStatementModel;

namespace Remotion.Data.Linq.UnitTests.Linq.SqlBackend.MappingResolution
{
  [TestFixture]
  public class NameCombiningExpressionVisitorTest
  {
    private IMappingResolutionContext _context;

    [SetUp]
    public void SetUp ()
    {
      _context = new MappingResolutionContext();
    }

    [Test]
    public void VisitNamedExpression_NoSqlEntityExpression_SameExpression ()
    {
      var expression = new NamedExpression ("test", Expression.Constant ("test"));

      var result = NamedExpressionCombiner.ProcessNames (_context, expression);

      Assert.That (result, Is.SameAs (expression));
    }

    [Test]
    public void VisitNamedExpression_NamedExpression_ReturnsCombinedExpression ()
    {
      var expression = new NamedExpression ("outer", new NamedExpression ("inner", new NamedExpression ("innermost", Expression.Constant ("test"))));

      var result = NamedExpressionCombiner.ProcessNames (_context, expression);

      Assert.That (result, Is.TypeOf (typeof (NamedExpression)));
      Assert.That (((NamedExpression) result).Name, Is.EqualTo ("outer_inner_innermost"));
    }

    [Test]
    public void VisitNamedExpression_ReturnsNamedExpression_InnerNameIsNull ()
    {
      var expression = new NamedExpression ("outer", new NamedExpression (null, Expression.Constant ("test")));

      var result = NamedExpressionCombiner.ProcessNames (_context, expression);

      Assert.That (result, Is.TypeOf (typeof (NamedExpression)));
      Assert.That (((NamedExpression) result).Name, Is.EqualTo ("outer"));
    }

    [Test]
    public void VisitNamedExpression_ReturnsNamedExpression_OuterNameIsNull ()
    {
      var expression = new NamedExpression (null, new NamedExpression ("inner", Expression.Constant ("test")));

      var result = NamedExpressionCombiner.ProcessNames (_context, expression);

      Assert.That (result, Is.TypeOf (typeof (NamedExpression)));
      Assert.That (((NamedExpression) result).Name, Is.EqualTo ("inner"));
    }

    [Test]
    public void VisitNamedExpression_SqlEntityExpression ()
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
    public void VisitNamedExpression_NewExpression ()
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
    public void VisitNamedExpression_NewExpression_NamedExpressionsInsideConstructorArgumentsCombined ()
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
    public void VisitNamedExpression_NewExpression_NoMembers ()
    {
      var expression = Expression.New (typeof (TypeForNewExpression).GetConstructor (new[] { typeof (int) }), new[] { Expression.Constant (0) });
      var namedExpression = new NamedExpression ("test", expression);

      var result = NamedExpressionCombiner.ProcessNames (_context, namedExpression);

      Assert.That (result, Is.TypeOf (typeof (NewExpression)));
      Assert.That (((NewExpression) result).Arguments.Count, Is.EqualTo (1));
      Assert.That (((NewExpression) result).Arguments[0], Is.TypeOf (typeof (NamedExpression)));
      Assert.That (((NewExpression) result).Members, Is.Null);
    }
  }
}