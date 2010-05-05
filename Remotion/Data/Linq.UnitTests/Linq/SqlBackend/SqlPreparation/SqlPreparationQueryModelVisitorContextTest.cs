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
using System.Collections.Generic;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.SqlBackend.SqlPreparation;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Data.Linq.UnitTests.Linq.Core;
using Rhino.Mocks;

namespace Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlPreparation
{
  [TestFixture]
  public class SqlPreparationQueryModelVisitorContextTest
  {
    private ISqlPreparationContext _parentContext;
    private MainFromClause _parentSource;
    private SqlTable _parentSqlTable;

    private ISqlPreparationContext _context;
    private MainFromClause _source;
    private SqlTable _sqlTable;

    private ISqlPreparationStage _stageMock = MockRepository.GenerateMock<ISqlPreparationStage>();
    private SqlPreparationQueryModelVisitor _visitor;

    [SetUp]
    public void SetUp ()
    {
      _parentContext = new SqlPreparationContext();
      _parentSource = ExpressionHelper.CreateMainFromClause_Cook();
      _parentSqlTable = new SqlTable (new UnresolvedTableInfo (typeof (int)));
      
      _visitor = new TestableSqlPreparationQueryModelVisitor (
          _parentContext, _stageMock, new UniqueIdentifierGenerator(), ResultOperatorHandlerRegistry.CreateDefault());
      _context = new SqlPreparationQueryModelVisitorContext (_parentContext, _visitor);

      _source = ExpressionHelper.CreateMainFromClause_Cook ();
      _sqlTable = new SqlTable (new UnresolvedTableInfo (typeof (int)));
    }

    [Test]
    public void AddQuerySourceMapping ()
    {
      _context.AddQuerySourceMapping (_source, _sqlTable);
      Assert.That (_context.QuerySourceMappingCount, Is.EqualTo (1));
    }

    [Test]
    public void GetSqlTableForQuerySource ()
    {
      _context.AddQuerySourceMapping (_source, _sqlTable);
      Assert.That (_context.GetSqlTableForQuerySource (_source), Is.SameAs (_sqlTable));
    }

    [Test]
    public void GetSqlTableForQuerySource_GetFromParentContext ()
    {
      _parentContext.AddQuerySourceMapping (_parentSource, _parentSqlTable);
      Assert.That (_context.GetSqlTableForQuerySource (_parentSource), Is.SameAs (_parentSqlTable));
    }

    [Test]
    [ExpectedException (typeof (KeyNotFoundException), ExpectedMessage =
        "The query source 's' (MainFromClause) could not be found in the list of processed query sources. Probably, the feature declaring 's' isn't "
        + "supported yet.")]
    public void GetSqlTableForQuerySource_Throws_WhenSourceNotAdded ()
    {
      _context.GetSqlTableForQuerySource (_source);
    }
  }
}