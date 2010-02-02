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
using System.Text;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Backend.DataObjectModel;
using Remotion.Data.Linq.Backend.SqlGeneration;
using Remotion.Data.Linq.Backend.SqlGeneration.SqlServer;
using Remotion.Data.Linq.UnitTests.TestDomain;

namespace Remotion.Data.Linq.UnitTests.Backend.SqlGeneration.SqlServer
{
  [TestFixture]
  public class WhereBuilderTest
  {
    private CommandBuilder _commandBuilder;
    private WhereBuilder _whereBuilder;
    
    private Constant _trueCriterion;

    [SetUp]
    public void SetUp ()
    {
      _commandBuilder = new CommandBuilder (
          new StringBuilder (),
          new List<CommandParameter> (),
          StubDatabaseInfo.Instance,
          new MethodCallSqlGeneratorRegistry ());

      _whereBuilder = new WhereBuilder (_commandBuilder);

      _trueCriterion = new Constant (true);
    }

    [Test]
    public void BuildWherePart_NullCriterion ()
    {
      _whereBuilder.BuildWherePart (new SqlGenerationData ());
      Assert.That (_commandBuilder.GetCommandText (), Is.Empty);
    }

    [Test]
    public void BuildWherePart_NonNullCriterion ()
    {
      _whereBuilder.BuildWherePart (new SqlGenerationData { Criterion = _trueCriterion });
      Assert.That (_commandBuilder.GetCommandText (), Is.EqualTo (" WHERE (1=1)"));
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "NULL constants are not supported as WHERE conditions.")]
    public void AppendCriterion_Constant_Null ()
    {
      CheckAppendCriterion (new Constant (null), null);
    }

    [Test]
    public void AppendCriterion_TrueValue()
    {
      ICriterion value = new Constant (true);
      CheckAppendCriterion (value, "(1=1)");
    }

    [Test]
    public void AppendCriterion_FalseValue ()
    {
      ICriterion value = new Constant (false);
      CheckAppendCriterion (value, "(1<>1)");
    }

    [Test]
    public void AppendCriterion_Column ()
    {
      ICriterion value = new Column (new Table ("foo", "foo_alias"), "col");
      CheckAppendCriterion (value, "[foo_alias].[col]=1");
    }

    [Test]
    public void AppendCriterion_ComplexCriterion ()
    {
      var binaryCondition1 = new BinaryCondition (new Constant ("a1"), new Constant ("a2"), BinaryCondition.ConditionKind.Equal);
      var binaryCondition2 = new BinaryCondition (new Constant ("b1"), new Constant ("b2"), BinaryCondition.ConditionKind.Equal);

      CheckAppendCriterion (
          new ComplexCriterion (binaryCondition1, binaryCondition2, ComplexCriterion.JunctionKind.And),
          "((@1 = @2) AND (@3 = @4))",
          new CommandParameter ("@1", "a1"),
          new CommandParameter ("@2", "a2"),
          new CommandParameter ("@3", "b1"),
          new CommandParameter ("@4", "b2"));
    }

    private void CheckAppendCriterion (ICriterion criterion, string expectedString, params CommandParameter[] expectedParameters)
    {
      _whereBuilder.AppendCriterion (criterion);

      Assert.That (_commandBuilder.GetCommandText(), Is.EqualTo (expectedString));
      Assert.That (_commandBuilder.GetCommandParameters (), Is.EqualTo (expectedParameters));
    }
  }
}
