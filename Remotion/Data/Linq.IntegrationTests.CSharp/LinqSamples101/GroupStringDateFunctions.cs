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

namespace Remotion.Data.Linq.IntegrationTests.CSharp.LinqSamples101
{
  internal class GroupStringDateFunctions:Executor
  {
    //This sample uses the + operator to concatenate string fields " +
    //and string literals in forming the Customers' calculated " +
    //Location value.")]
    public void LinqToSqlString01 ()
    {
      var q =
          from c in db.Customers
          select new { c.CustomerID, Location = c.City + ", " + c.Country };

      serializer.Serialize (q);
    }

    //This sample uses the Length property to find all Products whose " +
    //name is shorter than 10 characters.")]
    public void LinqToSqlString02 ()
    {
      var q =
          from p in db.Products
          where p.ProductName.Length < 10
          select p;

      serializer.Serialize (q);
    }

    //This sample uses the Contains method to find all Customers whose " +
    //contact name contains 'Anders'.")]
    public void LinqToSqlString03 ()
    {
      var q =
          from c in db.Customers
          where c.ContactName.Contains ("Anders")
          select c;

      serializer.Serialize (q);
    }

    //This sample uses the IndexOf method to find the first instance of " +
    //a space in each Customer's contact name.")]
    public void LinqToSqlString04 ()
    {
      var q =
          from c in db.Customers
          select new { c.ContactName, SpacePos = c.ContactName.IndexOf (" ") };

      serializer.Serialize (q);
    }

    //This sample uses the StartsWith method to find Customers whose " +
    //contact name starts with 'Maria'.")]
    public void LinqToSqlString05 ()
    {
      var q =
          from c in db.Customers
          where c.ContactName.StartsWith ("Maria")
          select c;

      serializer.Serialize (q);
    }

    //This sample uses the EndsWith method to find Customers whose " +
    //contact name ends with 'Anders'.")]
    public void LinqToSqlString06 ()
    {
      var q =
          from c in db.Customers
          where c.ContactName.EndsWith ("Anders")
          select c;

      serializer.Serialize (q);
    }

    //This sample uses the Substring method to return Product names starting " +
    //from the fourth letter.")]
    public void LinqToSqlString07 ()
    {
      var q =
          from p in db.Products
          select p.ProductName.Substring (3);

      serializer.Serialize (q);
    }

    //This sample uses the Substring method to find Employees whose " +
    //home phone numbers have '555' as the seventh through ninth digits.")]
    public void LinqToSqlString08 ()
    {
      var q =
          from e in db.Employees
          where e.HomePhone.Substring (6, 3) == "555"
          select e;

      serializer.Serialize (q);
    }

    //This sample uses the ToUpper method to return Employee names " +
    //where the last name has been converted to uppercase.")]
    public void LinqToSqlString09 ()
    {
      var q =
          from e in db.Employees
          select new { LastName = e.LastName.ToUpper (), e.FirstName };

      serializer.Serialize (q);
    }

    //This sample uses the ToLower method to return Category names " +
    //that have been converted to lowercase.")]
    public void LinqToSqlString10 ()
    {
      var q =
          from c in db.Categories
          select c.CategoryName.ToLower ();

      serializer.Serialize (q);
    }

    //This sample uses the Trim method to return the first five " +
    //digits of Employee home phone numbers, with leading and " +
    //trailing spaces removed.")]
    public void LinqToSqlString11 ()
    {
      var q =
          from e in db.Employees
          select e.HomePhone.Substring (0, 5).Trim ();

      serializer.Serialize (q);
    }

    //This sample uses the Insert method to return a sequence of " +
    //employee phone numbers that have a ) in the fifth position, " +
    //inserting a : after the ).")]
    public void LinqToSqlString12 ()
    {
      var q =
          from e in db.Employees
          where e.HomePhone.Substring (4, 1) == ")"
          select e.HomePhone.Insert (5, ":");

      serializer.Serialize (q);
    }

    //This sample uses the Remove method to return a sequence of " +
    //employee phone numbers that have a ) in the fifth position, " +
    //removing all characters starting from the tenth character.")]
    public void LinqToSqlString13 ()
    {
      var q =
          from e in db.Employees
          where e.HomePhone.Substring (4, 1) == ")"
          select e.HomePhone.Remove (9);

      serializer.Serialize (q);
    }

    //This sample uses the Remove method to return a sequence of " +
    //employee phone numbers that have a ) in the fifth position, " +
    //removing the first six characters.")]
    public void LinqToSqlString14 ()
    {
      var q =
          from e in db.Employees
          where e.HomePhone.Substring (4, 1) == ")"
          select e.HomePhone.Remove (0, 6);

      serializer.Serialize (q);
    }

    //This sample uses the Replace method to return a sequence of " +
    //Supplier information where the Country field has had " +
    //UK replaced with United Kingdom and USA replaced with " +
    //United States of America.")]
    public void LinqToSqlString15 ()
    {
      var q =
          from s in db.Suppliers
          select new
          {
            s.CompanyName,
            Country = s.Country.Replace ("UK", "United Kingdom")
                               .Replace ("USA", "United States of America")
          };

      serializer.Serialize (q);
    }

    //This sample uses the DateTime's Year property to " +
    //find Orders placed in 1997.")]
    public void LinqToSqlString16 ()
    {
      var q =
          from o in db.Orders
          where o.OrderDate.Value.Year == 1997
          select o;

      serializer.Serialize (q);
    }

    //This sample uses the DateTime's Month property to " +
    //find Orders placed in December.")]
    public void LinqToSqlString17 ()
    {
      var q =
          from o in db.Orders
          where o.OrderDate.Value.Month == 12
          select o;

      serializer.Serialize (q);
    }

    //This sample uses the DateTime's Day property to " +
    //find Orders placed on the 31st day of the month.")]
    public void LinqToSqlString18 ()
    {
      var q =
          from o in db.Orders
          where o.OrderDate.Value.Day == 31
          select o;

      serializer.Serialize (q);
    }

  }
}