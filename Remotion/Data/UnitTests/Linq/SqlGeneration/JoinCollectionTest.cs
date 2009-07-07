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
using NUnit.Framework;
using Remotion.Data.Linq.Backend.DataObjectModel;
using Remotion.Data.Linq.SqlGeneration;
using NUnit.Framework.SyntaxHelpers;

namespace Remotion.Data.UnitTests.Linq.SqlGeneration
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
      SingleJoin join = new SingleJoin (new Column (_initialTable, "b"), new Column (table1, "a"));
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
      SingleJoin join1 = new SingleJoin (new Column (_initialTable, "b"), new Column (table1, "a"));

      Table table2 = new Table ("outerLeft", null);
      SingleJoin join2 = new SingleJoin (new Column (table1, "d"), new Column (table2, "c"));

      FieldSourcePath path = new FieldSourcePath (_initialTable, new[] { join1, join2 });

      JoinCollection collection = new JoinCollection ();
      collection.AddPath (path);

      Assert.That (collection[_initialTable], Is.EqualTo (new object[] { join1, join2 }));
    }

    [Test]
    public void AddPath_PathTwice ()
    {
      Table table1 = new Table ("left", null);
      SingleJoin join1 = new SingleJoin (new Column (_initialTable, "b"), new Column (table1, "a"));

      Table table2 = new Table ("right", null);
      SingleJoin join2 = new SingleJoin (new Column (table1, "a"), new Column (table2, "a"));

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
      Table relatedTable1 = new Table ("related1", null);
      SingleJoin join1 = new SingleJoin (new Column (_initialTable, "b"), new Column (relatedTable1, "a"));

      Table relatedTable2 = new Table ("related2", null);
      SingleJoin join2 = new SingleJoin (new Column (_initialTable, "b"), new Column (relatedTable2, "a"));

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
      Table relatedTable1 = new Table ("related1", null);
      SingleJoin join1 = new SingleJoin (new Column (_initialTable, "b"), new Column (relatedTable1, "a"));
      
      Table initialTable2 = new Table ("initialTable2", "i2");
      Table relatedTable2 = new Table ("related2", null);
      SingleJoin join2 = new SingleJoin (new Column (initialTable2, "b"), new Column (relatedTable2, "a"));

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
      Table relatedTable1 = new Table ("related1", null);
      SingleJoin join1 = new SingleJoin (new Column (_initialTable, "b"), new Column (relatedTable1, "a"));

      Table relatedTable2a = new Table ("related2a", null);
      Table relatedTable2b = new Table ("related2b", null);
      SingleJoin join2A = new SingleJoin (new Column (relatedTable1, "d"), new Column (relatedTable2a, "c"));
      SingleJoin join2B = new SingleJoin (new Column (relatedTable1, "f"), new Column (relatedTable2b, "e"));

      FieldSourcePath path1 = new FieldSourcePath(_initialTable, new[] {join1,join2A});
      FieldSourcePath path2 = new FieldSourcePath (_initialTable, new[] { join1, join2B });

      JoinCollection collection = new JoinCollection ();
      collection.AddPath (path1);
      collection.AddPath (path2);
      
      Assert.That (collection[_initialTable], Is.EqualTo (new object[] { join1,join2A,join2B }));
    }

    [Test]
    public void Count()
    {
      Table relatedTable1 = new Table ("related1", null);
      SingleJoin join1a = new SingleJoin (new Column (_initialTable, "b"), new Column (relatedTable1, "a"));
      SingleJoin join1b = new SingleJoin (new Column (_initialTable, "c"), new Column (relatedTable1, "d"));

      Table initialTable2 = new Table ("initial2", null);
      Table relatedTable2 = new Table ("related2", null);
      SingleJoin join2 = new SingleJoin (new Column (initialTable2, "b"), new Column (relatedTable2, "a"));

      JoinCollection joins = new JoinCollection();
      Assert.AreEqual (0, joins.Count);

      joins.AddPath (new FieldSourcePath(_initialTable, new[] {join1a}));
      Assert.AreEqual (1, joins.Count);

      joins.AddPath (new FieldSourcePath (_initialTable, new[] { join1b }));
      Assert.AreEqual (1, joins.Count);

      joins.AddPath (new FieldSourcePath (initialTable2, new[] { join2 }));
      Assert.AreEqual (2, joins.Count);
    }

    [Test]
    public void Item ()
    {
      Table relatedTable1 = new Table ("related1", null);
      SingleJoin join1a = new SingleJoin (new Column (_initialTable, "b"), new Column (relatedTable1, "a"));
      SingleJoin join1b = new SingleJoin (new Column (_initialTable, "c"), new Column (relatedTable1, "d"));

      Table initialTable2 = new Table ("initial2", null);
      Table relatedTable2 = new Table ("related2", null);
      SingleJoin join2 = new SingleJoin (new Column (initialTable2, "b"), new Column (relatedTable2, "a"));

      JoinCollection joins = new JoinCollection ();
      joins.AddPath (new FieldSourcePath (_initialTable, new[] { join1a }));
      joins.AddPath (new FieldSourcePath (_initialTable, new[] { join1b }));
      joins.AddPath (new FieldSourcePath (initialTable2, new[] { join2 }));

      List<SingleJoin> item1 = joins[_initialTable];
      Assert.That (item1, Is.EqualTo (new[] {join1a, join1b}));

      List<SingleJoin> item2 = joins[initialTable2];
      Assert.That (item2, Is.EqualTo (new[] { join2 }));
    }

    [Test]
    public void GetEnumerator_Generic ()
    {
      Table relatedTable1 = new Table ("related1", null);
      SingleJoin join1a = new SingleJoin (new Column (_initialTable, "b"), new Column (relatedTable1, "a"));
      SingleJoin join1b = new SingleJoin (new Column (_initialTable, "c"), new Column (relatedTable1, "d"));

      Table initialTable2 = new Table ("initial2", null);
      Table relatedTable2 = new Table ("related2", null);
      SingleJoin join2 = new SingleJoin (new Column (initialTable2, "b"), new Column (relatedTable2, "a"));

      JoinCollection joins = new JoinCollection ();
      joins.AddPath (new FieldSourcePath (_initialTable, new[] { join1a }));
      joins.AddPath (new FieldSourcePath (_initialTable, new[] { join1b }));
      joins.AddPath (new FieldSourcePath (initialTable2, new[] { join2 }));

      List<KeyValuePair<IColumnSource, List<SingleJoin>>> list = new List<KeyValuePair<IColumnSource, List<SingleJoin>>> ();
      foreach (KeyValuePair<IColumnSource, List<SingleJoin>> item in joins)
        list.Add (item);

      Assert.AreEqual (2, list.Count);
      Assert.IsTrue (list[0].Key == _initialTable || list[1].Key == _initialTable);
      Assert.IsTrue (list[0].Key == initialTable2 || list[1].Key == initialTable2);

      List<SingleJoin> itemsForInitialTable = list[0].Key == _initialTable ? list[0].Value : list[1].Value;
      List<SingleJoin> itemsForInitialTable2 = list[0].Key == initialTable2 ? list[0].Value : list[1].Value;

      Assert.That (itemsForInitialTable, Is.EqualTo (new[] { join1a, join1b }));
      Assert.That (itemsForInitialTable2, Is.EqualTo (new[] { join2 }));
    }
  }
}
