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
using Remotion.Utilities;

namespace Remotion.Linq.SqlBackend.SqlGeneration
{
  public struct CommandParameter
  {
    private readonly string _name;
    private readonly object _value;

    public CommandParameter (string name, object value)
    {
      ArgumentUtility.CheckNotNullOrEmpty (nameof(name), name);

      _name = name;
      _value = value;
    }

    public string Name
    {
      get { return _name; }
    }

    public object Value
    {
      get { return _value; }
    }

    public override string ToString ()
    {
      return Name + "=" + Value;
    }
  }
}