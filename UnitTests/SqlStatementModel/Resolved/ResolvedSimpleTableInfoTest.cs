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
using Moq;
using NUnit.Framework;
using Remotion.Linq.SqlBackend.MappingResolution;
using Remotion.Linq.SqlBackend.SqlStatementModel;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Linq.SqlBackend.UnitTests.TestDomain;

namespace Remotion.Linq.SqlBackend.UnitTests.SqlStatementModel.Resolved
{
  [TestFixture]
  public class ResolvedSimpleTableInfoTest
  {
    private ResolvedSimpleTableInfo _tableInfo;

    [SetUp]
    public void SetUp ()
    {
      _tableInfo = SqlStatementModelObjectMother.CreateResolvedTableInfo ();
    }

    [Test]
    public void Accept ()
    {
      var tableInfoVisitorMock = new Mock<ITableInfoVisitor>();
      tableInfoVisitorMock.Setup (mock => mock.VisitSimpleTableInfo (_tableInfo)).Verifiable();

      _tableInfo.Accept (tableInfoVisitorMock.Object);

      tableInfoVisitorMock.Verify();
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
      var fakeResult = SqlStatementModelObjectMother.CreateSqlEntityDefinitionExpression (typeof (Cook));

      var generator = new UniqueIdentifierGenerator();
      var resolverMock = new Mock<IMappingResolver> (MockBehavior.Strict);
      var mappingResolutionContext = new MappingResolutionContext();

      resolverMock
          .Setup (mock => mock.ResolveSimpleTableInfo (_tableInfo))
          .Returns (fakeResult)
          .Verifiable();

      var result = _tableInfo.ResolveReference (sqlTable, resolverMock.Object, mappingResolutionContext, generator);

      resolverMock.Verify();
      Assert.That (result, Is.SameAs (fakeResult));
      Assert.That (mappingResolutionContext.GetSqlTableForEntityExpression ((SqlEntityExpression) result), Is.SameAs (sqlTable));
    }

    [Test]
    public new void ToString ()
    {
      var tableInfo = new ResolvedSimpleTableInfo (typeof (Cook), "CookTable", "t0");
      var result = tableInfo.ToString ();
      
      Assert.That (result, Is.EqualTo ("[CookTable] [t0]"));
    }
  }
}