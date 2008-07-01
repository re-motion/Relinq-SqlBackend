using System;
using System.Collections.Generic;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.Parsing;
using Remotion.Mixins;

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
    public ParseMode ParseMode { get; set; }
    
    public void AddSelectClause (SelectClause selectClause, List<FieldDescriptor> fieldDescriptors, IEvaluation evaluation)
    {
      Distinct = selectClause.Distinct;
      SelectEvaluations.Add(evaluation);

      foreach (var selectedField in fieldDescriptors)
        Joins.AddPath (selectedField.SourcePath);
    }

    public void AddLetClause (LetData letData, List<FieldDescriptor> fieldDescriptors)
    {
      LetEvaluations.Add (letData);

      foreach (var selectedField in fieldDescriptors)
        Joins.AddPath (selectedField.SourcePath);
    }

    public void AddFromClause(IColumnSource columnSource)
    {
      FromSources.Add (columnSource);
    }

    public void AddWhereClause (ICriterion criterion, List<FieldDescriptor> fieldDescriptors)
    {
      if (Criterion == null)
        Criterion = criterion;
      else
        Criterion = new ComplexCriterion (Criterion, criterion, ComplexCriterion.JunctionKind.And);

      foreach (var fieldDescriptor in fieldDescriptors)
        Joins.AddPath (fieldDescriptor.SourcePath);
    }

    public void AddOrderingFields(OrderingField orderingField)
    {
      OrderingFields.Add (orderingField);
      Joins.AddPath (orderingField.FieldDescriptor.SourcePath);
    }

    public void AddFirstOrderingFields (OrderingField orderingField)
    {
      List<OrderingField> newOrderingFields = new List<OrderingField> ();
      newOrderingFields.Add (orderingField);
      foreach (OrderingField field in OrderingFields)
      {
        newOrderingFields.Add (field);
      }
      OrderingFields.Clear();
      OrderingFields.AddRange (newOrderingFields);
      Joins.AddPath (orderingField.FieldDescriptor.SourcePath);
    }

    public SelectedObjectActivator GetSelectedObjectActivator ()
    {
      return ObjectFactory.Create<SelectedObjectActivator>().With (SelectEvaluations);
    }
  }
}