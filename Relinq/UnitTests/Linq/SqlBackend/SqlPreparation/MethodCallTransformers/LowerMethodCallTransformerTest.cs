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
using NUnit.Framework.SyntaxHelpers;
using Remotion.Linq.SqlBackend.SqlPreparation.MethodCallTransformers;
using Remotion.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;

namespace Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlPreparation.MethodCallTransformers
{
  [TestFixture]
  public class LowerMethodCallTransformerTest
  {
    [Test]
    public void SupportedMethods ()
    {
      Assert.IsTrue (LowerMethodCallTransformer.SupportedMethods.Contains (typeof (string).GetMethod("ToLower", new Type[0])));
    }

    [Test]
    public void Transform ()
    {
      var method = typeof (string).GetMethod ("ToLower", new Type[] { });
      var objectExpression = Expression.Constant ("Test");
      var expression = Expression.Call (objectExpression, method);
      var transformer = new LowerMethodCallTransformer();
      var result = transformer.Transform (expression);

      Assert.That (result, Is.InstanceOfType (typeof (SqlFunctionExpression)));
      Assert.That (result.Type, Is.EqualTo (typeof (string)));
      Assert.That (((SqlFunctionExpression) result).SqlFunctioName, Is.EqualTo ("LOWER"));
      Assert.That (((SqlFunctionExpression) result).Args[0], Is.EqualTo (objectExpression));
    }
  }
}