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

#pragma warning disable 1591
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable IntroduceOptionalParameters.Global
// ReSharper disable MemberCanBeProtected.Global
// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace

namespace JetBrains.Annotations
{
  /// <summary>
  /// When applied to target attribute, specifies a requirement for any type which is marked with 
  /// target attribute to implement or inherit specific type or types
  /// </summary>
  /// <example>
  /// <code>
  /// [BaseTypeRequired(typeof(IComponent)] // Specify requirement
  /// public class ComponentAttribute : Attribute 
  /// {}
  /// 
  /// [Component] // ComponentAttribute requires implementing IComponent interface
  /// public class MyComponent : IComponent
  /// {}
  /// </code>
  /// </example>
  [AttributeUsage (AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
  [BaseTypeRequired (typeof (Attribute))]
  sealed partial class BaseTypeRequiredAttribute : Attribute
  {
    /// <summary>
    /// Initializes new instance of BaseTypeRequiredAttribute
    /// </summary>
    /// <param name="baseType">Specifies which types are required</param>
    public BaseTypeRequiredAttribute (Type baseType)
    {
      BaseTypes = new[] { baseType };
    }

    /// <summary>
    /// Gets enumerations of specified base types
    /// </summary>
    public Type[] BaseTypes { get; private set; }
  }
}