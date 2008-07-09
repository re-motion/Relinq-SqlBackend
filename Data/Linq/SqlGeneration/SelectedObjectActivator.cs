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
using Remotion.Data.Linq.DataObjectModel;

namespace Remotion.Data.Linq.SqlGeneration
{
  public class SelectedObjectActivator
  {
    private readonly List<IEvaluation> _selectEvaluations;
    
    private object[] _values;
    private int _valueIndex;

    public SelectedObjectActivator (List<IEvaluation> selectEvaluations)
    {
      _selectEvaluations = selectEvaluations;
    }

    public virtual object CreateSelectedObject (object[] values)
    {
      _values = values;
      object result = InterpretSelectedData (_selectEvaluations[0]);
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
