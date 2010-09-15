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
  public class UserDefinedFunctionsTests:TestBase
  {
    /// <summary>
    /// This sample demonstrates using a scalar user-defined function in a projection.
    /// </summary>
    [Test]
    [Ignore ("TODO: RM-3312: The in-memory projection is incorrect if a NamedExpression contains another NamedExpression, entity expression, convert expression, etc.")]
    public void LinqToSqlUserDefined01 ()
    {
      var q = from c in DB.Categories
              select new { c.CategoryID, TotalUnitPrice = DB.Functions.TotalProductUnitPriceByCategory (c.CategoryID) };

      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());
    }

    /// <summary>
    /// This sample demonstrates using a scalar user-defined function in a where clause.
    /// </summary>
    [Test]
    public void LinqToSqlUserDefined02 ()
    {
      var q = from p in DB.Products
              where p.UnitPrice == DB.Functions.MinUnitPriceByCategory (p.CategoryID)
              select p;

      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());
    }

    /// <summary>
    /// This sample demonstrates selecting from a table-valued user-defined function.
    /// </summary>
    [Test]
    [Explicit ("Not tested: User-defined function in first from clause. This cannot be tested because it will always cause Linq-to-Sql to execute the query.")]
    public void LinqToSqlUserDefined03 ()
    {
      var q = from p in DB.Functions.ProductsUnderThisUnitPrice (10.25M)
              where !(p.Discontinued ?? false)
              select p;

      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());
    }

    /// <summary>
    /// This sample demonstrates joining to the results of a table-valued user-defined function.
    /// </summary>
    [Test]
    [Ignore ("TODO: RM-3313: Add a TableInfo type allowing user-defined functions to be used as tables")]
    public void LinqToSqlUserDefined04 ()
    {
      var q = from c in DB.Categories
              join p in DB.Functions.ProductsUnderThisUnitPrice (8.50M) on c.CategoryID equals p.CategoryID into prods
              from p in prods
              select new { c.CategoryID, c.CategoryName, p.ProductName, p.UnitPrice };

      TestExecutor.Execute (q, MethodBase.GetCurrentMethod());
    }
  }
}