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
using System.Collections.Generic;
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Linq.SqlBackend.Development.UnitTesting;
using Remotion.Linq.SqlBackend.SqlGeneration;

namespace Remotion.Linq.SqlBackend.UnitTests.SqlGeneration
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
      _sqlCommandBuilder.CreateParameter ("value");

      var expectedCommandParameter = new CommandParameter ("@1", "value");
      Assert.That (_sqlCommandBuilder.GetCommandParameters (), Is.EqualTo (new[] { expectedCommandParameter }));
    }

    [Test]
    public void GetInMemoryProjectionBody ()
    {
      Assert.That (_sqlCommandBuilder.GetInMemoryProjectionBody (), Is.Null);

      var expression = Expression.Constant (0);
      _sqlCommandBuilder.SetInMemoryProjectionBody (expression);

      Assert.That (_sqlCommandBuilder.GetInMemoryProjectionBody (), Is.SameAs (expression));
    }

    [Test]
    public void GetCommand ()
    {
      _sqlCommandBuilder.Append ("Test");
      _sqlCommandBuilder.CreateParameter ("value");

      var body = Expression.Constant (0);
      _sqlCommandBuilder.SetInMemoryProjectionBody (body);

      var data = _sqlCommandBuilder.GetCommand();

      Assert.That (data.CommandText, Is.EqualTo ("Test"));

      var expectedCommandParameter = new CommandParameter ("@1", "value");
      Assert.That (_sqlCommandBuilder.GetCommandParameters (), Is.EqualTo (new[] { expectedCommandParameter }));

      var expectedInMemoryProjection = Expression.Lambda<Func<IDatabaseResultRow, int>> (body, _sqlCommandBuilder.InMemoryProjectionRowParameter);
      SqlExpressionTreeComparer.CheckAreEqualTrees (expectedInMemoryProjection, data.GetInMemoryProjection<int> ());
    }

    [Test]
    public void GetCommand_NoTextSet ()
    {
      _sqlCommandBuilder.CreateParameter ("value");

      var body = Expression.Constant (0);
      _sqlCommandBuilder.SetInMemoryProjectionBody (body);
      Assert.That (
          () => _sqlCommandBuilder.GetCommand (),
          Throws.InvalidOperationException
              .With.Message.EqualTo (
                  "Command text must be appended before a command can be created."));
    }

    [Test]
    public void GetCommand_NoProjectionSet ()
    {
      _sqlCommandBuilder.Append ("Test");
      _sqlCommandBuilder.CreateParameter ("value");
      Assert.That (
          () => _sqlCommandBuilder.GetCommand (),
          Throws.InvalidOperationException
              .With.Message.EqualTo ("An in-memory projection body must be appended before a command can be created."));
    }

    [Test]
    public void GetCommand_NoParameterAdded_Works()
    {
      _sqlCommandBuilder.Append ("Test");

      var body = Expression.Constant (0);
      _sqlCommandBuilder.SetInMemoryProjectionBody (body);

      _sqlCommandBuilder.GetCommand ();
    }

    [Test]
    public void CreateParameter_Value ()
    {
      var result = _sqlCommandBuilder.CreateParameter ("test");

      Assert.That (result.Name, Is.EqualTo ("@1"));
      Assert.That (result.Value, Is.EqualTo ("test"));
    }

    [Test]
    public void GetOrCreateParameter ()
    {
      var expression = Expression.Constant ("test");
      var result = _sqlCommandBuilder.GetOrCreateParameter (expression);

      Assert.That (result.Name, Is.EqualTo ("@1"));
      Assert.That (result.Value, Is.EqualTo ("test"));
    }

    [Test]
    public void GetOrCreateParameter_Twice ()
    {
      var expression = Expression.Constant ("test");
      var result1 = _sqlCommandBuilder.GetOrCreateParameter (expression);
      var result2 = _sqlCommandBuilder.GetOrCreateParameter (expression);

      Assert.That (result1, Is.EqualTo (result2));
      Assert.That (_sqlCommandBuilder.GetCommandParameters(), Has.Length.EqualTo (1));
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
  }
}