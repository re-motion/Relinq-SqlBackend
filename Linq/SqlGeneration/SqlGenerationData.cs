using System.Collections.Generic;
using Remotion.Collections;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.Parsing;
using Remotion.Utilities;

namespace Remotion.Data.Linq.SqlGeneration
{
  public class SqlGenerationData
  {
    public SqlGenerationData()
    {
      FromSources = new List<IColumnSource> ();
      SelectEvaluations = new List<IEvaluation> ();
      OrderingFields = new List<OrderingField> ();
      Joins = new JoinCollection ();
      LetEvaluations = new List<LetData> ();
    }

    public List<IColumnSource> FromSources { get; private set; }
    public List<IEvaluation> SelectEvaluations { get; private set; }
    public ICriterion Criterion { get; set; }
    public bool Distinct { get; set; }
    public List<OrderingField> OrderingFields { get; private set; }
    public JoinCollection Joins { get; private set; }
    public List<LetData> LetEvaluations { get; private set; }
    public ParseContext ParseContext { get; set; }
    
    public void AddSelectClause (SelectClause selectClause, Tuple<List<FieldDescriptor>, List<IEvaluation>> evaluations)
    {
      Distinct = selectClause.Distinct;
      SelectEvaluations.AddRange(evaluations.B);
      foreach (var selectedField in evaluations.A)
        Joins.AddPath (selectedField.SourcePath);
    }

    public void AddLetClauses (LetData letData, Tuple<List<FieldDescriptor>, List<IEvaluation>> evaluations)
    {
      LetEvaluations.Add (letData);

      foreach (var selectedField in evaluations.A)
      {
        Joins.AddPath (selectedField.SourcePath);
      }   
    }

    public void AddFromClause(IColumnSource columnSource)
    {
      FromSources.Add (columnSource);
    }

    public void AddWhereClause (Tuple<List<FieldDescriptor>, ICriterion> criterions)
    {
      if (Criterion == null)
        Criterion = criterions.B;
      else
        Criterion = new ComplexCriterion (Criterion, criterions.B, ComplexCriterion.JunctionKind.And);

      foreach (var fieldDescriptor in criterions.A)
        Joins.AddPath (fieldDescriptor.SourcePath);
    }

    public void AddOrderingFields(OrderingField orderingField)
    {
      OrderingFields.Add (orderingField);
      Joins.AddPath (orderingField.FieldDescriptor.SourcePath);
    }
  }
}