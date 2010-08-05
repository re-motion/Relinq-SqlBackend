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
using System.Text;

namespace Remotion.Data.Linq.IntegrationTests.Utilities
{
  public class ComparisonResult
  {
    private readonly bool _isEqual;
    private readonly string _expected;
    private readonly string _actual;

    public ComparisonResult (bool isEqual, string expected, string actual)
    {
      _isEqual = isEqual;
      _actual = actual;
      _expected = expected;
    }

    public string Actual
    {
      get { return _actual; }
    }

    public string Expected
    {
      get { return _expected; }
    }

    public bool IsEqual
    {
      get { return _isEqual; }
    }

    public string GetDiffSet ()
    {
      var output = new StringBuilder();
      output.AppendLine ("expected:");
      output.Append (_expected);
      output.AppendLine ("actual:");
      output.Append (_actual);
      return output.ToString();
    }
  }
}