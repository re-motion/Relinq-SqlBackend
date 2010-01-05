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
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq;
using Remotion.Data.Linq.Backend;
using Remotion.Data.Linq.Backend.DataObjectModel;
using Remotion.Data.Linq.Backend.DetailParsing;
using Remotion.Data.Linq.Backend.FieldResolving;
using Remotion.Data.Linq.Backend.SqlGeneration;
using Remotion.Data.Linq.UnitTests.TestDomain;
using Remotion.Development.UnitTesting;

namespace Remotion.Data.Linq.UnitTests.Backend.SqlGeneration
{
  public class SqlGeneratorMock : SqlGeneratorBase<SqlGeneratorMockContext>
  {
    private SqlGeneratorMockContext _context = new SqlGeneratorMockContext();

    private readonly IOrderByBuilder _orderByBuilder;
    private readonly IWhereBuilder _whereBuilder;
    private readonly IFromBuilder _fromBuilder;
    private readonly ISelectBuilder _selectBuilder;
    private readonly ParseContext _referenceParseContext;

    public SqlGeneratorMock (QueryModel query, IDatabaseInfo databaseInfo,
        ISelectBuilder selectBuilder, IFromBuilder fromBuilder, IWhereBuilder whereBuilder, IOrderByBuilder orderByBuilder, ParseMode parseMode)
        : base (databaseInfo, parseMode)
    {
      _selectBuilder = selectBuilder;
      _fromBuilder = fromBuilder;
      _whereBuilder = whereBuilder;
      _orderByBuilder = orderByBuilder;

      var joinedTableContext = new JoinedTableContext (StubDatabaseInfo.Instance);
      var detailParserRegistries = new DetailParserRegistries (databaseInfo, parseMode);
      _referenceParseContext = new ParseContext (query, new List<FieldDescriptor>(), joinedTableContext);
      ReferenceVisitor = new SqlGeneratorVisitor (databaseInfo, parseMode, detailParserRegistries, _referenceParseContext);

      query.Accept (ReferenceVisitor);
      joinedTableContext.CreateAliases (query);
    }

    public bool CheckBaseProcessQueryMethod { get; set; }
    public SqlGeneratorVisitor ReferenceVisitor { get; private set; }

    public SqlGeneratorMockContext Context
    {
      get { return _context; }
      set { _context = value; }
    }

    protected override SqlGeneratorMockContext CreateContext ()
    {
      return _context;
    }

    protected override SqlGenerationData ProcessQuery (QueryModel queryModel)
    {
      if (CheckBaseProcessQueryMethod)
      {
        // reset counter so that we can compare the joins
        var uniqueIdentifierGenerator = (UniqueIdentifierGenerator) PrivateInvoke.GetNonPublicField (queryModel, "_uniqueIdentifierGenerator");
        uniqueIdentifierGenerator.Reset();
        
        SqlGenerationData sqlGenerationData = base.ProcessQuery(queryModel);

        Assert.AreEqual (ParseMode, sqlGenerationData.ParseMode);

        Assert.That (sqlGenerationData.SelectEvaluation, Is.EqualTo (ReferenceVisitor.SqlGenerationData.SelectEvaluation));
        Assert.That (sqlGenerationData.Criterion, Is.EqualTo (ReferenceVisitor.SqlGenerationData.Criterion));

        Assert.AreEqual (ReferenceVisitor.SqlGenerationData.Joins.Count, sqlGenerationData.Joins.Count);
        foreach (KeyValuePair<IColumnSource, IList<SingleJoin>> joinEntry in ReferenceVisitor.SqlGenerationData.Joins)
          Assert.That (sqlGenerationData.Joins[joinEntry.Key], Is.EqualTo (joinEntry.Value));

        Assert.That (sqlGenerationData.OrderingFields, Is.EqualTo (ReferenceVisitor.SqlGenerationData.OrderingFields));
        Assert.That (sqlGenerationData.FromSources, Is.EqualTo (ReferenceVisitor.SqlGenerationData.FromSources));
      }
      return ReferenceVisitor.SqlGenerationData;
    }

    protected override ParseContext CreateParseContext (QueryModel queryModel)
    {
      var parseContext = base.CreateParseContext (queryModel);
      var oldTableMapping = PrivateInvoke.GetNonPublicField (_referenceParseContext.JoinedTableContext, "_columnSources");
      PrivateInvoke.SetNonPublicField (parseContext.JoinedTableContext, "_columnSources", oldTableMapping);
      return parseContext;
    }

    protected override IOrderByBuilder CreateOrderByBuilder (SqlGeneratorMockContext context)
    {
      Assert.That (context, Is.SameAs (_context));
      return _orderByBuilder;
    }

    protected override IWhereBuilder CreateWhereBuilder (SqlGeneratorMockContext context)
    {
      Assert.That (context, Is.SameAs (_context));
      return _whereBuilder;
    }

    protected override IFromBuilder CreateFromBuilder (SqlGeneratorMockContext context)
    {
      Assert.That (context, Is.SameAs (_context));
      return _fromBuilder;
    }

    protected override ISelectBuilder CreateSelectBuilder (SqlGeneratorMockContext context)
    {
      Assert.That (context, Is.SameAs (_context));
      return _selectBuilder;
    }
  }
}
