using System;
using NUnit.Framework;
using Rubicon.Data.Linq.DataObjectModel;
using Rubicon.Data.Linq.SqlGeneration;
using NUnit.Framework.SyntaxHelpers;

namespace Rubicon.Data.Linq.UnitTests.SqlGenerationTest
{
  [TestFixture]
  public class JoinCollectionTest
  {
    private Table _initialTable;

    [SetUp]
    public void SetUp()
    {
      _initialTable = new Table("initial", "i");
    }

    [Test]
    public void AddPath_SimpleJoin ()
    {
      Table table1 = new Table ("left", null);
      SingleJoin join = new SingleJoin (new Column (table1, "a"), new Column (_initialTable, "b"));
      FieldSourcePath path = new FieldSourcePath(_initialTable, new[] { join });

      JoinCollection collection = new JoinCollection ();
      collection.AddPath (path);

      Assert.AreEqual (1, collection.Count);
      Assert.That (collection[_initialTable], Is.EqualTo (new object[] { join }));
    }

    [Test]
    public void AddPath_NestedJoin ()
    {
      Table table1 = new Table ("left", null);
      SingleJoin join1 = new SingleJoin (new Column (table1, "a"), new Column (_initialTable, "b"));

      Table table2 = new Table ("outerLeft", null);
      SingleJoin join2 = new SingleJoin (new Column (table2, "c"), new Column (table1, "d"));

      FieldSourcePath path = new FieldSourcePath (_initialTable, new[] { join1, join2 });

      JoinCollection collection = new JoinCollection ();
      collection.AddPath (path);

      Assert.That (collection[_initialTable], Is.EqualTo (new object[] { join1, join2 }));
    }

    [Test]
    public void AddPath_PathTwice ()
    {
      Table table1 = new Table ("left", null);
      SingleJoin join1 = new SingleJoin (new Column (table1, "a"), new Column (_initialTable, "b"));

      Table table2 = new Table ("right", null);
      SingleJoin join2 = new SingleJoin (new Column (table2, "a"), new Column (table1, "a"));

      FieldSourcePath path = new FieldSourcePath(_initialTable, new[] { join1, join2});

      JoinCollection collection = new JoinCollection ();
      collection.AddPath (path);
      collection.AddPath (path);

      Assert.AreEqual (1, collection.Count);
      Assert.That (collection[_initialTable], Is.EqualTo (new object[] { join1, join2 }));
    }

    [Test]
    public void AddPath_TwoJoinsSameTable ()
    {
      Table leftSide1 = new Table ("left", null);
      SingleJoin join1 = new SingleJoin (new Column (leftSide1, "a"), new Column (_initialTable, "b"));

      Table leftSide2 = new Table ("left2", null);
      SingleJoin join2 = new SingleJoin (new Column (leftSide2, "a"), new Column (_initialTable, "b"));

      FieldSourcePath path1 = new FieldSourcePath (_initialTable, new[] { join1 });
      FieldSourcePath path2 = new FieldSourcePath (_initialTable, new[] { join2 });

      JoinCollection collection = new JoinCollection ();
      collection.AddPath (path1);
      collection.AddPath (path2);

      Assert.AreEqual (1, collection.Count);
      Assert.That (collection[_initialTable], Is.EqualTo (new object[] { join1, join2 }));
    }

    [Test]
    public void AddPath_TwoPathsDifferentSourceTables ()
    {
      Table leftSide1 = new Table ("left", null);
      SingleJoin join1 = new SingleJoin (new Column (leftSide1, "a"), new Column (_initialTable, "b"));
      
      Table initialTable2 = new Table ("initialTable2", "i2");
      Table leftSide2 = new Table ("left2", null);
      SingleJoin join2 = new SingleJoin (new Column (leftSide2, "a"), new Column (initialTable2, "b"));

      FieldSourcePath path1 = new FieldSourcePath (_initialTable, new[] { join1 });
      FieldSourcePath path2 = new FieldSourcePath (initialTable2, new[] { join2 });

      JoinCollection collection = new JoinCollection ();
      collection.AddPath (path1);
      collection.AddPath (path2);

      Assert.AreEqual (2, collection.Count);
      Assert.That (collection[_initialTable], Is.EqualTo (new object[] { join1 }));
      Assert.That (collection[initialTable2], Is.EqualTo (new object[] { join2 }));
    }

    [Test]
    public void AddPath_DifferentPaths_EqualJoins ()
    {
      Table leftSide1 = new Table ("left", null);
      SingleJoin join1 = new SingleJoin (new Column (leftSide1, "a"), new Column (_initialTable, "b"));

      Table leftSide2a = new Table ("left2a", null);
      Table leftSide2b = new Table ("left2b", null);
      SingleJoin join2A = new SingleJoin (new Column (leftSide2a, "c"), new Column (leftSide1, "d"));
      SingleJoin join2B = new SingleJoin (new Column (leftSide2b, "e"), new Column (leftSide1, "f"));

      FieldSourcePath path1 = new FieldSourcePath(_initialTable, new[] {join1,join2A});
      FieldSourcePath path2 = new FieldSourcePath (_initialTable, new[] { join1, join2B });

      JoinCollection collection = new JoinCollection ();
      collection.AddPath (path1);
      collection.AddPath (path2);
      
      Assert.That (collection[_initialTable], Is.EqualTo (new object[] { join1,join2A,join2B }));
    }
  }
}