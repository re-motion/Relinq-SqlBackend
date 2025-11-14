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
  /// Describes a member reference representing an entity rather than a simple column.
  /// </summary>
  public class SqlEntityRefMemberExpression : Expression
  {
    private readonly Type _type;
    private readonly SqlEntityExpression _originatingEntity;
    private readonly MemberInfo _memberInfo;
    
    public SqlEntityRefMemberExpression (SqlEntityExpression originatingEntity, MemberInfo memberInfo)
    {
      ArgumentUtility.CheckNotNull (nameof(originatingEntity), originatingEntity);
      ArgumentUtility.CheckNotNull (nameof(memberInfo), memberInfo); 

      _type = ReflectionUtility.GetMemberReturnType (memberInfo);
      _originatingEntity = originatingEntity;
      _memberInfo = memberInfo;
    }

    public override ExpressionType NodeType
    {
      get { return ExpressionType.Extension; }
    }

    public override Type Type
    {
      get { return _type; }
    }

    public SqlEntityExpression OriginatingEntity
    {
      get { return _originatingEntity; }
    }

    public MemberInfo MemberInfo
    {
      get { return _memberInfo; }
    }

    protected override Expression VisitChildren (ExpressionVisitor visitor)
    {
      ArgumentUtility.CheckNotNull (nameof(visitor), visitor);
      return this;
    }

    protected override Expression Accept (ExpressionVisitor visitor)
    {
      ArgumentUtility.CheckNotNull (nameof(visitor), visitor);

      var specificVisitor = visitor as ISqlEntityRefMemberExpressionVisitor;
      if (specificVisitor != null)
        return specificVisitor.VisitSqlEntityRefMember(this);
      else
        return base.Accept (visitor);
    }

    public override string ToString ()
    {
      return string.Format ("{0}.[{1}]", _originatingEntity, _memberInfo.Name);
    }
  }
}