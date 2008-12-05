// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2008 rubicon informationstechnologie gmbh, www.rubicon.eu
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
using System.Linq;
using System.Text;
using NUnit.Framework;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.SqlGeneration;
using Remotion.Data.Linq.SqlGeneration.SqlServer;
using Remotion.Data.UnitTests.Linq.TestQueryGenerators;

namespace Remotion.Data.UnitTests.Linq.SqlGenerationTest.SqlServer
{
  [TestFixture]
  public class SelectBuilderTest
  {
    private CommandBuilder _commandBuilder;
    private SelectBuilder _selectBuilder;

    [SetUp]
    public void SetUp ()
    {
      _commandBuilder = new CommandBuilder (new StringBuilder (), new List<CommandParameter> (), StubDatabaseInfo.Instance, new MethodCallSqlGeneratorRegistry());
      _selectBuilder = new SelectBuilder (_commandBuilder);
    }

    [Test]
    public void CombineColumnItems()
    {
      IEvaluation evaluation = new NewObject (typeof (Student).GetConstructor(Type.EmptyTypes), 
        new Column (new Table ("s1", "s1"), "c1"),
        new Column (new Table ("s2", "s2"), "c2"),
        new Column (new Table ("s3", "s3"), "c3")
      );

      _selectBuilder.BuildSelectPart (evaluation, null);
      Assert.AreEqual ("SELECT [s1].[c1], [s2].[c2], [s3].[c3] ", _commandBuilder.GetCommandText());
    }
    
    [Test]
    public void SelectWithCount ()
    {
      IEvaluation evaluation = new Constant();
      var query = SelectTestQueryGenerator.CreateSimpleQuery (ExpressionHelper.CreateQuerySource ());
      var methodInfo = ParserUtility.GetMethod (() => Enumerable.Count (query));
      List<MethodCall> methodCalls = new List<MethodCall>();
      MethodCall methodCall = new MethodCall (methodInfo, evaluation, new List<IEvaluation> ());
      methodCalls.Add (methodCall);

      _selectBuilder.BuildSelectPart (evaluation, methodCalls);

      Assert.AreEqual ("SELECT COUNT (*) ", _commandBuilder.GetCommandText());
    }

    [Test]
    public void SelectWithDistinct ()
    {
      IEvaluation evaluation = new NewObject (typeof (Student).GetConstructor (Type.EmptyTypes),
        new Column (new Table ("s1", "s1"), "c1"),
        new Column (new Table ("s2", "s2"), "c2")
      );

      var query = SelectTestQueryGenerator.CreateSimpleQuery (ExpressionHelper.CreateQuerySource ());
      var methodInfo = ParserUtility.GetMethod (() => Enumerable.Distinct (query));
      List<MethodCall> methodCalls = new List<MethodCall> ();
      MethodCall methodCall = new MethodCall (methodInfo, evaluation, new List<IEvaluation> ());
      methodCalls.Add (methodCall);

      _selectBuilder.BuildSelectPart (evaluation, methodCalls);

      Assert.AreEqual ("SELECT DISTINCT [s1].[c1], [s2].[c2] ", _commandBuilder.GetCommandText ());
    }

    [Test]
    [Ignore]
    public void SelectWithAverage ()
    {
      IEvaluation evaluation = new NewObject (typeof (Student).GetConstructor (Type.EmptyTypes),
        new Column (new Table ("s1", "s1"), "c1")
      );

      var query = SelectTestQueryGenerator.CreateSimpleQueryOnID (ExpressionHelper.CreateQuerySource ());
      var methodInfo = ParserUtility.GetMethod (() => Enumerable.Average (query));
      List<MethodCall> methodCalls = new List<MethodCall> ();
      MethodCall methodCall = new MethodCall (methodInfo, evaluation, new List<IEvaluation> ());
      methodCalls.Add (methodCall);

      _selectBuilder.BuildSelectPart (evaluation, methodCalls);

      Assert.AreEqual ("SELECT AVG [s1].[c1] ", _commandBuilder.GetCommandText ());
    }

    [Test]
    public void SelectWithDistinctAndFirst ()
    {
      IEvaluation evaluation = new NewObject (typeof (Student).GetConstructor (Type.EmptyTypes),
       new Column (new Table ("o", "o"), "c")
     );

      var query = SelectTestQueryGenerator.CreateSimpleQuery (ExpressionHelper.CreateQuerySource ());
      var distinctMethod = ParserUtility.GetMethod (() => Enumerable.Distinct (query));
      var firstMethod = ParserUtility.GetMethod(() => Enumerable.First(query));
      MethodCall methodCallDistinct = new MethodCall (distinctMethod, evaluation, new List<IEvaluation> ());
      MethodCall methodCallFirst = new MethodCall (firstMethod, evaluation, new List<IEvaluation> ());
      List<MethodCall> methodCalls = new List<MethodCall> ();
      methodCalls.Add (methodCallDistinct);
      methodCalls.Add (methodCallFirst);

      _selectBuilder.BuildSelectPart (evaluation, methodCalls);

      Assert.AreEqual("SELECT DISTINCT TOP 1 [o].[c] ", _commandBuilder.GetCommandText());
    }

    [Test]
    public void SelectWithSingleSimple ()
    {
      IEvaluation evaluation = new Column(new Table("o","o"),"c");
      var query = SelectTestQueryGenerator.CreateSimpleQuery (ExpressionHelper.CreateQuerySource ());
      var singleMethod = ParserUtility.GetMethod (() => Enumerable.Single (query));

      MethodCall singleMethodCall = new MethodCall (singleMethod, evaluation, new List<IEvaluation> ());
      List<MethodCall> methodCalls = new List<MethodCall> ();
      methodCalls.Add (singleMethodCall);

      _selectBuilder.BuildSelectPart (evaluation, methodCalls);

      Assert.AreEqual ("SELECT TOP 1 [o].[c] ", _commandBuilder.GetCommandText());
    }

    [Test]
    [Ignore]
    public void SelectWithSingleComplex ()
    {
      //Predicat as WHERE-Clause
      IEvaluation evaluation = new Column (new Table ("o", "o"), "c");
      var query = SelectTestQueryGenerator.CreateSimpleQuery (ExpressionHelper.CreateQuerySource ());
      var singleMethod = ParserUtility.GetMethod (() => Enumerable.Single (query,(i => i.First == "Test")));

      MethodCall singleMethodCall = new MethodCall (singleMethod, evaluation, new List<IEvaluation>());
      List<MethodCall> methodCalls = new List<MethodCall> ();
      methodCalls.Add (singleMethodCall);

      _selectBuilder.BuildSelectPart (evaluation, methodCalls);
      
      Assert.AreEqual ("SELECT TOP 1 [o].[c]  ", _commandBuilder.GetCommandText ());
      //where clause
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "Method 'ElementAt' is not supported.")]
    public void Select_UnknownMethod ()
    {
      IEvaluation evaluation = new Constant ();
      var query = SelectTestQueryGenerator.CreateSimpleQuery (ExpressionHelper.CreateQuerySource ());
      var methodInfo = ParserUtility.GetMethod (() => Enumerable.ElementAt (query, 5));
      List<MethodCall> methodCalls = new List<MethodCall> ();
      MethodCall methodCall = new MethodCall (methodInfo, evaluation, new List<IEvaluation> ());
      methodCalls.Add (methodCall);
      _selectBuilder.BuildSelectPart (evaluation, methodCalls);
    }

    [Test]
    public void SelectWithFirst ()
    {
      IEvaluation evaluation = new Column (new Table ("o", "o"), "*");
      var query = SelectTestQueryGenerator.CreateSimpleQuery (ExpressionHelper.CreateQuerySource ());
      var methodInfo = ParserUtility.GetMethod (() => Enumerable.First (query));

      List<MethodCall> methodCalls = new List<MethodCall> ();
      MethodCall methodCall = new MethodCall (methodInfo, evaluation, new List<IEvaluation>());
      methodCalls.Add (methodCall);

      _selectBuilder.BuildSelectPart (evaluation, methodCalls);

      Assert.AreEqual ("SELECT TOP 1 [o].* ", _commandBuilder.GetCommandText ());
    }
   
    [Test]
    public void BinaryEvaluations_Add ()
    {
      Column c1 = new Column (new Table ("s1", "s1"), "c1");
      Column c2 = new Column (new Table ("s2", "s2"), "c2");
      
      BinaryEvaluation binaryEvaluation = new BinaryEvaluation(c1,c2,BinaryEvaluation.EvaluationKind.Add);

      _selectBuilder.BuildSelectPart (binaryEvaluation, null);
      Assert.AreEqual ("SELECT ([s1].[c1] + [s2].[c2]) ", _commandBuilder.GetCommandText ());
    }
  }
}
