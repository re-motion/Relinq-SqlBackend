// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
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
using System.Linq;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Backend.DataObjectModel;
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
    }

    public IEvaluation SelectEvaluation { get; private set; }
    public List<IColumnSource> FromSources { get; private set; }
    public ICriterion Criterion { get; set; }
    public List<OrderingField> OrderingFields { get; private set; }
    public JoinCollection Joins { get; private set; }
    public ParseMode ParseMode { get; set; }
    public List<ResultOperatorBase> ResultOperators { get; set; }
    
    public void SetSelectEvaluation (IEvaluation evaluation, List<FieldDescriptor> fieldDescriptors)
    {
      if (SelectEvaluation != null)
        throw new InvalidOperationException ("There can only be one select clause.");

      SelectEvaluation = evaluation;

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

    public void PrependOrderingFields (IEnumerable<OrderingField> orderingFields)
    {
      OrderingFields.InsertRange (0, orderingFields);
      foreach (var orderingField in orderingFields)
        Joins.AddPath (orderingField.FieldDescriptor.SourcePath);
    }

    public SelectedObjectActivator GetSelectedObjectActivator ()
    {
      return ObjectFactory.Create<SelectedObjectActivator>(ParamList.Create (SelectEvaluation));
    }
  }
}
