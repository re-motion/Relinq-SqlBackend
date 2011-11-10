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
using System.Linq;
using Remotion.Linq.SqlBackend.MappingResolution;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Linq.Utilities;

namespace Remotion.Linq.SqlBackend.SqlStatementModel
{
  /// <summary>
  /// <see cref="SqlTable"/> represents a data source in a <see cref="SqlStatement"/>.
  /// </summary>
  public class SqlTable : SqlTableBase
  {
    private ITableInfo _tableInfo;

    public SqlTable (ITableInfo tableInfo, JoinSemantics joinSemantics)
        : base (tableInfo.ItemType, joinSemantics)
    {
      ArgumentUtility.CheckNotNull ("tableInfo", tableInfo);
      
      _tableInfo = tableInfo;
    }

    public ITableInfo TableInfo
    {
      get { return _tableInfo; }
      set
      {
        ArgumentUtility.CheckNotNull ("value", value);
        if (_tableInfo != null)
        {
          if (_tableInfo.ItemType != value.ItemType)
            throw new ArgumentTypeException ("value", _tableInfo.ItemType, value.ItemType);
        }
        _tableInfo = value;
      }
    }

    public override void Accept (ISqlTableBaseVisitor visitor)
    {
      visitor.VisitSqlTable (this);
    }

    public override IResolvedTableInfo GetResolvedTableInfo ()
    {
      return TableInfo.GetResolvedTableInfo();
    }

    public override string ToString ()
    {
      return TableInfo + JoinedTables.Aggregate ("", (s, t) => s + " " + t);
    }
  }
}