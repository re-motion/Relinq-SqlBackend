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
using Remotion.Linq.SqlBackend.SqlStatementModel.Resolved;
using Remotion.Linq.SqlBackend.Utilities;
using Remotion.Utilities;

namespace Remotion.Linq.SqlBackend.SqlStatementModel.Unresolved
{
  /// <summary>
  /// <see cref="UnresolvedGroupReferenceTableInfo"/> constitutes an implementation of <see cref="ITableInfo"/> for data sources returning
  /// items from a sequence produced by another <see cref="SqlTable"/> called the <see cref="ReferencedGroupSource"/>.
  /// </summary>
  public class UnresolvedGroupReferenceTableInfo : ITableInfo
  {
    private readonly Type _itemType;
    private readonly SqlTableBase _referencedGroupSource;

    public UnresolvedGroupReferenceTableInfo (SqlTableBase referencedGroupSource)
    {
      ArgumentUtility.CheckNotNull (nameof(referencedGroupSource), referencedGroupSource);

      _referencedGroupSource = referencedGroupSource;
      _itemType = ReflectionUtility.GetItemTypeOfClosedGenericIEnumerable (referencedGroupSource.ItemType, "referencedGroupSource");
    }

    public Type ItemType
    {
      get { return _itemType; }
    }

    public SqlTableBase ReferencedGroupSource
    {
      get { return _referencedGroupSource; }
    }

    public IResolvedTableInfo GetResolvedTableInfo ()
    {
      throw new InvalidOperationException ("This table has not yet been resolved; call the resolution step first.");
    }

    public ITableInfo Accept (ITableInfoVisitor visitor)
    {
      ArgumentUtility.CheckNotNull (nameof(visitor), visitor);

      return visitor.VisitUnresolvedGroupReferenceTableInfo (this);
    }

    public override string ToString ()
    {
      return string.Format ("GROUP-REF-TABLE({0})", new SqlTableReferenceExpression(_referencedGroupSource));
    }

  }
}