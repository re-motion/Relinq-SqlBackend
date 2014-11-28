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
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Remotion.Linq.SqlBackend.MappingResolution;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Utilities;

namespace Remotion.Linq.SqlBackend.SqlStatementModel
{
  // TODO RMLNQSQL-64: Document breaking change: SqlTableBase and SqlJoinedTable have been removed.
  /// <summary>
  /// <see cref="SqlTable"/> represents a data source in a <see cref="SqlStatement"/>.
  /// </summary>
  public class SqlTable
  {
    public class LeftJoinData
    {
      private readonly SqlTable _joinedTable;
      private readonly Expression _joinCondition;

      public LeftJoinData (SqlTable joinedTable, Expression joinCondition)
      {
        ArgumentUtility.CheckNotNull ("joinedTable", joinedTable);
        ArgumentUtility.CheckNotNull ("joinCondition", joinCondition);

        _joinedTable = joinedTable;
        _joinCondition = joinCondition;
      }

      public SqlTable JoinedTable
      {
        get { return _joinedTable; }
      }

      public Expression JoinCondition
      {
        get { return _joinCondition; }
      }
    }

    private readonly JoinSemantics _joinSemantics;

    private readonly List<SqlJoin> _orderedJoins = new List<SqlJoin>();
    private readonly Dictionary<MemberInfo, SqlJoin> _joinsByMemberInfo = new Dictionary<MemberInfo, SqlJoin>();

    private ITableInfo _tableInfo;

    // TODO RMLNQSQL-1: Remove the joinSemantics parameter here?
    public SqlTable (ITableInfo tableInfo, JoinSemantics joinSemantics)
    {
      ArgumentUtility.CheckNotNull ("tableInfo", tableInfo);

      _joinSemantics = joinSemantics;
      _tableInfo = tableInfo;
    }

    public JoinSemantics JoinSemantics
    {
      get { return _joinSemantics; }
    }

    public IEnumerable<SqlJoin> OrderedJoins
    {
      get { return _orderedJoins; }
    }


    public Type ItemType
    {
      get { return _tableInfo.ItemType; }
    }

    /// <remarks>The property is currently mutable because of a missing refactoring. It could be made immutable by using the 
    /// <see cref="IMappingResolutionContext"/> to map <see cref="SqlTableReferenceExpression"/> instances pointing to old <see cref="SqlTable"/>
    /// objects to those pointing to the new <see cref="SqlTable"/> instances.</remarks>
    public ITableInfo TableInfo
    {
      get { return _tableInfo; }
      set
      {
        Assertion.IsNotNull (_tableInfo);
        try
        {
          ArgumentUtility.CheckNotNull ("value", value);

          if (ItemType != value.ItemType)
            throw ArgumentUtility.CreateArgumentTypeException ("value", value.ItemType, _tableInfo.ItemType);

          _tableInfo = value;
        }
        finally
        {
          Assertion.IsNotNull (_tableInfo);
        }
      }
    }

    public IResolvedTableInfo GetResolvedTableInfo ()
    {
      return TableInfo.GetResolvedTableInfo();
    }

    public SqlJoin GetOrAddLeftJoinByMember (MemberInfo memberInfo, Func<LeftJoinData> joinDataFactory)
    {
      ArgumentUtility.CheckNotNull ("memberInfo", memberInfo);
      ArgumentUtility.CheckNotNull ("joinDataFactory", joinDataFactory);

      SqlJoin sqlJoin;
      if (!_joinsByMemberInfo.TryGetValue (memberInfo, out sqlJoin))
      {
        var joinData = joinDataFactory();
        sqlJoin = new SqlJoin (joinData.JoinedTable, JoinSemantics.Left, joinData.JoinCondition);
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

    public override string ToString ()
    {
      var sb = new StringBuilder();
      sb.Append (TableInfo);
      AppendJoinString (sb, OrderedJoins);

      return sb.ToString();
    }

    private void AppendJoinString (StringBuilder sb, IEnumerable<SqlJoin> orderedJoins)
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