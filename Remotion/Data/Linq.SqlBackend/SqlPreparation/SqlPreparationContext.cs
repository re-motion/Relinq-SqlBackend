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
using System.Linq.Expressions;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Unresolved;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.SqlBackend.SqlPreparation
{
  
  /// <summary>
  /// <see cref="SqlPreparationContext"/> holds context information required during SQL preparation stage.
  /// </summary>
  public class SqlPreparationContext : ISqlPreparationContext
  {
    private readonly ISqlPreparationContext _parentContext;
    private readonly SqlPreparationQueryModelVisitor _visitor;
    private readonly Dictionary<Expression, Expression> _mapping;

    public SqlPreparationContext () : this(null, null)
    {
    }

    public SqlPreparationContext (ISqlPreparationContext parentContext, SqlPreparationQueryModelVisitor visitor)
    {
      _parentContext = parentContext;
      _visitor = visitor;
      _mapping = new Dictionary<Expression, Expression>();
    }

    public void AddExpressionMapping (Expression original, Expression replacement)
    {
      ArgumentUtility.CheckNotNull ("original", original);
      ArgumentUtility.CheckNotNull ("replacement", replacement);

      _mapping[original] = replacement;
    }

    public Expression TryGetExpressionMapping (Expression original)
    {
      ArgumentUtility.CheckNotNull ("original", original);

      Expression result = TryGetExpressionMappingFromHierarchy (original);
      if (result != null) // search this context and parent context's for query source
        return result;

      if (_visitor != null)
      {
        // if whole hierarchy doesn't contain source, check whether it's a group join; group joins are lazily added
        var keyAsQuerySourceReferenceExpression = original as QuerySourceReferenceExpression;
        if (keyAsQuerySourceReferenceExpression != null)
        {
          var groupJoinClause = keyAsQuerySourceReferenceExpression.ReferencedQuerySource as GroupJoinClause;
          if (groupJoinClause != null)
            return new SqlTableReferenceExpression (_visitor.AddJoinClause (groupJoinClause.JoinClause));
        }
      }
      return null;
    }

    public Expression TryGetExpressionMappingFromHierarchy (Expression original)
    {
      ArgumentUtility.CheckNotNull ("original", original);

      Expression result;
      if (_mapping.TryGetValue (original, out result))
        return result;

      if (_parentContext != null)
        return _parentContext.TryGetExpressionMappingFromHierarchy (original);

      return null;
    }
  }
}