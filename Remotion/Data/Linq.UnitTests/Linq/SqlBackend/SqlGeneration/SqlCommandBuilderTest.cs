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
using System.Collections.Generic;
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.SqlBackend.SqlGeneration;
using Remotion.Data.Linq.UnitTests.Linq.Core.Parsing;

namespace Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlGeneration
{
  [TestFixture]
  public class SqlCommandBuilderTest
  {
    private SqlCommandBuilder _sqlCommandBuilder;

    [SetUp]
    public void SetUp ()
    {
      _sqlCommandBuilder = new SqlCommandBuilder();
    }

    [Test]
    public void GetCommandText ()
    {
      _sqlCommandBuilder.Append ("Test");

      Assert.That (_sqlCommandBuilder.GetCommandText(), Is.EqualTo ("Test"));
    }

    [Test]
    public void GetCommandParameters ()
    {
      var commandParameter = new CommandParameter ("@1", "value");
      _sqlCommandBuilder.CreateParameter ("value");

      Assert.That (_sqlCommandBuilder.GetCommandParameters().Length, Is.EqualTo (1));
      Assert.That (_sqlCommandBuilder.GetCommandParameters()[0], Is.EqualTo (commandParameter));
    }

    [Test]
    public void CreateParameter ()
    {
      var result = _sqlCommandBuilder.CreateParameter ("test");

      Assert.That (result.Name, Is.EqualTo ("@1"));
      Assert.That (result.Value, Is.EqualTo ("test"));
    }

    [Test]
    public void AppendParameter ()
    {
      _sqlCommandBuilder.AppendParameter ("test");

      var commandParameter = _sqlCommandBuilder.GetCommandParameters();

      Assert.That (commandParameter[0].Name, Is.EqualTo ("@1"));
      Assert.That (commandParameter[0].Value, Is.EqualTo ("test"));
    }

    [Test]
    public void AppendStringLiteral ()
    {
      _sqlCommandBuilder.AppendStringLiteral ("'test'");
      Assert.That (_sqlCommandBuilder.GetCommandText(), Is.EqualTo ("'''test'''"));
    }

    [Test]
    public void AppendSeparated ()
    {
      _sqlCommandBuilder.AppendSeparated (",", new List<string> { "Hugo", "Sepp" },(cb, value) => cb.Append(value));
      Assert.That (_sqlCommandBuilder.GetCommandText (), Is.EqualTo ("Hugo,Sepp"));
    }

    [Test]
    public void GetInMemoryProjection_NoProjectionSet ()
    {
      var result = _sqlCommandBuilder.GetInMemoryProjection ();

      Assert.That (result, Is.Null);
    }

    [Test]
    public void GetInMemoryProjection_ProjectionSet ()
    {
      var body = Expression.Constant (0);
      _sqlCommandBuilder.SetInMemoryProjectionBody (body);
      
      var result = _sqlCommandBuilder.GetInMemoryProjection ();

      var expectedExpression = Expression.Lambda<Func<IDatabaseResultRow, object>> (
          Expression.Convert (body, typeof (object)), 
          _sqlCommandBuilder.InMemoryProjectionRowParameter);
      ExpressionTreeComparer.CheckAreEqualTrees (expectedExpression, result);
    }
  }
}