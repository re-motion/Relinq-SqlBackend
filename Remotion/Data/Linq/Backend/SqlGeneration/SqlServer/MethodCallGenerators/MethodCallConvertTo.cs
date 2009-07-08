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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Remotion.Data.Linq.Backend.DataObjectModel;

namespace Remotion.Data.Linq.Backend.SqlGeneration.SqlServer.MethodCallGenerators
{
  public class MethodCallConvertTo : IMethodCallSqlGenerator
  {
    private Dictionary<Type, string> _mappingTypes;

    public MethodCallConvertTo ()
    {
      _mappingTypes = new Dictionary<Type, string>();
      _mappingTypes.Add (typeof (string), "nvarchar(max)");
      _mappingTypes.Add (typeof (bool), "bit");
      _mappingTypes.Add (typeof (Int64), "bigint");
      _mappingTypes.Add (typeof (DateTime), "date");
      _mappingTypes.Add (typeof (double), "FLOAT");
      _mappingTypes.Add (typeof (int), "int");
      _mappingTypes.Add (typeof (decimal), "numeric");
      _mappingTypes.Add (typeof (char), "nvarchar(1)");
      _mappingTypes.Add (typeof (byte), "tinyint");
    }

    public void GenerateSql (MethodCall methodCall, ICommandBuilder commandBuilder)
    {
      if (methodCall.Arguments.Count != 1)
        throw new ArgumentException ("Wrong number of arguments in evaluation");

      Type type = methodCall.EvaluationMethodInfo.ReturnType;
      string exp;
      if (_mappingTypes.TryGetValue (type, out exp))
      {
        commandBuilder.Append ("CONVERT(" + exp + ",");
        commandBuilder.AppendEvaluation (methodCall.Arguments[0]);
        commandBuilder.Append (") ");
      }
      else
        throw new NotSupportedException ("TypeCast is not supported by linq parser.");
    }

    public IEnumerable<MethodInfo> GetSupportedConvertMethods ()
    {
      var methodNames = _mappingTypes.Keys.ToLookup (t => "To" + t.Name);
      return from m in typeof (Convert).GetMethods()
             where m.GetParameters().Length == 1 && methodNames.Contains (m.Name)
             select m;
    }
  }
}