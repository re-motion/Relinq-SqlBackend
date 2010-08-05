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
using Remotion.Data.Linq.IntegrationTests.TestDomain.Northwind;

namespace Remotion.Data.Linq.IntegrationTests.CSharp.LinqSamples101
{
  internal class InheritanceTests:TestBase
  {

    //This sample returns all contacts where the city is London.")]
    public void LinqToSqlInheritance01 ()
    {
      var cons = from c in DB.Contacts
                 select c;

      //List<String> strings = new List<String>();
      //foreach (var con in cons)
      //{
      //  strings.Add (string.Format ("Company name: {0}, Phone: {1}, This is a {2}", con.CompanyName, con.Phone, con.GetType ()));
      //}
      TestExecutor.Execute (cons, MethodBase.GetCurrentMethod ());

    }

    //This sample uses OfType to return all customer contacts.")]
    public void LinqToSqlInheritance02 ()
    {
      var cons = from c in DB.Contacts.OfType<CustomerContact> ()
                 select c;

      TestExecutor.Execute (cons, MethodBase.GetCurrentMethod ());
    }

    //This sample uses IS to return all shipper contacts.")]
    public void LinqToSqlInheritance03 ()
    {
      var cons = from c in DB.Contacts
                 where c is ShipperContact
                 select c;

      TestExecutor.Execute (cons, MethodBase.GetCurrentMethod ());
    }

    //This sample uses AS to return FullContact or null.")]
    public void LinqToSqlInheritance04 ()
    {
      var cons = from c in DB.Contacts
                 select c as FullContact;

      TestExecutor.Execute (cons, MethodBase.GetCurrentMethod ());
    }

    //This sample uses a cast to retrieve customer contacts who live in London.")]
    public void LinqToSqlInheritance05 ()
    {
      var cons = from c in DB.Contacts
                 where c.ContactType == "Customer" && ((CustomerContact) c).City == "London"
                 select c;

      TestExecutor.Execute (cons, MethodBase.GetCurrentMethod ());
    }
  }
}