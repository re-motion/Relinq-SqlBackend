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
using Remotion.Data.Linq.SqlBackend.SqlPreparation;
using Remotion.Data.Linq.SqlBackend.SqlPreparation.MethodCallTransformers;
using Rhino.Mocks;

namespace Remotion.Data.Linq.UnitTests.Linq.SqlBackend.SqlPreparation
{
  [TestFixture]
  public class MethodCallTransformerRegistryTest
  {
    [Test]
    public void CreateDefault ()
    {
      var registry = MethodCallTransformerRegistry.CreateDefault();

      Assert.That (registry.Registries.Length, Is.EqualTo (2));
      Assert.That (registry.Registries[0], Is.TypeOf (typeof (MethodInfoBasedMethodCallTransformerRegistry)));
      Assert.That (registry.Registries[1], Is.TypeOf (typeof (NameBasedMethodCallTransformerRegistry)));
    }

    [Test]
    public void GetTransformer ()
    {
      var registryMock = MockRepository.GenerateStrictMock<IMethodCallTransformerRegistry>();
      var methodCallTransformerRegistry = new MethodCallTransformerRegistry (registryMock);
      var methodCallExpression = Expression.Call (
          Expression.Constant ("test"),
          typeof (string).GetMethod ("Concat", new[] { typeof (string), typeof (string) }),
          Expression.Constant ("a"),
          Expression.Constant ("b"));
      var fakeTransformer = new ContainsFulltextMethodCallTransformer();

      registryMock
          .Expect (mock => mock.GetTransformer (methodCallExpression))
          .Return (fakeTransformer);
      registryMock.Replay();

      var result = methodCallTransformerRegistry.GetTransformer (methodCallExpression);

      registryMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (fakeTransformer));
    }
  }
}