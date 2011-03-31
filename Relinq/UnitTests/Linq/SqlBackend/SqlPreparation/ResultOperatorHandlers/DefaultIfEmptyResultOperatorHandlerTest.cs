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
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Linq.UnitTests.Linq.Core.TestDomain;
using Remotion.Linq.UnitTests.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq;
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.Clauses.StreamedData;
using Remotion.Linq.SqlBackend.SqlPreparation;
using Remotion.Linq.SqlBackend.SqlPreparation.ResultOperatorHandlers;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions;

namespace Remotion.Linq.UnitTests.Linq.SqlBackend.SqlPreparation.ResultOperatorHandlers
{
  [TestFixture]
  public class DefaultIfEmptyResultOperatorHandlerTest
  {
    private ISqlPreparationStage _stage;
    private UniqueIdentifierGenerator _generator;
    private DefaultIfEmptyResultOperatorHandler _handler;
    private SqlStatementBuilder _sqlStatementBuilder;
    private ISqlPreparationContext _context;

    [SetUp]
    public void SetUp ()
    {
      _generator = new UniqueIdentifierGenerator ();
      _stage = new DefaultSqlPreparationStage (
          CompoundMethodCallTransformerProvider.CreateDefault(), ResultOperatorHandlerRegistry.CreateDefault(), _generator);
      _handler = new DefaultIfEmptyResultOperatorHandler();
      _sqlStatementBuilder = new SqlStatementBuilder (SqlStatementModelObjectMother.CreateSqlStatement())
                             {
                                 DataInfo = new StreamedSequenceInfo (typeof (Cook[]), Expression.Constant (new Cook()))
                             };
      _context = SqlStatementModelObjectMother.CreateSqlPreparationContext ();
    }

    [Test]
    public void HandleResultOperator ()
    {
      var resultOperator = new DefaultIfEmptyResultOperator (Expression.Constant (null));

      _handler.HandleResultOperator (resultOperator, _sqlStatementBuilder, _generator, _stage, _context);

      Assert.That (_sqlStatementBuilder.SqlTables.Count, Is.EqualTo (1));
      Assert.That (_sqlStatementBuilder.SqlTables[0], Is.TypeOf (typeof (SqlJoinedTable)));
      Assert.That (((SqlJoinedTable) _sqlStatementBuilder.SqlTables[0]).JoinSemantics, Is.EqualTo (JoinSemantics.Left));
      
      var joinInfo = (ResolvedJoinInfo)((SqlJoinedTable) _sqlStatementBuilder.SqlTables[0]).JoinInfo;
      Assert.That (joinInfo, Is.TypeOf (typeof (ResolvedJoinInfo)));
      Assert.That (joinInfo.ForeignTableInfo, Is.TypeOf (typeof (ResolvedSubStatementTableInfo)));
      Assert.That (joinInfo.LeftKey, Is.TypeOf (typeof (SqlLiteralExpression)));
      Assert.That (((SqlLiteralExpression) joinInfo.LeftKey).Value, Is.EqualTo(1));
      Assert.That (joinInfo.RightKey, Is.TypeOf(typeof (SqlLiteralExpression)));
      Assert.That (((SqlLiteralExpression) joinInfo.RightKey).Value, Is.EqualTo (1));
      Assert.That (joinInfo.ForeignTableInfo, Is.TypeOf (typeof (ResolvedSubStatementTableInfo))); // moved to sub-statement
      Assert.That (_context.GetExpressionMapping (((StreamedSequenceInfo) _sqlStatementBuilder.DataInfo).ItemExpression), Is.Not.Null);
    }
  }
}