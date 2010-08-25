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
using System.Collections.Generic;
using System.Data.Linq.Mapping;
using System.IO;
using System.Reflection;
using Remotion.Data.Linq.IntegrationTests.TestDomain.Northwind;
using Remotion.Data.Linq.IntegrationTests.Utilities;

namespace Remotion.Data.Linq.IntegrationTests.CSharp.LinqSamples101
{
  public class Executor
  {
    protected static readonly string connString = "Data Source=localhost;Initial Catalog=Northwind; Integrated Security=SSPI;";

    protected static NorthwindDataContext db;
    protected static TestResultSerializer serializer;

    public static void Main ()
    {
      InitSample();
      CallAllTypes (Assembly.Load ("Remotion.Data.Linq.IntegrationTests.CSharp"));

      Console.WriteLine ("finished! :)");
      Console.Read();
    }

    private static void InitSample ()
    {
      // Creates a new Northwind object to start fresh with an empty object cache
      // Active ADO.NET connection will be reused by new Northwind object

      TextWriter oldLog;
      if (db == null)
        oldLog = null;
      else
        oldLog = db.Log;

      db = new NorthwindDataContext (connString) { Log = oldLog };
      serializer = new TestResultSerializer (new StreamWriter ("C:\\csharpTestOut.txt"), memberInfo => !memberInfo.IsDefined (typeof (AssociationAttribute), false));
    }

    private static void CallAllTypes (Assembly assembly)
    {
      List<Type> classes = new List<Type> (assembly.GetTypes ());
      classes.Sort ((m1, m2) => m1.Name.CompareTo (m2.Name));

      foreach (var classType in classes)
      {
        if (classType.BaseType != null)
        {
          if (classType.BaseType.Name.Equals ("Executor"))
          {
            Console.WriteLine ("Call Methods Class: " + classType.Name);
            CallAllMethods (classType);
          }
        }
      }
    }

    private static void CallAllMethods (Type testClass)
    {
      List<MethodInfo> methods = new List<MethodInfo>(testClass.GetMethods());
      methods.Sort ((m1, m2) => m1.Name.CompareTo (m2.Name));
      
      foreach (var methodInfo in methods)
      {
        object instance = Activator.CreateInstance (testClass);

        if (methodInfo.Name.Contains ("LinqToSql"))
        {
          Console.WriteLine ("\t Call: " + methodInfo.Name);
          serializer.Serialize  ("Call: " + methodInfo.Name);
          methodInfo.Invoke (instance, null);
        }
      }
    }
  }
}