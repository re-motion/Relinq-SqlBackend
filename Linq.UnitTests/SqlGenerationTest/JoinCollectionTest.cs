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
      Join join = new Join(leftSide, rightSide, new Column(), new Column());

      JoinCollection collection = new JoinCollection();
      collection.Add (join);

      Assert.AreEqual (1, collection.Count);
      Assert.That (collection[rightSide], Is.EqualTo(new object[] { join }));
    }

    [Test]
    public void Add_NestedJoin ()
    {
      Table leftSide = new Table ("left", null);
      Table rightSide = new Table ("right", null);
      Join innerJoin = new Join (leftSide, rightSide, new Column (), new Column ());

      Table outerLeftSide = new Table ("outerLeft", null);
      Join outerJoin = new Join(outerLeftSide, innerJoin, new Column(), new Column());

      JoinCollection collection = new JoinCollection ();
      collection.Add (outerJoin);

      Assert.AreEqual (1, collection.Count);
      Assert.That (collection[rightSide], Is.EqualTo (new object[] { outerJoin }));
    }

    [Test]
    public void Add_JoinTwice ()
    {
      Table leftSide = new Table ("left", null);
      Table rightSide = new Table ("right", null);
      Join join = new Join (leftSide, rightSide, new Column (), new Column ());

      JoinCollection collection = new JoinCollection ();
      collection.Add (join);
      collection.Add (join);

      Assert.AreEqual (1, collection.Count);
      Assert.That (collection[rightSide], Is.EqualTo (new object[] { join }));
    }

    [Test]
    public void Add_TwoJoinsSameTable ()
    {
      Table leftSide1 = new Table ("left", null);
      Table rightSide = new Table ("right", null);
      Join join1 = new Join (leftSide1, rightSide, new Column (), new Column ());

      Table leftSide2 = new Table ("left2", null);
      Join join2 = new Join (leftSide2, rightSide, new Column (), new Column ());

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
      Join join1 = new Join (leftSide1, rightSide1, new Column (), new Column ());

      Table leftSide2 = new Table ("left2", null);
      Table rightSide2 = new Table ("right2", null);
      Join join2 = new Join (leftSide2, rightSide2, new Column (), new Column ());

      JoinCollection collection = new JoinCollection ();
      collection.Add (join1);
      collection.Add (join2);

      Assert.AreEqual (2, collection.Count);
      Assert.That (collection[rightSide1], Is.EqualTo (new object[] { join1 }));
      Assert.That (collection[rightSide2], Is.EqualTo (new object[] { join2 }));
    }
  }
}