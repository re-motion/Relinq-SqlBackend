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
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.SqlBackend.SqlPreparation.MethodCallTransformers;
using Remotion.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;

namespace Remotion.Linq.SqlBackend.UnitTests.SqlPreparation.MethodCallTransformers
{
  [TestFixture]
  public class ConvertMethodCallTransformerTest
  {
    [Test]
    public void SupportedMethods ()
    {
      Assert.That (ConvertMethodCallTransformer.SupportedMethods.Contains (typeof (Convert).GetMethod("ToString", new[] { typeof (int)})), Is.True);
      Assert.That (ConvertMethodCallTransformer.SupportedMethods.Contains (typeof (Convert).GetMethod("ToString", new[] { typeof (bool)})), Is.True);
      Assert.That (ConvertMethodCallTransformer.SupportedMethods.Contains (typeof (Convert).GetMethod("ToString", new[] { typeof (object)})), Is.True);
      Assert.That (ConvertMethodCallTransformer.SupportedMethods.Contains (typeof (Convert).GetMethod("ToString", new[] { typeof (decimal)})), Is.True);
      Assert.That (ConvertMethodCallTransformer.SupportedMethods.Contains (typeof (Convert).GetMethod("ToString", new[] { typeof (double)})), Is.True);
      Assert.That (ConvertMethodCallTransformer.SupportedMethods.Contains (typeof (Convert).GetMethod("ToString", new[] { typeof (float)})), Is.True);
      Assert.That (ConvertMethodCallTransformer.SupportedMethods.Contains (typeof (Convert).GetMethod("ToString", new[] { typeof (long)})), Is.True);
      Assert.That (ConvertMethodCallTransformer.SupportedMethods.Contains (typeof (Convert).GetMethod("ToString", new[] { typeof (short)})), Is.True);
      Assert.That (ConvertMethodCallTransformer.SupportedMethods.Contains (typeof (Convert).GetMethod("ToString", new[] { typeof (char)})), Is.True);
      Assert.That (ConvertMethodCallTransformer.SupportedMethods.Contains (typeof (Convert).GetMethod("ToString", new[] { typeof (byte)})), Is.True);
      Assert.That (ConvertMethodCallTransformer.SupportedMethods.Contains (typeof (Convert).GetMethod("ToString", new[] { typeof (string)})), Is.True);
      Assert.That (ConvertMethodCallTransformer.SupportedMethods.Contains (typeof (Convert).GetMethod("ToInt64", new[] { typeof (string)})), Is.True);
      Assert.That (ConvertMethodCallTransformer.SupportedMethods.Contains (typeof (Convert).GetMethod("ToInt64", new[] { typeof (bool)})), Is.True);
      Assert.That (ConvertMethodCallTransformer.SupportedMethods.Contains (typeof (Convert).GetMethod("ToInt64", new[] { typeof (byte)})), Is.True);
      Assert.That (ConvertMethodCallTransformer.SupportedMethods.Contains (typeof (Convert).GetMethod("ToInt64", new[] { typeof (char)})), Is.True);
      Assert.That (ConvertMethodCallTransformer.SupportedMethods.Contains (typeof (Convert).GetMethod("ToInt64", new[] { typeof (DateTime)})), Is.True);
      Assert.That (ConvertMethodCallTransformer.SupportedMethods.Contains (typeof (Convert).GetMethod("ToInt64", new[] { typeof (decimal)})), Is.True);
      Assert.That (ConvertMethodCallTransformer.SupportedMethods.Contains (typeof (Convert).GetMethod("ToInt64", new[] { typeof (float)})), Is.True);
      Assert.That (ConvertMethodCallTransformer.SupportedMethods.Contains (typeof (Convert).GetMethod("ToInt64", new[] { typeof (long)})), Is.True);
      Assert.That (ConvertMethodCallTransformer.SupportedMethods.Contains (typeof (Convert).GetMethod("ToInt64", new[] { typeof (object)})), Is.True);
      Assert.That (ConvertMethodCallTransformer.SupportedMethods.Contains (typeof (Convert).GetMethod("ToInt64", new[] { typeof (short)})), Is.True);
      Assert.That (ConvertMethodCallTransformer.SupportedMethods.Contains (typeof (Convert).GetMethod("ToInt32", new[] { typeof (string)})), Is.True);
      Assert.That (ConvertMethodCallTransformer.SupportedMethods.Contains (typeof (Convert).GetMethod("ToInt32", new[] { typeof (bool)})), Is.True);
      Assert.That (ConvertMethodCallTransformer.SupportedMethods.Contains (typeof (Convert).GetMethod("ToInt32", new[] { typeof (byte)})), Is.True);
      Assert.That (ConvertMethodCallTransformer.SupportedMethods.Contains (typeof (Convert).GetMethod("ToInt32", new[] { typeof (char)})), Is.True);
      Assert.That (ConvertMethodCallTransformer.SupportedMethods.Contains (typeof (Convert).GetMethod("ToInt32", new[] { typeof (DateTime)})), Is.True);
      Assert.That (ConvertMethodCallTransformer.SupportedMethods.Contains (typeof (Convert).GetMethod("ToInt32", new[] { typeof (decimal)})), Is.True);
      Assert.That (ConvertMethodCallTransformer.SupportedMethods.Contains (typeof (Convert).GetMethod("ToInt32", new[] { typeof (float) })), Is.True);
      Assert.That (ConvertMethodCallTransformer.SupportedMethods.Contains (typeof (Convert).GetMethod("ToInt32", new[] { typeof (long)})), Is.True);
      Assert.That (ConvertMethodCallTransformer.SupportedMethods.Contains (typeof (Convert).GetMethod("ToInt32", new[] { typeof (object)})), Is.True);
      Assert.That (ConvertMethodCallTransformer.SupportedMethods.Contains (typeof (Convert).GetMethod("ToInt32", new[] { typeof (short)})), Is.True);
      Assert.That (ConvertMethodCallTransformer.SupportedMethods.Contains (typeof (Convert).GetMethod("ToInt16", new[] { typeof (string)})), Is.True);
      Assert.That (ConvertMethodCallTransformer.SupportedMethods.Contains (typeof (Convert).GetMethod("ToInt16", new[] { typeof (bool)})), Is.True);
      Assert.That (ConvertMethodCallTransformer.SupportedMethods.Contains (typeof (Convert).GetMethod("ToInt16", new[] { typeof (byte) })), Is.True);
      Assert.That (ConvertMethodCallTransformer.SupportedMethods.Contains (typeof (Convert).GetMethod("ToInt16", new[] { typeof (char)})), Is.True);
      Assert.That (ConvertMethodCallTransformer.SupportedMethods.Contains (typeof (Convert).GetMethod("ToInt16", new[] { typeof (DateTime)})), Is.True);
      Assert.That (ConvertMethodCallTransformer.SupportedMethods.Contains (typeof (Convert).GetMethod("ToInt16", new[] { typeof (decimal)})), Is.True);
      Assert.That (ConvertMethodCallTransformer.SupportedMethods.Contains (typeof (Convert).GetMethod("ToInt16", new[] { typeof (float) })), Is.True);
      Assert.That (ConvertMethodCallTransformer.SupportedMethods.Contains (typeof (Convert).GetMethod("ToInt16", new[] { typeof (long) })), Is.True);
      Assert.That (ConvertMethodCallTransformer.SupportedMethods.Contains (typeof (Convert).GetMethod("ToInt16", new[] { typeof (object)})), Is.True);
      Assert.That (ConvertMethodCallTransformer.SupportedMethods.Contains (typeof (Convert).GetMethod("ToInt16", new[] { typeof (short)})), Is.True);
      Assert.That (ConvertMethodCallTransformer.SupportedMethods.Contains (typeof (Convert).GetMethod("ToBoolean", new[] { typeof (string) })), Is.True);
      Assert.That (ConvertMethodCallTransformer.SupportedMethods.Contains (typeof (Convert).GetMethod("ToBoolean", new[] { typeof (int)})), Is.True);
      Assert.That (ConvertMethodCallTransformer.SupportedMethods.Contains (typeof (Convert).GetMethod("ToBoolean", new[] { typeof (char) })), Is.True);
      Assert.That (ConvertMethodCallTransformer.SupportedMethods.Contains (typeof (Convert).GetMethod("ToBoolean", new[] { typeof (byte) })), Is.True);
      Assert.That (ConvertMethodCallTransformer.SupportedMethods.Contains (typeof (Convert).GetMethod("ToBoolean", new[] { typeof (DateTime)})), Is.True);
      Assert.That (ConvertMethodCallTransformer.SupportedMethods.Contains (typeof (Convert).GetMethod("ToBoolean", new[] { typeof (decimal)})), Is.True);
      Assert.That (ConvertMethodCallTransformer.SupportedMethods.Contains (typeof (Convert).GetMethod("ToBoolean", new[] { typeof (double)})), Is.True);
      Assert.That (ConvertMethodCallTransformer.SupportedMethods.Contains (typeof (Convert).GetMethod("ToBoolean", new[] { typeof (float)})), Is.True);
      Assert.That (ConvertMethodCallTransformer.SupportedMethods.Contains (typeof (Convert).GetMethod("ToBoolean", new[] { typeof (long)})), Is.True);
      Assert.That (ConvertMethodCallTransformer.SupportedMethods.Contains (typeof (Convert).GetMethod("ToBoolean", new[] { typeof (object)})), Is.True);
      Assert.That (ConvertMethodCallTransformer.SupportedMethods.Contains (typeof (Convert).GetMethod("ToBoolean", new[] { typeof (short) })), Is.True);
    }

    [Test]
    public void Transform ()
    {
      var method = typeof (Convert).GetMethod ("ToInt32", new[] { typeof (string) });
      var argument = Expression.Constant ("1");

      var expression = Expression.Call (method, argument);
      var transformer = new ConvertMethodCallTransformer();
      var result = transformer.Transform (expression);

      var expectedResult = new SqlConvertExpression (typeof (int), Expression.Constant ("1"));

      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }
  }
}