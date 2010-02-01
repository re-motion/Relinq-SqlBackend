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
using System.Reflection;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Backend;
using Remotion.Data.Linq.Backend.DataObjectModel;
using Remotion.Data.Linq.Backend.FieldResolving;
using Remotion.Data.Linq.UnitTests.TestDomain;

namespace Remotion.Data.Linq.UnitTests.Backend.FieldResolving
{
  [TestFixture]
  public class FieldSourcePathBuilderTest
  {
    private JoinedTableContext _context;
    private Table _initialTable;
    private PropertyInfo _studentDetailMember;
    private PropertyInfo _studentMember;

    [SetUp]
    public void SetUp()
    {
      _context = new JoinedTableContext (StubDatabaseInfo.Instance);
      _initialTable = new Table ("initial", "i");

      _studentDetailMember = typeof (Student_Detail_Detail).GetProperty ("Student_Detail");
      _studentMember = typeof (Student_Detail).GetProperty ("Student");
    }

    [Test]
    public void BuildFieldSourcePath_NoJoin ()
    {
      var joinMembers = new MemberInfo[] { };
      FieldSourcePath result =
          new FieldSourcePathBuilder ().BuildFieldSourcePath (StubDatabaseInfo.Instance, _context, _initialTable, joinMembers);

      var expected = new FieldSourcePath(_initialTable, new SingleJoin[0]);
      Assert.That (result, Is.EqualTo (expected));
    }

    [Test]
    public void BuildFieldSourcePath_SimpleJoin ()
    {
      var joinMembers = new MemberInfo[] { _studentMember };
      FieldSourcePath result =
          new FieldSourcePathBuilder ().BuildFieldSourcePath (StubDatabaseInfo.Instance, _context, _initialTable, joinMembers);

      Table relatedTable = ((IDatabaseInfo) StubDatabaseInfo.Instance).GetTableForRelation (_studentMember, null);
      var joinColumns = ((IDatabaseInfo) StubDatabaseInfo.Instance).GetJoinColumnNames (_studentMember);

      var singleJoin = new SingleJoin (new Column (_initialTable, joinColumns.PrimaryKey), new Column (relatedTable, joinColumns.ForeignKey));
      var expected = new FieldSourcePath (_initialTable, new[] { singleJoin });
      Assert.That (result, Is.EqualTo (expected));
    }

    [Test]
    public void BuildFieldSourcePath_NestedJoin ()
    {
      var joinMembers = new MemberInfo[] { _studentDetailMember, _studentMember };
      FieldSourcePath result =
          new FieldSourcePathBuilder ().BuildFieldSourcePath (StubDatabaseInfo.Instance, _context, _initialTable, joinMembers);

      Table relatedTable1 = ((IDatabaseInfo) StubDatabaseInfo.Instance).GetTableForRelation (_studentDetailMember, null);
      var joinColumns1 = ((IDatabaseInfo) StubDatabaseInfo.Instance).GetJoinColumnNames (_studentDetailMember);

      Table relatedTable2 = ((IDatabaseInfo) StubDatabaseInfo.Instance).GetTableForRelation (_studentMember, null);
      var joinColumns2 = ((IDatabaseInfo) StubDatabaseInfo.Instance).GetJoinColumnNames (_studentMember);

      var singleJoin1 = new SingleJoin (new Column (_initialTable, joinColumns1.PrimaryKey), new Column (relatedTable1, joinColumns1.ForeignKey));
      var singleJoin2 = new SingleJoin (new Column (relatedTable1, joinColumns2.PrimaryKey), new Column (relatedTable2, joinColumns2.ForeignKey));
      var expected = new FieldSourcePath (_initialTable, new[] { singleJoin1, singleJoin2 });
      Assert.That (result, Is.EqualTo (expected));
    }

    [Test]
    public void BuildFieldSourcePath_UsesContext ()
    {
      Assert.That (_context.Count, Is.EqualTo (0));
      BuildFieldSourcePath_SimpleJoin ();
      Assert.That (_context.Count, Is.EqualTo (1));
    }

  }
}
