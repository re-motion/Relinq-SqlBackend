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
using System.Linq;
using System.Reflection;
using System.Text;
using Remotion.Data.Linq.IntegrationTests.Utilities;

namespace Remotion.Data.Linq.IntegrationTests.CSharp.LinqSamples101
{
  class GroupUnionAllIntersect:TestBase
  {
    //This sample uses Concat to return a sequence of all Customer and Employee phone/fax numbers.")]
    public void LinqToSqlUnion01 ()
    {
      var q = (
               from c in DB.Customers
               select c.Phone
              ).Concat (
               from c in DB.Customers
               select c.Fax
              ).Concat (
               from e in DB.Employees
               select e.HomePhone
              );

      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());
    }

    //This sample uses Concat to return a sequence of all Customer and Employee name and phone number mappings.")]
    public void LinqToSqlUnion02 ()
    {
      var q = (
               from c in DB.Customers
               select new { Name = c.CompanyName, c.Phone }
              ).Concat (
               from e in DB.Employees
               select new { Name = e.FirstName + " " + e.LastName, Phone = e.HomePhone }
              );

      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());
    }

    //This sample uses Union to return a sequence of all countries that either Customers or Employees are in.")]
    public void LinqToSqlUnion03 ()
    {
      var q = (
               from c in DB.Customers
               select c.Country
              ).Union (
               from e in DB.Employees
               select e.Country
              );

      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());
    }

    //This sample uses Intersect to return a sequence of all countries that both Customers and Employees live in.")]
    public void LinqToSqlUnion04 ()
    {
      var q = (
               from c in DB.Customers
               select c.Country
              ).Intersect (
               from e in DB.Employees
               select e.Country
              );

      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());
    }

    //This sample uses Except to return a sequence of all countries that Customers live in but no Employees live in.")]
    public void LinqToSqlUnion05 ()
    {
      var q = (
               from c in DB.Customers
               select c.Country
              ).Except (
               from e in DB.Employees
               select e.Country
              );

      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());
    }
  }
}
