// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (c) rubicon IT GmbH, www.rubicon.eu
// 
// re-linq is free software; you can redistribute it and/or modify it under 
// the terms of the GNU Lesser General Public License as published by the 
// Free Software Foundation; either version 2.1 of the License, 
// or (at your option) any later version.
// 
// re-linq is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-linq; if not, see http://www.gnu.org/licenses.
// 
using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Linq.SqlBackend.MappingResolution;
using Remotion.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Linq.UnitTests.Linq.Core.TestDomain;
using Remotion.Linq.Clauses.StreamedData;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Rhino.Mocks;

namespace Remotion.Linq.UnitTests.Linq.SqlBackend.SqlStatementModel.Resolved
{
  [TestFixture]
  public class ResolvedSubStatementTableInfoTest
  {
    private ResolvedSubStatementTableInfo _tableInfo;
    private SqlStatement _sqlStatement;

    [SetUp]
    public void SetUp ()
    {
      _sqlStatement = new SqlStatementBuilder (SqlStatementModelObjectMother.CreateSqlStatement_Resolved (typeof (Cook)))
                         {
                             SelectProjection = new NamedExpression ("test", Expression.Constant (5)),
                             DataInfo = new StreamedSequenceInfo (typeof (int[]), Expression.Constant (0))
                         }.GetSqlStatement();
      _tableInfo = new ResolvedSubStatementTableInfo ("c", _sqlStatement);
    }

    [Test]
    public void Initialization_ItemType ()
    {
      Assert.That (_tableInfo.ItemType, Is.EqualTo (typeof (int)));
    }

    [Test]
    public void Initialization_ItemTypeWithCovariantSubstatement ()
    {
      var sqlStatement = new SqlStatementBuilder (SqlStatementModelObjectMother.CreateSqlStatement_Resolved (typeof (Cook)))
      {
        SelectProjection = new NamedExpression ("test", Expression.Constant (5)),
        DataInfo = new StreamedSequenceInfo (typeof (object[]), Expression.Constant (0))
      }.GetSqlStatement ();
      var tableInfo = new ResolvedSubStatementTableInfo ("c", sqlStatement);

      Assert.That (tableInfo.ItemType, Is.EqualTo (typeof (object)));
    }

    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void Initialization_NoSequenceData_ThrowsExeption ()
    {
      var sqlStatement = new SqlStatementBuilder ()
      {
        SelectProjection = Expression.Constant (new Cook ()),
        DataInfo = new StreamedScalarValueInfo(typeof(Cook))
      }.GetSqlStatement ();
      
      new ResolvedSubStatementTableInfo ("c", sqlStatement);
    }

    [Test]
    public void ResolveReference ()
    {
      var sqlTable = new SqlTable (_tableInfo, JoinSemantics.Inner);

      var generator = new UniqueIdentifierGenerator ();
      var resolverMock = MockRepository.GenerateStrictMock<IMappingResolver> ();
      var mappingResolutionContext = new MappingResolutionContext ();

      resolverMock.Replay ();

      var result = _tableInfo.ResolveReference (sqlTable, resolverMock, mappingResolutionContext, generator);

      Assert.That (result, Is.TypeOf (typeof (SqlColumnDefinitionExpression)));
      Assert.That (((SqlColumnDefinitionExpression) result).ColumnName, Is.EqualTo ("test"));
      Assert.That (((SqlColumnDefinitionExpression) result).OwningTableAlias, Is.EqualTo (_tableInfo.TableAlias));
      Assert.That (result.Type, Is.EqualTo (typeof (int)));
    }

     [Test]
    public new void ToString ()
    {
      var sqlStatement = new SqlStatementBuilder ()
      {
        SelectProjection = Expression.Constant (new Cook ()),
        DataInfo = new StreamedSequenceInfo (typeof (IQueryable<int>), Expression.Constant (0))
      }.GetSqlStatement ();
      var subStatementInfo = new ResolvedSubStatementTableInfo ("c", sqlStatement);

      var result = subStatementInfo.ToString ();
      Assert.That (result, Is.EqualTo ("(" + sqlStatement + ") [c]"));
    }
  }
}