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
using Remotion.Utilities;
using NUnit.Framework.Constraints;

namespace Remotion.Linq.SqlBackend.UnitTests.NUnit
{
  public static class NUnitExtensions
  {
    public static EqualConstraint ArgumentExceptionMessageEqualTo (this ConstraintExpression constraintExpression, string message, string paramName)
    {
      AssertThatMessageContainsWhitespaces (message: message);
      AssertThatParameterDoesNotContainWhitespaces (paramName: paramName);
      return constraintExpression.With.Message.EqualTo (new ArgumentException (message: message, paramName: paramName).Message);
    }

    private static void AssertThatMessageContainsWhitespaces (string message)
    {
      Assertion.IsTrue (message.Contains (" "), "The exception message must contain at least one whitespace.\r\nmessage: {0}", message);
    }

    private static void AssertThatParameterDoesNotContainWhitespaces (string paramName)
    {
      Assertion.IsFalse (paramName.Contains (" "), "The parameter must not contain any whitespaces.\r\nparamName: {0}", paramName);
    }
  }
}
