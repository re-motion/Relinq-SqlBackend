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
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.IntegrationTests.TestDomain.Northwind;
using Remotion.Data.Linq.IntegrationTests.Utilities;
using Remotion.Data.Linq.SqlBackend.MappingResolution;
using Remotion.Data.Linq.SqlBackend.SqlGeneration;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Rhino.Mocks;

namespace Remotion.Data.Linq.IntegrationTests.UnitTests.Utilities
{
  [TestFixture]
  public class RelinqQueryExecutorTest
  {
    private MainFromClause _mainFromClause;
    private SelectClause _selectClause;
    private QueryModel _queryModel;
    private IMappingResolver _resolverStub;

    [SetUp]
    public void SetUp ()
    {
      // var query = from c in Customers select null
      _mainFromClause = new MainFromClause ("c", typeof (Customer), Expression.Constant (new Customer[0]));
      _selectClause = new SelectClause (Expression.Constant (null, typeof (Customer)));
      _queryModel = new QueryModel (_mainFromClause, _selectClause);

      _resolverStub = MockRepository.GenerateStub<IMappingResolver> ();
      _resolverStub
          .Stub (stub => stub.ResolveTableInfo (Arg<UnresolvedTableInfo>.Is.Anything, Arg<UniqueIdentifierGenerator>.Is.Anything))
          .Return (new ResolvedSimpleTableInfo (typeof (Customer), "CustomerTable", "t0"));
      _resolverStub
          .Stub (stub => stub.ResolveConstantExpression ((ConstantExpression) _selectClause.Selector))
          .Return (_selectClause.Selector);
    }

    [Test]
    public void ExecuteCollection ()
    {
      var fakeResult = new[] { new Customer(), new Customer() };
      
      var retrieverMock = MockRepository.GenerateStrictMock<IQueryResultRetriever>();
      retrieverMock
          .Expect (stub => stub.GetResults (
              Arg<Func<IDatabaseResultRow, Customer>>.Is.Anything, 
              Arg.Is ("SELECT NULL AS [value] FROM [CustomerTable] AS [t0]"), 
              Arg<CommandParameter[]>.List.Equal (new CommandParameter[0])))
          .Return (fakeResult);
      retrieverMock.Replay();

      var executor = new RelinqQueryExecutor (retrieverMock, _resolverStub);
      var result = executor.ExecuteCollection<Customer> (_queryModel);

      retrieverMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (fakeResult));
    }
  }
}