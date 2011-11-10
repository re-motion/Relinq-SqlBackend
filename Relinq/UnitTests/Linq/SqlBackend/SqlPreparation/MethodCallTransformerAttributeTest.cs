// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (C) rubicon IT GmbH, www.rubicon.eu
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
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Linq.UnitTests.Linq.Core.TestDomain;
using Remotion.Linq.SqlBackend.SqlPreparation;

namespace Remotion.Linq.UnitTests.Linq.SqlBackend.SqlPreparation
{
  [TestFixture]
  public class MethodCallTransformerAttributeTest
  {
    [Test]
    public void GetTransformer ()
    {
      var attribute = new MethodCallTransformerAttribute (typeof (Cook.FullNameTransformer));
      var result = attribute.GetTransformer ();

      Assert.That (result, Is.Not.Null);
      Assert.That (result, Is.TypeOf (typeof (Cook.FullNameTransformer)));
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage =
        "The method call transformer "
        + "'Remotion.Linq.UnitTests.Linq.SqlBackend.SqlPreparation.MethodCallTransformerAttributeTest+TransformerWithoutPublicDefaultCtor' "
        + "has no public default constructor and therefore cannot be used with the MethodCallTransformerAttribute.")]
    public void GetTransformer_AttributedMethod_NoCtor ()
    {
      var attribute = new MethodCallTransformerAttribute (typeof (TransformerWithoutPublicDefaultCtor));
      attribute.GetTransformer ();
    }

    class TransformerWithoutPublicDefaultCtor : IMethodCallTransformer
    {
      private TransformerWithoutPublicDefaultCtor ()
      {
      }

      public Expression Transform (MethodCallExpression methodCallExpression)
      {
        throw new NotImplementedException ();
      }
    }
  }
}