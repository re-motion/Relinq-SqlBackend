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

using NUnit.Framework;

namespace Remotion.Linq.IntegrationTests.CSharp.LinqSamples101
{
  [TestFixture]
  public class ExternalMappingTests : TestBase
  {
    /// <summary>
    /// This sample demonstrates how to create a data context that uses an external XML mapping source.
    /// </summary>
    [Test]
    [Explicit ("Not tested: External mapping")]
    public void LinqToSqlExternal01 ()
    {
      ////load the mapping source
      //string path = Path.GetFullPath (Path.Combine (Environment.CurrentDirectory, @"..\..\..\Linq.IntegrationTests\TestDomain\Northwind\NorthwindMapped.map"));

      //XmlMappingSource mappingSource = XmlMappingSource.FromXml (File.ReadAllText (path));

      //// create context using mapping source
      //Mapped.NorthwindMapped nw = new Mapped.NorthwindMapped (db.Connection, mappingSource);

      //// demonstrate use of an externally-mapped entity 
      //serializer.Serialize ("****** Externally-mapped entity ******");
      //Mapped.Order order = nw.Orders.First ();
      //serializer.Serialize (order);

      //// demonstrate use of an externally-mapped inheritance hierarchy
      //var contacts = from c in nw.Contacts
      //               where c is Mapped.EmployeeContact
      //               select c;
      //serializer.Serialize (Environment.NewLine);
      //serializer.Serialize ("****** Externally-mapped inheritance hierarchy ******");
      //foreach (var contact in contacts)
      //{
      //  serializer.Serialize (String.Format ("Company name: {0}", contact.CompanyName));
      //  serializer.Serialize (String.Format ("Phone: {0}", contact.Phone));
      //  serializer.Serialize (String.Format ("This is a {0}", contact.GetType ()));
      //  serializer.Serialize (Environment.NewLine);
      //}

      //// demonstrate use of an externally-mapped stored procedure
      //serializer.Serialize (Environment.NewLine);
      //serializer.Serialize ("****** Externally-mapped stored procedure ******");
      //foreach (Mapped.CustOrderHistResult result in nw.CustomerOrderHistory ("ALFKI"))
      //{
      //  serializer.Serialize (result);
      //}

      //// demonstrate use of an externally-mapped scalar user defined function
      //serializer.Serialize (Environment.NewLine);
      //serializer.Serialize ("****** Externally-mapped scalar UDF ******");
      //var totals = from c in nw.Categories
      //             select new { c.CategoryID, TotalUnitPrice = nw.TotalProductUnitPriceByCategory (c.CategoryID) };
      //serializer.Serialize (totals);

      //// demonstrate use of an externally-mapped table-valued user-defined function
      //serializer.Serialize (Environment.NewLine);
      //serializer.Serialize ("****** Externally-mapped table-valued UDF ******");
      //var products = from p in nw.ProductsUnderThisUnitPrice (9.75M)
      //               where p.SupplierID == 8
      //               select p;
      //serializer.Serialize (products);
    }
     
  }
}