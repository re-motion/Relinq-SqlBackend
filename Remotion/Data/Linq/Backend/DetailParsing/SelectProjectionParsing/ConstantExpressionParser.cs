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
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.Backend.DetailParsing.SelectProjectionParsing
{
  public class ConstantExpressionParser : ISelectProjectionParser
  {
    private readonly WhereConditionParsing.ConstantExpressionParser _innerParser;

    public ConstantExpressionParser (IDatabaseInfo databaseInfo)
    {
      _innerParser = new WhereConditionParsing.ConstantExpressionParser (databaseInfo);
    }

    public IEvaluation Parse (ConstantExpression constantExpression, ParseContext parseContext)
    {
      ArgumentUtility.CheckNotNull ("constantExpression", constantExpression);
      ArgumentUtility.CheckNotNull ("parseContext", parseContext);
      return _innerParser.Parse (constantExpression, parseContext);
    }

    IEvaluation ISelectProjectionParser.Parse (Expression expression, ParseContext parseContext)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      ArgumentUtility.CheckNotNull ("parseContext", parseContext);
      return Parse ((ConstantExpression) expression, parseContext);
    }

    public bool CanParse (Expression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      return expression is ConstantExpression;
    }
  }
}
