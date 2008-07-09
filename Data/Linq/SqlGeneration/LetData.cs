/* Copyright (C) 2005 - 2008 rubicon informationstechnologie gmbh
 *
 * This program is free software: you can redistribute it and/or modify it under 
 * the terms of the re:motion license agreement in license.txt. If you did not 
 * receive it, please visit http://www.re-motion.org/licensing.
 * 
 * Unless otherwise provided, this software is distributed on an "AS IS" basis, 
 * WITHOUT WARRANTY OF ANY KIND, either express or implied. 
 */

using System.Collections.Generic;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Utilities;

namespace Remotion.Data.Linq.SqlGeneration
{
  public class LetData
  {
    public LetColumnSource CorrespondingColumnSource;

    public LetData (IEvaluation evaluation, string name,LetColumnSource columnSource)
    {
      ArgumentUtility.CheckNotNull ("evaluation", evaluation);
      ArgumentUtility.CheckNotNull ("name", name);
      ArgumentUtility.CheckNotNull ("columnSource", columnSource);
      
      Evaluation = evaluation;
      Name = name;
      CorrespondingColumnSource = columnSource;
    }

    public IEvaluation Evaluation { get; private set; }
    public string Name { get; private set; }
  }
}
