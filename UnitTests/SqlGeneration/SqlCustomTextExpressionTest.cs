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
using NUnit.Framework;
using Remotion.Linq.SqlBackend.SqlGeneration;
using Moq;

namespace Remotion.Linq.SqlBackend.UnitTests.SqlGeneration
{
  [TestFixture]
  public class SqlCustomTextExpressionTest
  {
    private SqlCustomTextExpression _sqlCustomTextExpression;

    [SetUp]
    public void SetUp ()
    {
      _sqlCustomTextExpression = new SqlCustomTextExpression ("test", typeof (string));
    }


    [Test]
    public void Generate ()
    {
      var commandBuilder = new SqlCommandBuilder();
      var visitor = new Mock<ExpressionVisitor>();
      var stage = new Mock<ISqlGenerationStage>();

      _sqlCustomTextExpression.Generate (commandBuilder, visitor.Object, stage.Object);

      Assert.That (commandBuilder.GetCommandText(), Is.EqualTo ("test"));
    }

    [Test]
    public void VisitChildren_ReturnsThis ()
    {
      var visitorMock = new Mock<ExpressionVisitor>(MockBehavior.Strict);

      var result = ExtensionExpressionTestHelper.CallVisitChildren (_sqlCustomTextExpression, visitorMock.Object);

      visitorMock.Verify ();
      Assert.That (result, Is.SameAs (_sqlCustomTextExpression));
    }

    [Test]
    public void To_String ()
    {
      var result = _sqlCustomTextExpression.ToString();

      Assert.That (result, Is.EqualTo ("test"));
    }
  }
}