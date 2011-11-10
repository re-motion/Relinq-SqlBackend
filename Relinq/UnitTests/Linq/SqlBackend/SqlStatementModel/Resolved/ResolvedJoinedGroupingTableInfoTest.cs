// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (C) rubicon IT GmbH, www.rubicon.eu
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
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Linq.Clauses.StreamedData;
using Remotion.Linq.SqlBackend.MappingResolution;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Linq.UnitTests.Linq.Core.TestDomain;
using Rhino.Mocks;

namespace Remotion.Linq.UnitTests.Linq.SqlBackend.SqlStatementModel.Resolved
{
  [TestFixture]
  public class ResolvedJoinedGroupingTableInfoTest
  {
    private SqlStatement _sqlStatement;
    private ResolvedJoinedGroupingTableInfo _tableInfo;

    [SetUp]
    public void SetUp ()
    {
      _sqlStatement = new SqlStatementBuilder (SqlStatementModelObjectMother.CreateSqlStatement_Resolved (typeof (Cook)))
      {
        SelectProjection = new NamedExpression ("test", Expression.Constant (5)),
        DataInfo = new StreamedSequenceInfo (typeof (int[]), Expression.Constant (0))
      }.GetSqlStatement ();

      _tableInfo = new ResolvedJoinedGroupingTableInfo (
          "q0",
          _sqlStatement,
          SqlStatementModelObjectMother.CreateSqlGroupingSelectExpression(),
          "q1");
    }

    [Test]
    public void To_String ()
    {
      Assert.That (_tableInfo.ToString(), Is.EqualTo ("JOINED-GROUPING([q1], (" + _tableInfo.SqlStatement + ") [q0])"));
    }

    [Test]
    public void Accept ()
    {
      var tableInfoVisitorMock = MockRepository.GenerateMock<ITableInfoVisitor>();
      tableInfoVisitorMock.Expect (mock => mock.VisitJoinedGroupingTableInfo (_tableInfo));

      tableInfoVisitorMock.Replay();
      _tableInfo.Accept (tableInfoVisitorMock);

      tableInfoVisitorMock.VerifyAllExpectations();
    }

    [Test]
    public void GetResolvedTableInfo ()
    {
      var result = _tableInfo.GetResolvedTableInfo();

      Assert.That (result, Is.SameAs (_tableInfo));
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
  }
}