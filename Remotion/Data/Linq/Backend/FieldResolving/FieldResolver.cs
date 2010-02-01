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
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Remotion.Data.Linq.Backend.DataObjectModel;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.Backend.FieldResolving
{
  public class FieldResolver
  {
    public IDatabaseInfo DatabaseInfo { get; private set; }
    private readonly IResolveFieldAccessPolicy _policy;

    public FieldResolver (IDatabaseInfo databaseInfo, IResolveFieldAccessPolicy policy)
    {
      ArgumentUtility.CheckNotNull ("databaseInfo", databaseInfo);
      ArgumentUtility.CheckNotNull ("policy", policy);

      DatabaseInfo = databaseInfo;
      _policy = policy;
    }

    public FieldDescriptor ResolveField (Expression fieldAccessExpression, JoinedTableContext joinedTableContext)
    {
      ArgumentUtility.CheckNotNull ("fieldAccessExpression", fieldAccessExpression);

      var result = FieldResolverVisitor.ParseFieldAccess (DatabaseInfo, fieldAccessExpression, _policy.OptimizeRelatedKeyAccess());
      var clause = result.QuerySourceReferenceExpression.ReferencedQuerySource as FromClauseBase;
      if (clause == null)
      {
        var message = string.Format (
            "References to clauses of type '{0}' are not supported by this class.",
            result.QuerySourceReferenceExpression.ReferencedQuerySource.GetType().Name);
        throw new NotSupportedException (message);
      }

      return CreateFieldDescriptor (
          joinedTableContext.GetColumnSource (clause),
          result.QuerySourceReferenceExpression,
          result.JoinMembers,
          result.AccessedMember, joinedTableContext);
    }

    private FieldDescriptor CreateFieldDescriptor (
        IColumnSource firstSource, 
        QuerySourceReferenceExpression referenceExpression, 
        IEnumerable<MemberInfo> joinMembers, 
        MemberInfo accessedMember, 
        JoinedTableContext joinedTableContext)
    {
      // Documentation example: sdd.Student_Detail.Student.First
      // joinMembers == "Student_Detail", "Student"

      var memberInfos = AdjustMemberInfos (referenceExpression, joinMembers, accessedMember);
      MemberInfo accessedMemberForColumn = memberInfos.AccessedMember;
      IEnumerable<MemberInfo> joinMembersForCalculation = memberInfos.JoinedMembers;

      var pathBuilder = new FieldSourcePathBuilder();
      FieldSourcePath fieldData = pathBuilder.BuildFieldSourcePath (DatabaseInfo, joinedTableContext, firstSource, joinMembersForCalculation);

      try
      {
        var column = DatabaseInfoUtility.GetColumn (DatabaseInfo, fieldData.LastSource, accessedMemberForColumn);
        return new FieldDescriptor (accessedMember, fieldData, column);
      }
      catch (UnmappedItemException ex)
      {
        throw new FieldAccessResolveException (ex.Message, ex);
      }
    }

    private MemberInfoChain AdjustMemberInfos (
        QuerySourceReferenceExpression referenceExpression, IEnumerable<MemberInfo> joinedMembers, MemberInfo accessedMember)
    {
      if (accessedMember == null)
      {
        Debug.Assert (joinedMembers.Count() == 0, "Number of joinMembers must be 0.");
        return _policy.AdjustMemberInfosForDirectAccessOfQuerySource (referenceExpression);
      }
      else if (DatabaseInfo.IsRelationMember (accessedMember))
        return _policy.AdjustMemberInfosForRelation (joinedMembers, accessedMember);
      else
        return new MemberInfoChain (joinedMembers.ToArray(), accessedMember);
    }
  }
}
