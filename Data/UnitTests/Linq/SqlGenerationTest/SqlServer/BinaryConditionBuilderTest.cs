/* Copyright (C) 2005 - 2008 rubicon informationstechnologie gmbh
 *
 * This program is free software: you can redistribute it and/or modify it under 
 * the terms of the re:motion license agreement in license.txt. If you did not 
 * receive it, please visit http://www.re-motion.org/licensing.
 * 
 * Unless otherwise provided, this software is distributed on an "AS IS" basis, 
 * WITHOUT WARRANTY OF ANY KIND, either express or implied. 
 */

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq;
using Remotion.Data.Linq.Clauses;
using Rhino.Mocks;
using Remotion.Collections;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.SqlGeneration;
using Remotion.Data.Linq.SqlGeneration.SqlServer;
using Remotion.Development.UnitTesting;

namespace Remotion.Data.UnitTests.Linq.SqlGenerationTest.SqlServer
{
  [TestFixture]
  public class BinaryConditionBuilderTest
  {
    [Test]
    public void BuildBinaryConditionPart_BinaryConditions ()
    {
      CheckBuildBinaryConditionPart_Constants (
          new BinaryCondition (new Constant ("foo"), new Constant ("foo"), BinaryCondition.ConditionKind.Equal),
          "(@1 = @2)");
      CheckBuildBinaryConditionPart_Constants (
          new BinaryCondition (new Constant ("foo"), new Constant ("foo"), BinaryCondition.ConditionKind.NotEqual),
          "(@1 <> @2)");
      CheckBuildBinaryConditionPart_Constants (
          new BinaryCondition (new Constant ("foo"), new Constant ("foo"), BinaryCondition.ConditionKind.LessThan),
          "(@1 < @2)");
      CheckBuildBinaryConditionPart_Constants (
          new BinaryCondition (new Constant ("foo"), new Constant ("foo"), BinaryCondition.ConditionKind.GreaterThan),
          "(@1 > @2)");
      CheckBuildBinaryConditionPart_Constants (
          new BinaryCondition (new Constant ("foo"), new Constant ("foo"), BinaryCondition.ConditionKind.LessThanOrEqual),
          "(@1 <= @2)");
      CheckBuildBinaryConditionPart_Constants (
          new BinaryCondition (new Constant ("foo"), new Constant ("foo"), BinaryCondition.ConditionKind.GreaterThanOrEqual),
          "(@1 >= @2)");
      CheckBuildBinaryConditionPart_Constants (
          new BinaryCondition (new Constant ("foo"), new Constant ("foo"), BinaryCondition.ConditionKind.Like),
          "(@1 LIKE @2)");
    }

    [Test]
    public void BuildBinaryConditionPart_BinaryConditions_WithColumns ()
    {
      Column c1 = new Column (new Table ("a", "b"), "foo");
      Column c2 = new Column (new Table ("c", "d"), "bar");
      CheckBuildBinaryConditionPart (
          new BinaryCondition (c1, c2, BinaryCondition.ConditionKind.Equal),
          "(([b].[foo] IS NULL AND [d].[bar] IS NULL) OR [b].[foo] = [d].[bar])");
      CheckBuildBinaryConditionPart (
          new BinaryCondition (c1, c2, BinaryCondition.ConditionKind.NotEqual),
          "(([b].[foo] IS NULL AND [d].[bar] IS NOT NULL) OR ([b].[foo] IS NOT NULL AND [d].[bar] IS NULL) OR [b].[foo] <> [d].[bar])");
      CheckBuildBinaryConditionPart (
          new BinaryCondition (c1, c2, BinaryCondition.ConditionKind.LessThan),
          "([b].[foo] < [d].[bar])");
      CheckBuildBinaryConditionPart (
          new BinaryCondition (c1, c2, BinaryCondition.ConditionKind.GreaterThan),
          "([b].[foo] > [d].[bar])");
      CheckBuildBinaryConditionPart (
          new BinaryCondition (c1, c2, BinaryCondition.ConditionKind.LessThanOrEqual),
          "(([b].[foo] IS NULL AND [d].[bar] IS NULL) OR [b].[foo] <= [d].[bar])");
      CheckBuildBinaryConditionPart (
          new BinaryCondition (c1, c2, BinaryCondition.ConditionKind.GreaterThanOrEqual),
          "(([b].[foo] IS NULL AND [d].[bar] IS NULL) OR [b].[foo] >= [d].[bar])");
      CheckBuildBinaryConditionPart (
          new BinaryCondition (c1, c2, BinaryCondition.ConditionKind.Like),
          "([b].[foo] LIKE [d].[bar])");
    }

    [Test]
    public void BuildBinaryConditionPart_BinaryConditions_WithColumn_LeftSide ()
    {
      Column c1 = new Column (new Table ("a", "b"), "foo");
      CheckBuildBinaryConditionPart (
          new BinaryCondition (c1, new Constant ("const"), BinaryCondition.ConditionKind.Equal),
          "([b].[foo] = @1)", new CommandParameter ("@1", "const"));
      CheckBuildBinaryConditionPart (
          new BinaryCondition (c1, new Constant ("const"), BinaryCondition.ConditionKind.NotEqual),
          "([b].[foo] IS NULL OR [b].[foo] <> @1)", new CommandParameter ("@1", "const"));
      CheckBuildBinaryConditionPart (
          new BinaryCondition (c1, new Constant ("const"), BinaryCondition.ConditionKind.LessThan),
          "([b].[foo] < @1)", new CommandParameter ("@1", "const"));
      CheckBuildBinaryConditionPart (
          new BinaryCondition (c1, new Constant ("const"), BinaryCondition.ConditionKind.GreaterThan),
          "([b].[foo] > @1)", new CommandParameter ("@1", "const"));
      CheckBuildBinaryConditionPart (
          new BinaryCondition (c1, new Constant ("const"), BinaryCondition.ConditionKind.LessThanOrEqual),
          "([b].[foo] <= @1)", new CommandParameter ("@1", "const"));
      CheckBuildBinaryConditionPart (
          new BinaryCondition (c1, new Constant ("const"), BinaryCondition.ConditionKind.GreaterThanOrEqual),
          "([b].[foo] >= @1)", new CommandParameter ("@1", "const"));
      CheckBuildBinaryConditionPart (
          new BinaryCondition (c1, new Constant ("const"), BinaryCondition.ConditionKind.Like),
          "([b].[foo] LIKE @1)", new CommandParameter ("@1", "const"));
    }

    [Test]
    public void BuildBinaryConditionPart_BinaryConditions_WithColumn_RightSide ()
    {
      Column c1 = new Column (new Table ("a", "b"), "foo");
      CheckBuildBinaryConditionPart (
          new BinaryCondition (new Constant ("const"), c1, BinaryCondition.ConditionKind.Equal),
          "(@1 = [b].[foo])", new CommandParameter ("@1", "const"));
      CheckBuildBinaryConditionPart (
          new BinaryCondition (new Constant ("const"), c1, BinaryCondition.ConditionKind.NotEqual),
          "([b].[foo] IS NULL OR @1 <> [b].[foo])", new CommandParameter ("@1", "const"));
      CheckBuildBinaryConditionPart (
          new BinaryCondition (new Constant ("const"), c1, BinaryCondition.ConditionKind.LessThan),
          "(@1 < [b].[foo])", new CommandParameter ("@1", "const"));
      CheckBuildBinaryConditionPart (
          new BinaryCondition (new Constant ("const"), c1, BinaryCondition.ConditionKind.GreaterThan),
          "(@1 > [b].[foo])", new CommandParameter ("@1", "const"));
      CheckBuildBinaryConditionPart (
          new BinaryCondition (new Constant ("const"), c1, BinaryCondition.ConditionKind.LessThanOrEqual),
          "(@1 <= [b].[foo])", new CommandParameter ("@1", "const"));
      CheckBuildBinaryConditionPart (
          new BinaryCondition (new Constant ("const"), c1, BinaryCondition.ConditionKind.GreaterThanOrEqual),
          "(@1 >= [b].[foo])", new CommandParameter ("@1", "const"));
      CheckBuildBinaryConditionPart (
          new BinaryCondition (new Constant ("const"), c1, BinaryCondition.ConditionKind.Like),
          "(@1 LIKE [b].[foo])", new CommandParameter ("@1", "const"));
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "The binary condition kind 2147483647 is not supported.")]
    public void BuildBinaryConditionPart_InvalidBinaryConditionKind ()
    {
      CheckBuildBinaryConditionPart (new BinaryCondition (new Constant ("foo"), new Constant ("foo"), (BinaryCondition.ConditionKind) int.MaxValue), null);
    }

    [Test]
    public void BuildBinaryConditionPart_BinaryConditionLeftNull ()
    {
      BinaryCondition binaryCondition = new BinaryCondition (new Constant (null), new Constant ("foo"), BinaryCondition.ConditionKind.Equal);
      CheckBuildBinaryConditionPart (binaryCondition, "@1 IS NULL", new CommandParameter ("@1", "foo"));
    }

    [Test]
    public void BuildBinaryConditionPart_BinaryConditionRightNull ()
    {
      BinaryCondition binaryCondition = new BinaryCondition (new Constant ("foo"), new Constant (null), BinaryCondition.ConditionKind.Equal);
      CheckBuildBinaryConditionPart (binaryCondition, "@1 IS NULL", new CommandParameter ("@1", "foo"));
    }

    [Test]
    public void BuildBinaryConditionPart_BinaryConditionIsNotNull ()
    {
      BinaryCondition binaryCondition = new BinaryCondition (new Constant (null), new Constant ("foo"), BinaryCondition.ConditionKind.NotEqual);
      CheckBuildBinaryConditionPart (binaryCondition, "@1 IS NOT NULL", new CommandParameter ("@1", "foo"));
    }

    [Test]
    public void BuildBinaryConditionPart_BinaryConditionNullIsNotNull ()
    {
      BinaryCondition binaryCondition = new BinaryCondition (new Constant (null), new Constant (null), BinaryCondition.ConditionKind.NotEqual);
      CheckBuildBinaryConditionPart (binaryCondition, "NULL IS NOT NULL");
    }

    [Test]
    [ExpectedException (typeof (NotImplementedException), ExpectedMessage = "The method or operation is not implemented.")]
    public void BuildBinaryConditionPart_InvalidValue ()
    {
      BinaryCondition binaryCondition = new BinaryCondition (new PseudoValue(), new Constant (null), BinaryCondition.ConditionKind.NotEqual);
      CheckBuildBinaryConditionPart (binaryCondition, null);
    }

    [Test]
    public void BuildBinaryConditionPart_ContainsCondition ()
    {
      MockRepository mockRepository = new MockRepository ();

      QueryModel queryModel = ExpressionHelper.CreateQueryModel ();
      SubQuery subQuery = new SubQuery (queryModel, null);

      CommandBuilder commandBuilder = new CommandBuilder (new StringBuilder (), new List<CommandParameter> (), StubDatabaseInfo.Instance, new MethodCallSqlGeneratorRegistry());
      commandBuilder.AddParameter (1);

      BinaryConditionBuilder binaryConditionBuilderMock = mockRepository.CreateMock<BinaryConditionBuilder> (commandBuilder, StubDatabaseInfo.Instance);
      ISqlGeneratorBase subQueryGeneratorMock = mockRepository.CreateMock<ISqlGeneratorBase> ();

      Expect.Call (PrivateInvoke.InvokeNonPublicMethod (binaryConditionBuilderMock, "CreateSqlGeneratorForSubQuery", subQuery, StubDatabaseInfo.Instance,
          commandBuilder)).Return (subQueryGeneratorMock);
      Expect.Call (subQueryGeneratorMock.BuildCommand (subQuery.QueryModel)).Do ((Func<QueryModel, CommandData>) delegate
      {
        commandBuilder.Append ("x");
        commandBuilder.AddParameter (0);
        return new CommandData();
      });

      mockRepository.ReplayAll ();
      BinaryCondition binaryCondition = new BinaryCondition(subQuery, new Constant ("foo"), BinaryCondition.ConditionKind.Contains);
      binaryConditionBuilderMock.BuildBinaryConditionPart (binaryCondition);
      mockRepository.VerifyAll ();

      Assert.AreEqual ("@2 IN (x)", commandBuilder.GetCommandText ());
      Assert.That (commandBuilder.GetCommandParameters (),
          Is.EqualTo (new[] { new CommandParameter ("@1", 1), new CommandParameter ("@2", "foo"), new CommandParameter ("@3", 0) }));
    }

    [Test]
    public void CreateSqlGeneratorForSubQuery ()
    {
      SubQuery subQuery = new SubQuery (ExpressionHelper.CreateQueryModel (), null);
      CommandBuilder commandBuilder = new CommandBuilder (new StringBuilder (), new List<CommandParameter> (), StubDatabaseInfo.Instance, new MethodCallSqlGeneratorRegistry());
      BinaryConditionBuilder conditionBuilder = new BinaryConditionBuilder (commandBuilder, StubDatabaseInfo.Instance);
      InlineSqlServerGenerator subQueryGenerator = (InlineSqlServerGenerator) PrivateInvoke.InvokeNonPublicMethod (conditionBuilder, "CreateSqlGeneratorForSubQuery",
          subQuery, StubDatabaseInfo.Instance, commandBuilder);
      Assert.AreSame (StubDatabaseInfo.Instance, subQueryGenerator.DatabaseInfo);
      Assert.AreEqual (ParseMode.SubQueryInWhere, subQueryGenerator.ParseMode);
    }

    [Test]
    public void BuildBinaryCondition_ContainsFulltext ()
    {
      Column column = new Column (new Table ("Student", "s"), "First");
      Constant constant = new Constant("Test");
      BinaryCondition binaryCondition = new BinaryCondition (column, constant, BinaryCondition.ConditionKind.ContainsFulltext);

      CommandBuilder commandBuilder = new CommandBuilder (new StringBuilder (), new List<CommandParameter> (), StubDatabaseInfo.Instance, new MethodCallSqlGeneratorRegistry());
      BinaryConditionBuilder binaryConditionBuilder = new BinaryConditionBuilder (commandBuilder, StubDatabaseInfo.Instance);

      binaryConditionBuilder.BuildBinaryConditionPart (binaryCondition);

      Assert.AreEqual("CONTAINS ([s].[First],@1)", commandBuilder.GetCommandText());
      Assert.That (commandBuilder.GetCommandParameters (), Is.EqualTo (new object[] { new CommandParameter ("@1", "Test") }));
    }

    [Test]
    public void BuildBinaryCondition_MethodCall ()
    {
      ParameterExpression parameter = parameter = Expression.Parameter (typeof (Student), "s");
      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause (parameter, ExpressionHelper.CreateQuerySource ());
      IColumnSource fromSource = fromSource = fromClause.GetFromSource (StubDatabaseInfo.Instance);

      MethodInfo methodInfo = typeof (string).GetMethod ("ToUpper", new Type[] { });
      Column column = new Column (fromSource, "FirstColumn");
      MethodCall methodCall = new MethodCall (methodInfo, column, null);

      BinaryCondition binaryCondition = new BinaryCondition(methodCall,new Constant("Test"),BinaryCondition.ConditionKind.Equal);

      SqlServerGenerator sqlServerGenerator = new SqlServerGenerator (StubDatabaseInfo.Instance);
      CommandBuilder commandBuilder = new CommandBuilder (new StringBuilder (), new List<CommandParameter> (), StubDatabaseInfo.Instance, sqlServerGenerator.MethodCallRegistry);
      BinaryConditionBuilder binaryConditionBuilder = new BinaryConditionBuilder (commandBuilder, StubDatabaseInfo.Instance);

      binaryConditionBuilder.BuildBinaryConditionPart (binaryCondition);

      Assert.AreEqual ("(UPPER([s].[FirstColumn]) = @1)", commandBuilder.GetCommandText ());
      Assert.That (commandBuilder.GetCommandParameters (), Is.EqualTo (new object[] { new CommandParameter ("@1", "Test") }));
    }

    public class PseudoValue : IValue
    {
      public void Accept (IEvaluationVisitor visitor)
      {
        throw new NotImplementedException();
      }
    }


    private static void CheckBuildBinaryConditionPart_Constants (BinaryCondition binaryCondition, string expectedString)
    {
      CheckBuildBinaryConditionPart (binaryCondition, expectedString,
          new CommandParameter ("@1", "foo"), new CommandParameter ("@2", "foo"));
    }

    private static void CheckBuildBinaryConditionPart (BinaryCondition condition, string expectedString,
       params CommandParameter[] expectedParameters)
    {
      CommandBuilder commandBuilder = new CommandBuilder (new StringBuilder (), new List<CommandParameter> (), StubDatabaseInfo.Instance, new MethodCallSqlGeneratorRegistry());
      BinaryConditionBuilder binaryConditionBuilder = new BinaryConditionBuilder (commandBuilder, StubDatabaseInfo.Instance);

      binaryConditionBuilder.BuildBinaryConditionPart (condition);

      Assert.AreEqual (expectedString, commandBuilder.GetCommandText());
      Assert.That (commandBuilder.GetCommandParameters(), Is.EqualTo (expectedParameters));
    }
  }
}
