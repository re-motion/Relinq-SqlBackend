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
  public class ExistsInAnyAllContainsTests:TestBase
  {
    ///<summary>
    ///This sample uses Any to return only Customers that have no Orders.
    ///</summary>
    [Test]
    public void LinqToSqlExists01 ()
    {
      var q =
          from c in DB.Customers
          where !c.Orders.Any ()
          select c;

      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());
    }

    /// <summary>
    /// This sample uses Any to return only Categories that have at least one Discontinued product.
    /// </summary>
    [Test]
    [Ignore ("TODO RM-3798: SQL Backend: InvalidOperationException is thrown when a comparison or join condition involves a nullable and a non-nullable expression")]
    public void LinqToSqlExists02 ()
    {
      var q =
          from c in DB.Categories
          where c.Products.Any (p => p.Discontinued)
          select c;

      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());
    }

    /// <summary>
    /// This sample uses All to return Customers whom all of their orders have been shipped to their own city or whom have no orders.
    /// </summary>
    [Test]
    public void LinqToSqlExists03 ()
    {
      var q =
          from c in DB.Customers
          where c.Orders.All (o => o.ShipCity == c.City)
          select c;

      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());
    }

    /// <summary>
    /// This sample uses Contain to find which Customer contains an order with OrderID 10248.
    /// </summary>
    [Test]
    public void LinqToSqlExists04 ()
    {
      var order = (from o in DB.Orders
                   where o.OrderID == 10248
                   select o).First ();

      var q = DB.Customers.Where (p => p.Orders.Contains (order)).ToList ();

      TestExecutor.Execute (new { order, q }, MethodBase.GetCurrentMethod());
    }

    /// <summary>
    /// This sample uses Contains to find customers whose city is Seattle, London, Paris or Vancouver.
    /// </summary>
    [Test]
    public void LinqToSqlExists05 ()
    {
      var cities = new[] { "Seattle", "London", "Vancouver", "Paris" };
      var q = DB.Customers.Where (p => cities.Contains (p.City)).ToList ();

      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());
    }
  }
}
