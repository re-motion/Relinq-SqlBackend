// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// as published by the Free Software Foundation; either version 2.1 of the 
// License, or (at your option) any later version.
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
using Remotion.Data.Linq.Backend.DataObjectModel;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Backend.SqlGeneration.SqlServer
{
  public class WhereBuilder : IWhereBuilder
  {
    private readonly CommandBuilder _commandBuilder;
    private readonly IDatabaseInfo _databaseInfo;
    private readonly BinaryConditionBuilder _builder;

    public WhereBuilder (CommandBuilder commandBuilder, IDatabaseInfo databaseInfo)
    {
      ArgumentUtility.CheckNotNull ("commandBuilder", commandBuilder);
      ArgumentUtility.CheckNotNull ("databaseInfo", databaseInfo);
      _commandBuilder = commandBuilder;
      _databaseInfo = databaseInfo;
      _builder = new BinaryConditionBuilder (_commandBuilder);
    }

    public BinaryConditionBuilder Builder
    {
      get { return _builder; }
    }

    public IDatabaseInfo DatabaseInfo
    {
      get { return _databaseInfo; }
    }

    public void BuildWherePart (ICriterion criterion)
    {
      if (criterion != null)
      {
        _commandBuilder.Append (" WHERE ");
  
        if (criterion is Constant)
        {
          Constant constant = (Constant) criterion;
          if (constant.Value == null)
            throw new NotSupportedException ("NULL constants are not supported as WHERE conditions.");
          else
            _commandBuilder.AppendEvaluation (constant);
        }
        else if (criterion is Column)
        {
          _commandBuilder.AppendEvaluation (criterion);
          _commandBuilder.Append ("=1");
        }
        else
        {
          _commandBuilder.AppendEvaluation (criterion);
        }
      }
    }
  }
}
