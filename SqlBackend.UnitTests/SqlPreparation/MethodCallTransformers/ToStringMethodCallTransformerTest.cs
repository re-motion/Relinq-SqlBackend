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
  public class ToStringMethodCallTransformerTest
  {
    [Test]
    public void SupportedMethods ()
    {
      Assert.That (ToStringMethodCallTransformer.SupportedMethods.Contains (typeof (object).GetMethod("ToString")), Is.True);
    }

    [Test]
    public void Transform ()
    {
      var method = typeof (object).GetMethod ("ToString", new Type[] { });
      var argument = Expression.Constant (1);
      var expression = Expression.Call (argument, method);

      var transformer = new ToStringMethodCallTransformer();
      var result = transformer.Transform (expression);

      var expectedResult = new SqlConvertExpression (typeof (string), Expression.Constant (1));

      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }
  }
}