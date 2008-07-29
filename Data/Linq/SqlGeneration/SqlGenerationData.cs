/* Copyright (C) 2005 - 2008 rubicon informationstechnologie gmbh
 *
 * This program is free software: you can redistribute it and/or modify it under 
 * the terms of the re:motion license agreement in license.txt. If you did not 
 * receive it, please visit http://www.re-motion.org/licensing.
 * 
 * Unless otherwise provided, this software is distributed on an "AS IS" basis, 
 * WITHOUT WARRANTY OF ANY KIND, either express or implied. 
 */

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
      SelectEvaluation = null;
      OrderingFields = new List<OrderingField> ();
      Joins = new JoinCollection ();
      LetEvaluations = new List<LetData> ();
    }

    public bool Distinct { get; set; }
    public IEvaluation SelectEvaluation { get; private set; }
    public List<IColumnSource> FromSources { get; private set; }
    public ICriterion Criterion { get; set; }
    public List<OrderingField> OrderingFields { get; private set; }
    public JoinCollection Joins { get; private set; }
    public List<LetData> LetEvaluations { get; private set; }
    public ParseMode ParseMode { get; set; }
    
    public void SetSelectClause (bool distinct, List<FieldDescriptor> fieldDescriptors, IEvaluation evaluation)
    {
      if (SelectEvaluation != null)
        throw new InvalidOperationException ("There can only be one select clause.");

      Distinct = distinct;
      SelectEvaluation = evaluation;

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
      return ObjectFactory.Create<SelectedObjectActivator>().With (SelectEvaluation);
    }
  }
}
