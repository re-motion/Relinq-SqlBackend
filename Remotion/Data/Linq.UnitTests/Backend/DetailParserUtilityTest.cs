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
using Remotion.Data.Linq.Backend;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.UnitTests.TestQueryGenerators;

namespace Remotion.Data.Linq.UnitTests.Backend
{
  [TestFixture]
  public class DetailParserUtilityTest
  {
    [Test]
    public void CheckNumberOfArguments_Succeed ()
    {
      MethodCallExpression selectExpression = SelectTestQueryGenerator.CreateSimpleQuery_SelectExpression (ExpressionHelper.CreateCookQueryable ());
      DetailParserUtility.CheckNumberOfArguments (selectExpression, "Select", 2);
    }

    [Test]
    [ExpectedException (typeof (ParserException), ExpectedMessage = "Expected at least 1 argument for Select method call, found '2 arguments'.")]
    public void CheckNumberOfArguments_Fail ()
    {
      MethodCallExpression selectExpression = SelectTestQueryGenerator.CreateSimpleQuery_SelectExpression (ExpressionHelper.CreateCookQueryable ());
      DetailParserUtility.CheckNumberOfArguments (selectExpression, "Select", 1);
    }

    [Test]
    public void CheckParameterType_Succeed ()
    {
      MethodCallExpression selectExpression = SelectTestQueryGenerator.CreateSimpleQuery_SelectExpression (ExpressionHelper.CreateCookQueryable ());
      DetailParserUtility.CheckParameterType<ConstantExpression> (selectExpression, "Select", 0);
    }

    [Test]
    [ExpectedException (typeof (ParserException), ExpectedMessage = "Expected ParameterExpression for argument 0 of Select method call, found "
        + "'ConstantExpression (TestQueryable<Cook>())'.")]
    public void CheckParameterType_Fail ()
    {
      MethodCallExpression selectExpression = SelectTestQueryGenerator.CreateSimpleQuery_SelectExpression (ExpressionHelper.CreateCookQueryable ());
      DetailParserUtility.CheckParameterType<ParameterExpression> (selectExpression, "Select", 0);
    }
  }
}
