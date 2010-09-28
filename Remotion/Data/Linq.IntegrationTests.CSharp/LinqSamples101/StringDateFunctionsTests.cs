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
    [Ignore ("RM-3308: The SQL generated for some string manipulation functions doesn't deal with spaces correctly")]
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
    [Ignore ("RM-3309: Support for additional string manipulation routines: Trim, Insert")]
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
    [Ignore ("RM-3309: Support for additional string manipulation routines: Trim, Insert")]
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
    [Ignore ("RM-3268: Support for Nullable<T>.HasValue and Nullable<T>.Value")]
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
    [Ignore ("RM-3268: Support for Nullable<T>.HasValue and Nullable<T>.Value")]
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
    [Ignore ("RM-3268: Support for Nullable<T>.HasValue and Nullable<T>.Value")]
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