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
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Linq.Utilities;
using Remotion.Utilities;

namespace Remotion.Linq.SqlBackend.SqlStatementModel.Unresolved
{
  // TODO RMLNQSQL-64: Test
  /// <summary>
  /// <see cref="UnresolvedJoinInfo"/> represents the data source defined by a member access in a LINQ expression.
  /// </summary>
  public class UnresolvedJoinTableInfo : ITableInfo
  {
    private readonly SqlEntityExpression _originatingEntity;
    private readonly MemberInfo _memberInfo;
    private readonly JoinCardinality _cardinality;
    private readonly Type _itemType;

    public UnresolvedJoinTableInfo (SqlEntityExpression originatingEntity, MemberInfo memberInfo, JoinCardinality cardinality)
    {
      ArgumentUtility.CheckNotNull ("originatingEntity", originatingEntity);
      ArgumentUtility.CheckNotNull ("memberInfo", memberInfo);

      _originatingEntity = originatingEntity;
      _memberInfo = memberInfo;
      _cardinality = cardinality;

      var memberReturnType = ReflectionUtility.GetMemberReturnType (memberInfo);
      if (Cardinality == JoinCardinality.One)
        _itemType = memberReturnType;
      else
        _itemType = ReflectionUtility.GetItemTypeOfClosedGenericIEnumerable (memberReturnType, "memberInfo");
    }

    public SqlEntityExpression OriginatingEntity
    {
      get { return _originatingEntity; }
    }

    public MemberInfo MemberInfo
    {
      get { return _memberInfo; }
    }

    public JoinCardinality Cardinality
    {
      get { return _cardinality; }
    }

    public virtual Type ItemType
    {
      get { return _itemType; }
    }

    public IResolvedTableInfo GetResolvedTableInfo ()
    {
      throw new InvalidOperationException ("This table has not yet been resolved; call the resolution step first.");
    }

    public virtual ITableInfo Accept (ITableInfoVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);
      return visitor.VisitUnresolvedJoinTableInfo (this);
    }

    public override string ToString ()
    {
      return string.Format ("TABLE({0}.{1})", MemberInfo.DeclaringType.Name, MemberInfo.Name);
    }
  }
}