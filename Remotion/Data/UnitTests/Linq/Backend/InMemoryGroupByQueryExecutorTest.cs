// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// version 3.0 as published by the Free Software Foundation.
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
using System.Linq;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Collections;
using Remotion.Data.Linq;
using Remotion.Data.Linq.Backend;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Clauses.ResultOperators;
using Remotion.Data.Linq.EagerFetching;
using Remotion.Data.UnitTests.Linq.Parsing;
using Remotion.Utilities;
using Rhino.Mocks;

namespace Remotion.Data.UnitTests.Linq.Backend
{
  [TestFixture]
  public class InMemoryGroupByQueryExecutorTest
  {
    private IQueryExecutor _queryExecutorMock;
    private InMemoryGroupByQueryExecutor _inMemoryExecutor;
    private QueryModel _queryModel;

    [SetUp]
    public void SetUp ()
    {
      _queryExecutorMock = new MockRepository().StrictMock<IQueryExecutor> ();
      _inMemoryExecutor = new InMemoryGroupByQueryExecutor (_queryExecutorMock);

      var query = from s in ExpressionHelper.CreateQuerySource ()
                  group s.ID by s.Last;
      
      _queryModel = ExpressionHelper.ParseQuery (query.Expression);
    }

    [Test]
    [ExpectedException (typeof (ArgumentException))]
    public void ExecuteCollectionWithGrouping_Generic_WithSelectClause ()
    {
      _inMemoryExecutor.ExecuteCollectionWithGrouping<string, int> (ExpressionHelper.CreateQueryModel ());
    }

    [Test]
    public void ExecuteCollectionWithGrouping_Generic_ExecutesEquivalentSelectQuery ()
    {
      _queryExecutorMock
          .Expect (mock => mock.ExecuteCollection<Tuple<string, int>> (
              Arg<QueryModel>.Matches (qm => qm.MainFromClause.ToString () == _queryModel.MainFromClause.ToString ()),
              Arg<IEnumerable<FetchRequestBase>>.List.Count (Rhino.Mocks.Constraints.Is.Equal (0))))
          .Return (new Tuple<string, int>[0])
          .WhenCalled (mi =>
          {
            var qm = (QueryModel) mi.Arguments[0];
            var expectedSelector = ExpressionHelper.Resolve<Student, Tuple<string, int>> (
                qm.MainFromClause, s => new Tuple<string, int> (s.Last, s.ID));
            ExpressionTreeComparer.CheckAreEqualTrees (expectedSelector, ((SelectClause) qm.SelectOrGroupClause).Selector);
          });

      _queryExecutorMock.Replay ();

      _inMemoryExecutor.ExecuteCollectionWithGrouping<string, int> (_queryModel);

      _queryExecutorMock.VerifyAllExpectations ();
    }

    [Test]
    public void ExecuteCollectionWithGrouping_Generic_ExecutesEquivalentSelectQuery_WithClone ()
    {
      _queryExecutorMock
          .Expect (mock => mock.ExecuteCollection<Tuple<string, int>> (
              Arg<QueryModel>.Is.NotSame (_queryModel),
              Arg<IEnumerable<FetchRequestBase>>.Is.Anything))
          .Return (new Tuple<string, int>[0]);

      _queryExecutorMock.Replay ();

      _inMemoryExecutor.ExecuteCollectionWithGrouping<string, int> (_queryModel);

      _queryExecutorMock.VerifyAllExpectations ();
    }

    [Test]
    public void ExecuteCollectionWithGrouping_Generic_ExecutesEquivalentSelectQuery_WithoutResultOperators ()
    {
      _queryModel.ResultOperators.Add (new TakeResultOperator (2));

      _queryExecutorMock
          .Expect (mock => mock.ExecuteCollection<Tuple<string, int>> (
              Arg<QueryModel>.Matches (qm => qm.ResultOperators.Count == 0),
              Arg<IEnumerable<FetchRequestBase>>.Is.Anything))
          .Return (new Tuple<string, int>[0]);

      _queryExecutorMock.Replay ();

      _inMemoryExecutor.ExecuteCollectionWithGrouping<string, int> (_queryModel);

      _queryExecutorMock.VerifyAllExpectations ();
    }

    [Test]
    public void ExecuteCollectionWithGrouping_Generic_GroupsInMemory ()
    {
      _queryExecutorMock
          .Expect (mock => mock.ExecuteCollection<Tuple<string, int>> (
              Arg<QueryModel>.Is.Anything, 
              Arg<IEnumerable<FetchRequestBase>>.Is.Anything))
          .Return (new[] { Tuple.NewTuple ("Garcia", 1), Tuple.NewTuple ("Miller", 2), Tuple.NewTuple ("Garcia", 3), Tuple.NewTuple ("Johnson", 4)});

      _queryExecutorMock.Replay ();

      var result = _inMemoryExecutor.ExecuteCollectionWithGrouping<string, int> (_queryModel).ToArray();

      _queryExecutorMock.VerifyAllExpectations ();

      Assert.That (result.Length, Is.EqualTo (3));

      Assert.That (result[0].Key, Is.EqualTo ("Garcia"));
      Assert.That (result[0].ToArray(), Is.EqualTo (new[] { 1, 3 }));

      Assert.That (result[1].Key, Is.EqualTo ("Miller"));
      Assert.That (result[1].ToArray (), Is.EqualTo (new[] { 2 }));

      Assert.That (result[2].Key, Is.EqualTo ("Johnson"));
      Assert.That (result[2].ToArray (), Is.EqualTo (new[] { 4 }));
    }

    [Test]
    public void ExecuteCollectionWithGrouping_Generic_PerformsResultOperatorsInMemory ()
    {
      _queryModel.ResultOperators.Add (new TakeResultOperator (2));

      _queryExecutorMock
          .Expect (mock => mock.ExecuteCollection<Tuple<string, int>> (
              Arg<QueryModel>.Is.Anything,
              Arg<IEnumerable<FetchRequestBase>>.Is.Anything))
          .Return (new[] { Tuple.NewTuple ("Garcia", 1), Tuple.NewTuple ("Miller", 2), Tuple.NewTuple ("Garcia", 3), Tuple.NewTuple ("Johnson", 4) });
      _queryExecutorMock.Replay ();

      var result = _inMemoryExecutor.ExecuteCollectionWithGrouping<string, int> (_queryModel).ToArray();

      _queryExecutorMock.VerifyAllExpectations ();

      Assert.That (result.Length, Is.EqualTo (2));
    }

    [Test]
    [ExpectedException (typeof (NotSupportedException), ExpectedMessage = "ExecuteCollectionWithGrouping does not support scalar result operators, "
        + "found a 'CountResultOperator'.")]
    public void ExecuteCollectionWithGrouping_Generic_WithScalarResultOperator ()
    {
      _queryModel.ResultOperators.Add (new CountResultOperator ());

      _queryExecutorMock
          .Expect (mock => mock.ExecuteCollection<Tuple<string, int>> (
              Arg<QueryModel>.Is.Anything,
              Arg<IEnumerable<FetchRequestBase>>.Is.Anything))
          .Return (new[] { Tuple.NewTuple ("Garcia", 1), Tuple.NewTuple ("Miller", 2), Tuple.NewTuple ("Garcia", 3), Tuple.NewTuple ("Johnson", 4) });
      _queryExecutorMock.Replay ();

      _inMemoryExecutor.ExecuteCollectionWithGrouping<string, int> (_queryModel).ToArray ();

      _queryExecutorMock.VerifyAllExpectations ();
    }

    [Test]
    public void ExecuteCollection_NonGeneric_InfersTypes ()
    {
      _queryExecutorMock
          .Expect (mock => mock.ExecuteCollection<Tuple<string, int>> (
              Arg<QueryModel>.Is.Anything,
              Arg<IEnumerable<FetchRequestBase>>.Is.Anything))
          .Return (new[] { Tuple.NewTuple ("Garcia", 1), Tuple.NewTuple ("Miller", 2), Tuple.NewTuple ("Garcia", 3), Tuple.NewTuple ("Johnson", 4) });
      _queryExecutorMock.Replay ();

      var result = _inMemoryExecutor.ExecuteCollectionWithGrouping<IGrouping<string, int>> (_queryModel).ToArray ();

      _queryExecutorMock.VerifyAllExpectations ();

      Assert.That (result.Length, Is.EqualTo (3));
    }

    [Test]
    [ExpectedException (typeof (ArgumentException))]
    public void ExecuteCollection_NonGeneric_WithSelectClause ()
    {
      _inMemoryExecutor.ExecuteCollectionWithGrouping<IGrouping<string, int>> (ExpressionHelper.CreateQueryModel());
    }

    [Test]
    [ExpectedException (typeof (ArgumentTypeException), ExpectedMessage = "The query model returns groupings of type 'IGrouping<System.String, "
        + "System.Int32>', but 'System.Linq.IGrouping`2[[System.Double, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],"
        + "[System.Double, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]' was requested.\r\nParameter name: T")]
    public void ExecuteCollection_NonGeneric_InvalidTypeSpecified ()
    {
      _queryExecutorMock
          .Expect (mock => mock.ExecuteCollection<Tuple<string, int>> (
              Arg<QueryModel>.Is.Anything,
              Arg<IEnumerable<FetchRequestBase>>.Is.Anything))
          .Return (new Tuple<string, int>[0]);
      _queryExecutorMock.Replay ();

      _inMemoryExecutor.ExecuteCollectionWithGrouping<IGrouping<double, double>> (_queryModel);
    }

    
  }
}