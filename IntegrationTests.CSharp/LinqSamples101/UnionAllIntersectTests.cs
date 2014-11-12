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
using System.Reflection;
using NUnit.Framework;

namespace Remotion.Linq.IntegrationTests.CSharp.LinqSamples101
{
  [TestFixture]
  public class UnionAllIntersectTests:TestBase
  {
    /// <summary>
    /// This sample uses Concat to return a sequence of all Customer and Employee phone/fax numbers.
    /// </summary>
    [Test]
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

    /// <summary>
    /// This sample uses Concat to return a sequence of all Customer and Employee name and phone number mappings.
    /// </summary>
    [Test]
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

    /// <summary>
    /// This sample uses Union to return a sequence of all countries that either Customers or Employees are in.
    /// </summary>
    [Test]
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

    /// <summary>
    /// This sample uses Intersect to return a sequence of all countries that both Customers and Employees live in.
    /// </summary>
    [Test]
    [Ignore ("RMLNQSQL-62: Support for the Intersect and Except query operators")]
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

    /// <summary>
    /// This sample uses Except to return a sequence of all countries that Customers live in but no Employees live in.
    /// </summary>
    [Test]
    [Ignore ("RMLNQSQL-62: Support for the Intersect and Except query operators")]
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
