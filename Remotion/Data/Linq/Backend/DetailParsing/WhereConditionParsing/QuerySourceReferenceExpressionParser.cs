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
using Remotion.Data.Linq.Backend.DataObjectModel;
using Remotion.Data.Linq.Backend.FieldResolving;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.Backend.DetailParsing.WhereConditionParsing
{
  public class QuerySourceReferenceExpressionParser : IWhereConditionParser
  {
    private readonly FieldResolver _resolver;

    public QuerySourceReferenceExpressionParser (FieldResolver resolver)
    {
      ArgumentUtility.CheckNotNull ("resolver", resolver);
      _resolver = resolver;
    }

    public ICriterion Parse (QuerySourceReferenceExpression referenceExpression, ParseContext parseContext)
    {
      FieldDescriptor fieldDescriptor = _resolver.ResolveField (referenceExpression, parseContext.JoinedTableContext);
      parseContext.FieldDescriptors.Add (fieldDescriptor);
      return fieldDescriptor.GetMandatoryColumn();
    }

    ICriterion IWhereConditionParser.Parse (Expression expression, ParseContext parseContext)
    {
      return Parse ((QuerySourceReferenceExpression) expression, parseContext);
    }

    public bool CanParse (Expression expression)
    {
      return expression is QuerySourceReferenceExpression;
    }
  }
}
