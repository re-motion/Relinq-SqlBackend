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
using System.Reflection;
using NUnit.Framework;

namespace Remotion.Data.Linq.IntegrationTests.CSharp.LinqSamples101
{
  [TestFixture]
  public class GroupStringDateFunctions:TestBase
  {
    /// <summary>
    /// This sample uses the + operator to concatenate string fields and string literals in forming the Customers' calculated Location value.
    /// </summary>
    [Test]
    public void LinqToSqlString01 ()
    {
      var q =
          from c in DB.Customers
          select new { c.CustomerID, Location = c.City + ", " + c.Country };

      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());
    }

    /// <summary>
    /// This sample uses the Length property to find all Products whose name is shorter than 10 characters.
    /// </summary>
    [Test]
    public void LinqToSqlString02 ()
    {
      var q =
          from p in DB.Products
          where p.ProductName.Length < 10
          select p;

      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());
    }

    /// <summary>
    /// This sample uses the Contains method to find all Customers whose contact name contains 'Anders'.
    /// </summary>
    [Test]
    public void LinqToSqlString03 ()
    {
      var q =
          from c in DB.Customers
          where c.ContactName.Contains ("Anders")
          select c;

      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());
    }

    /// <summary>
    /// This sample uses the IndexOf method to find the first instance of a space in each Customer's contact name.
    /// </summary>
    [Test]
    public void LinqToSqlString04 ()
    {
      var q =
          from c in DB.Customers
          select new { c.ContactName, SpacePos = c.ContactName.IndexOf (" ") };

      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());
    }

    /// <summary>
    /// This sample uses the StartsWith method to find Customers whose contact name starts with 'Maria'.
    /// </summary>
    [Test]
    public void LinqToSqlString05 ()
    {
      var q =
          from c in DB.Customers
          where c.ContactName.StartsWith ("Maria")
          select c;

      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());
    }

    /// <summary>
    /// This sample uses the EndsWith method to find Customers whose contact name ends with 'Anders'.
    /// </summary>
    [Test]
    public void LinqToSqlString06 ()
    {
      var q =
          from c in DB.Customers
          where c.ContactName.EndsWith ("Anders")
          select c;

      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());
    }

    /// <summary>
    /// This sample uses the Substring method to return Product names starting from the fourth letter.
    /// </summary>
    [Test]
    public void LinqToSqlString07 ()
    {
      var q =
          from p in DB.Products
          select p.ProductName.Substring (3);

      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());
    }

    /// <summary>
    /// This sample uses the Substring method to find Employees whose home phone numbers have '555' as the seventh through ninth digits.
    /// </summary>
    [Test]
    public void LinqToSqlString08 ()
    {
      var q =
          from e in DB.Employees
          where e.HomePhone.Substring (6, 3) == "555"
          select e;

      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());
    }

    /// <summary>
    /// This sample uses the ToUpper method to return Employee names where the last name has been converted to uppercase.
    /// </summary>
    [Test]
    public void LinqToSqlString09 ()
    {
      var q =
          from e in DB.Employees
          select new { LastName = e.LastName.ToUpper (), e.FirstName };

      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());
    }

    /// <summary>
    /// This sample uses the ToLower method to return Category names that have been converted to lowercase.
    /// </summary>
    [Test]
    public void LinqToSqlString10 ()
    {
      var q =
          from c in DB.Categories
          select c.CategoryName.ToLower ();

      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());
    }

    /// <summary>
    /// This sample uses the Trim method to return the first five digits of Employee home phone numbers, 
    /// with leading and trailing spaces removed.
    /// </summary>
    [Test]
    public void LinqToSqlString11 ()
    {
      var q =
          from e in DB.Employees
          select e.HomePhone.Substring (0, 5).Trim ();

      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());
    }

    /// <summary>
    /// This sample uses the Insert method to return a sequence of employee phone numbers that have a ) 
    /// in the fifth position, inserting a : after the ).
    /// </summary>
    [Test]
    public void LinqToSqlString12 ()
    {
      var q =
          from e in DB.Employees
          where e.HomePhone.Substring (4, 1) == ")"
          select e.HomePhone.Insert (5, ":");

      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());
    }

    /// <summary>
    /// This sample uses the Remove method to return a sequence of employee phone numbers that have a ) 
    /// in the fifth position, removing all characters starting from the tenth character.
    /// </summary>
    [Test]
    public void LinqToSqlString13 ()
    {
      var q =
          from e in DB.Employees
          where e.HomePhone.Substring (4, 1) == ")"
          select e.HomePhone.Remove (9);

      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());
    }

    /// <summary>
    /// This sample uses the Remove method to return a sequence of employee phone numbers that have a ) 
    /// in the fifth position, removing the first six characters.
    /// </summary>
    [Test]
    public void LinqToSqlString14 ()
    {
      var q =
          from e in DB.Employees
          where e.HomePhone.Substring (4, 1) == ")"
          select e.HomePhone.Remove (0, 6);

      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());
    }

    /// <summary>
    /// This sample uses the Replace method to return a sequence of Supplier information where the Country field
    /// has had UK replaced with United Kingdom and USA replaced with United States of America.
    /// </summary>
    [Test]
    public void LinqToSqlString15 ()
    {
      var q =
          from s in DB.Suppliers
          select new
          {
            s.CompanyName,
            Country = s.Country.Replace ("UK", "United Kingdom")
                               .Replace ("USA", "United States of America")
          };

      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());
    }

    /// <summary>
    /// This sample uses the DateTime's Year property to find Orders placed in 1997.
    /// </summary>
    [Test]
    public void LinqToSqlString16 ()
    {
      var q =
          from o in DB.Orders
          where o.OrderDate.Value.Year == 1997
          select o;

      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());
    }

    /// <summary>
    /// This sample uses the DateTime's Month property to find Orders placed in December.
    /// </summary>
    [Test]
    public void LinqToSqlString17 ()
    {
      var q =
          from o in DB.Orders
          where o.OrderDate.Value.Month == 12
          select o;

      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());
    }

    /// <summary>
    /// This sample uses the DateTime's Day property to find Orders placed on the 31st day of the month.
    /// </summary>
    [Test]
    public void LinqToSqlString18 ()
    {
      var q =
          from o in DB.Orders
          where o.OrderDate.Value.Day == 31
          select o;

      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());
    }

  }
}