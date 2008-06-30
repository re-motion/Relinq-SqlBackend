using System;
using System.Collections.Generic;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Collections;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.SqlGeneration;
using Remotion.Data.Linq.UnitTests.SqlGenerationTest.SqlServer;
using Rhino.Mocks;

namespace Remotion.Data.Linq.UnitTests.SqlGenerationTest
{
  [TestFixture]
  public class SelectedObjectActivatorTest
  {
    [Test]
    public void GetValue_ForColumn ()
    {
      Column column = new Column(new Table(),"column");
      List<IEvaluation> evaluations = new List<IEvaluation> { column };
      object[] values = new object[] { 1 };

      SelectedObjectActivator selectedObjectActivator = new SelectedObjectActivator (evaluations);
      object result = selectedObjectActivator.CreateSelectedObject (values);

      Assert.That (result, Is.EqualTo (1));
    }

    [Test]
    public void GetConstant_ForConstant ()
    {
      Constant constant = new Constant("test");
      List<IEvaluation> evaluations = new List<IEvaluation> { constant };
      object[] values = new object[] {};

      SelectedObjectActivator selectedObjectActivator = new SelectedObjectActivator (evaluations);
      object result = selectedObjectActivator.CreateSelectedObject (values);

      Assert.That (result, Is.EqualTo ("test"));
    }

    [Test]
    public void NewObject ()
    {
      NewObject newObject = new NewObject (typeof (object).GetConstructors()[0]);
      List<IEvaluation> evaluations = new List<IEvaluation> { newObject };
      object[] values = new object[] { };

      SelectedObjectActivator selectedObjectActivator = new SelectedObjectActivator (evaluations);
      object result = selectedObjectActivator.CreateSelectedObject (values);

      Assert.That (result, Is.Not.Null);
      Assert.That (result.GetType(), Is.EqualTo (typeof (object)));
    }

    [Test]
    public void NewObject_WithArguments ()
    {
      Constant constant = new Constant("1");
      Column column = new Column (new Table (), "column");
      NewObject newObject = new NewObject (typeof (Tuple<string,string>).GetConstructors ()[0], constant, column);

      List<IEvaluation> evaluations = new List<IEvaluation> { newObject };
      object[] values = new object[] { "test" };

      SelectedObjectActivator selectedObjectActivator = new SelectedObjectActivator (evaluations);
      object result = selectedObjectActivator.CreateSelectedObject (values);

      Assert.That (result, Is.Not.Null);
      Assert.That (result, Is.EqualTo (new Tuple<string,string>("1", "test")));  
    }

    [Test]
    public void NewObject_NewObject ()
    {
      Constant constant = new Constant ("1");
      Column column = new Column (new Table(), "column");
      Column innerColumn = new Column (new Table(), "innerColumn");
      NewObject innerNewObject = new NewObject (typeof (DateTime).GetConstructor (new[] { typeof (long) }), innerColumn);
      NewObject newObject = new NewObject (typeof (Tuple<string, string, DateTime>).GetConstructors()[0], constant, column, innerNewObject);

      List<IEvaluation> evaluations = new List<IEvaluation> { newObject };
      object[] values = new object[] { "test", 1234L };

      SelectedObjectActivator selectedObjectActivator = new SelectedObjectActivator (evaluations);
      object result = selectedObjectActivator.CreateSelectedObject (values);

      Assert.That (result, Is.Not.Null);
      Assert.That (result, Is.EqualTo (new Tuple<string, string, DateTime> ("1", "test", new DateTime (1234L))));
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage = "Too few values.\r\nParameter name: values")]
    public void TooFewValues ()
    {
      Column column = new Column ();
      List<IEvaluation> evaluations = new List<IEvaluation> { column };
      object[] values = new object[] { };

      SelectedObjectActivator selectedObjectActivator = new SelectedObjectActivator (evaluations);
      selectedObjectActivator.CreateSelectedObject (values);
    }

    [Test]
    [ExpectedException (typeof (ArgumentException), ExpectedMessage = "Too many values.\r\nParameter name: values")]
    public void TooManyValues ()
    {
      Constant constant = new Constant ("test");
      List<IEvaluation> evaluations = new List<IEvaluation> { constant };
      object[] values = new object[] { 1 };

      SelectedObjectActivator selectedObjectActivator = new SelectedObjectActivator (evaluations);
      selectedObjectActivator.CreateSelectedObject (values);
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "Evaluation type DummyEvaluation is not supported.")]
    public void NonSupportedEvaluations ()
    {
      var mockEvaluation = new DummyEvaluation ();
      List<IEvaluation> evaluations = new List<IEvaluation> { mockEvaluation };
      object[] values = new object[] { };

      SelectedObjectActivator selectedObjectActivator = new SelectedObjectActivator (evaluations);
      selectedObjectActivator.CreateSelectedObject (values);
    }
  }
}