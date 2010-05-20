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
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Clauses.StreamedData;
using Remotion.Data.Linq.SqlBackend.SqlPreparation;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Data.Linq.UnitTests.Linq.Core.TestDomain;
using Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlStatementModel;
using Rhino.Mocks;

namespace Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlPreparation
{
  [TestFixture]
  public class SqlPreparationFromExpressionVisitorTest
  {
    private ISqlPreparationStage _stageMock;
    private UniqueIdentifierGenerator _generator;
    private SqlPreparationContext _context;
    private MethodCallTransformerRegistry _registry;

    [SetUp]
    public void SetUp ()
    {
      _stageMock = MockRepository.GenerateMock<ISqlPreparationStage>();
      _generator = new UniqueIdentifierGenerator();
      _context = new SqlPreparationContext();
      _registry = MethodCallTransformerRegistry.CreateDefault();
    }

    [Test]
    public void GetTableForFromExpression_ConstantExpression_ReturnsUnresolvedTable ()
    {
      var expression = Expression.Constant (new Cook[0]);

      var result = SqlPreparationFromExpressionVisitor.GetTableForFromExpression (
          expression, new MainFromClause ("c", typeof (Cook), Expression.Constant (new Cook())), _stageMock, _generator, _registry, _context);

      Assert.That (result, Is.TypeOf (typeof (SqlTable)));

      var tableInfo = ((SqlTable) result).TableInfo;
      Assert.That (tableInfo, Is.TypeOf (typeof (UnresolvedTableInfo)));

      Assert.That (tableInfo.ItemType, Is.SameAs (typeof (Cook)));
    }

    [Test]
    public void GetTableForFromExpression_SqlMemberExpression_ReturnsJoinedTable ()
    {
      // from r in Restaurant => sqlTable 
      // from c in r.Cooks => MemberExpression (QSRExpression (r), "Cooks") => Join: sqlTable.Cooks

      var memberInfo = typeof (Restaurant).GetProperty ("Cooks");
      var sqlTable = SqlStatementModelObjectMother.CreateSqlTable (memberInfo.DeclaringType);
      var memberExpression = Expression.MakeMemberAccess (Expression.Constant (new Restaurant()), memberInfo);

      var result = SqlPreparationFromExpressionVisitor.GetTableForFromExpression (
          memberExpression, new MainFromClause ("c", typeof (Cook), Expression.Constant (new Cook())), _stageMock, _generator, _registry, _context);

      Assert.That (result, Is.TypeOf (typeof (SqlJoinedTable)));
      Assert.That (sqlTable.JoinedTables.ToArray().Contains (result), Is.False);
      Assert.That (((SqlJoinedTable) result).JoinSemantics, Is.EqualTo (JoinSemantics.Inner));

      var joinInfo = ((SqlJoinedTable) result).JoinInfo;

      Assert.That (joinInfo, Is.TypeOf (typeof (UnresolvedCollectionJoinInfo)));

      Assert.That (((UnresolvedCollectionJoinInfo) joinInfo).MemberInfo, Is.EqualTo (memberInfo));
      Assert.That (joinInfo.ItemType, Is.SameAs (typeof (Cook)));
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage =
        "Expressions of type 'CustomExpression' cannot be used as the SqlTables of a from clause.")]
    public void GetTableForFromExpression_UnsupportedExpression_Throws ()
    {
      var customExpression = new CustomExpression (typeof (Cook[]));

      SqlPreparationFromExpressionVisitor.GetTableForFromExpression (
          customExpression, new MainFromClause ("c", typeof (Cook), Expression.Constant (new Cook())), _stageMock, _generator, _registry, _context);
    }

    [ExpectedException (typeof (NotSupportedException))]
    [Test]
    public void VisitEntityRefMemberExpression_ThrowsNotSupportException ()
    {
      var memberInfo = typeof (Restaurant).GetProperty ("Cooks");
      var entityExpression = new SqlEntityDefinitionExpression (
          typeof (Cook), "c", null, new SqlColumnDefinitionExpression (typeof (string), "c", "Name", false));
      var expression = new SqlEntityRefMemberExpression (entityExpression, memberInfo);

      SqlPreparationFromExpressionVisitor.GetTableForFromExpression (
          expression, new MainFromClause ("c", typeof (Cook), Expression.Constant (new Cook())), _stageMock, _generator, _registry, _context);
    }

    [Test]
    public void VisitSqlSubStatementExpression ()
    {
      var sqlStatement = new SqlStatementBuilder (SqlStatementModelObjectMother.CreateSqlStatement_Resolved (typeof (Cook[])))
                         {
                             DataInfo = new StreamedSequenceInfo (typeof (IQueryable<Cook>), Expression.Constant (new Cook()))
                         }.GetSqlStatement();

      var sqlSubStatementExpression = new SqlSubStatementExpression (sqlStatement);

      var result = (SqlTable) SqlPreparationFromExpressionVisitor.GetTableForFromExpression (
          sqlSubStatementExpression,
          new MainFromClause ("c", typeof (Cook), Expression.Constant (new Cook ())),
          _stageMock,
          _generator,
          _registry,
          _context);

      Assert.That (result.TableInfo, Is.InstanceOfType (typeof (ResolvedSubStatementTableInfo)));
      var condition = (ResolvedSubStatementTableInfo) result.TableInfo;
      Assert.That (condition.SqlStatement, Is.EqualTo (sqlStatement));
      Assert.That (condition.TableAlias, Is.EqualTo ("q0"));
      Assert.That (condition.ItemType, Is.EqualTo (typeof (Cook)));
    }

    [Test]
    public void VisitMemberExpression ()
    {
      var memberExpression = Expression.MakeMemberAccess (Expression.Constant (new Cook()), typeof (Cook).GetProperty ("IllnessDays"));
      var result = SqlPreparationFromExpressionVisitor.GetTableForFromExpression (
          memberExpression, new MainFromClause ("c", typeof (Cook), Expression.Constant (new Cook ())), _stageMock, _generator, _registry, _context);

      Assert.That (result, Is.TypeOf (typeof (SqlJoinedTable)));
      Assert.That (((SqlJoinedTable) result).JoinInfo, Is.TypeOf (typeof (UnresolvedCollectionJoinInfo)));
      Assert.That (((UnresolvedCollectionJoinInfo) ((SqlJoinedTable) result).JoinInfo).SourceExpression, Is.EqualTo (memberExpression.Expression));
      Assert.That (((UnresolvedCollectionJoinInfo) ((SqlJoinedTable) result).JoinInfo).MemberInfo, Is.EqualTo (memberExpression.Member));
    }

    [Test]
    public void VisitSqlTableReferenceExpression ()
    {
      var memberInfo = typeof (Restaurant).GetProperty ("Cooks");
      var sqlTable = SqlStatementModelObjectMother.CreateSqlTable (memberInfo.DeclaringType);
      var expression = new SqlTableReferenceExpression (sqlTable);

      var result = SqlPreparationFromExpressionVisitor.GetTableForFromExpression (
          expression, new MainFromClause ("c", typeof (Cook), Expression.Constant (new Cook ())), _stageMock, _generator, _registry, _context);

      Assert.That (result, Is.SameAs (sqlTable));
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException))]
    public void VisitSqlEntityRefMemberExpression ()
    {
      var memberInfo = typeof (Restaurant).GetProperty ("Cooks");
      var entityExpression = new SqlEntityDefinitionExpression (
          typeof (Cook), "c", null, new SqlColumnDefinitionExpression (typeof (string), "c", "Name", false));
      var expression = new SqlEntityRefMemberExpression (entityExpression, memberInfo);

      SqlPreparationFromExpressionVisitor.GetTableForFromExpression (expression, new MainFromClause ("c", typeof (Cook), Expression.Constant (new Cook ())), _stageMock, _generator, _registry, _context);
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException))]
    public void VisitSqlEntityConstantExpression ()
    {
      var expression = new SqlEntityConstantExpression (typeof (Cook), "test", "test");

      SqlPreparationFromExpressionVisitor.GetTableForFromExpression (expression, new MainFromClause ("c", typeof (Cook), Expression.Constant (new Cook ())), _stageMock, _generator, _registry, _context);
    }
  }
}