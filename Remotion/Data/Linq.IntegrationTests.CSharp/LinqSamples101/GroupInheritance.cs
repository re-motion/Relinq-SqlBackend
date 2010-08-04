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
using System.Linq;
using Remotion.Data.Linq.IntegrationTests.TestDomain.Northwind;

namespace Remotion.Data.Linq.IntegrationTests.CSharp.LinqSamples101
{
  internal class GroupInheritance:Executor
  {

    //This sample returns all contacts where the city is London.")]
    public void LinqToSqlInheritance01 ()
    {
      var cons = from c in db.Contacts
                 select c;

      foreach (var con in cons)
      {
        serializer.Serialize (string.Format ("Company name: {0}", con.CompanyName));
        serializer.Serialize (string.Format ("Phone: {0}", con.Phone));
        serializer.Serialize (string.Format ("This is a {0}", con.GetType ()));
        serializer.Serialize (Environment.NewLine);
      }
    }

    //This sample uses OfType to return all customer contacts.")]
    public void LinqToSqlInheritance02 ()
    {
      var cons = from c in db.Contacts.OfType<CustomerContact> ()
                 select c;

      serializer.Serialize (cons);
    }

    //This sample uses IS to return all shipper contacts.")]
    public void LinqToSqlInheritance03 ()
    {
      var cons = from c in db.Contacts
                 where c is ShipperContact
                 select c;

      serializer.Serialize (cons);
    }

    //This sample uses AS to return FullContact or null.")]
    public void LinqToSqlInheritance04 ()
    {
      var cons = from c in db.Contacts
                 select c as FullContact;

      serializer.Serialize (cons);
    }

    //This sample uses a cast to retrieve customer contacts who live in London.")]
    public void LinqToSqlInheritance05 ()
    {
      var cons = from c in db.Contacts
                 where c.ContactType == "Customer" && ((CustomerContact) c).City == "London"
                 select c;

      serializer.Serialize (cons);
    }
  }
}