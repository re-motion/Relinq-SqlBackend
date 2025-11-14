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
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using Remotion.Utilities;

namespace Remotion.Linq.SqlBackend.SqlStatementModel.Resolved
{
  /// <summary>
  /// <see cref="SqlEntityExpression"/> represents an entity in a SQL expression. It consists of a list of columns, a primary key (which is usually
  /// part of the columns list), and a table alias identifying the table or substatement the entity stems from. An entity can have a name, which
  /// is used to prefix all of its columns with in the generated SQL. 
  /// </summary>
  public abstract class SqlEntityExpression : Expression
  {
    private readonly Type _entityType;
    private readonly string _tableAlias;
    private readonly string _name;
    private readonly Func<SqlEntityExpression, Expression> _identityExpressionGenerator;

    protected SqlEntityExpression (Type entityType, string tableAlias, string entityName, Func<SqlEntityExpression, Expression> identityExpressionGenerator)
    {
      ArgumentUtility.CheckNotNull (nameof(tableAlias), tableAlias);
      ArgumentUtility.CheckNotNull (nameof(identityExpressionGenerator), identityExpressionGenerator);

      _entityType = entityType;
      _tableAlias = tableAlias;
      _name = entityName;
      _identityExpressionGenerator = identityExpressionGenerator;
    }

    public abstract ReadOnlyCollection<SqlColumnExpression> Columns { get; }
    

    public override ExpressionType NodeType
    {
      get { return ExpressionType.Extension; }
    }

    public override Type Type
    {
      get { return _entityType; }
    }

    public string TableAlias
    {
      get { return _tableAlias; }
    }

    public string Name
    {
      get { return _name; }
    }

    public Func<SqlEntityExpression, Expression> IdentityExpressionGenerator
    {
      get { return _identityExpressionGenerator; }
    }



    public abstract SqlColumnExpression GetColumn (Type type, string columnName, bool isPrimaryKeyColumn);
    public abstract SqlEntityExpression CreateReference (string newTableAlias, Type newType);
    
    // TODO: Remove itemType parameter
    public abstract SqlEntityExpression Update (Type itemType, string tableAlias, string entityName);

    public Expression GetIdentityExpression()
    {
      return _identityExpressionGenerator (this);
    }

    protected override Expression Accept (ExpressionVisitor visitor)
    {
      var specificVisitor = visitor as IResolvedSqlExpressionVisitor;
      if (specificVisitor != null)
        return specificVisitor.VisitSqlEntity (this);
      else
        return base.Accept (visitor);
    }
  }
}