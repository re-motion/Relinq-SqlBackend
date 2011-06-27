// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
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
using System.Reflection;
using Remotion.Linq.Utilities;

namespace Remotion.Linq.SqlBackend.SqlPreparation
{
  /// <summary>
  /// When applied to a method (or property get accessor), defines that the SQL backend should use the specified <see cref="IMethodCallTransformer"/>
  /// to handle that method (or property). The attribute is evaluated only if there isn't already a <see cref="IMethodCallTransformer"/> registered
  /// by <see cref="MethodInfo"/>, but it is evaluated before a transformer is searched by name.
  /// </summary>
  [AttributeUsage (AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
  public class MethodCallTransformerAttribute : Attribute, IMethodCallTransformerAttribute
  {
    private readonly Type _transformerType;

    /// <summary>
    /// Initializes a new instance of the <see cref="MethodCallTransformerAttribute"/> class.
    /// </summary>
    /// <param name="transformerType">
    /// The type of <see cref="IMethodCallTransformer"/> to use for handling the method this attribute is applied to. This type must have a public
    /// default constructor.
    /// </param>
    public MethodCallTransformerAttribute (Type transformerType)
    {
      ArgumentUtility.CheckNotNull ("transformerType", transformerType);

      if (!typeof (IMethodCallTransformer).IsAssignableFrom (transformerType))
        throw new ArgumentException ("The argument must be a Type implementing IMethodCallTransformer.", "transformerType");

      _transformerType = transformerType;
    }

    /// <summary>
    /// Gets the type of <see cref="IMethodCallTransformer"/> to use for handling the method this attribute is applied to.
    /// </summary>
    /// <value>The type of <see cref="IMethodCallTransformer"/> to use.</value>
    public Type TransformerType
    {
      get { return _transformerType; }
    }

    /// <summary>
    /// Gets the transformer identified by this <see cref="MethodCallTransformerAttribute"/>.
    /// </summary>
    /// <returns>An instance of the <see cref="TransformerType"/>.</returns>
    public IMethodCallTransformer GetTransformer ()
    {
      try
      {
        return (IMethodCallTransformer) Activator.CreateInstance (TransformerType);
      }
      catch (MissingMethodException ex)
      {
        var message = string.Format (
            "The method call transformer '{0}' has no public default constructor and therefore cannot be used with the MethodCallTransformerAttribute.", 
            TransformerType);
        throw new InvalidOperationException (message, ex);
      }
    }
  }
}