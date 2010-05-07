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
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.SqlBackend.SqlPreparation;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Data.Linq.UnitTests.Linq.Core;

namespace Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlPreparation
{
  [TestFixture]
  public class SqlPreparationContextTest
  {
    private ISqlPreparationContext _context;
    private MainFromClause _source;
    private SqlTable _sqlTable;

    [SetUp]
    public void SetUp ()
    {
      _context = new SqlPreparationContext();
      _source = ExpressionHelper.CreateMainFromClause_Cook();
      var source = new UnresolvedTableInfo (typeof (int));
      _sqlTable = new SqlTable (source);
    }

    [Test]
    public void AddContextMapping ()
    {
      _context.AddContextMapping (new QuerySourceReferenceExpression(_source), new SqlTableReferenceExpression(_sqlTable));
      Assert.That (_context.QuerySourceMappingCount, Is.EqualTo (1));
    }

    [Test]
    public void GetContextMapping ()
    {
      var querySourceReferenceExpression = new QuerySourceReferenceExpression (_source);
      _context.AddContextMapping (querySourceReferenceExpression, new SqlTableReferenceExpression (_sqlTable));
      Assert.That (((SqlTableReferenceExpression) _context.GetContextMapping (querySourceReferenceExpression)).SqlTable, 
        Is.SameAs (_sqlTable));
    }

    [Test]
    public void TryGetContextMappingFromHierarchy ()
    {
      var querySourceReferenceExpression = new QuerySourceReferenceExpression (_source);
      _context.AddContextMapping (querySourceReferenceExpression, new SqlTableReferenceExpression (_sqlTable));
      Expression result = _context.TryGetContextMappingFromHierarchy (querySourceReferenceExpression);
      Assert.That (((SqlTableReferenceExpression) result).SqlTable, Is.SameAs (_sqlTable));
    }

    [Test]
    [ExpectedException (typeof (KeyNotFoundException), ExpectedMessage =
        "The expression 'Cook' could not be found in the list of processed expressions. Probably, the feature declaring 'Cook' isn't supported yet.")]
    public void GetContextMapping_Throws_WhenExpressionNotAdded ()
    {
      _source = ExpressionHelper.CreateMainFromClause_Cook();
      _context.GetContextMapping (new QuerySourceReferenceExpression(_source));
    }

    [Test]
    public void TryGetContextMappingFromHierarchy_ReturnsFalseWhenSourceNotAdded ()
    {
      _source = ExpressionHelper.CreateMainFromClause_Cook ();
      Expression result = _context.TryGetContextMappingFromHierarchy (new QuerySourceReferenceExpression(_source));
      
      Assert.That (result, Is.Null);
    }
  }
}