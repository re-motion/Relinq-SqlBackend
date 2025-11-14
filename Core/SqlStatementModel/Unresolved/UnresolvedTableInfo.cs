// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (c) rubicon IT GmbH, www.rubicon.eu
// 
// re-linq is free software; you can redistribute it and/or modify it under 
// the terms of the GNU Lesser General Public License as published by the 
// Free Software Foundation; either version 2.1 of the License, 
// or (at your option) any later version.
// 
// re-linq is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-linq; if not, see http://www.gnu.org/licenses.
// 
using System;
using System.Linq.Expressions;
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Utilities;

namespace Remotion.Linq.SqlBackend.SqlStatementModel.Unresolved
{
  /// <summary>
  /// <see cref="UnresolvedTableInfo"/> holds a <see cref="ConstantExpression"/> representing the data source defined by a LINQ query.
  /// </summary>
  public class UnresolvedTableInfo : ITableInfo
  {
    private readonly Type _itemType;

    public UnresolvedTableInfo (Type itemType)
    {
      ArgumentUtility.CheckNotNull (nameof(itemType), itemType);

      _itemType = itemType;
    }

    public virtual Type ItemType
    {
      get { return _itemType;  }
    }

    public virtual ITableInfo Accept (ITableInfoVisitor visitor)
    {
      ArgumentUtility.CheckNotNull (nameof(visitor), visitor);
      return visitor.VisitUnresolvedTableInfo (this);
    }

    public virtual IResolvedTableInfo GetResolvedTableInfo ()
    {
      throw new InvalidOperationException ("This table has not yet been resolved; call the resolution step first.");
    }

    public override string ToString ()
    {
      return string.Format ("TABLE({0})", ItemType.Name);
    }
  }
}