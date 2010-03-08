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
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.SqlBackend.SqlGeneration;
using Remotion.Data.Linq.SqlBackend.SqlGeneration.MethodCallGenerators;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Data.Linq.UnitTests.TestDomain;
using Rhino.Mocks;

namespace Remotion.Data.Linq.UnitTests.SqlBackend.SqlGeneration
{
  [TestFixture]
  public class SqlGeneratingExpressionVisitorTest
  {
    private SqlCommandBuilder _commandBuilder;
    private Expression _leftIntegerExpression;
    private Expression _leftStringExpression;
    private Expression _rightIntegerExpression;
    private Expression _rightStringExpression;
    private MethodCallSqlGeneratorRegistry _methodCallRegistry;

    [SetUp]
    public void SetUp ()
    {
      _commandBuilder = new SqlCommandBuilder();
      _leftIntegerExpression = Expression.Constant (1);
      _leftStringExpression = Expression.Constant ("Left");
      _rightIntegerExpression = Expression.Constant (2);
      _rightStringExpression = Expression.Constant ("Right");
      _methodCallRegistry = new MethodCallSqlGeneratorRegistry();
    }

    [Test]
    public void GenerateSql_VisitSqlColumnExpression ()
    {
      var sqlColumnExpression = new SqlColumnExpression (typeof (int), "s", "ID");
      SqlGeneratingExpressionVisitor.GenerateSql (sqlColumnExpression, _commandBuilder, _methodCallRegistry);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("[s].[ID]"));
    }

    [Test]
    public void GenerateSql_VisitSqlColumnListExpression ()
    {
      var sqlColumnListExpression = new SqlColumnListExpression (
          typeof (Cook),
          new[]
          {
              new SqlColumnExpression (typeof (string), "t", "ID"),
              new SqlColumnExpression (typeof (string), "t", "Name"),
              new SqlColumnExpression (typeof (string), "t", "City")
          });
      SqlGeneratingExpressionVisitor.GenerateSql (sqlColumnListExpression, _commandBuilder, _methodCallRegistry);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("[t].[ID],[t].[Name],[t].[City]"));
    }

    [Test]
    public void VisitConstantExpression_TrueParameter ()
    {
      var expression = Expression.Constant (true);
      SqlGeneratingExpressionVisitor.GenerateSql (expression, _commandBuilder, _methodCallRegistry);

      Assert.That (_commandBuilder.GetCommandParameters().Length, Is.EqualTo (1));
      Assert.That (_commandBuilder.GetCommandParameters()[0].Value, Is.EqualTo (1));
    }

    [Test]
    public void VisitConstantExpression_FalseParameter ()
    {
      var expression = Expression.Constant (false);
      SqlGeneratingExpressionVisitor.GenerateSql (expression, _commandBuilder, _methodCallRegistry);

      Assert.That (_commandBuilder.GetCommandParameters().Length, Is.EqualTo (1));
      Assert.That (_commandBuilder.GetCommandParameters()[0].Value, Is.EqualTo (0));
    }

    [Test]
    public void VisitConstantExpression_NullValue ()
    {
      var expression = Expression.Constant (null);
      SqlGeneratingExpressionVisitor.GenerateSql (expression, _commandBuilder, _methodCallRegistry);

      Assert.That (_commandBuilder.GetCommandParameters().Length, Is.EqualTo (0));
      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo ("NULL"));
    }

    [Test]
    public void VisitConstantExpression_StringParameter ()
    {
      var expression = Expression.Constant ("Test");
      SqlGeneratingExpressionVisitor.GenerateSql (expression, _commandBuilder, _methodCallRegistry);

      Assert.That (_commandBuilder.GetCommandParameters().Length, Is.EqualTo (1));
      Assert.That (_commandBuilder.GetCommandParameters()[0].Value, Is.EqualTo ("Test"));
    }

    [Test]
    public void VisitBinaryExpression_Add ()
    {
      Expression binaryExpression = Expression.Add (_leftIntegerExpression, _rightIntegerExpression);
      SqlGeneratingExpressionVisitor.GenerateSql (binaryExpression, _commandBuilder, _methodCallRegistry);

      var result = _commandBuilder.GetCommandText();

      Assert.That (result, Is.EqualTo (("(@1 + @2)")));
    }

    [Test]
    public void VisitBinaryExpression_Subtract ()
    {
      Expression binaryExpression = Expression.Subtract (_leftIntegerExpression, _rightIntegerExpression);
      SqlGeneratingExpressionVisitor.GenerateSql (binaryExpression, _commandBuilder, _methodCallRegistry);

      var result = _commandBuilder.GetCommandText();

      Assert.That (result, Is.EqualTo (("(@1 - @2)")));
    }

    [Test]
    public void VisitBinaryExpression_Multiply ()
    {
      Expression binaryExpression = Expression.Multiply (_leftIntegerExpression, _rightIntegerExpression);
      SqlGeneratingExpressionVisitor.GenerateSql (binaryExpression, _commandBuilder, _methodCallRegistry);

      var result = _commandBuilder.GetCommandText();

      Assert.That (result, Is.EqualTo (("(@1 * @2)")));
    }

    [Test]
    public void VisitBinaryExpression_Divide ()
    {
      Expression binaryExpression = Expression.Divide (_leftIntegerExpression, _rightIntegerExpression);
      SqlGeneratingExpressionVisitor.GenerateSql (binaryExpression, _commandBuilder, _methodCallRegistry);

      var result = _commandBuilder.GetCommandText();

      Assert.That (result, Is.EqualTo (("(@1 / @2)")));
    }

    [Test]
    public void VisitBinaryExpression_Modulo ()
    {
      Expression binaryExpression = Expression.Modulo (_leftIntegerExpression, _rightIntegerExpression);
      SqlGeneratingExpressionVisitor.GenerateSql (binaryExpression, _commandBuilder, _methodCallRegistry);

      var result = _commandBuilder.GetCommandText();

      Assert.That (result, Is.EqualTo (("(@1 % @2)")));
    }

    [Test]
    public void VisitBinaryExpression_AddChecked ()
    {
      Expression binaryExpression = Expression.AddChecked (_leftIntegerExpression, _rightIntegerExpression);
      SqlGeneratingExpressionVisitor.GenerateSql (binaryExpression, _commandBuilder, _methodCallRegistry);

      var result = _commandBuilder.GetCommandText();

      Assert.That (result, Is.EqualTo (("(@1 + @2)")));
    }

    [Test]
    public void VisitBinaryExpression_MultiplyChecked ()
    {
      Expression binaryExpression = Expression.MultiplyChecked (_leftIntegerExpression, _rightIntegerExpression);
      SqlGeneratingExpressionVisitor.GenerateSql (binaryExpression, _commandBuilder, _methodCallRegistry);

      var result = _commandBuilder.GetCommandText();

      Assert.That (result, Is.EqualTo (("(@1 * @2)")));
    }

    [Test]
    public void VisitBinaryExpression_SubtractChecked ()
    {
      Expression binaryExpression = Expression.SubtractChecked (_leftIntegerExpression, _rightIntegerExpression);
      SqlGeneratingExpressionVisitor.GenerateSql (binaryExpression, _commandBuilder, _methodCallRegistry);

      var result = _commandBuilder.GetCommandText();

      Assert.That (result, Is.EqualTo (("(@1 - @2)")));
    }

    //example: (true AND true)
    [Test]
    [Ignore ("Review special bool cases")]
    public void VisitBinaryExpression_AndAlso_Boolean ()
    {
      Expression expression = Expression.Constant (true);

      Expression binaryExpression = Expression.AndAlso (expression, expression);
      SqlGeneratingExpressionVisitor.GenerateSql (binaryExpression, _commandBuilder, _methodCallRegistry);

      var result = _commandBuilder.GetCommandText();

      Assert.That (result, Is.EqualTo (("@1=@2")));
    }

    //example: (true AND (1<2))
    [Test]
    [Ignore("Review special bool cases")]
    public void VisitBinaryExpression ()
    {
      Expression expression = Expression.Constant (true);
      Expression innerBinaryExpression = Expression.LessThan (_leftIntegerExpression, _rightIntegerExpression);

      Expression binaryExpression = Expression.AndAlso (expression, innerBinaryExpression);

      SqlGeneratingExpressionVisitor.GenerateSql (binaryExpression, _commandBuilder, _methodCallRegistry);

      var result = _commandBuilder.GetCommandText ();

      Assert.That (result, Is.EqualTo (("@1=1  AND (@2 < @3)")));
    }
    
    [Test]
    public void VisitBinaryExpression_OrElse ()
    {
      Expression expression = Expression.Constant (true);

      Expression binaryExpression = Expression.OrElse (expression, expression);
      SqlGeneratingExpressionVisitor.GenerateSql (binaryExpression, _commandBuilder, _methodCallRegistry);

      var result = _commandBuilder.GetCommandText();

      Assert.That (result, Is.EqualTo (("(@1 OR @2)")));
    }

    [Test]
    public void VisitBinaryExpression_And ()
    {
      Expression binaryExpression = Expression.And (_leftIntegerExpression, _rightIntegerExpression);
      SqlGeneratingExpressionVisitor.GenerateSql (binaryExpression, _commandBuilder, _methodCallRegistry);

      var result = _commandBuilder.GetCommandText();

      Assert.That (result, Is.EqualTo (("(@1 & @2)")));
    }

    [Test]
    public void VisitBinaryExpression_Or ()
    {
      Expression binaryExpression = Expression.Or (_leftIntegerExpression, _rightIntegerExpression);
      SqlGeneratingExpressionVisitor.GenerateSql (binaryExpression, _commandBuilder, _methodCallRegistry);

      var result = _commandBuilder.GetCommandText();

      Assert.That (result, Is.EqualTo (("(@1 | @2)")));
    }

    [Test]
    public void VisitBinaryExpression_ExclusiveOr ()
    {
      Expression binaryExpression = Expression.ExclusiveOr (_leftIntegerExpression, _rightIntegerExpression);
      SqlGeneratingExpressionVisitor.GenerateSql (binaryExpression, _commandBuilder, _methodCallRegistry);

      var result = _commandBuilder.GetCommandText();

      Assert.That (result, Is.EqualTo (("(@1 ^ @2)")));
    }

    [Test]
    public void VisitBinaryExpression_Equals ()
    {
      Expression binaryExpression = Expression.Equal (_leftIntegerExpression, _rightIntegerExpression);
      SqlGeneratingExpressionVisitor.GenerateSql (binaryExpression, _commandBuilder, _methodCallRegistry);

      var result = _commandBuilder.GetCommandText();

      Assert.That (result, Is.EqualTo (("(@1 = @2)")));
    }

    [Test]
    public void VisitBinaryExpression_GreaterThan ()
    {
      Expression binaryExpression = Expression.GreaterThan (_leftIntegerExpression, _rightIntegerExpression);
      SqlGeneratingExpressionVisitor.GenerateSql (binaryExpression, _commandBuilder, _methodCallRegistry);

      var result = _commandBuilder.GetCommandText();

      Assert.That (result, Is.EqualTo (("(@1 > @2)")));
    }

    [Test]
    public void VisitBinaryExpression_GreaterThanOrEqual ()
    {
      Expression binaryExpression = Expression.GreaterThanOrEqual (_leftIntegerExpression, _rightIntegerExpression);
      SqlGeneratingExpressionVisitor.GenerateSql (binaryExpression, _commandBuilder, _methodCallRegistry);

      var result = _commandBuilder.GetCommandText();

      Assert.That (result, Is.EqualTo (("(@1 >= @2)")));
    }

    [Test]
    public void VisitBinaryExpression_LessThan ()
    {
      Expression binaryExpression = Expression.LessThan (_leftIntegerExpression, _rightIntegerExpression);
      SqlGeneratingExpressionVisitor.GenerateSql (binaryExpression, _commandBuilder, _methodCallRegistry);

      var result = _commandBuilder.GetCommandText();

      Assert.That (result, Is.EqualTo (("(@1 < @2)")));
    }

    [Test]
    public void VisitBinaryExpression_LessThanOrEqual ()
    {
      Expression binaryExpression = Expression.LessThanOrEqual (_leftIntegerExpression, _rightIntegerExpression);
      SqlGeneratingExpressionVisitor.GenerateSql (binaryExpression, _commandBuilder, _methodCallRegistry);

      var result = _commandBuilder.GetCommandText();

      Assert.That (result, Is.EqualTo (("(@1 <= @2)")));
    }

    [Test]
    public void VisitBinaryExpression_NotEqual ()
    {
      Expression binaryExpression = Expression.NotEqual (_leftIntegerExpression, _rightIntegerExpression);
      SqlGeneratingExpressionVisitor.GenerateSql (binaryExpression, _commandBuilder, _methodCallRegistry);

      var result = _commandBuilder.GetCommandText();

      Assert.That (result, Is.EqualTo (("(@1 <> @2)")));
    }

    [Test]
    public void VisitBinaryExpression_Coalesce ()
    {
      Expression binaryExpression = Expression.Coalesce (_leftStringExpression, _rightStringExpression);
      SqlGeneratingExpressionVisitor.GenerateSql (binaryExpression, _commandBuilder, _methodCallRegistry);

      var result = _commandBuilder.GetCommandText();

      Assert.That (result, Is.EqualTo (("(COALESCE (@1, @2)")));
    }

    [Test]
    public void VisitBinaryExpression_EqualWithNullOnRightSide ()
    {
      Expression nullExpression = Expression.Constant (null);
      Expression binaryExpression = Expression.Equal (_leftStringExpression, nullExpression);

      SqlGeneratingExpressionVisitor.GenerateSql (binaryExpression, _commandBuilder, _methodCallRegistry);

      var result = _commandBuilder.GetCommandText();

      Assert.That (result, Is.EqualTo (("(@1 IS NULL)")));
    }

    [Test]
    public void VisitBinaryExpression_NotEqualWithNullOnRightSide ()
    {
      Expression nullExpression = Expression.Constant (null);
      Expression binaryExpression = Expression.NotEqual (_leftStringExpression, nullExpression);

      SqlGeneratingExpressionVisitor.GenerateSql (binaryExpression, _commandBuilder, _methodCallRegistry);

      var result = _commandBuilder.GetCommandText();

      Assert.That (result, Is.EqualTo (("(@1 IS NOT NULL)")));
    }

    [Test]
    public void VisitBinaryExpression_EqualWithNullOnLeftSide ()
    {
      Expression nullExpression = Expression.Constant (null);
      Expression binaryExpression = Expression.Equal (nullExpression, _rightStringExpression);

      SqlGeneratingExpressionVisitor.GenerateSql (binaryExpression, _commandBuilder, _methodCallRegistry);

      var result = _commandBuilder.GetCommandText();

      Assert.That (result, Is.EqualTo (("(@1 IS NULL)")));
    }

    [Test]
    public void VisitBinaryExpression_NotEqualWithNullOnLeftSide ()
    {
      Expression nullExpression = Expression.Constant (null);
      Expression binaryExpression = Expression.NotEqual (nullExpression, _rightStringExpression);

      SqlGeneratingExpressionVisitor.GenerateSql (binaryExpression, _commandBuilder, _methodCallRegistry);

      var result = _commandBuilder.GetCommandText();

      Assert.That (result, Is.EqualTo (("(@1 IS NOT NULL)")));
    }

    [Test]
    public void VisitBinaryExpression_AddWithNull ()
    {
      Expression nullExpression = Expression.Constant (null, typeof (string));
      Expression binaryExpression = Expression.Coalesce (nullExpression, _rightStringExpression);

      SqlGeneratingExpressionVisitor.GenerateSql (binaryExpression, _commandBuilder, _methodCallRegistry);

      var result = _commandBuilder.GetCommandText();
      Assert.That (result, Is.EqualTo (("(COALESCE (NULL, @1)")));
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException))]
    public void NotSupportedNodeType ()
    {
      BinaryExpression binaryExpression = Expression.Power (Expression.Constant (2D), Expression.Constant (3D));
      SqlGeneratingExpressionVisitor.GenerateSql (binaryExpression, _commandBuilder, _methodCallRegistry);
    }

    [Test]
    public void VisitUnaryExpression_UnaryNot ()
    {
      var unaryNotExpression = Expression.Not (Expression.Constant (1));
      SqlGeneratingExpressionVisitor.GenerateSql (unaryNotExpression, _commandBuilder, _methodCallRegistry);
      var result = _commandBuilder.GetCommandText();

      Assert.That (result, Is.EqualTo ("NOT @1"));
    }

    [Test]
    public void VisitUnaryExpression_UnaryNegate ()
    {
      var unaryNotExpression = Expression.Negate (Expression.Constant (1));

      SqlGeneratingExpressionVisitor.GenerateSql (unaryNotExpression, _commandBuilder, _methodCallRegistry);
      var result = _commandBuilder.GetCommandText();

      Assert.That (result, Is.EqualTo ("-@1"));
    }

    [Test]
    public void VisitUnaryExpression_UnaryPlus ()
    {
      var unaryNotExpression = Expression.UnaryPlus (Expression.Constant (1));

      SqlGeneratingExpressionVisitor.GenerateSql (unaryNotExpression, _commandBuilder, _methodCallRegistry);
      var result = _commandBuilder.GetCommandText();

      Assert.That (result, Is.EqualTo ("+@1"));
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException))]
    public void VisitUnaryExpression_NotSupported ()
    {
      var unaryExpression = Expression.TypeAs (Expression.Constant ("1"), typeof (string));
      SqlGeneratingExpressionVisitor.GenerateSql (unaryExpression, _commandBuilder, _methodCallRegistry);
    }

    [Test]
    public void VisitMethodCallExpression_CallsGenerateSql ()
    {
      var method = typeof (string).GetMethod ("ToUpper", new Type[] { });
      var methodCallExpression = Expression.Call (Expression.Constant ("Test"), method);

      var sqlGeneratorMock = MockRepository.GenerateMock<IMethodCallSqlGenerator>();
      sqlGeneratorMock.Expect (
          mock => mock.GenerateSql (Arg<MethodCallExpression>.Is.Anything, Arg<SqlCommandBuilder>.Is.Anything, Arg<ExpressionTreeVisitor>.Is.Anything));
      _methodCallRegistry.Register (method, sqlGeneratorMock);

      sqlGeneratorMock.Replay();
      SqlGeneratingExpressionVisitor.GenerateSql (methodCallExpression, _commandBuilder, _methodCallRegistry);
      sqlGeneratorMock.VerifyAllExpectations();
    }

    [Test]
    public void VistMethodCallExpression_ToUpper ()
    {
      var method = typeof (string).GetMethod ("ToUpper", new Type[] { });
      var methodCallExpression = Expression.Call (Expression.Constant ("Test"), method);

      var registry = new MethodCallSqlGeneratorRegistry();
      registry.Register (method, new MethodCallUpper());

      SqlGeneratingExpressionVisitor.GenerateSql (methodCallExpression, _commandBuilder, registry);

      Assert.That (_commandBuilder.GetCommandText (), Is.EqualTo ("UPPER(@1)"));
    }

    [Test]
    public void VistMethodCallExpression_ToLower ()
    {
      var method = typeof (string).GetMethod ("ToLower", new Type[] { });
      var methodCallExpression = Expression.Call (Expression.Constant ("Test"), method);

      var registry = new MethodCallSqlGeneratorRegistry ();
      registry.Register (method, new MethodCallLower ());

      SqlGeneratingExpressionVisitor.GenerateSql (methodCallExpression, _commandBuilder, registry);

      Assert.That (_commandBuilder.GetCommandText (), Is.EqualTo ("LOWER(@1)"));
    }

    [Test]
    public void VistMethodCallExpression_Remove ()
    {
      var method = typeof (string).GetMethod ("Remove", new[] {typeof(int), typeof(int) });
      var methodCallExpression = Expression.Call (Expression.Constant ("Test"), method, Expression.Constant(0), Expression.Constant(1));

      var registry = new MethodCallSqlGeneratorRegistry ();
      registry.Register (method, new MethodCallRemove ());

      SqlGeneratingExpressionVisitor.GenerateSql (methodCallExpression, _commandBuilder, registry);

      Assert.That (_commandBuilder.GetCommandText (), Is.EqualTo ("STUFF(@1,@2,@3,LEN(@4), \"\")"));
    }

    [ExpectedException (typeof (NotSupportedException), ExpectedMessage =
        "The expression '[2147483647]' cannot be translated to SQL text by this SQL generator. Expression type 'NotSupportedExpression' is not supported."
        )]
    [Test]
    public void GenerateSql_UnsupportedExpression ()
    {
      var unknownExpression = new NotSupportedExpression (typeof (int));
      SqlGeneratingExpressionVisitor.GenerateSql (unknownExpression, _commandBuilder, _methodCallRegistry);
    }
  }
}