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
using Remotion.Linq.Clauses.ExpressionTreeVisitors;
using Remotion.Linq.Utilities;

namespace Remotion.Linq.SqlBackend.SqlStatementModel.Resolved
{
  /// <summary>
  /// <see cref="ResolvedJoinInfo"/> represents a join between two database tables.
  /// </summary>
  public class ResolvedJoinInfo : IJoinInfo
  {
    private readonly IResolvedTableInfo _foreignTableInfo;
    private readonly Expression _leftKey;
    private readonly Expression _rightKey;

    public ResolvedJoinInfo (IResolvedTableInfo foreignTableInfo, Expression leftKey, Expression rightKey)
    {
      ArgumentUtility.CheckNotNull ("foreignTableInfo", foreignTableInfo);
      ArgumentUtility.CheckNotNull ("leftKey", leftKey);
      ArgumentUtility.CheckNotNull ("rightKey", rightKey);
      
      _foreignTableInfo = foreignTableInfo;
      _leftKey = leftKey;
      _rightKey = rightKey;
    }

    public virtual Type ItemType
    {
      get { return _foreignTableInfo.ItemType; }
    }

    public IResolvedTableInfo ForeignTableInfo
    {
      get { return _foreignTableInfo; }
    }

    public Expression LeftKey
    {
      get { return _leftKey; }
    }

    public Expression RightKey
    {
      get { return _rightKey; }
    }

    public virtual IJoinInfo Accept (IJoinInfoVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);
      return visitor.VisitResolvedJoinInfo (this);
    }

    public ResolvedJoinInfo GetResolvedJoinInfo ()
    {
      return this;
    }

    public override string ToString ()
    {
      return string.Format (
          "{0} ON {1} = {2}", 
          ForeignTableInfo, 
          FormattingExpressionTreeVisitor.Format (LeftKey), 
          FormattingExpressionTreeVisitor.Format (RightKey));
    }
  }
}