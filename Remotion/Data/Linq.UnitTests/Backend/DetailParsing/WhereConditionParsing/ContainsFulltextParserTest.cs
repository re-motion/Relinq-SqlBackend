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
using Remotion.Data.Linq.Backend;
using Remotion.Data.Linq.Backend.DataObjectModel;
using Remotion.Data.Linq.Backend.DetailParsing;
using Remotion.Data.Linq.Backend.DetailParsing.WhereConditionParsing;
using Remotion.Data.Linq.Backend.FieldResolving;
using Remotion.Data.Linq.UnitTests.TestDomain;

namespace Remotion.Data.Linq.UnitTests.Backend.DetailParsing.WhereConditionParsing
{
  [TestFixture]
  public class ContainsFulltextParserTest : DetailParserTestBase
  {
    [Test]
    public void ParseContainsFulltext ()
    {
      string methodName = "ContainsFulltext";
      string pattern = "Test";
      CheckParsingOfContainsFulltext (methodName, pattern);
    }

    public static bool Contains ()
    {
      return true;
    }

    private void CheckParsingOfContainsFulltext (string methodName, string pattern)
    {
      MethodCallExpression methodCallExpression = Expression.Call (
          null,
          typeof (ContainsFulltextExtensionMethod).GetMethod (methodName),
          Student_First_Expression,
          Expression.Constant ("Test"));

      var resolver = new FieldResolver (StubDatabaseInfo.Instance, new WhereFieldAccessPolicy (StubDatabaseInfo.Instance));

      var parserRegistry = new WhereConditionParserRegistry (StubDatabaseInfo.Instance);
      parserRegistry.RegisterParser (typeof (ConstantExpression), new ConstantExpressionParser (StubDatabaseInfo.Instance));
      parserRegistry.RegisterParser (typeof (MemberExpression), new MemberExpressionParser (resolver));

      var parser = new ContainsFullTextParser (parserRegistry);

      ICriterion actualCriterion = parser.Parse (methodCallExpression, ParseContext);
      ICriterion expectedCriterion = new BinaryCondition (
          new Column (new Table ("studentTable", "s"), "FirstNameColumn"), new Constant (pattern), BinaryCondition.ConditionKind.ContainsFulltext);
      Assert.That (actualCriterion, Is.EqualTo (expectedCriterion));
    }
  }
}
