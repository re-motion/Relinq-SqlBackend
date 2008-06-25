using System.Collections.Generic;
using System.Text;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.Parsing.Details;
using Remotion.Data.Linq.Parsing.FieldResolving;
using Remotion.Data.Linq.SqlGeneration;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;

namespace Remotion.Data.Linq.UnitTests.SqlGenerationTest
{
  public class SqlGeneratorMock : SqlGeneratorBase<SqlGeneratorMockContext>
  {
    private SqlGeneratorMockContext _context = new SqlGeneratorMockContext();

    private readonly IOrderByBuilder _orderByBuilder;
    private readonly IWhereBuilder _whereBuilder;
    private readonly IFromBuilder _fromBuilder;
    private readonly ISelectBuilder _selectBuilder;

    public SqlGeneratorMock (QueryModel query, IDatabaseInfo databaseInfo,
        ISelectBuilder selectBuilder, IFromBuilder fromBuilder, IWhereBuilder whereBuilder, IOrderByBuilder orderByBuilder, ParseMode parseMode)
        : base (databaseInfo, parseMode)
    {
      _selectBuilder = selectBuilder;
      _fromBuilder = fromBuilder;
      _whereBuilder = whereBuilder;
      _orderByBuilder = orderByBuilder;

      JoinedTableContext joinedTableContext = new JoinedTableContext();
      DetailParser detailParser = new DetailParser (query, databaseInfo, joinedTableContext, parseMode);
      Visitor = new SqlGeneratorVisitor (databaseInfo, parseMode, detailParser, new ParseContext (query, query.GetExpressionTree(), new List<FieldDescriptor>(), joinedTableContext));
      query.Accept (Visitor);
      joinedTableContext.CreateAliases();
    }

    public bool CheckBaseProcessQueryMethod { get; set; }
    public SqlGeneratorVisitor Visitor { get; private set; }

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
        SqlGenerationData sqlGenerationData = base.ProcessQuery(queryModel);
        //SqlGeneratorVisitor visitor2 = base.ProcessQuery();

        Assert.AreEqual (ParseMode, sqlGenerationData.ParseMode);

        Assert.That (sqlGenerationData.SelectEvaluations, Is.EqualTo (Visitor.SqlGenerationData.SelectEvaluations));
        Assert.That (sqlGenerationData.Criterion, Is.EqualTo (Visitor.SqlGenerationData.Criterion));

        Assert.AreEqual (Visitor.SqlGenerationData.Joins.Count, sqlGenerationData.Joins.Count);
        foreach (KeyValuePair<IColumnSource, List<SingleJoin>> joinEntry in Visitor.SqlGenerationData.Joins)
          Assert.That (sqlGenerationData.Joins[joinEntry.Key], Is.EqualTo (joinEntry.Value));

        Assert.That (sqlGenerationData.OrderingFields, Is.EqualTo (Visitor.SqlGenerationData.OrderingFields));
        Assert.That (sqlGenerationData.FromSources, Is.EqualTo (Visitor.SqlGenerationData.FromSources));
      }
      return Visitor.SqlGenerationData;
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