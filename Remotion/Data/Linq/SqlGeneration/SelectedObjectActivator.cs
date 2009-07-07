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
using Remotion.Data.Linq.Backend.DataObjectModel;

namespace Remotion.Data.Linq.SqlGeneration
{
  public class SelectedObjectActivator
  {
    private readonly IEvaluation _selectEvaluation;
    
    private object[] _values;
    private int _valueIndex;

    public SelectedObjectActivator (IEvaluation selectEvaluation)
    {
      _selectEvaluation = selectEvaluation;
    }

    public virtual object CreateSelectedObject (object[] values)
    {
      _values = values;
      object result = InterpretSelectedData (_selectEvaluation);
      if (values.Length > _valueIndex)
        throw new ArgumentException ("Too many values.", "values");
      return result;
    }

    protected virtual object InterpretSelectedData (IEvaluation selectEvaluation)
    {
      if (selectEvaluation is Constant)
        return InterpretConstant ((Constant) selectEvaluation);
      else if (selectEvaluation is NewObject)
        return InterpretNewObject ((NewObject) selectEvaluation);
      else if (selectEvaluation is Column)
        return InterpretColumn ((Column) selectEvaluation);
      else
        throw new NotSupportedException ("Evaluation type " + selectEvaluation.GetType().Name + " is not supported.");
    }

    protected virtual object InterpretNewObject (NewObject newObject)
    {
      object[] parameters = new object[newObject.ConstructorArguments.Length];

      for (int i = 0; i < parameters.Length; i++)
        parameters[i] = InterpretSelectedData (newObject.ConstructorArguments[i]);

      return newObject.ConstructorInfo.Invoke (parameters);
    }

    protected virtual object InterpretColumn (Column column)
    {
      if (_values.Length <= _valueIndex)
        throw new ArgumentException ("Too few values.", "values");
      object value = _values[_valueIndex];
      _valueIndex++;
      return value;
    }

    protected virtual object InterpretConstant (Constant constant)
    {
      return constant.Value;
    }

  }
}
