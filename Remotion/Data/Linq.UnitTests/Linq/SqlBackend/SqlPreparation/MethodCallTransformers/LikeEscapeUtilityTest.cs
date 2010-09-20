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
using Remotion.Data.Linq.SqlBackend.SqlPreparation.MethodCallTransformers;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;
using Remotion.Data.Linq.UnitTests.Linq.Core.Parsing;

namespace Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlPreparation.MethodCallTransformers
{
  [TestFixture]
  public class LikeEscapeUtilityTest
  {
    [Test]
    public void Escape_Bracket ()
    {
      var result = LikeEscapeUtility.Escape ("test[test", @"\");

      Assert.That (result, Is.EqualTo (@"test\[test"));
    }

    [Test]
    public void Escape_Percent ()
    {
      var result = LikeEscapeUtility.Escape ("test%test", @"\");

      Assert.That (result, Is.EqualTo (@"test\%test"));
    }

    [Test]
    public void Escape_Underline ()
    {
      var result = LikeEscapeUtility.Escape ("test_test", @"\");

      Assert.That (result, Is.EqualTo (@"test\_test"));
    }

    [Test]
    public void Escape_EscapeSequence ()
    {
      var result = LikeEscapeUtility.Escape (@"test\test", @"\");

      Assert.That (result, Is.EqualTo (@"test\\test"));
    }

    [Test]
    public void Escape_Expression ()
    {
      var expression = Expression.Constant ("test[test");

      var result = LikeEscapeUtility.Escape (expression, @"\");

      var expectedResult = 
        new SqlFunctionExpression(typeof(string), "REPLACE",
          new SqlFunctionExpression(typeof(string), "REPLACE",
            new SqlFunctionExpression(typeof(string), "REPLACE",
              new SqlFunctionExpression (typeof (string), "REPLACE", expression, 
                new SqlLiteralExpression (@"\"), new SqlLiteralExpression (@"\\")), 
                  new SqlLiteralExpression("%"), new SqlLiteralExpression(@"\%")),
                    new SqlLiteralExpression("_"), new SqlLiteralExpression(@"\_")),
                      new SqlLiteralExpression("["), new SqlLiteralExpression(@"\["));

      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }
  }
}