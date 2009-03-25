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
      evaluation = AppendResultModifiers(resultModifiers, evaluation);

      if (evaluation)
      {
        //_commandBuilder.Append ("( ");
        AppendEvaluation (selectEvaluation);
        //_commandBuilder.Append (") ");
      }
    }

    private bool AppendResultModifiers (List<MethodCall> resultModifiers, bool evaluation)
    {
      if (resultModifiers != null)
      {
        
        // TODO: use methodCallRegistry => AppendEvaluation (methodCall)?
        foreach (var methodCall in resultModifiers)
        {
          AppendEvaluation (methodCall);
          //string method = methodCall.EvaluationMethodInfo.Name;
          
          //if (method == "Count")
          //{
          //  _commandBuilder.Append ("COUNT (*) ");
          //  evaluation = false;
          //}
          //else
          //{
          //  if (method == "Distinct")
          //    _commandBuilder.Append ("DISTINCT ");
          //  else if ((method == "First") || (method == "Single")) // TODO: Single must select TOP 2 so that an exception is thrown when more than one element is returned.
          //    _commandBuilder.Append ("TOP 1 ");
          //  else
          //  {
          //    string message = string.Format ("Method '{0}' is not supported.", method);
          //    throw new NotSupportedException (message);
          //  }
          //}
        }
      }
      return evaluation;
    }

    private void AppendEvaluation (IEvaluation selectEvaluation)
    {
      _commandBuilder.AppendEvaluation (selectEvaluation);
      _commandBuilder.Append (" ");
    }
  }
}
