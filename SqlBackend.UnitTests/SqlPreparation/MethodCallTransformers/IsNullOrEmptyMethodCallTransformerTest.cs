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
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Linq.SqlBackend.Development.UnitTesting;
using Remotion.Linq.SqlBackend.SqlPreparation.MethodCallTransformers;
using Remotion.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;

namespace Remotion.Linq.SqlBackend.UnitTests.SqlPreparation.MethodCallTransformers
{
  [TestFixture]
  public class IsNullOrEmptyMethodCallTransformerTest
  {
    [Test]
    public void SupportedMethods ()
    {
      Assert.That (IsNullOrEmptyMethodCallTransformer.SupportedMethods.Contains (typeof (string).GetMethod( "IsNullOrEmpty", new[]{typeof(string)})));
    }

    [Test]
    public void Transform ()
    {
      var method = typeof (string).GetMethod ("IsNullOrEmpty", new[] { typeof(string) });
      var objectExpression = Expression.Constant ("Test");
      var expression = Expression.Call (method, objectExpression);
      var transformer = new IsNullOrEmptyMethodCallTransformer ();

      var result = transformer.Transform (expression);

      var expectedIsNullExpression = new SqlIsNullExpression (objectExpression);
      var expectedLenExpression = new SqlLengthExpression (objectExpression);
      var expectedResult = Expression.OrElse (expectedIsNullExpression, Expression.Equal (expectedLenExpression, new SqlLiteralExpression(0)));
      
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }
  }
}