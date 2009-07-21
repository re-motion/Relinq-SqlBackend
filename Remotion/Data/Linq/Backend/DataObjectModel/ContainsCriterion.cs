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
using System;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Backend.DataObjectModel
{
  public class ContainsCriterion : ICriterion
  {
    public ContainsCriterion (SubQuery subQuery, IEvaluation item)
    {
      ArgumentUtility.CheckNotNull ("subQuery", subQuery);
      ArgumentUtility.CheckNotNull ("item", item);

      if (subQuery.Alias != null)
        throw new ArgumentException ("SubQueries used in a ContainsCriterion must not have an alias set.");

      SubQuery = subQuery;
      Item = item;
    }

    public SubQuery SubQuery { get; private set; }
    public IEvaluation Item { get; private set; }

    public void Accept (IEvaluationVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);
      visitor.VisitContainsCriterion (this);
    }

    public override bool Equals (object obj)
    {
      var other = obj as ContainsCriterion;
      return other != null && Item.Equals (other.Item) && SubQuery.Equals (other.SubQuery);
    }

    public override int GetHashCode ()
    {
      return EqualityUtility.GetRotatedHashCode (Item.GetHashCode(), SubQuery.GetHashCode());
    }
  }
}