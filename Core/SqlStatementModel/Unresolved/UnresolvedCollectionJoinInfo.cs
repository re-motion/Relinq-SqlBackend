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
using System.Linq.Expressions;
using System.Reflection;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Linq.SqlBackend.Utilities;
using Remotion.Utilities;

namespace Remotion.Linq.SqlBackend.SqlStatementModel.Unresolved
{
  /// <summary>
  /// <see cref="UnresolvedCollectionJoinInfo"/> represents the data source defined by a member access to a collection in a LINQ expression.
  /// </summary>
  public class UnresolvedCollectionJoinInfo : IJoinInfo
  {
    private readonly Expression _sourceExpression;
    private readonly MemberInfo _memberInfo;
    private readonly Type _itemType;

    public UnresolvedCollectionJoinInfo (Expression sourceExpression, MemberInfo memberInfo)
    {
      ArgumentUtility.CheckNotNull (nameof(sourceExpression), sourceExpression);
      ArgumentUtility.CheckNotNull (nameof(memberInfo), memberInfo);

      _memberInfo = memberInfo;
      _sourceExpression = sourceExpression;

      var memberReturnType = ReflectionUtility.GetMemberReturnType (memberInfo);
      _itemType = ReflectionUtility.GetItemTypeOfClosedGenericIEnumerable (memberReturnType, "memberInfo");
    }

    public Expression SourceExpression
    {
      get { return _sourceExpression; }
    }

    public MemberInfo MemberInfo
    {
      get { return _memberInfo; }
    }

    public Type ItemType
    {
      get { return _itemType; }
    }

    public IJoinInfo Accept (IJoinInfoVisitor visitor)
    {
      ArgumentUtility.CheckNotNull (nameof(visitor), visitor);
      return visitor.VisitUnresolvedCollectionJoinInfo (this);
    }

    public ResolvedJoinInfo GetResolvedJoinInfo ()
    {
      throw new InvalidOperationException ("This join has not yet been resolved; call the resolution step first.");
    }

    public override string ToString ()
    {
      return string.Format ("{0}.{1}", MemberInfo.DeclaringType.Name, MemberInfo.Name);
    }
  }
}