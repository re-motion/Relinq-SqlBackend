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
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Data.Linq.SqlBackend.SqlPreparation.MethodCallTransformers;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;
using Remotion.Data.Linq.UnitTests.Linq.Core.Parsing;

namespace Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlPreparation.MethodCallTransformers
{
  [TestFixture]
  public class ConvertMethodCallTransformerTest
  {
    [Test]
    public void SupportedMethods ()
    {
      Assert.IsTrue (ConvertMethodCallTransformer.SupportedMethods.Contains (typeof (Convert).GetMethod ("ToString", new[] { typeof (int) })));
      Assert.IsTrue (ConvertMethodCallTransformer.SupportedMethods.Contains (typeof (Convert).GetMethod ("ToString", new[] { typeof (bool) })));
      Assert.IsTrue (ConvertMethodCallTransformer.SupportedMethods.Contains (typeof (Convert).GetMethod ("ToString", new[] { typeof (object) })));
      Assert.IsTrue (ConvertMethodCallTransformer.SupportedMethods.Contains (typeof (Convert).GetMethod ("ToInt64", new[] { typeof (string) })));
      Assert.IsTrue (ConvertMethodCallTransformer.SupportedMethods.Contains (typeof (Convert).GetMethod ("ToInt64", new[] { typeof (string) })));
      Assert.IsTrue (ConvertMethodCallTransformer.SupportedMethods.Contains (typeof (Convert).GetMethod ("ToInt64", new[] { typeof (bool) })));
      Assert.IsTrue (ConvertMethodCallTransformer.SupportedMethods.Contains (typeof (Convert).GetMethod ("ToInt32", new[] { typeof (string) })));
      Assert.IsTrue (ConvertMethodCallTransformer.SupportedMethods.Contains (typeof (Convert).GetMethod ("ToInt32", new[] { typeof (bool) })));
      Assert.IsTrue (ConvertMethodCallTransformer.SupportedMethods.Contains (typeof (Convert).GetMethod ("ToInt16", new[] { typeof (string) })));
      Assert.IsTrue (ConvertMethodCallTransformer.SupportedMethods.Contains (typeof (Convert).GetMethod ("ToInt16", new[] { typeof (bool) })));
      Assert.IsTrue (ConvertMethodCallTransformer.SupportedMethods.Contains (typeof (Convert).GetMethod ("ToBoolean", new[] { typeof (string) })));
      Assert.IsTrue (ConvertMethodCallTransformer.SupportedMethods.Contains (typeof (Convert).GetMethod ("ToBoolean", new[] { typeof (int) })));
    }

    [Test]
    public void Transform ()
    {
      var method = typeof (Convert).GetMethod ("ToString", new Type[] { });
      var argument = Expression.Constant (1);
      var expression = Expression.Call (argument, method);

      var transformer = new ConvertMethodCallTransformer ();
      var result = transformer.Transform (expression);

      var fakeResult = new SqlConvertExpression (typeof(string), Expression.Constant(1));

      ExpressionTreeComparer.CheckAreEqualTrees (result, fakeResult);
    }
  }
}