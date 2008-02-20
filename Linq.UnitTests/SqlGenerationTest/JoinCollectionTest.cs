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
    public void AddTree_SimpleJoin()
    {
      Table leftSide = new Table ("left", null);
      Table rightSide = new Table ("right", null);
      JoinTree joinTree = new JoinTree(leftSide, rightSide, new Column(leftSide, "a"), new Column(rightSide, "b"));

      JoinCollection collection = new JoinCollection();
      collection.AddTree (joinTree);

      Assert.AreEqual (1, collection.Count);
      Assert.That (collection[rightSide], Is.EqualTo(new object[] { joinTree.GetSingleJoinForRoot() }));
    }

    [Test]
    public void AddTree_NestedJoin ()
    {
      Table leftSide = new Table ("left", null);
      Table rightSide = new Table ("right", null);
      JoinTree innerJoinTree = new JoinTree (leftSide, rightSide, new Column (leftSide, "a"), new Column (rightSide, "b"));

      Table outerLeftSide = new Table ("outerLeft", null);
      JoinTree outerJoinTree = new JoinTree(outerLeftSide, innerJoinTree, new Column(outerLeftSide, "c"), new Column(leftSide, "d"));

      JoinCollection collection = new JoinCollection ();
      collection.AddTree (outerJoinTree);

      Assert.That (collection[rightSide], Is.EqualTo (new object[] { outerJoinTree.GetSingleJoinForRoot(), innerJoinTree.GetSingleJoinForRoot() }));
    }

    [Test]
    public void AddTree_JoinTwice ()
    {
      Table leftSide = new Table ("left", null);
      Table rightSide = new Table ("right", null);
      JoinTree joinTree = new JoinTree (leftSide, rightSide, new Column (leftSide, "a"), new Column (rightSide, "b"));

      JoinCollection collection = new JoinCollection ();
      collection.AddTree (joinTree);
      collection.AddTree (joinTree);

      Assert.AreEqual (1, collection.Count);
      Assert.That (collection[rightSide], Is.EqualTo (new object[] { joinTree.GetSingleJoinForRoot() }));
    }

    [Test]
    public void AddTree_TwoJoinsSameTable ()
    {
      Table leftSide1 = new Table ("left", null);
      Table rightSide = new Table ("right", null);
      JoinTree join1 = new JoinTree (leftSide1, rightSide, new Column (leftSide1, "a"), new Column (rightSide, "b"));

      Table leftSide2 = new Table ("left2", null);
      JoinTree join2 = new JoinTree (leftSide2, rightSide, new Column (leftSide2, "a"), new Column (rightSide, "b"));

      JoinCollection collection = new JoinCollection ();
      collection.AddTree (join1);
      collection.AddTree (join2);

      Assert.AreEqual (1, collection.Count);
      Assert.That (collection[rightSide], Is.EqualTo (new object[] { join1.GetSingleJoinForRoot(), join2.GetSingleJoinForRoot() }));
    }

    [Test]
    public void AddTree_TwoJoinsDifferentTable ()
    {
      Table leftSide1 = new Table ("left", null);
      Table rightSide1 = new Table ("right", null);
      JoinTree join1 = new JoinTree (leftSide1, rightSide1, new Column (leftSide1, "a"), new Column (rightSide1, "b"));

      Table leftSide2 = new Table ("left2", null);
      Table rightSide2 = new Table ("right2", null);
      JoinTree join2 = new JoinTree (leftSide2, rightSide2, new Column (leftSide2, "a"), new Column (rightSide2, "b"));

      JoinCollection collection = new JoinCollection ();
      collection.AddTree (join1);
      collection.AddTree (join2);

      Assert.AreEqual (2, collection.Count);
      Assert.That (collection[rightSide1], Is.EqualTo (new object[] { join1.GetSingleJoinForRoot() }));
      Assert.That (collection[rightSide2], Is.EqualTo (new object[] { join2.GetSingleJoinForRoot() }));
    }

    [Test]
    public void AddTree_DifferentJoins_SameParts ()
    {
      Table leftSide1 = new Table ("left", null);
      Table rightSide = new Table ("right", null);
      JoinTree innerJoin = new JoinTree (leftSide1, rightSide, new Column (leftSide1, "a"), new Column (rightSide, "b"));

      Table leftSide2a = new Table ("left2a", null);
      Table leftSide2b = new Table ("left2b", null);
      JoinTree outerJoinA = new JoinTree (leftSide2a, innerJoin, new Column (leftSide2a, "c"), new Column (leftSide1, "d"));
      JoinTree outerJoinB = new JoinTree (leftSide2b, innerJoin, new Column (leftSide2b, "e"), new Column (leftSide1, "f"));

      JoinCollection collection = new JoinCollection ();
      collection.AddTree (outerJoinA);
      collection.AddTree (outerJoinB);

      SingleJoin singleJoin1 = outerJoinA.GetSingleJoinForRoot();
      SingleJoin singleJoin2 = innerJoin.GetSingleJoinForRoot ();
      SingleJoin singleJoin3 = outerJoinB.GetSingleJoinForRoot ();

      Assert.That (collection[rightSide], Is.EqualTo (new object[] { singleJoin1, singleJoin2, singleJoin3 }));
    }
  }
}