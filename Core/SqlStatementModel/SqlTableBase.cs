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
using System.Text;
using Remotion.Linq.SqlBackend.MappingResolution;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Utilities;

namespace Remotion.Linq.SqlBackend.SqlStatementModel
{
  // TODO RMLNQSQL-64: Merge with SqlTable.
  /// <summary>
  /// Provides a base class for SQL tables, both stand-alone tables and joined tables.
  /// </summary>
  public abstract class SqlTableBase
  {
    private readonly List<SqlJoin> _orderedJoins = new List<SqlJoin>();
    private readonly Dictionary<MemberInfo, SqlJoin> _joinsByMemberInfo = new Dictionary<MemberInfo, SqlJoin>();
    
    private readonly Type _itemType;
    private readonly JoinSemantics _joinSemantics;

    public abstract void Accept (ISqlTableBaseVisitor visitor);
    
    // TODO RMLNQSQL-64: Remove joinSemantics?
    protected SqlTableBase (Type itemType, JoinSemantics joinSemantics)
    {
      ArgumentUtility.CheckNotNull ("itemType", itemType);

      _itemType = itemType;
      _joinSemantics = joinSemantics;
    }

    public abstract IResolvedTableInfo GetResolvedTableInfo ();

    public Type ItemType
    {
      get { return _itemType; }
    }

    public IEnumerable<SqlJoin> OrderedJoins
    {
      get { return _orderedJoins; }
    }

    public JoinSemantics JoinSemantics
    {
      get { return _joinSemantics; }
    }

    public SqlJoin GetOrAddLeftJoinByMember (MemberInfo memberInfo, Func<SqlJoin> joinFactory)
    {
      ArgumentUtility.CheckNotNull ("memberInfo", memberInfo);
      ArgumentUtility.CheckNotNull ("joinFactory", joinFactory);

      SqlJoin sqlJoin;
      if (!_joinsByMemberInfo.TryGetValue (memberInfo, out sqlJoin))
      {
        sqlJoin = joinFactory();
        _joinsByMemberInfo.Add (memberInfo, sqlJoin);
        AddJoin (sqlJoin);
      }

      return sqlJoin;
    }

    public void AddJoin (SqlJoin sqlJoin)
    {
      ArgumentUtility.CheckNotNull ("sqlJoin", sqlJoin);
      _orderedJoins.Add (sqlJoin);
    }

    public SqlJoin GetJoinByMember (MemberInfo relationMember)
    {
      ArgumentUtility.CheckNotNull ("relationMember", relationMember);

      return _joinsByMemberInfo[relationMember];
    }

    protected void AppendJoinString (StringBuilder sb, IEnumerable<SqlJoin> orderedJoins)
    {
      foreach (var sqlJoin in orderedJoins)
      {
        sb
            .Append (" ")
            .Append (sqlJoin.JoinSemantics.ToString().ToUpper())
            .Append (" JOIN ")
            .Append (sqlJoin.JoinedTable.TableInfo)
            .Append (" ON ")
            .Append (sqlJoin.JoinCondition);
        AppendJoinString (sb, sqlJoin.JoinedTable.OrderedJoins);
      }
    }
  }
}