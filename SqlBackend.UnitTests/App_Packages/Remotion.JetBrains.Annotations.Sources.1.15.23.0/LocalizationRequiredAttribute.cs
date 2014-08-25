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
  /// Indicates that marked element should be localized or not.
  /// </summary>
  [AttributeUsage (AttributeTargets.All, AllowMultiple = false, Inherited = true)]
  sealed partial class LocalizationRequiredAttribute : Attribute
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="LocalizationRequiredAttribute"/> class with
    /// <see cref="Required"/> set to <see langword="true"/>.
    /// </summary>
    public LocalizationRequiredAttribute ()
        : this (true)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalizationRequiredAttribute"/> class.
    /// </summary>
    /// <param name="required"><c>true</c> if a element should be localized; otherwise, <c>false</c>.</param>
    public LocalizationRequiredAttribute (bool required)
    {
      Required = required;
    }

    /// <summary>
    /// Gets a value indicating whether a element should be localized.
    /// <value><c>true</c> if a element should be localized; otherwise, <c>false</c>.</value>
    /// </summary>
    public bool Required { get; private set; }

    /// <summary>
    /// Returns whether the value of the given object is equal to the current <see cref="LocalizationRequiredAttribute"/>.
    /// </summary>
    /// <param name="obj">The object to test the value equality of. </param>
    /// <returns>
    /// <c>true</c> if the value of the given object is equal to that of the current; otherwise, <c>false</c>.
    /// </returns>
    public override bool Equals (object obj)
    {
      var attribute = obj as LocalizationRequiredAttribute;
      return attribute != null && attribute.Required == Required;
    }

    /// <summary>
    /// Returns the hash code for this instance.
    /// </summary>
    /// <returns>A hash code for the current <see cref="LocalizationRequiredAttribute"/>.</returns>
    public override int GetHashCode ()
    {
      return base.GetHashCode ();
    }
  }
}