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
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Text;
using Remotion.Linq.Clauses.ExpressionTreeVisitors;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Parsing;
using Remotion.Linq.Utilities;
using System.Linq;

namespace Remotion.Linq.SqlBackend.SqlStatementModel.SqlSpecificExpressions
{
  /// <summary>
  /// Represents a SQL CASE WHEN expression.
  /// </summary>
  public class SqlCaseExpression : ExtensionExpression
  {
    public class CaseWhenPair
    {
      private readonly Expression _when;
      private readonly Expression _then;

      public CaseWhenPair (Expression when, Expression then)
      {
        ArgumentUtility.CheckNotNull ("when", when);
        ArgumentUtility.CheckNotNull ("then", then);

        if (!BooleanUtility.IsBooleanType (when.Type))
          throw new ArgumentException ("The WHEN expression's type must be boolean.", "when");

        _when = when;
        _then = then;
      }

      public Expression When
      {
        get { return _when; }
      }

      public Expression Then
      {
        get { return _then; }
      }

      public CaseWhenPair VisitChildren (ExpressionTreeVisitor visitor)
      {
        var newWhen = visitor.VisitExpression (_when);
        var newThen = visitor.VisitExpression (_then);

        return Update (newWhen, newThen);
      }

      public CaseWhenPair Update (Expression newWhen, Expression newThen)
      {
        if (newWhen != _when || newThen != _then)
          return new CaseWhenPair (newWhen, newThen);

        return this;
      }

      public override string ToString ()
      {
        return string.Format ("WHEN {0} THEN {1}", FormattingExpressionTreeVisitor.Format (_when), FormattingExpressionTreeVisitor.Format (_then));
      }
    }

    public static SqlCaseExpression CreateIfThenElse (Type type, Expression test, Expression thenCase, Expression elseCase)
    {
      ArgumentUtility.CheckNotNull ("type", type);
      ArgumentUtility.CheckNotNull ("test", test);
      ArgumentUtility.CheckNotNull ("thenCase", thenCase);
      ArgumentUtility.CheckNotNull ("elseCase", elseCase);

      return new SqlCaseExpression (type, new[] { new CaseWhenPair (test, thenCase) }, elseCase);
    }

    public static SqlCaseExpression CreateIfThenElseNull (Type type, Expression test, Expression trueCase, Expression falseCase)
    {
      ArgumentUtility.CheckNotNull ("type", type);
      ArgumentUtility.CheckNotNull ("test", test);
      ArgumentUtility.CheckNotNull ("trueCase", trueCase);
      ArgumentUtility.CheckNotNull ("falseCase", falseCase);

      return new SqlCaseExpression (type, new[] { new CaseWhenPair (test, trueCase), new CaseWhenPair (Not (test), falseCase) }, Constant (null, type));
    }

    private readonly ReadOnlyCollection<CaseWhenPair> _cases;
    private readonly Expression _elseCase;
    
    public SqlCaseExpression (Type type, IEnumerable<CaseWhenPair> cases, Expression elseCase)
      : base (ArgumentUtility.CheckNotNull ("type", type))
    {
      ArgumentUtility.CheckNotNull ("cases", cases);

      if (elseCase == null && type.IsValueType && Nullable.GetUnderlyingType (type) == null)
        throw new ArgumentException ("When no ELSE case is given, the expression's result type must be nullable.", "type");

      var casesArray = cases.ToArray();
      if (casesArray.Any (c => !type.IsAssignableFrom (c.Then.Type)))
        throw new ArgumentException ("The THEN expressions' types must match the expression type.", "cases");

      if (elseCase != null && !type.IsAssignableFrom (elseCase.Type))
        throw new ArgumentException ("The ELSE expression's type must match the expression type.", "elseCase");

      _cases = Array.AsReadOnly (casesArray);
      _elseCase = elseCase;
    }

    public ReadOnlyCollection<CaseWhenPair> Cases
    {
      get { return _cases; }
    }

    public Expression ElseCase
    {
      get { return _elseCase; }
    }

    protected override Expression VisitChildren (ExpressionTreeVisitor visitor)
    {
      var newCases = visitor.VisitList (_cases, p => p.VisitChildren (visitor));
      var newElseCase = _elseCase != null ? visitor.VisitExpression (_elseCase) : null;

      return Update (newCases, newElseCase);
    }

    // ReSharper disable ParameterTypeCanBeEnumerable.Global
    public SqlCaseExpression Update (ReadOnlyCollection<CaseWhenPair> newCases, Expression newElseCase)
    // ReSharper restore ParameterTypeCanBeEnumerable.Global
    {
      if (newCases != _cases || newElseCase != _elseCase)
        return new SqlCaseExpression (Type, newCases, newElseCase);

      return this;
    }

    public override Expression Accept (ExpressionTreeVisitor visitor)
    {
      var specificVisitor = visitor as ISqlSpecificExpressionVisitor;
      if (specificVisitor != null)
        return specificVisitor.VisitSqlCaseExpression (this);
      else
        return base.Accept (visitor);
    }

    public override string ToString ()
    {
      var stringBuilder = new StringBuilder();
      stringBuilder.Append ("CASE");
      foreach (var caseWhenPair in _cases)
      {
        stringBuilder.Append (" " );
        stringBuilder.Append (caseWhenPair);
      }

      if (_elseCase != null)
      {
        stringBuilder.Append (" ELSE ");
        stringBuilder.Append (FormattingExpressionTreeVisitor.Format (_elseCase));
      }
      stringBuilder.Append (" END");
      return stringBuilder.ToString();
    }

 }
}