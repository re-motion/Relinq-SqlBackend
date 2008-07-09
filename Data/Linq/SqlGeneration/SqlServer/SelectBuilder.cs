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

    public void BuildSelectPart (List<IEvaluation> selectEvaluations,bool distinct)
    {
      ArgumentUtility.CheckNotNull ("selectEvaluations", selectEvaluations);
      ArgumentUtility.CheckNotNull ("distinct", distinct);
      
      if (distinct)
        _commandBuilder.Append ("SELECT DISTINCT ");
      else
        _commandBuilder.Append ("SELECT ");

      if (selectEvaluations.Count == 0)
        throw new InvalidOperationException ("The query does not select any fields from the data source.");

      _commandBuilder.AppendEvaluations (selectEvaluations);
      _commandBuilder.Append(" ");
    }
  }
}
