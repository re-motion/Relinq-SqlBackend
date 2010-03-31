// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System;
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.SqlBackend.SqlGeneration.MethodCallTransformers;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;

namespace Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlGeneration.MethodCallTransformers
{
  [TestFixture]
  public class ReplaceMethodCallTransformerTest
  {
    [Test]
    public void Transform ()
    {
      var method = typeof (string).GetMethod ("Replace", new Type[] {typeof(string), typeof(string) });
      var objectExpression = Expression.Constant ("TAst");
      var expression = Expression.Call (objectExpression, method, Expression.Constant("A"), Expression.Constant("B"));
      var transformer = new ReplaceMethodCallTransformer ();
      var result = transformer.Transform (expression);

      Assert.That (result, Is.InstanceOfType (typeof (SqlFunctionExpression)));
      Assert.That (result.Type, Is.EqualTo (typeof (string)));
      Assert.That (((SqlFunctionExpression) result).SqlFunctioName, Is.EqualTo ("REPLACE"));
      Assert.That (((SqlFunctionExpression) result).Prefix, Is.EqualTo (objectExpression));
      Assert.That (((SqlFunctionExpression) result).Args.Length, Is.EqualTo (2));
    }
  }
}