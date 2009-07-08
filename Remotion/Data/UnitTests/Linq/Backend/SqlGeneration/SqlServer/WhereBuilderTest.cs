// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// version 3.0 as published by the Free Software Foundation.
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
using System.Reflection;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Backend.DataObjectModel;
using Remotion.Data.Linq.Backend.SqlGeneration.SqlServer;
using Remotion.Data.Linq.Backend.SqlGeneration;
using System.Collections.Generic;

namespace Remotion.Data.UnitTests.Linq.Backend.SqlGeneration.SqlServer
{
  [TestFixture]
  public class WhereBuilderTest
  {
    [Test]
    public void AppendCriterion_TrueValue()
    {
      ICriterion value = new Constant (true);
      const string expectedString = "(1=1)";
      CheckAppendCriterion_Value(value, expectedString);
    }

    [Test]
    public void AppendCriterion_FalseValue ()
    {
      ICriterion value = new Constant (false);
      const string expectedString = "(1<>1)";
      CheckAppendCriterion_Value (value, expectedString);
    }

    [Test]
    public void AppendCriterion_Column ()
    {
      ICriterion value = new Column (new Table ("foo", "foo_alias"), "col");
      const string expectedString = "[foo_alias].[col]=1";
      CheckAppendCriterion_Value (value, expectedString);
    }

    [Test]
    public void AppendCriterion_ComplexCriterion ()
    {
      BinaryCondition binaryCondition1 = new BinaryCondition (new Constant ("foo"), new Constant ("foo"), BinaryCondition.ConditionKind.Equal);
      BinaryCondition binaryCondition2 = new BinaryCondition (new Constant ("foo"), new Constant ("foo"), BinaryCondition.ConditionKind.Equal);
      
      CheckAppendCriterion_ComplexCriterion
          (new ComplexCriterion (binaryCondition1, binaryCondition2, ComplexCriterion.JunctionKind.And), "AND");
      CheckAppendCriterion_ComplexCriterion
          (new ComplexCriterion (binaryCondition1, binaryCondition2, ComplexCriterion.JunctionKind.Or), "OR");
    }

    [Test]
    public void AppendCriterion_NotCriterion()
    {
      NotCriterion notCriterion = new NotCriterion(new Constant("foo"));
      CheckAppendCriterion (notCriterion, "NOT @1", new CommandParameter ("@1", "foo"));
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "NULL constants are not supported as WHERE conditions.")]
    public void AppendCriterion_NULL ()
    {
      CheckAppendCriterion (new Constant (null), null);
    }

    class PseudoCriterion : ICriterion
    {
      public void Accept (IEvaluationVisitor visitor)
      {
        throw new NotImplementedException();
      }
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "The criterion kind PseudoCriterion is not supported.")]
    public void InvalidCriterionKind_NotSupportedException()
    {
      CheckAppendCriterion (new PseudoCriterion(), null);
    }

    private static void CheckAppendCriterion (ICriterion criterion, string expectedString,
                                              params CommandParameter[] expectedParameters)
    {
      CommandBuilder commandBuilder = new CommandBuilder (new StringBuilder (), new List<CommandParameter> (), StubDatabaseInfo.Instance, new MethodCallSqlGeneratorRegistry ());
      WhereBuilder whereBuilder = new WhereBuilder (commandBuilder, StubDatabaseInfo.Instance);

      whereBuilder.BuildWherePart (criterion);

      Assert.AreEqual (" WHERE " + expectedString, commandBuilder.GetCommandText());
      Assert.That (commandBuilder.GetCommandParameters(), Is.EqualTo (expectedParameters));
    }

    private static void CheckAppendCriterion_Value (ICriterion value, string expectedString)
    {
      CheckAppendCriterion (value, expectedString);
    }
    
    private static void CheckAppendCriterion_ComplexCriterion (ICriterion criterion, string expectedOperator)
    {
      CheckAppendCriterion (criterion, "((@1 = @2) " + expectedOperator + " (@3 = @4))",
                            new CommandParameter ("@1", "foo"),
                            new CommandParameter ("@2", "foo"),
                            new CommandParameter ("@3", "foo"),
                            new CommandParameter ("@4", "foo")
          );
    }

    

  }
}