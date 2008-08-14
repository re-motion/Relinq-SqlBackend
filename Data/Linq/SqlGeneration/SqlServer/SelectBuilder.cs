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
using Remotion.Utilities;
using Remotion.Data.Linq.DataObjectModel;

namespace Remotion.Data.Linq.SqlGeneration.SqlServer
{
  public class SelectBuilder : ISelectBuilder
  {
    private readonly CommandBuilder _commandBuilder;

    public SelectBuilder (CommandBuilder commandBuilder)
    {
      ArgumentUtility.CheckNotNull ("commandBuilder", commandBuilder);
      _commandBuilder = commandBuilder;
    }

    public void BuildSelectPart (IEvaluation selectEvaluation, List<MethodCall> resultModifiers)
    {
      ArgumentUtility.CheckNotNull ("selectEvaluation", selectEvaluation);
      bool evaluation = true;
      _commandBuilder.Append ("SELECT ");
      //at the moment list may only has one method
      if (resultModifiers != null)
      {
        foreach (var methodCall in resultModifiers)
        {
          string method = methodCall.EvaluationMethodInfo.Name;
          
          if (method == "Count")
          {
            _commandBuilder.Append ("COUNT (*) ");
            evaluation = false;
          }
          else
          {
            if (method == "Distinct")
              _commandBuilder.Append ("DISTINCT ");
            else if ((method == "First") || (method == "Single"))
              _commandBuilder.Append ("TOP 1 ");
          else
            {
              string message = string.Format ("Method '{0}' is not supported.", method);
              throw new NotSupportedException (message);
            }
          }
        }
      }

      if (evaluation)
      {
        //_commandBuilder.Append ("( ");
        AppendEvaluation (selectEvaluation);
        //_commandBuilder.Append (") ");
      }
    }
    
    private void AppendEvaluation (IEvaluation selectEvaluation)
    {
      _commandBuilder.AppendEvaluation (selectEvaluation);
      _commandBuilder.Append (" ");
    }
  }
}
