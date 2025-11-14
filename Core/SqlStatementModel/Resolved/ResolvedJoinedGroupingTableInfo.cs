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
using Remotion.Linq.SqlBackend.MappingResolution;
using Remotion.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Utilities;

namespace Remotion.Linq.SqlBackend.SqlStatementModel.Resolved
{
  /// <summary>
  /// <see cref="ResolvedJoinedGroupingTableInfo"/> constitutes an implementation of <see cref="ITableInfo"/> that contains a sub-statement
  /// that returns items from groupings produced by another <see cref="SqlTable"/>. <see cref="ResolvingTableInfoVisitor"/> creates this for
  /// an <see cref="UnresolvedGroupReferenceTableInfo"/> that points to a <see cref="UnresolvedGroupReferenceTableInfo.ReferencedGroupSource"/> 
  /// with a <see cref="SqlGroupingSelectExpression"/>.
  /// </summary>
  public class ResolvedJoinedGroupingTableInfo : ResolvedSubStatementTableInfo
  {
    private readonly SqlGroupingSelectExpression _associatedGroupingSelectExpression;
    private readonly string _groupSourceTableAlias;

    public ResolvedJoinedGroupingTableInfo (
        string tableAlias, 
        SqlStatement sqlStatement, 
        SqlGroupingSelectExpression associatedGroupingSelectExpression,
        string groupSourceTableAlias)
      : base (tableAlias, sqlStatement)
    {
      ArgumentUtility.CheckNotNull (nameof(associatedGroupingSelectExpression), associatedGroupingSelectExpression);
      ArgumentUtility.CheckNotNull (nameof(groupSourceTableAlias), groupSourceTableAlias);

      _associatedGroupingSelectExpression = associatedGroupingSelectExpression;
      _groupSourceTableAlias = groupSourceTableAlias;
    }

    public SqlGroupingSelectExpression AssociatedGroupingSelectExpression
    {
      get { return _associatedGroupingSelectExpression; }
    }

    public string GroupSourceTableAlias
    {
      get { return _groupSourceTableAlias; }
    }

    public override ITableInfo Accept (ITableInfoVisitor visitor)
    {
      return visitor.VisitJoinedGroupingTableInfo (this);
    }

    public override string ToString ()
    {
      return string.Format ("JOINED-GROUPING([{0}], {1})", GroupSourceTableAlias, base.ToString());
    }

  }
}