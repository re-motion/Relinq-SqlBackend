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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Remotion.Data.Linq.IntegrationTests.TestDomain.Northwind;

namespace Remotion.Data.Linq.IntegrationTests.CSharp.LinqSamples101
{
  [TestFixture]
  public class ConversionOperatorsTests:TestBase
  {
    private bool isValidProduct (Product p)
    {
      return p.ProductName.LastIndexOf ('C') == 0;
    }


    ///<summary>
    ///This sample uses AsEnumerable so that the client-side IEnumerable<T> implementation of Where is used, 
    ///instead of the default IQueryable<T> implementation which would be converted to SQL and executed on the server.
    ///This is necessary because the where clause references a user-defined client-side method, isValidProduct, 
    ///which cannot be converted to SQL.
    ///</summary>
    [Test]
    public void LinqToSqlConversion01 ()
    {
      var q =
          from p in DB.Products.AsEnumerable()
          where isValidProduct (p)
          select p;

      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());
    }

    ///<summary>
    ///This sample uses ToArray to immediately evaluate a query into an array and get the 3rd element.
    ///</summary>
    [Test]
    public void LinqToSqlConversion02 ()
    {
      var q =
          from c in DB.Customers
          where c.City == "London"
          select c;

      Customer[] qArray = q.ToArray();
      TestExecutor.Execute (qArray[3], MethodBase.GetCurrentMethod());
    }

    ///<summary>
    ///This sample uses ToList to immediately evaluate a query into a List<T>.
    ///</summary>
    [Test]
    [Ignore ("Bug or missing feature in Relinq - InvalidCastException - Unable to cast System.Byte[] to System.Data.Linq.Binary")]
    public void LinqToSqlConversion03 ()
    {
      var q =
          from e in DB.Employees
          where e.HireDate >= new DateTime (1994, 1, 1)
          select e;

      List<Employee> qList = q.ToList();
      TestExecutor.Execute (qList, MethodBase.GetCurrentMethod ());
    }

    ///<summary>
    ///This sample uses ToDictionary to immediately evaluate a query and a key expression into an Dictionary<K, T>.
    ///</summary>
    [Test]
    public void LinqToSqlConversion04 ()
    {
      var q =
          from p in DB.Products
          where p.UnitsInStock <= p.ReorderLevel && !p.Discontinued
          select p;


      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());
    }
  }
}