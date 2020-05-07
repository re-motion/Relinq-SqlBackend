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
using NUnit.Framework;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.SqlBackend.SqlPreparation;
using Remotion.Linq.SqlBackend.SqlPreparation.ResultOperatorHandlers;
using Remotion.Linq.SqlBackend.UnitTests.SqlPreparation.ResultOperatorHandlers;
using Moq;

namespace Remotion.Linq.SqlBackend.UnitTests.SqlPreparation
{
  [TestFixture]
  public class ResultOperatorHandlerRegistryTest
  {
    [Test]
    public void CreateDefault ()
    {
      var registry = ResultOperatorHandlerRegistry.CreateDefault ();

      Assert.That (registry.GetItem(typeof (CastResultOperator)), Is.TypeOf (typeof (CastResultOperatorHandler)));
      Assert.That (registry.GetItem(typeof (ContainsResultOperator)), Is.TypeOf (typeof (ContainsResultOperatorHandler)));
      Assert.That (registry.GetItem(typeof (CountResultOperator)), Is.TypeOf (typeof (CountResultOperatorHandler)));
      Assert.That (registry.GetItem(typeof (DistinctResultOperator)), Is.TypeOf (typeof (DistinctResultOperatorHandler)));
      Assert.That (registry.GetItem(typeof (FirstResultOperator)), Is.TypeOf (typeof (FirstResultOperatorHandler)));
      Assert.That (registry.GetItem(typeof (OfTypeResultOperator)), Is.TypeOf (typeof (OfTypeResultOperatorHandler)));
      Assert.That (registry.GetItem(typeof (SingleResultOperator)), Is.TypeOf (typeof (SingleResultOperatorHandler)));
      Assert.That (registry.GetItem(typeof (TakeResultOperator)), Is.TypeOf (typeof (TakeResultOperatorHandler)));
      Assert.That (registry.GetItem(typeof (AnyResultOperator)), Is.TypeOf (typeof (AnyResultOperatorHandler)));
    }

    [Test]
    public void GetItem ()
    {
      var registry = new ResultOperatorHandlerRegistry();
      var handlerMock = new Mock<IResultOperatorHandler>();
      registry.Register (typeof (CastResultOperator), handlerMock.Object);

      var result = registry.GetItem(typeof (CastResultOperator));

      Assert.That (result, Is.SameAs(handlerMock.Object));
    }

    [Test]
    public void GetItem_ForBaseResultOperator ()
    {
      var registry = new ResultOperatorHandlerRegistry ();
      var handlerMock = new Mock<IResultOperatorHandler>();
      registry.Register (typeof (ResultOperatorBase), handlerMock.Object);

      var result = registry.GetItem (typeof (InheritedResultOperator));

      Assert.That (result, Is.SameAs (handlerMock.Object));
    }

    [Test]
    public void GetItem_NotFound ()
    {
      var registry = new ResultOperatorHandlerRegistry ();

      var result = registry.GetItem (typeof (InheritedResultOperator));

      Assert.That (result, Is.Null);
    }

    class InheritedResultOperator : TestChoiceResultOperator 
    {
      public InheritedResultOperator (bool returnDefaultWhenEmpty)
          : base(returnDefaultWhenEmpty)
      {
      }
    }
  }
}