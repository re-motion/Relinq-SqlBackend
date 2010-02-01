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
using System.Reflection;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.Backend.DataObjectModel
{
  public static class DatabaseInfoUtility
  {
    public static JoinColumnNames? GetJoinColumnNames (IDatabaseInfo databaseInfo, MemberInfo relationMember)
    {
      ArgumentUtility.CheckNotNull ("databaseInfo", databaseInfo);
      ArgumentUtility.CheckNotNull ("relationMember", relationMember);

      var columns = databaseInfo.GetJoinColumnNames (relationMember);
      if (columns == null)
      {
        string message =
            string.Format ("The member '{0}.{1}' does not identify a relation.", relationMember.DeclaringType.FullName, relationMember.Name);
        throw new InvalidOperationException (message);
      }
      else
        return columns;
    }

    public static Column GetColumn (IDatabaseInfo databaseInfo, IColumnSource columnSource, MemberInfo member)
    {
      ArgumentUtility.CheckNotNull ("databaseInfo", databaseInfo);
      ArgumentUtility.CheckNotNull ("member", member);

      var columnName = databaseInfo.GetColumnName (member);
      if (columnName == null)
        throw new UnmappedItemException (string.Format ("The member '{0}.{1}' does not identify a queryable column.", member.DeclaringType, member.Name));
      else
        return new Column (columnSource, columnName);
    }

    public static MemberInfo GetPrimaryKeyMember (IDatabaseInfo databaseInfo, Type entityType)
    {
      MemberInfo primaryKeyMember = databaseInfo.GetPrimaryKeyMember (entityType);
      if (primaryKeyMember == null)
      {
        var message = string.Format ("The primary key member of type '{0}' cannot be determined because it is no entity type.", entityType.FullName);
        throw new InvalidOperationException (message);
      }
      else
        return primaryKeyMember;
    }
  }
}
