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
using System.Collections.Generic;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.SqlBackend.SqlPreparation
{
  /// <summary>
  /// <see cref="SqlPreparationContext"/> is a helper class which maps <see cref="IQuerySource"/> to <see cref="SqlTable"/>.
  /// If the <see cref="SqlTableBase"/> for a <see cref="IQuerySource"/> is not found, it tries to get it from it's parent context.
  /// </summary>
  public class SqlPreparationQueryModelVisitorContext : ISqlPreparationContext
  {
    private readonly ISqlPreparationContext _parentContext;
    private readonly SqlPreparationQueryModelVisitor _visitor;
    private readonly Dictionary<IQuerySource, SqlTableBase> _mapping;

    public SqlPreparationQueryModelVisitorContext (ISqlPreparationContext parentContext, SqlPreparationQueryModelVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("parentContext", parentContext);
      ArgumentUtility.CheckNotNull ("visitor", visitor);

      _parentContext = parentContext;
      _visitor = visitor;
      _mapping = new Dictionary<IQuerySource, SqlTableBase>();
    }
    
    public int QuerySourceMappingCount
    {
      get { return _mapping.Count;  }
    }

    public void AddQuerySourceMapping (IQuerySource source, SqlTableBase sqlTable)
    {
      ArgumentUtility.CheckNotNull ("source", source);
      ArgumentUtility.CheckNotNull ("sqlTable", sqlTable);

      _mapping.Add (source, sqlTable);
    }

    public SqlTableBase GetSqlTableForQuerySource (IQuerySource source)
    {
      ArgumentUtility.CheckNotNull ("source", source);
      
      SqlTableBase result;
      if (_mapping.TryGetValue (source, out result))
        return result;

      if(_parentContext.TryGetSqlTableForQuerySource(source, out result))
      {
        return result;
      }
      else
      {
        var groupJoinClause = source as GroupJoinClause;
        if (groupJoinClause != null)
          return _visitor.AddJoinClause (groupJoinClause.JoinClause);
      }
      var message = string.Format (
           "The query source '{0}' ({1}) could not be found in the list of processed query sources. Probably, the feature declaring '{0}' isn't "
           + "supported yet.",
           source.ItemName,
           source.GetType ().Name);
      throw new KeyNotFoundException (message);
    }

    public bool TryGetSqlTableForQuerySource (IQuerySource source, out SqlTableBase sqlTableBase)
    {
      if (_mapping.TryGetValue (source, out sqlTableBase))
        return true;

      if (_parentContext.TryGetSqlTableForQuerySource (source, out sqlTableBase))
        return true;

      var groupJoinClause = source as GroupJoinClause;
      if (groupJoinClause != null)
      {
        sqlTableBase = _visitor.AddJoinClause (groupJoinClause.JoinClause);
        return true;
      }

      return false;
    }
  }
}