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
using System.Linq;
using Remotion.Linq.SqlBackend.MappingResolution;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Utilities;

namespace Remotion.Linq.SqlBackend.SqlStatementModel
{
  /// <summary>
  /// <see cref="SqlJoinedTable"/> represents a joined data source in a <see cref="SqlStatement"/>.
  /// </summary>
  public class SqlJoinedTable : SqlTableBase, ITableInfo
  {
    private IJoinInfo _joinInfo;

    public SqlJoinedTable (IJoinInfo joinInfo, JoinSemantics joinSemantics)
        : base (joinInfo.ItemType, joinSemantics)
    {
      ArgumentUtility.CheckNotNull ("joinInfo", joinInfo);
      ArgumentUtility.CheckNotNull ("joinSemantics", joinSemantics);

      _joinInfo = joinInfo;
    }

    public IJoinInfo JoinInfo
    {
      get { return _joinInfo; }
      set
      {
        ArgumentUtility.CheckNotNull ("value", value);
        if (_joinInfo != null)
        {
          if (_joinInfo.ItemType != value.ItemType)
            throw ArgumentUtility.CreateArgumentTypeException ("value", value.ItemType, _joinInfo.ItemType);
        }
        _joinInfo = value;
      }
    }

    public override void Accept (ISqlTableBaseVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);

      visitor.VisitSqlJoinedTable (this);
    }

    public override IResolvedTableInfo GetResolvedTableInfo ()
    {
      return JoinInfo.GetResolvedJoinInfo().ForeignTableInfo;
    }

    public ITableInfo Accept (ITableInfoVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);

      return visitor.VisitSqlJoinedTable (this);
    }

    public override string ToString ()
    {
      return JoinSemantics.ToString ().ToUpper () + " JOIN " + JoinInfo + JoinedTables.Aggregate ("", (s, t) => s + " " + t);
    }
  }
}