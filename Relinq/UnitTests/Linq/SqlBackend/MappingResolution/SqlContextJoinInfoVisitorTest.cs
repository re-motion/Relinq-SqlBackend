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
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.SqlBackend.MappingResolution;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Data.Linq.UnitTests.Linq.Core.TestDomain;
using Rhino.Mocks;

namespace Remotion.Data.Linq.UnitTests.Linq.SqlBackend.MappingResolution
{
  [TestFixture]
  public class SqlContextJoinInfoVisitorTest
  {
    private IMappingResolutionStage _stageMock;
    private IMappingResolutionContext _mappingResolutionContext;

    [SetUp]
    public void SetUp ()
    {
      _mappingResolutionContext = new MappingResolutionContext();
      _stageMock = MockRepository.GenerateMock<IMappingResolutionStage> ();
    }

    [Test]
    public void ApplyContext_VisitUnresolvedJoinInfo ()
    {
      var tableInfo = new ResolvedSimpleTableInfo (typeof (Cook), "CookTable", "c");
      var sqlTable = new SqlTable (tableInfo, JoinSemantics.Inner);
      var entityExpression = new SqlEntityDefinitionExpression (typeof (Cook), "c", null, new SqlColumnDefinitionExpression (typeof (string), "c", "Name", false));
      var unresolvedJoinInfo = new UnresolvedJoinInfo (entityExpression, typeof (Cook).GetProperty ("ID"), JoinCardinality.One);
      
      var result = SqlContextJoinInfoVisitor.ApplyContext (unresolvedJoinInfo, SqlExpressionContext.ValueRequired, _stageMock, _mappingResolutionContext);

      Assert.That (result, Is.SameAs (unresolvedJoinInfo));
    }

    [Test]
    public void ApplyContext_VisitUnresolvedCollectionJoinInfo ()
    {
      var unresolvedJoinInfo = new UnresolvedCollectionJoinInfo (Expression.Constant (new Cook ()), typeof (Cook).GetProperty ("IllnessDays"));
      
      var result = SqlContextJoinInfoVisitor.ApplyContext (unresolvedJoinInfo, SqlExpressionContext.ValueRequired, _stageMock, _mappingResolutionContext);

      Assert.That (result, Is.SameAs (unresolvedJoinInfo));
    }

    [Test]
    public void ApplyContext_VisitResolvedJoinInfo_SameForeignTableInfo ()
    {
      var tableInfo = new ResolvedSimpleTableInfo (typeof (Cook), "CookTable", "c");
      var resolvedJoinInfo = new ResolvedJoinInfo (tableInfo, new SqlColumnDefinitionExpression (typeof (int), "c", "ID", false), new SqlColumnDefinitionExpression (typeof (int), "r", "CookID", false));
      
      _stageMock
          .Expect (mock => mock.ApplyContext (resolvedJoinInfo.ForeignTableInfo, SqlExpressionContext.ValueRequired, _mappingResolutionContext))
          .Return (tableInfo);
      _stageMock.Replay ();

      var result = SqlContextJoinInfoVisitor.ApplyContext (resolvedJoinInfo, SqlExpressionContext.ValueRequired, _stageMock, _mappingResolutionContext);

      _stageMock.VerifyAllExpectations ();
      Assert.That (result, Is.SameAs (resolvedJoinInfo));
    }

    [Test]
    public void ApplyContext_VisitResolvedJoinInfo_NewForeignTableInfo ()
    {
      var tableInfo = new ResolvedSimpleTableInfo (typeof (Cook), "CookTable", "c");
      var resolvedJoinInfo = new ResolvedJoinInfo (tableInfo, new SqlColumnDefinitionExpression (typeof (int), "c", "ID", false), new SqlColumnDefinitionExpression (typeof (int), "r", "CookID", false));
      var fakeTableInfo = new ResolvedSimpleTableInfo (typeof (Restaurant), "RestaurantTable", "r");

      _stageMock
          .Expect (mock => mock.ApplyContext (resolvedJoinInfo.ForeignTableInfo, SqlExpressionContext.ValueRequired, _mappingResolutionContext))
          .Return (fakeTableInfo);
      _stageMock.Replay ();

      var result = SqlContextJoinInfoVisitor.ApplyContext (resolvedJoinInfo, SqlExpressionContext.ValueRequired, _stageMock, _mappingResolutionContext);

      _stageMock.VerifyAllExpectations ();
      Assert.That (((ResolvedJoinInfo) result).ForeignTableInfo, Is.SameAs (fakeTableInfo));
    }
  }
}