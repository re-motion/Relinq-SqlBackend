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
    [Test]
    public void Add_SimpleJoin()
    {
      Table leftSide = new Table ("left", null);
      Table rightSide = new Table ("right", null);
      JoinTree joinTree = new JoinTree(leftSide, rightSide, new Column(), new Column());

      JoinCollection collection = new JoinCollection();
      collection.Add (joinTree);

      Assert.AreEqual (1, collection.Count);
      Assert.That (collection[rightSide], Is.EqualTo(new object[] { joinTree }));
    }

    [Test]
    public void Add_NestedJoin ()
    {
      Table leftSide = new Table ("left", null);
      Table rightSide = new Table ("right", null);
      JoinTree innerJoinTree = new JoinTree (leftSide, rightSide, new Column (), new Column ());

      Table outerLeftSide = new Table ("outerLeft", null);
      JoinTree outerJoinTree = new JoinTree(outerLeftSide, innerJoinTree, new Column(), new Column());

      JoinCollection collection = new JoinCollection ();
      collection.Add (outerJoinTree);

      Assert.AreEqual (1, collection.Count);
      Assert.That (collection[rightSide], Is.EqualTo (new object[] { outerJoinTree }));
    }

    [Test]
    public void Add_JoinTwice ()
    {
      Table leftSide = new Table ("left", null);
      Table rightSide = new Table ("right", null);
      JoinTree joinTree = new JoinTree (leftSide, rightSide, new Column (), new Column ());

      JoinCollection collection = new JoinCollection ();
      collection.Add (joinTree);
      collection.Add (joinTree);

      Assert.AreEqual (1, collection.Count);
      Assert.That (collection[rightSide], Is.EqualTo (new object[] { joinTree }));
    }

    [Test]
    public void Add_TwoJoinsSameTable ()
    {
      Table leftSide1 = new Table ("left", null);
      Table rightSide = new Table ("right", null);
      JoinTree join1 = new JoinTree (leftSide1, rightSide, new Column (), new Column ());

      Table leftSide2 = new Table ("left2", null);
      JoinTree join2 = new JoinTree (leftSide2, rightSide, new Column (), new Column ());

      JoinCollection collection = new JoinCollection ();
      collection.Add (join1);
      collection.Add (join2);

      Assert.AreEqual (1, collection.Count);
      Assert.That (collection[rightSide], Is.EqualTo (new object[] { join1, join2 }));
    }

    [Test]
    public void Add_TwoJoinsDifferentTable ()
    {
      Table leftSide1 = new Table ("left", null);
      Table rightSide1 = new Table ("right", null);
      JoinTree join1 = new JoinTree (leftSide1, rightSide1, new Column (), new Column ());

      Table leftSide2 = new Table ("left2", null);
      Table rightSide2 = new Table ("right2", null);
      JoinTree join2 = new JoinTree (leftSide2, rightSide2, new Column (), new Column ());

      JoinCollection collection = new JoinCollection ();
      collection.Add (join1);
      collection.Add (join2);

      Assert.AreEqual (2, collection.Count);
      Assert.That (collection[rightSide1], Is.EqualTo (new object[] { join1 }));
      Assert.That (collection[rightSide2], Is.EqualTo (new object[] { join2 }));
    }
  }
}