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
using System.Linq.Expressions;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel;
using Remotion.Data.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.SqlBackend.MappingResolution
{
  /// <summary>
  /// Provides a default implementation of <see cref="IMappingResolutionStage"/>.
  /// </summary>
  public class DefaultMappingResolutionStage : IMappingResolutionStage
  {
    private readonly IMappingResolver _resolver;
    private readonly UniqueIdentifierGenerator _uniqueIdentifierGenerator;

    public DefaultMappingResolutionStage (IMappingResolver resolver, UniqueIdentifierGenerator uniqueIdentifierGenerator)
    {
      ArgumentUtility.CheckNotNull ("resolver", resolver);
      ArgumentUtility.CheckNotNull ("uniqueIdentifierGenerator", uniqueIdentifierGenerator);
      
      _uniqueIdentifierGenerator = uniqueIdentifierGenerator;
      _resolver = resolver;
    }

    // TODO Review 2564: Add a single protected virtual ResolveExpression method; use that method from all expression resolver methods.
    public virtual Expression ResolveSelectExpression (Expression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      return ResolvingExpressionVisitor.ResolveExpression (expression, _resolver, _uniqueIdentifierGenerator, this);
    }

    public virtual Expression ResolveWhereExpression (Expression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      return ResolvingExpressionVisitor.ResolveExpression (expression, _resolver, _uniqueIdentifierGenerator, this);
    }

    public virtual Expression ResolveOrderingExpression (Expression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      return ResolvingExpressionVisitor.ResolveExpression (expression, _resolver, _uniqueIdentifierGenerator, this);
    }

    public virtual Expression ResolveTopExpression (Expression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      return ResolvingExpressionVisitor.ResolveExpression (expression, _resolver, _uniqueIdentifierGenerator, this);
    }

    public virtual IResolvedTableInfo ResolveTableInfo (ITableInfo tableInfo)
    {
      ArgumentUtility.CheckNotNull ("tableInfo", tableInfo);

      return ResolvingTableInfoVisitor.ResolveTableInfo (tableInfo, _resolver, _uniqueIdentifierGenerator, this);
    }

    public virtual ResolvedJoinInfo ResolveJoinInfo (IJoinInfo joinInfo)
    {
      ArgumentUtility.CheckNotNull ("joinInfo", joinInfo);

      return ResolvingJoinInfoVisitor.ResolveJoinInfo (joinInfo, _resolver, _uniqueIdentifierGenerator, this);
    }

    public virtual SqlStatement ResolveSqlStatement (SqlStatement sqlStatement)
    {
      ArgumentUtility.CheckNotNull ("sqlStatement", sqlStatement);

      return SqlStatementResolver.ResolveExpressions (this, sqlStatement);
    }

    public Expression ResolveCollectionSourceExpression (Expression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      return ResolvingExpressionVisitor.ResolveExpression (expression, _resolver, _uniqueIdentifierGenerator, this);
    }
  }
}