using System.Collections.Generic;
using System.Text;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.Parsing.FieldResolving;
using Remotion.Data.Linq.SqlGeneration;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;

namespace Remotion.Data.Linq.UnitTests.SqlGenerationTest
{
  public class SqlGeneratorMock : SqlGeneratorBase
  {
    private readonly IOrderByBuilder _orderByBuilder;
    private readonly IWhereBuilder _whereBuilder;
    private readonly IFromBuilder _fromBuilder;
    private readonly ISelectBuilder _selectBuilder;
    private readonly StringBuilder _commandText = new StringBuilder();
    private readonly List<CommandParameter> _commandParameters = new List<CommandParameter>();

    public SqlGeneratorMock (QueryModel query, IDatabaseInfo databaseInfo,
        ISelectBuilder selectBuilder, IFromBuilder fromBuilder, IWhereBuilder whereBuilder, IOrderByBuilder orderByBuilder, ParseContext parseContext)
        : base (query, databaseInfo, parseContext)
    {
      _selectBuilder = selectBuilder;
      _fromBuilder = fromBuilder;
      _whereBuilder = whereBuilder;
      _orderByBuilder = orderByBuilder;

      JoinedTableContext joinedTableContext = new JoinedTableContext();
      Visitor = new SqlGeneratorVisitor (query, databaseInfo, joinedTableContext, parseContext);
      query.Accept (Visitor);
      joinedTableContext.CreateAliases();
    }

    public bool CheckBaseProcessQueryMethod { get; set; }
    public SqlGeneratorVisitor Visitor { get; private set; }

    public override StringBuilder CommandText
    {
      get { return _commandText; }
    }

    public override List<CommandParameter> CommandParameters
    {
      get { return _commandParameters; }
    }

    protected override SqlGeneratorVisitor ProcessQuery ()
    {
      if (CheckBaseProcessQueryMethod)
      {
        SqlGeneratorVisitor visitor2 = base.ProcessQuery();

        Assert.AreEqual (ParseContext, visitor2.ParseContext);

        Assert.That (visitor2.SqlGenerationData.SelectEvaluations, Is.EqualTo (Visitor.SqlGenerationData.SelectEvaluations));
        Assert.That (visitor2.SqlGenerationData.Criterion, Is.EqualTo (Visitor.SqlGenerationData.Criterion));

        Assert.AreEqual (Visitor.SqlGenerationData.Joins.Count, visitor2.SqlGenerationData.Joins.Count);
        foreach (KeyValuePair<IColumnSource, List<SingleJoin>> joinEntry in Visitor.SqlGenerationData.Joins)
          Assert.That (visitor2.SqlGenerationData.Joins[joinEntry.Key], Is.EqualTo (joinEntry.Value));

        Assert.That (visitor2.SqlGenerationData.OrderingFields, Is.EqualTo (Visitor.SqlGenerationData.OrderingFields));
        Assert.That (visitor2.SqlGenerationData.FromSources, Is.EqualTo (Visitor.SqlGenerationData.FromSources));
      }
      return Visitor;
    }

    protected override IOrderByBuilder CreateOrderByBuilder ()
    {
      return _orderByBuilder;
    }

    protected override IWhereBuilder CreateWhereBuilder ()
    {
      return _whereBuilder;
    }

    protected override IFromBuilder CreateFromBuilder ()
    {
      return _fromBuilder;
    }

    protected override ISelectBuilder CreateSelectBuilder ()
    {
      return _selectBuilder;
    }
  }
}