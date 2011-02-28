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
using Remotion.Linq;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ExpressionTreeVisitors;
using Remotion.Linq.Parsing;
using Remotion.Linq.Utilities;

namespace Remotion.Data.Linq.SqlBackend.SqlStatementModel.Unresolved
{
  /// <summary>
  /// Describes a member reference representing an entity rather than a simple column.
  /// </summary>
  public class SqlEntityRefMemberExpression : ExtensionExpression
  {
    private readonly SqlEntityExpression _originatingEntity;
    private readonly MemberInfo _memberInfo;
    
    public SqlEntityRefMemberExpression (SqlEntityExpression originatingEntity, MemberInfo memberInfo)
      : base (ReflectionUtility.GetMemberReturnType (ArgumentUtility.CheckNotNull ("memberInfo", memberInfo)))
    {
      ArgumentUtility.CheckNotNull ("entityExpression", originatingEntity);

      _originatingEntity = originatingEntity;
      _memberInfo = memberInfo;
    }

    public SqlEntityExpression OriginatingEntity
    {
      get { return _originatingEntity; }
    }

    public MemberInfo MemberInfo
    {
      get { return _memberInfo; }
    }

    protected override Expression VisitChildren (ExpressionTreeVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);
      return this;
    }

    public override Expression Accept (ExpressionTreeVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);

      var specificVisitor = visitor as IUnresolvedSqlExpressionVisitor;
      if (specificVisitor != null)
        return specificVisitor.VisitSqlEntityRefMemberExpression(this);
      else
        return base.Accept (visitor);
    }

    public override string ToString ()
    {
      return string.Format ("{0}.[{1}]", FormattingExpressionTreeVisitor.Format (_originatingEntity), _memberInfo.Name);
    }
  }
}