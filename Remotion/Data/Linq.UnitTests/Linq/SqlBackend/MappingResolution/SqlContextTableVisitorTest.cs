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
using NUnit.Framework;
using Remotion.Data.Linq.SqlBackend.MappingResolution;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Data.Linq.UnitTests.Linq.Core.TestDomain;
using Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlStatementModel;
using Rhino.Mocks;
using NUnit.Framework.SyntaxHelpers;

namespace Remotion.Data.Linq.UnitTests.Linq.SqlBackend.MappingResolution
{
  [TestFixture]
  public class SqlContextTableVisitorTest
  {
    private IMappingResolutionStage _stageMock;

    [SetUp]
    public void SetUp ()
    {
      _stageMock = MockRepository.GenerateMock<IMappingResolutionStage> ();
    }

    [Test]
    public void ApplyContext_SqlStatementNotChanged_SameTableInfo ()
    {
      var subStatement = SqlStatementModelObjectMother.CreateSqlStatement_Resolved (typeof (IQueryable<>).MakeGenericType (typeof (Cook)));
      var subStatementTableInfo = new ResolvedSubStatementTableInfo ("c", subStatement);
      var sqlTable = new SqlTable (subStatementTableInfo);

      _stageMock
          .Expect (mock => mock.ApplyContext (subStatement, SqlExpressionContext.ValueRequired))
          .Return (subStatement);
      _stageMock.Replay ();

      SqlContextTableVisitor.ApplyContext (sqlTable, SqlExpressionContext.ValueRequired, _stageMock);

      Assert.That (sqlTable.TableInfo, Is.SameAs (subStatementTableInfo));
      _stageMock.VerifyAllExpectations ();
    }

    [Test]
    public void ApplyContext_SqlStatementChanged_NewTableInfo ()
    {
      var subStatement = SqlStatementModelObjectMother.CreateSqlStatement_Resolved (typeof (IQueryable<>).MakeGenericType (typeof (Cook)));
      var subStatementTableInfo = new ResolvedSubStatementTableInfo ("c", subStatement);
      var sqlTable = new SqlTable (subStatementTableInfo);
      var returnedStatement = SqlStatementModelObjectMother.CreateSqlStatement_Resolved (typeof (IQueryable<>).MakeGenericType (typeof (Cook)));
     
      _stageMock
          .Expect (mock => mock.ApplyContext (subStatement, SqlExpressionContext.ValueRequired))
          .Return (returnedStatement);
      _stageMock.Replay();

      SqlContextTableVisitor.ApplyContext (sqlTable, SqlExpressionContext.ValueRequired, _stageMock);

      Assert.That (sqlTable.TableInfo, Is.Not.SameAs (subStatementTableInfo));
      _stageMock.VerifyAllExpectations();
    }
  }
}