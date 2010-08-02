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
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.IntegrationTests.CSharp
{
  /// <summary>
  /// Provides functionality to serialize the result of LINQ tests into a human-readable and machine-readable format for comparison between tests.
  /// </summary>
  public class TestResultSerializer
  {
    private readonly TextWriter _textWriter;

    public TestResultSerializer (TextWriter textWriter)
    {
      ArgumentUtility.CheckNotNull ("textWriter", textWriter);
      _textWriter = textWriter;
    }

    public void Serialize (object value)
    {
      if (value == null)
        _textWriter.WriteLine ("null");
      else if (value is string)
        SerializeString ((string) value);
      else if (value is ValueType)
        _textWriter.WriteLine (value);
      else
        SerializeComplexValue (value);
    }

    private void SerializeString (string value)
    {
      var escapedValue = value.Replace ("'", "''");
      _textWriter.WriteLine ("'" + escapedValue + "'");
    }

    private void SerializeComplexValue (object value)
    {
      Debug.Assert (value != null, "should be handled by caller");
      _textWriter.WriteLine (value.GetType().Name);

      foreach (var memberInfo in value.GetType().GetMembers (BindingFlags.Public | BindingFlags.Instance))
      {
        object memberValue;
        if (TryGetValue (value, memberInfo, out memberValue))
        {
          _textWriter.Write ("  ");
          _textWriter.Write (memberInfo.Name);
          _textWriter.Write (": ");

          Serialize (memberValue);
        }
      }
    }

    private bool TryGetValue (object instance, MemberInfo memberInfo, out object value)
    {
      if (memberInfo.MemberType == MemberTypes.Property)
      {
        value = ((PropertyInfo) memberInfo).GetValue (instance, null);
        return true;
      }
      else if (memberInfo.MemberType == MemberTypes.Field)
      {
        value = ((FieldInfo) memberInfo).GetValue (instance);
        return true;
      }
      else
      {
        value = null;
        return false;
      }
    }
  }
}