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
using NUnit.Framework;
using Remotion.Data.Linq;
using Remotion.Data.Linq.Backend.DataObjectModel;
using Remotion.Data.Linq.Backend.DetailParsing;
using Remotion.Data.Linq.Backend.FieldResolving;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.UnitTests.TestDomain;

namespace Remotion.Data.Linq.UnitTests.Backend.DetailParsing
{
  public abstract class DetailParserTestBase
  {
    protected QueryModel QueryModel;
    protected ParseContext ParseContext;

    private MainFromClause _cookClause;
    private QuerySourceReferenceExpression _cookReference;

    private MemberExpression _cook_First_Expression;
    private MemberExpression _cook_Last_Expression;
    private MemberExpression _cook_ID_Expression;

    public MainFromClause CookClause
    {
      get { return _cookClause; }
      set { _cookClause = value; }
    }

    public QuerySourceReferenceExpression CookReference
    {
      get { return _cookReference; }
    }

    public MemberExpression CookFirstExpression
    {
      get { return _cook_First_Expression; }
    }

    public MemberExpression CookLastExpression
    {
      get { return _cook_Last_Expression; }
    }

    public MemberExpression CookIDExpression
    {
      get { return _cook_ID_Expression; }
    }

    [SetUp]
    public virtual void SetUp ()
    {
      QueryModel = ExpressionHelper.CreateQueryModel_Cook ();
      ParseContext = new ParseContext(QueryModel, new List<FieldDescriptor>(), new JoinedTableContext (StubDatabaseInfo.Instance));

      _cookClause = ExpressionHelper.CreateMainFromClause_Cook ();
      _cookReference = new QuerySourceReferenceExpression (_cookClause);
      _cook_First_Expression = Expression.MakeMemberAccess (_cookReference, typeof (Cook).GetProperty ("FirstName"));
      _cook_Last_Expression = Expression.MakeMemberAccess (_cookReference, typeof (Cook).GetProperty ("Name"));
      _cook_ID_Expression = Expression.MakeMemberAccess (_cookReference, typeof (Cook).GetProperty ("ID"));
    }
  }
}
