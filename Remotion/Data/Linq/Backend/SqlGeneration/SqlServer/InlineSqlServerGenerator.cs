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
using System.Collections.Generic;
using System.Text;
using Remotion.Data.Linq.Backend.SqlGeneration.SqlServer;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Backend.SqlGeneration.SqlServer
{
  // If a fixedCommandBuilder is specified, the SqlServerGenerator can only be used to create one query from one thread. Otherwise, it is
  // stateless and can be used for multiple queries from multiple threads.
  public class InlineSqlServerGenerator : SqlServerGenerator
  {
    private readonly CommandBuilder _fixedCommandBuilder;

    public InlineSqlServerGenerator (IDatabaseInfo databaseInfo, CommandBuilder fixedCommandBuilder, ParseMode parseMode)
      : base (databaseInfo, parseMode)
    {
      ArgumentUtility.CheckNotNull ("fixedCommandBuilder", fixedCommandBuilder);
      _fixedCommandBuilder = fixedCommandBuilder;
    }

    protected override SqlServerGenerationContext CreateContext ()
    {
      return new SqlServerGenerationContext (_fixedCommandBuilder);
    }
  }
}
