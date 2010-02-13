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
using System.Collections;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Backend.DetailParsing;
using Remotion.Data.Linq.Backend.DetailParsing.WhereConditionParsing;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.UnitTests.TestDomain;

namespace Remotion.Data.Linq.UnitTests.Backend.DetailParsing
{
  [TestFixture]
  public class ParserRegistryTest
  {
    [Test]
    public void GetParsers_NoParserRegistered ()
    {
      ParserRegistry parserRegistry = new ParserRegistry ();
      IEnumerable resultList = parserRegistry.GetParsers (typeof (Expression));
      Assert.IsFalse (resultList.GetEnumerator ().MoveNext ());
    }

    [Test]
    public void GetParsers_ParsersRegistered ()
    {
      ConstantExpressionParser parser1 = new ConstantExpressionParser (StubDatabaseInfo.Instance);
      ConstantExpressionParser parser2 = new ConstantExpressionParser (StubDatabaseInfo.Instance);
      ConstantExpressionParser parser3 = new ConstantExpressionParser (StubDatabaseInfo.Instance);

      ParserRegistry parserRegistry = new ParserRegistry ();
      parserRegistry.RegisterParser (typeof (ConstantExpression), parser1);
      parserRegistry.RegisterParser (typeof (ConstantExpression), parser2);
      parserRegistry.RegisterParser (typeof (BinaryExpression), parser3);

      Assert.That (parserRegistry.GetParsers (typeof (ConstantExpression)).ToArray (), Is.EqualTo (new[] { parser2, parser1 }));
      Assert.That (parserRegistry.GetParsers (typeof (BinaryExpression)).ToArray (), Is.EqualTo (new[] { parser3 }));
   }

    [Test]
    public void GetParser_LastRegisteredParser ()
    {
      ConstantExpressionParser parser1 = new ConstantExpressionParser (StubDatabaseInfo.Instance);
      ConstantExpressionParser parser2 = new ConstantExpressionParser (StubDatabaseInfo.Instance);

      ParserRegistry parserRegistry = new ParserRegistry();

      parserRegistry.RegisterParser (typeof (ConstantExpression), parser1);
      parserRegistry.RegisterParser (typeof (ConstantExpression), parser2);
      
      Assert.That (parserRegistry.GetParser (Expression.Constant (0)), Is.SameAs (parser2));
    }

    [Test]
    [ExpectedException (typeof (ParserException), ExpectedMessage = "Cannot parse 5, no appropriate parser found")]
    public void GetParser_NoParserFound ()
    {
      ParserRegistry parserRegistry = new ParserRegistry ();

      ConstantExpression constantExpression = Expression.Constant (5);
      parserRegistry.GetParser (constantExpression);
    }

#if !NET_3_5
    [Test]
    public void GetParsers_ClosestMatch ()
    {
      ConstantExpressionParser parser1 = new ConstantExpressionParser (StubDatabaseInfo.Instance);
      ConstantExpressionParser parser2 = new ConstantExpressionParser (StubDatabaseInfo.Instance);
      ConstantExpressionParser parser3 = new ConstantExpressionParser (StubDatabaseInfo.Instance);

      ParserRegistry parserRegistry = new ParserRegistry();
      parserRegistry.RegisterParser (typeof (BinaryExpression), parser1);
      parserRegistry.RegisterParser (typeof (Expression), parser1);
      parserRegistry.RegisterParser (typeof (ConstantExpression), parser3);

      Assert.That (parserRegistry.GetParsers (typeof (BinaryExpression)).ToArray(), Is.EqualTo (new[] { parser1 }));
      Assert.That (parserRegistry.GetParsers (typeof (Expression)).ToArray(), Is.EqualTo (new[] { parser3 }));
      Assert.That (parserRegistry.GetParsers (typeof (ConstantExpression)).ToArray(), Is.EqualTo (new[] { parser2 }));
    }
#endif
  }
}
