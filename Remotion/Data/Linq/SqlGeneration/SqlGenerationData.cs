// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2008 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// version 3.0 as published by the Free Software Foundation.
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
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.Parsing;
using Remotion.Mixins;
using Remotion.Reflection;

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

    public IEvaluation SelectEvaluation { get; private set; }
    public List<IColumnSource> FromSources { get; private set; }
    public ICriterion Criterion { get; set; }
    public List<OrderingField> OrderingFields { get; private set; }
    public JoinCollection Joins { get; private set; }
    public List<LetData> LetEvaluations { get; private set; }
    public ParseMode ParseMode { get; set; }
    public List<MethodCall> ResultModifiers { get; set; }
    
    public void SetSelectClause (List<MethodCall> resultModifiers, List<FieldDescriptor> fieldDescriptors, IEvaluation evaluation)
    {
      if (SelectEvaluation != null)
        throw new InvalidOperationException ("There can only be one select clause.");

      //needed for correct order when generating sql code
      if (resultModifiers != null)
        resultModifiers.Reverse();
      ResultModifiers = resultModifiers;
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
      return ObjectFactory.Create<SelectedObjectActivator>(ParamList.Create (SelectEvaluation));
    }
  }
}
