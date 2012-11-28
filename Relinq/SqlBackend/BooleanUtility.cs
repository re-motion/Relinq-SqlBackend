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
using Remotion.Linq.Utilities;

namespace Remotion.Linq.SqlBackend
{
  /// <summary>
  /// Provides utility functions around boolean expressions.
  /// </summary>
  public static class BooleanUtility
  {
    public static bool IsBooleanType (Type type)
    {
      ArgumentUtility.CheckNotNull ("type", type);
      return type == typeof (bool) || type == typeof (bool?);
    }

    public static Type GetMatchingIntType (Type type)
    {
      ArgumentUtility.CheckNotNull ("type", type);
      if (type == typeof (bool))
        return typeof (int);
      else if (type == typeof (bool?))
        return typeof (int?);
      else
        throw new ArgumentException ("Type must be Boolean or Nullable<Boolean>.", "type");
    }

    public static Type GetMatchingBoolType (Type type)
    {
      ArgumentUtility.CheckNotNull ("type", type);
      if (type == typeof (int))
        return typeof (bool);
      else if (type == typeof (int?))
        return typeof (bool?);
      else
        throw new ArgumentException ("Type must be Int32 or Nullable<Int32>.", "type");
    }
  }
}