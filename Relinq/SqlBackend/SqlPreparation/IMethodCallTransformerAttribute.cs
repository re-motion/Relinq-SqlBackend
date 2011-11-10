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

namespace Remotion.Linq.SqlBackend.SqlPreparation
{
  /// <summary>
  /// Defines an interface for custom attributes that, when applied to a method (or property get accessor), defines that the SQL backend should use 
  /// a specific <see cref="IMethodCallTransformer"/> to handle that method (or property). The attribute is evaluated by 
  /// <see cref="AttributeBasedMethodCallTransformerProvider"/>, and there must only be one attribute implementing 
  /// <see cref="IMethodCallTransformerAttribute"/> on any method.
  /// </summary>
  public interface IMethodCallTransformerAttribute
  {
    /// <summary>
    /// Gets the transformer identified by this <see cref="IMethodCallTransformerAttribute"/>.
    /// </summary>
    /// <returns>An instance of the <see cref="MethodCallTransformerAttribute.TransformerType"/>.</returns>
    /// <exception cref="InvalidOperationException">
    /// There was an error when instantiating the transformer. The exception message describes the problem. The 
    /// <see cref="AttributeBasedMethodCallTransformerProvider"/> wraps this exception.
    /// </exception>
    IMethodCallTransformer GetTransformer ();
  }
}