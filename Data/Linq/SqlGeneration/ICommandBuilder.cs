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
  public interface ICommandBuilder
  {
    void Append (string text);
    void AppendEvaluation (IEvaluation evaluation);
    void AppendSeparatedItems<T> (IEnumerable<T> items, Action<T> appendAction);
    void AppendEvaluations (IEnumerable<IEvaluation> evaluations);
    void AppendConstant (Constant constant);
    CommandParameter AddParameter (object value);
  }
}