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
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.SqlBackend.SqlPreparation;
using Remotion.Linq.SqlBackend.SqlPreparation.MethodCallTransformers;
using Remotion.Linq.SqlBackend.UnitTests.TestDomain;

namespace Remotion.Linq.SqlBackend.UnitTests.SqlPreparation
{
  [TestFixture]
  public class CompoundMethodCallTransformerProviderTest
  {
    [Test]
    public void CreateDefault ()
    {
      var registry = CompoundMethodCallTransformerProvider.CreateDefault();

      Assert.That (registry.Providers.Length, Is.EqualTo (2));
      Assert.That (registry.Providers[0], Is.TypeOf (typeof (MethodInfoBasedMethodCallTransformerRegistry)));
      Assert.That (registry.Providers[1], Is.TypeOf (typeof (NameBasedMethodCallTransformerRegistry)));
    }

    [Test]
    public void GetTransformer ()
    {
      var registryMock = new Mock<IMethodCallTransformerProvider> (MockBehavior.Strict);
      var methodCallTransformerRegistry = new CompoundMethodCallTransformerProvider (registryMock.Object);
      var methodCallExpression = ExpressionHelper.CreateMethodCallExpression<Cook>();
      var fakeTransformer = new ContainsFulltextMethodCallTransformer();

      registryMock
          .Setup (mock => mock.GetTransformer (methodCallExpression))
          .Returns (fakeTransformer)
          .Verifiable();

      var result = methodCallTransformerRegistry.GetTransformer (methodCallExpression);

      registryMock.Verify();
      Assert.That (result, Is.SameAs (fakeTransformer));
    }
  }
}