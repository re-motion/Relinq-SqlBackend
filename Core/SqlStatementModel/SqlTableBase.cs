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
using System.Collections.Generic;
using System.Reflection;
using Remotion.Linq.SqlBackend.MappingResolution;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Utilities;

namespace Remotion.Linq.SqlBackend.SqlStatementModel
{
  /// <summary>
  /// Provides a base class for SQL tables, both stand-alone tables and joined tables.
  /// </summary>
  public abstract class SqlTableBase
  {
    private readonly Dictionary<MemberInfo, SqlJoinedTable> _joinedTables = new Dictionary<MemberInfo, SqlJoinedTable>();
    private readonly Type _itemType;
    private readonly JoinSemantics _joinSemantics;

    public abstract void Accept (ISqlTableBaseVisitor visitor);
    
    protected SqlTableBase (Type itemType, JoinSemantics joinSemantics)
    {
      ArgumentUtility.CheckNotNull (nameof(itemType), itemType);

      _itemType = itemType;
      _joinSemantics = joinSemantics;
    }

    public abstract IResolvedTableInfo GetResolvedTableInfo ();

    public Type ItemType
    {
      get { return _itemType; }
    }

    public IEnumerable<SqlJoinedTable> JoinedTables
    {
      get { return _joinedTables.Values; }
    }

    public JoinSemantics JoinSemantics
    {
      get { return _joinSemantics; }
    }

    public SqlJoinedTable GetOrAddLeftJoin (IJoinInfo joinInfo, MemberInfo memberInfo)
    {
      ArgumentUtility.CheckNotNull (nameof(joinInfo), joinInfo);

      if (!_joinedTables.ContainsKey (memberInfo))
        _joinedTables.Add (memberInfo, new SqlJoinedTable (joinInfo, JoinSemantics.Left));

      return _joinedTables[memberInfo];
    }

    public SqlJoinedTable GetJoin (MemberInfo relationMember)
    {
      ArgumentUtility.CheckNotNull (nameof(relationMember), relationMember);

      return _joinedTables[relationMember];
    }
    
  }
}