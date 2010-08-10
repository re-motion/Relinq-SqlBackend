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
using System.Linq;
using Remotion.Data.Linq.SqlBackend.MappingResolution;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.SqlBackend.SqlStatementModel
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
      ArgumentUtility.CheckNotNull ("joinSemanticd", joinSemantics);

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
            throw new ArgumentTypeException ("value", _joinInfo.ItemType, value.ItemType);
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
      return JoinInfo.GetResolvedLeftJoinInfo().ForeignTableInfo;
    }

    public ITableInfo Accept (ITableInfoVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);

      return visitor.VisitSqlJoinedTable (this);
    }

    public override string ToString ()
    {
      return string.Format ("{0} JOIN ", JoinSemantics.ToString ().ToUpper ()) + JoinInfo + JoinedTables.Aggregate ("", (s, t) => s + " " + t);
    }
  }
}