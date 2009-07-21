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
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq;
using Remotion.Data.Linq.Backend;
using Remotion.Data.Linq.Backend.DataObjectModel;
using Rhino.Mocks;

namespace Remotion.Data.UnitTests.Linq.Backend.DataObjectModel
{
  [TestFixture]
  public class ContainsCriterionTest
  {
    private SubQuery _subQuery;
    private IEvaluation _item;
    private ContainsCriterion _containsCriterion;

    [SetUp]
    public void SetUp ()
    {
      QueryModel queryModel = ExpressionHelper.CreateQueryModel ();
      _subQuery = new SubQuery (queryModel, ParseMode.SubQueryInSelect, null);
      _item = new Constant (1);
      _containsCriterion = new ContainsCriterion (_subQuery, _item);
    }

    [Test]
    [ExpectedException (typeof (ArgumentException))]
    public void Initialization_WithNonNullAlias ()
    {
      var subQuery = new SubQuery (ExpressionHelper.CreateQueryModel(), ParseMode.SubQueryInSelect, "foo");
      new ContainsCriterion (subQuery, _item);
    }

    [Test]
    public void Equals_True ()
    {
      var otherCriterion = new ContainsCriterion (new SubQuery (_subQuery.QueryModel, _subQuery.ParseMode, _subQuery.Alias), new Constant (1));
      Assert.That (otherCriterion, Is.EqualTo (_containsCriterion));
    }

    [Test]
    public void Equals_False ()
    {
      var otherCriterion1 = new ContainsCriterion (new SubQuery (ExpressionHelper.CreateQueryModel(), _subQuery.ParseMode, null), new Constant (1));
      var otherCriterion2 = new ContainsCriterion (new SubQuery (_subQuery.QueryModel, _subQuery.ParseMode, null), new Constant (2));

      Assert.That (otherCriterion1, Is.Not.EqualTo (_containsCriterion));
      Assert.That (otherCriterion2, Is.Not.EqualTo (_containsCriterion));
    }

    [Test]
    public void GetHashCode_EqualObjects ()
    {
      var otherCriterion = new ContainsCriterion (new SubQuery (_subQuery.QueryModel, _subQuery.ParseMode, _subQuery.Alias), new Constant (1));
      Assert.That (otherCriterion.GetHashCode (), Is.EqualTo (_containsCriterion.GetHashCode ()));
    }

    [Test]
    public void Accept ()
    {
      var visitorMock = MockRepository.GenerateMock<IEvaluationVisitor>();
      _containsCriterion.Accept (visitorMock);

      visitorMock.AssertWasCalled (mock => mock.VisitContainsCriterion (_containsCriterion));
    }
  }
}
