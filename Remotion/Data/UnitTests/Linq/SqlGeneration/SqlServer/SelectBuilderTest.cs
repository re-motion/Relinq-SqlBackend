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
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Clauses.ResultModifications;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.SqlGeneration;
using Remotion.Data.Linq.SqlGeneration.SqlServer;

namespace Remotion.Data.UnitTests.Linq.SqlGeneration.SqlServer
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
          new StringBuilder(), new List<CommandParameter>(), StubDatabaseInfo.Instance, new MethodCallSqlGeneratorRegistry());
      _selectBuilder = new SelectBuilder (_commandBuilder);
    }

    [Test]
    public void CombineColumnItems ()
    {
      IEvaluation evaluation = new NewObject (
          typeof (Student).GetConstructor (Type.EmptyTypes),
          new Column (new Table ("s1", "s1"), "c1"),
          new Column (new Table ("s2", "s2"), "c2"),
          new Column (new Table ("s3", "s3"), "c3")
          );

      _selectBuilder.BuildSelectPart (evaluation, new List<ResultModificationBase> ());
      Assert.AreEqual ("SELECT [s1].[c1], [s2].[c2], [s3].[c3] ", _commandBuilder.GetCommandText());
    }

    [Test]
    public void BinaryEvaluations_Add ()
    {
      Column c1 = new Column (new Table ("s1", "s1"), "c1");
      Column c2 = new Column (new Table ("s2", "s2"), "c2");

      BinaryEvaluation binaryEvaluation = new BinaryEvaluation (c1, c2, BinaryEvaluation.EvaluationKind.Add);
      
      _selectBuilder.BuildSelectPart (binaryEvaluation, new List<ResultModificationBase>());
      Assert.AreEqual ("SELECT ([s1].[c1] + [s2].[c2]) ", _commandBuilder.GetCommandText());
    }

    [Test]
    public void ResultModifications_Count ()
    {
      // TODO 594: The SQL generated here is actually wrong, although it will work in many cases. The problem is that the projection
      // of the Select clause is not used for counting, which might lead to invalid results if NULLs are involved. What Count should really do
      // is create a new SELECT clause around the original SQL query, like this: SELECT COUNT (*) FROM (<SQL QUERY>).
      Column c1 = new Column (new Table ("s1", "s1"), "c1");

      _selectBuilder.BuildSelectPart (c1, new List<ResultModificationBase> { new CountResultModification (ExpressionHelper.CreateSelectClause()) });
      Assert.AreEqual ("SELECT COUNT (*) ", _commandBuilder.GetCommandText ());
    }

    [Test]
    public void ResultModifications_First ()
    {
      Column c1 = new Column (new Table ("s1", "s1"), "c1");

      _selectBuilder.BuildSelectPart (c1, new List<ResultModificationBase> { new FirstResultModification (ExpressionHelper.CreateSelectClause (), false) });
      Assert.AreEqual ("SELECT TOP 1 [s1].[c1] ", _commandBuilder.GetCommandText ());
    }

    [Test]
    public void ResultModifications_Single ()
    {
      Column c1 = new Column (new Table ("s1", "s1"), "c1");

      _selectBuilder.BuildSelectPart (c1, new List<ResultModificationBase> { new SingleResultModification (ExpressionHelper.CreateSelectClause (), false) });
      Assert.AreEqual ("SELECT TOP 1 [s1].[c1] ", _commandBuilder.GetCommandText ());
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException))]
    public void ResultModifications_Last_NotSupported ()
    {
      Column c1 = new Column (new Table ("s1", "s1"), "c1");

      _selectBuilder.BuildSelectPart (c1, new List<ResultModificationBase> { new LastResultModification (ExpressionHelper.CreateSelectClause (), false) });
    }

    [Test]
    public void ResultModifications_Take ()
    {
      Column c1 = new Column (new Table ("s1", "s1"), "c1");

      _selectBuilder.BuildSelectPart (c1, new List<ResultModificationBase> { new TakeResultModification (ExpressionHelper.CreateSelectClause (), 7) });
      Assert.AreEqual ("SELECT TOP 7 [s1].[c1] ", _commandBuilder.GetCommandText ());
    }

    [Test]
    public void ResultModifications_Distinct ()
    {
      Column c1 = new Column (new Table ("s1", "s1"), "c1");

      _selectBuilder.BuildSelectPart (c1, new List<ResultModificationBase> { new DistinctResultModification (ExpressionHelper.CreateSelectClause ()) });
      Assert.AreEqual ("SELECT DISTINCT [s1].[c1] ", _commandBuilder.GetCommandText ());
    }
  }
}