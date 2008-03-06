using System.Collections.Generic;
using System.Text;
using Rubicon.Data.Linq.DataObjectModel;
using Rubicon.Data.Linq.Parsing.FieldResolving;
using Rubicon.Data.Linq.SqlGeneration;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;

namespace Rubicon.Data.Linq.UnitTests.SqlGenerationTest
{
  public class SqlGeneratorMock : SqlGeneratorBase
  {
    private readonly IOrderByBuilder _orderByBuilder;
    private readonly IWhereBuilder _whereBuilder;
    private readonly IFromBuilder _fromBuilder;
    private readonly ISelectBuilder _selectBuilder;

    public SqlGeneratorMock (QueryExpression query, IDatabaseInfo databaseInfo,
        ISelectBuilder selectBuilder, IFromBuilder fromBuilder, IWhereBuilder whereBuilder, IOrderByBuilder orderByBuilder)
        : base (query, databaseInfo)
    {
      _selectBuilder = selectBuilder;
      _fromBuilder = fromBuilder;
      _whereBuilder = whereBuilder;
      _orderByBuilder = orderByBuilder;

      JoinedTableContext joinedTableContext = new JoinedTableContext();
      Visitor = new SqlGeneratorVisitor (query, databaseInfo, joinedTableContext);
      query.Accept (Visitor);
      joinedTableContext.CreateAliases();
    }

    public bool CheckBaseProcessQueryMethod { get; set; }
    public SqlGeneratorVisitor Visitor { get; private set; }
    public new StringBuilder CommandText 
    {
      get { return base.CommandText; } 
    }

    public new List<CommandParameter> CommandParameters
    {
      get { return base.CommandParameters; }
    }

    protected override SqlGeneratorVisitor ProcessQuery ()
    {
      if (CheckBaseProcessQueryMethod)
      {
        SqlGeneratorVisitor visitor2 = base.ProcessQuery();

        Assert.That (visitor2.Columns, Is.EqualTo (Visitor.Columns));
        Assert.That (visitor2.Criterion, Is.EqualTo (Visitor.Criterion));
        
        Assert.AreEqual (Visitor.Joins.Count, visitor2.Joins.Count);
        foreach (KeyValuePair<Table, List<SingleJoin>> joinEntry in Visitor.Joins)
          Assert.That (visitor2.Joins[joinEntry.Key], Is.EqualTo (joinEntry.Value));

        Assert.That (visitor2.OrderingFields, Is.EqualTo (Visitor.OrderingFields));
        Assert.That (visitor2.Tables, Is.EqualTo (Visitor.Tables));
      }
      return Visitor;
    }

    protected override IOrderByBuilder CreateOrderByBuilder (StringBuilder commandText)
    {
      return _orderByBuilder;
    }

    protected override IWhereBuilder CreateWhereBuilder (StringBuilder commandText, List<CommandParameter> commandParameters)
    {
      return _whereBuilder;
    }

    protected override IFromBuilder CreateFromBuilder (StringBuilder commandText)
    {
      return _fromBuilder;
    }

    protected override ISelectBuilder CreateSelectBuilder (StringBuilder commandText)
    {
      return _selectBuilder;
    }
  }
}