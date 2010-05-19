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
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.SqlBackend.MappingResolution;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Data.Linq.UnitTests.Linq.Core.TestDomain;

namespace Remotion.Data.Linq.UnitTests.Linq.SqlBackend.MappingResolution
{
  [TestFixture]
  public class MappingResolutionContextTest
  {
    private SqlEntityExpression _entityExpression;
    private MappingResolutionContext _context;
    private SqlTable _sqlTable;

    [SetUp]
    public void SetUp ()
    {
      _context = new MappingResolutionContext();
      _entityExpression = new SqlEntityDefinitionExpression (typeof (Cook), "c", null, new SqlColumnDefinitionExpression (typeof (string), "c", "Name", false));
      _sqlTable = new SqlTable (new ResolvedSimpleTableInfo (typeof (Cook), "CookTable", "c"));
    }

    [Test]
    public void AddMapping_EntityExists ()
    {
      _context.AddSqlEntityMapping (_entityExpression, _sqlTable);

      Assert.That (_context.GetSqlTableForEntityExpression (_entityExpression), Is.SameAs (_sqlTable));
    }

    [Test]
    public void GetSqlTableForEntityExpression_EntityDoesNotExist ()
    {
      Assert.That (_context.GetSqlTableForEntityExpression (_entityExpression), Is.Null);
    }
  }
}