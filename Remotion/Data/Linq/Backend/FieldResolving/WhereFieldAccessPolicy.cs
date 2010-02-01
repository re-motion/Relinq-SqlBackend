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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Remotion.Data.Linq.Backend.DataObjectModel;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.Backend.FieldResolving
{
  public class WhereFieldAccessPolicy : IResolveFieldAccessPolicy
  {
    private readonly IDatabaseInfo _databaseInfo;

    public WhereFieldAccessPolicy (IDatabaseInfo databaseInfo)
    {
      ArgumentUtility.CheckNotNull ("databaseInfo", databaseInfo);
      _databaseInfo = databaseInfo;
    }

    public MemberInfoChain AdjustMemberInfosForDirectAccessOfQuerySource (
        QuerySourceReferenceExpression referenceExpression)
    {
      ArgumentUtility.CheckNotNull ("referenceExpression", referenceExpression);
      var primaryKeyMember = _databaseInfo.GetPrimaryKeyMember (referenceExpression.Type);
      return new MemberInfoChain (new MemberInfo[0], primaryKeyMember);
    }

    public MemberInfoChain AdjustMemberInfosForRelation (IEnumerable<MemberInfo> joinedMembers, MemberInfo accessedMember)
    {
      ArgumentUtility.CheckNotNull ("accessedMember", accessedMember);
      ArgumentUtility.CheckNotNull ("joinMembers", joinedMembers);

      // for field accesses of the form "sdd.Student", where sdd.Student does not directly represent a mapped column (because the foreign key is
      // on the other side), we have to return the equivalent of "sdd.Student.ID"
      if (!_databaseInfo.HasColumn (accessedMember))
      {
        var primaryKeyMember = DatabaseInfoUtility.GetPrimaryKeyMember (_databaseInfo, ReflectionUtility.GetFieldOrPropertyType (accessedMember));
        return new MemberInfoChain ((joinedMembers.Concat (new[] { accessedMember })).ToArray(), primaryKeyMember);
      }
      else
        return new MemberInfoChain (joinedMembers.ToArray(), accessedMember);
    }

    public bool OptimizeRelatedKeyAccess ()
    {
      return true;
    }
  }
}
