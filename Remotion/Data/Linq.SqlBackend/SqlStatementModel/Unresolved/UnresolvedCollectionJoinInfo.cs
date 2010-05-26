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
using System.Linq.Expressions;
using System.Reflection;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.SqlBackend.SqlStatementModel.Unresolved
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
      ArgumentUtility.CheckNotNull ("sourceExpression", sourceExpression);
      ArgumentUtility.CheckNotNull ("memberInfo", memberInfo);

      _memberInfo = memberInfo;
      _sourceExpression = sourceExpression;

      var memberReturnType = ReflectionUtility.GetFieldOrPropertyType (memberInfo);
      _itemType = ReflectionUtility.GetItemTypeOfIEnumerable (memberReturnType, "memberInfo");
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
      ArgumentUtility.CheckNotNull ("visitor", visitor);
      return visitor.VisitUnresolvedCollectionJoinInfo (this);
    }

    public ResolvedJoinInfo GetResolvedLeftJoinInfo ()
    {
      throw new InvalidOperationException ("This join has not yet been resolved; call the resolution step first.");
    }

    public override string ToString ()
    {
      return string.Format ("{0}.{1}", MemberInfo.DeclaringType.Name, MemberInfo.Name);
    }
  }
}