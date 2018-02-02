//Microsoft Public License (Ms-PL)

//This license governs use of the accompanying software. If you use the software, you
//accept this license. If you do not accept the license, do not use the software.

//1. Definitions
//The terms "reproduce," "reproduction," "derivative works," and "distribution" have the
//same meaning here as under U.S. copyright law.
//A "contribution" is the original software, or any additions or changes to the software.
//A "contributor" is any person that distributes its contribution under this license.
//"Licensed patents" are a contributor's patent claims that read directly on its contribution.

//2. Grant of Rights
//(A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
//each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, 
//prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
//(B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
//each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, 
//sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.

//3. Conditions and Limitations
//(A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
//(B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, your patent license from 
//such contributor to the software ends automatically.
//(C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution notices that are present 
//in the software.
//(D) If you distribute any portion of the software in source code form, you may do so only under this license by including a complete copy of 
//this license with your distribution. If you distribute any portion of the software in compiled or object code form, you may only do so under a 
//license that complies with this license.
//(E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees or conditions. 
//You may have additional consumer rights under your local laws which this license cannot change. To the extent permitted under your local laws,
//the contributors exclude the implied warranties of merchantability, fitness for a particular purpose and non-infringement.

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using Remotion.Linq.IntegrationTests.Common.TestDomain.Northwind;

namespace Remotion.Linq.IntegrationTests.CSharp.LinqSamples101
{
  [TestFixture]
  public class AdvancedTests:TestBase
  {
    ///<summary>
    ///This sample builds a query dynamically to return the contact name of each customer. 
    /// </summary>
    [Test]
    public void LinqToSqlAdvanced01 ()
    {
      ParameterExpression param = Expression.Parameter (typeof (Customer), "c");
      Expression selector = Expression.Property (param, typeof (Customer).GetProperty ("ContactName"));
      Expression pred = Expression.Lambda (selector, param);

      IQueryable<Customer> custs = DB.Customers;
      Expression expr = Expression.Call (typeof (Queryable), "Select", new[] { typeof (Customer), typeof (string) }, Expression.Constant (custs), pred);
      IQueryable<string> query = DB.Customers.AsQueryable ().Provider.CreateQuery<string> (expr);

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    /// <summary>
    /// This sample builds a query dynamically to filter for Customers in London. 
    /// </summary>
    [Test]
    public void LinqToSqlAdvanced02 ()
    {
      IQueryable<Customer> custs = DB.Customers;
      ParameterExpression param = Expression.Parameter (typeof (Customer), "c");
      Expression right = Expression.Constant ("London");
      Expression left = Expression.Property (param, typeof (Customer).GetProperty ("City"));
      Expression filter = Expression.Equal (left, right);
      Expression pred = Expression.Lambda (filter, param);

      Expression expr = Expression.Call (typeof (Queryable), "Where", new[] { typeof (Customer) }, Expression.Constant (custs), pred);
      IQueryable<Customer> query = DB.Customers.AsQueryable ().Provider.CreateQuery<Customer> (expr);
      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    // NOTE: This query contains a bug - the first value assigned to expr is never used. However, this bug stems from the original samples.
    // NOTE: The query therefore does _not_ filter by London.
    /// <summary>
    /// This sample builds a query dynamically to filter for Customers in London
    /// and order them by ContactName.
    /// </summary>
    [Test]
    public void LinqToSqlAdvanced03 ()
    {
      ParameterExpression param = Expression.Parameter (typeof (Customer), "c");
      Expression left = Expression.Property (param, typeof (Customer).GetProperty ("City"));
      Expression right = Expression.Constant ("London");
      Expression filter = Expression.Equal (left, right);
      Expression pred = Expression.Lambda (filter, param);

      IQueryable custs = DB.Customers;

      Expression expr = Expression.Call (typeof (Queryable), "Where",
          new[] { typeof (Customer) }, Expression.Constant (custs), pred);

      expr = Expression.Call (typeof (Queryable), "OrderBy",
          new[] { typeof (Customer), typeof (string) }, custs.Expression, Expression.Lambda (Expression.Property (param, "ContactName"), param));


      IQueryable<Customer> query = DB.Customers.AsQueryable ().Provider.CreateQuery<Customer> (expr);

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    ///<summary>
    ///This sample dynamically builds a Union to return a sequence of all countries where either 
    ///a customer or an employee live.
    ///</summary>
    [Test]
    public void LinqToSqlAdvanced04 ()
    {
      IQueryable<Customer> custs = DB.Customers;
      ParameterExpression param1 = Expression.Parameter (typeof (Customer), "e");
      Expression left1 = Expression.Property (param1, typeof (Customer).GetProperty ("City"));
      Expression pred1 = Expression.Lambda (left1, param1);

      IQueryable<Employee> employees = DB.Employees;
      ParameterExpression param2 = Expression.Parameter (typeof (Employee), "c");
      Expression left2 = Expression.Property (param2, typeof (Employee).GetProperty ("City"));
      Expression pred2 = Expression.Lambda (left2, param2);

      Expression expr1 = Expression.Call (typeof (Queryable), "Select", new[] { typeof (Customer), typeof (string) }, Expression.Constant (custs), pred1);
      Expression expr2 = Expression.Call (typeof (Queryable), "Select", new[] { typeof (Employee), typeof (string) }, Expression.Constant (employees), pred2);

      IQueryable<string> q1 = DB.Customers.AsQueryable ().Provider.CreateQuery<string> (expr1);
      IQueryable<string> q2 = DB.Employees.AsQueryable ().Provider.CreateQuery<string> (expr2);

      var q3 = q1.Union (q2);

      TestExecutor.Execute (q3, MethodBase.GetCurrentMethod ());
    }


    /// <summary>
    /// This sample demonstrates how we insert a new Contact and retrieve the
    /// newly assigned ContactID from the database.
    /// </summary>
    [Test]
    [Explicit ("Not tested: Submit")]
    public void LinqToSqlAdvanced05 ()
    {
      //Console.WriteLine ("ContactID is marked as an identity column");
      //Contact con = new Contact () { CompanyName = "New Era", Phone = "(123)-456-7890" };

      //db.Contacts.InsertOnSubmit (con);
      //db.SubmitChanges ();

      //Console.WriteLine ();
      //Console.WriteLine ("The ContactID of the new record is {0}", con.ContactID);

      //cleanup130 (con.ContactID);

    }
    
    //void cleanup130 (int contactID)
    //{
    //  SetLogging (false);
    //  Contact con = DB.Contacts.Where (c => c.ContactID == contactID).First ();
    //  DB.Contacts.DeleteOnSubmit (con);
    //  DB.SubmitChanges ();
    //}

    ///<summary>
    ///This sample uses orderbyDescending and Take to return the 
    ///discontinued products of the top 10 most expensive products.
    ///</summary>
    [Test]
    public void LinqToSqlAdvanced06 ()
    {
      var prods = from p in DB.Products.OrderByDescending (p => p.UnitPrice).Take (10)
                  where p.Discontinued
                  select p;

      TestExecutor.Execute (prods, MethodBase.GetCurrentMethod ());
    }
  }
}