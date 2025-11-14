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
using System.Reflection;
using Remotion.Linq.Utilities;
using Remotion.Utilities;

// ReSharper disable once CheckNamespace
namespace Remotion.Linq.SqlBackend.Utilities
{
  internal static class ReflectionUtility
  {
    public static Type GetMemberReturnType (MemberInfo member)
    {
      ArgumentUtility.CheckNotNull (nameof(member), member);

      var propertyInfo = member as PropertyInfo;
      if (propertyInfo != null)
        return propertyInfo.PropertyType;

      var fieldInfo = member as FieldInfo;
      if (fieldInfo != null)
        return fieldInfo.FieldType;

      var methodInfo = member as MethodInfo;
      if (methodInfo != null)
        return methodInfo.ReturnType;

      throw new ArgumentException ("Argument must be FieldInfo, PropertyInfo, or MethodInfo.", nameof(member));
    }

    public static Type GetItemTypeOfClosedGenericIEnumerable (Type enumerableType, string argumentName)
    {
      ArgumentUtility.CheckNotNull (nameof(enumerableType), enumerableType);
      ArgumentUtility.CheckNotNullOrEmpty (nameof(argumentName), argumentName);

      Type itemType;
      if (!ItemTypeReflectionUtility.TryGetItemTypeOfClosedGenericIEnumerable (enumerableType, out itemType))
      {
        var message = string.Format ("Expected a closed generic type implementing IEnumerable<T>, but found '{0}'.", enumerableType);
        throw new ArgumentException (message, argumentName);
      }

      return itemType;
    }
  }
}
