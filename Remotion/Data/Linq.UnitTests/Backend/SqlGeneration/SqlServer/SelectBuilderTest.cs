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
using System.Text;
using NUnit.Framework;
using Remotion.Data.Linq.Backend.DataObjectModel;
using Remotion.Data.Linq.Backend.SqlGeneration;
using Remotion.Data.Linq.Backend.SqlGeneration.SqlServer;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Clauses.ResultOperators;
using Remotion.Data.Linq.UnitTests.TestDomain;

namespace Remotion.Data.Linq.UnitTests.Backend.SqlGeneration.SqlServer
{
  [TestFixture]
  public class SelectBuilderTest
  {
    private CommandBuilder _commandBuilder;
    private SelectBuilder _selectBuilder;

    [SetUp]
    public void SetUp ()
    {
      _commandBuilder = new CommandBuilder (
          new SqlServerGenerator (StubDatabaseInfo.Instance),
          new StringBuilder(), 
          new List<CommandParameter>(), 
          StubDatabaseInfo.Instance, 
          new MethodCallSqlGeneratorRegistry());
      _selectBuilder = new SelectBuilder (_commandBuilder);
    }

    [Test]
    public void BuildSelectPart ()
    {
      IEvaluation evaluation = new NewObject (
          typeof (Cook).GetConstructor (Type.EmptyTypes),
          new Column (new Table ("s1", "s1"), "c1"),
          new Column (new Table ("s2", "s2"), "c2"),
          new Column (new Table ("s3", "s3"), "c3")
          );

      var sqlGenerationData = new SqlGenerationData();
      sqlGenerationData.SetSelectEvaluation (evaluation, new List<FieldDescriptor> ());

      sqlGenerationData.ResultOperators = new List<ResultOperatorBase> { new FirstResultOperator (true) };

      _selectBuilder.BuildSelectPart (sqlGenerationData);
      Assert.AreEqual ("SELECT TOP 1 [s1].[c1], [s2].[c2], [s3].[c3] ", _commandBuilder.GetCommandText ());
    }

    [Test]
    public void AppendSelectEvaluation_NewObject ()
    {
      IEvaluation evaluation = new NewObject (
          typeof (Cook).GetConstructor (Type.EmptyTypes),
          new Column (new Table ("s1", "s1"), "c1"),
          new Column (new Table ("s2", "s2"), "c2"),
          new Column (new Table ("s3", "s3"), "c3")
          );

      _selectBuilder.AppendSelectEvaluation (evaluation, new List<ResultOperatorBase> ());
      Assert.AreEqual ("[s1].[c1], [s2].[c2], [s3].[c3] ", _commandBuilder.GetCommandText());
    }

    [Test]
    public void AppendSelectEvaluation_BinaryEvaluation ()
    {
      var c1 = new Column (new Table ("s1", "s1"), "c1");
      var c2 = new Column (new Table ("s2", "s2"), "c2");

      var binaryEvaluation = new BinaryEvaluation (c1, c2, BinaryEvaluation.EvaluationKind.Add);
      
      _selectBuilder.AppendSelectEvaluation (binaryEvaluation, new List<ResultOperatorBase>());
      Assert.AreEqual ("([s1].[c1] + [s2].[c2]) ", _commandBuilder.GetCommandText());
    }

    [Test]
    public void AppendSelectEvaluationResultOperators__Count ()
    {
      // TODO 594: The SQL generated here is actually wrong, although it will work in many cases. The problem is that the projection
      // of the Select clause is not used for counting, which might lead to invalid results if NULLs are involved. What Count should really do
      // is create a new SELECT clause around the original SQL query, like this: SELECT COUNT (*) FROM (<SQL QUERY>).
      var c1 = new Column (new Table ("s1", "s1"), "c1");

      _selectBuilder.AppendSelectEvaluation (c1, new List<ResultOperatorBase> { new CountResultOperator () });
      Assert.AreEqual ("COUNT (*) ", _commandBuilder.GetCommandText ());
    }

    [Test]
    public void AppendSelectEvaluation_ResultOperators_First ()
    {
      var c1 = new Column (new Table ("s1", "s1"), "c1");

      _selectBuilder.AppendSelectEvaluation (c1, new List<ResultOperatorBase> { new FirstResultOperator (false) });
      Assert.AreEqual ("TOP 1 [s1].[c1] ", _commandBuilder.GetCommandText ());
    }

    [Test]
    public void AppendSelectEvaluation_ResultOperators_Single ()
    {
      var c1 = new Column (new Table ("s1", "s1"), "c1");

      _selectBuilder.AppendSelectEvaluation (c1, new List<ResultOperatorBase> { new SingleResultOperator (false) });
      Assert.AreEqual ("TOP 1 [s1].[c1] ", _commandBuilder.GetCommandText ());
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException))]
    public void AppendSelectEvaluation_ResultOperators_Last_NotSupported ()
    {
      var c1 = new Column (new Table ("s1", "s1"), "c1");

      _selectBuilder.AppendSelectEvaluation (c1, new List<ResultOperatorBase> { new LastResultOperator (false) });
    }

    [Test]
    public void AppendSelectEvaluation_ResultOperators_Take ()
    {
      var c1 = new Column (new Table ("s1", "s1"), "c1");

      _selectBuilder.AppendSelectEvaluation (c1, new List<ResultOperatorBase> { new TakeResultOperator (Expression.Constant (7)) });
      Assert.AreEqual ("TOP 7 [s1].[c1] ", _commandBuilder.GetCommandText ());
    }

    [Test]
    public void AppendSelectEvaluation_ResultOperators_Distinct ()
    {
      var c1 = new Column (new Table ("s1", "s1"), "c1");

      _selectBuilder.AppendSelectEvaluation (c1, new List<ResultOperatorBase> { new DistinctResultOperator () });
      Assert.AreEqual ("DISTINCT [s1].[c1] ", _commandBuilder.GetCommandText ());
    }

    [Test]
    public void AppendSelectEvaluation_ResultOperators_Cast ()
    {
      var c1 = new Column (new Table ("s1", "s1"), "c1");

      _selectBuilder.AppendSelectEvaluation (c1, new List<ResultOperatorBase> { new CastResultOperator (typeof (int)) });
      Assert.AreEqual ("[s1].[c1] ", _commandBuilder.GetCommandText ());
    }
  }
}
