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
using NUnit.Framework;
using Remotion.Data.Linq.UnitTests.Linq.Core.Parsing.ExpressionTreeVisitorTests;
using Remotion.Data.Linq.SqlBackend.SqlGeneration;
using Remotion.Data.Linq.UnitTests.Linq.Core.TestDomain;
using System.Linq.Expressions;
using Rhino.Mocks;

namespace Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlGeneration
{
  [TestFixture]
  public class SqlCompositeCustomTextGeneratorExpressionTest
  {
    [Test]
    public void Generate ()
    {
      var expression1 = Expression.Constant ("5");
      var expression2 = Expression.Constant ("1");
      var sqlCompositeCustomTextGeneratorExpression = new SqlCompositeCustomTextGeneratorExpression (typeof (Cook), expression1, expression2);

      var visitor = MockRepository.GeneratePartialMock<TestableExpressionTreeVisitor>();
      var commandBuilder = new SqlCommandBuilder();

      visitor
          .Expect (mock => mock.VisitExpression (expression1))
          .Return (expression1);
      visitor
          .Expect (mock => mock.VisitExpression (expression2))
          .Return (expression2);
      visitor.Replay();

      sqlCompositeCustomTextGeneratorExpression.Generate (commandBuilder, visitor, MockRepository.GenerateStub<ISqlGenerationStage>());

      visitor.VerifyAllExpectations();
    }
  }
}