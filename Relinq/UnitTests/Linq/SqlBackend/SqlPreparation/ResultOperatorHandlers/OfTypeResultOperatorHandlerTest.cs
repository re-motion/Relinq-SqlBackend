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
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Linq.UnitTests.Linq.Core.Parsing;
using Remotion.Linq.UnitTests.Linq.Core.TestDomain;
using Remotion.Linq.UnitTests.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq;
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.Clauses.StreamedData;
using Remotion.Linq.SqlBackend.SqlPreparation;
using Remotion.Linq.SqlBackend.SqlPreparation.ResultOperatorHandlers;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;

namespace Remotion.Linq.UnitTests.Linq.SqlBackend.SqlPreparation.ResultOperatorHandlers
{
  [TestFixture]
  public class OfTypeResultOperatorHandlerTest
  {
    private ISqlPreparationStage _stageMock;
    private UniqueIdentifierGenerator _generator;
    private OfTypeResultOperatorHandler _handler;
    private SqlStatementBuilder _sqlStatementBuilder;
    private ISqlPreparationContext _context;

    [SetUp]
    public void SetUp ()
    {
      _generator = new UniqueIdentifierGenerator ();
      _stageMock = new DefaultSqlPreparationStage (
          CompoundMethodCallTransformerProvider.CreateDefault(), ResultOperatorHandlerRegistry.CreateDefault(), _generator);
      _handler = new OfTypeResultOperatorHandler ();
      _sqlStatementBuilder = new SqlStatementBuilder (SqlStatementModelObjectMother.CreateSqlStatementWithCook());
      _context = SqlStatementModelObjectMother.CreateSqlPreparationContext ();
    }

    [Test]
    public void HandleResultOperator ()
    {
      var originalSelectProjection = _sqlStatementBuilder.SelectProjection;
      var resultOperator = new OfTypeResultOperator (typeof (Chef));

      _handler.HandleResultOperator (resultOperator, _sqlStatementBuilder, _generator, _stageMock, _context);

      Assert.That (_sqlStatementBuilder.WhereCondition, Is.TypeOf (typeof (TypeBinaryExpression)));
      Assert.That (((TypeBinaryExpression) _sqlStatementBuilder.WhereCondition).TypeOperand, Is.EqualTo (typeof (Chef)));

      var expectedSelectProjection = Expression.Convert (originalSelectProjection, typeof (Chef));
      ExpressionTreeComparer.CheckAreEqualTrees (expectedSelectProjection, _sqlStatementBuilder.SelectProjection);
      
      Assert.That (_sqlStatementBuilder.DataInfo, Is.TypeOf (typeof (StreamedSequenceInfo)));
      Assert.That (((StreamedSequenceInfo) _sqlStatementBuilder.DataInfo).DataType, Is.EqualTo (typeof (IQueryable<>).MakeGenericType (typeof (Chef))));
    }

    [Test]
    public void HandleResultOperator_AfterGroupExpression_CreatesSubStatement ()
    {
      _sqlStatementBuilder.GroupByExpression = Expression.Constant ("group");

      var resultOperator = new OfTypeResultOperator(typeof(Chef));

      _handler.HandleResultOperator(resultOperator, _sqlStatementBuilder, _generator, _stageMock, _context);

      Assert.That (_sqlStatementBuilder.SqlTables.Count, Is.EqualTo (1));
      Assert.That (((SqlTable) _sqlStatementBuilder.SqlTables[0]).TableInfo, Is.TypeOf (typeof (ResolvedSubStatementTableInfo)));
    }
  }
}