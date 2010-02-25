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
      _studentMember = typeof (Student_Detail).GetProperty ("Cook");
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

      Table relatedTable = StubDatabaseInfo.Instance.GetTableForRelation (_studentMember, null);
      var singleJoin = StubDatabaseInfo.Instance.GetJoinForMember (_studentMember, _initialTable, relatedTable);

      var expected = new FieldSourcePath (_initialTable, new[] { singleJoin });
      Assert.That (result, Is.EqualTo (expected));
    }

    [Test]
    public void BuildFieldSourcePath_NestedJoin ()
    {
      var joinMembers = new MemberInfo[] { _studentDetailMember, _studentMember };
      FieldSourcePath result =
          new FieldSourcePathBuilder ().BuildFieldSourcePath (StubDatabaseInfo.Instance, _context, _initialTable, joinMembers);

      Table relatedTable1 = StubDatabaseInfo.Instance.GetTableForRelation (_studentDetailMember, null);
      var singleJoin1 = StubDatabaseInfo.Instance.GetJoinForMember (_studentDetailMember, _initialTable, relatedTable1);

      Table relatedTable2 = StubDatabaseInfo.Instance.GetTableForRelation (_studentMember, null);
      var singleJoin2 = StubDatabaseInfo.Instance.GetJoinForMember (_studentMember, relatedTable1, relatedTable2);

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
