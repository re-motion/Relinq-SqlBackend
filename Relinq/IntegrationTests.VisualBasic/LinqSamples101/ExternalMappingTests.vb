'Microsoft Public License (Ms-PL)

'This license governs use of the accompanying software. If you use the software, you
'accept this license. If you do not accept the license, do not use the software.

'1. Definitions
'The terms "reproduce," "reproduction," "derivative works," and "distribution" have the
'same meaning here as under U.S. copyright law.
'A "contribution" is the original software, or any additions or changes to the software.
'A "contributor" is any person that distributes its contribution under this license.
'"Licensed patents" are a contributor's patent claims that read directly on its contribution.

'2. Grant of Rights
'(A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
'each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, 
'prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
'(B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
'each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, 
'sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.

'3. Conditions and Limitations
'(A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
'(B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, your patent license from 
'such contributor to the software ends automatically.
'(C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution notices that are present 
'in the software.
'(D) If you distribute any portion of the software in source code form, you may do so only under this license by including a complete copy of 
'this license with your distribution. If you distribute any portion of the software in compiled or object code form, you may only do so under a 
'license that complies with this license.
'(E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees or conditions. 
'You may have additional consumer rights under your local laws which this license cannot change. To the extent permitted under your local laws,
'the contributors exclude the implied warranties of merchantability, fitness for a particular purpose and non-infringement.

Option Infer On
Option Strict On

Imports NUnit.Framework

Imports Remotion.Linq.IntegrationTests.Common


Namespace LinqSamples101
  <TestFixture()> _
  Public Class ExternalMappingTests
    Inherits TestBase
    'This sample demonstrates how to create a data context that uses an external XML mapping source.
    <Test()> _
    <Explicit("Not tested: External mapping")> _
    Public Sub LinqToSqlExternal01()
      '' load the mapping source
      'Dim path2 = Path.Combine(Application.StartupPath, "..\..\..\Linq.IntegrationTests\TestDomain\Northwind\NorthwindMapped.map")
      'Dim mappingSource As XmlMappingSource = XmlMappingSource.FromXml(File.ReadAllText(path2))
      ''Notice that each type in the NorthwindMapped.map file contains the fully-qualified
      ''name (i.e. it *includes* the Root Namespace).  So for example, the following element in the
      ''mapping file:
      ''    <Type Name="Mapped.AddressSplit">
      ''
      ''becomes:
      ''    <Type Name="SampleQueries.Mapped.AddressSplit">
      ''
      ''since SampleQueries is the Root Namespace defined for this project
      '' create context using mapping source
      'Dim nw As New Mapped.NorthwindMapped(db.Connection, mappingSource)
      '' demonstrate use of an externally-mapped entity 
      'Console.WriteLine("****** Externally-mapped entity ******")
      'Dim order As Mapped.Order = nw.Orders.First()
      'serializer.Serialize(order)
      '' demonstrate use of an externally-mapped inheritance hierarchy
      'Dim contacts = From c In nw.Contacts _
      '               Where TypeOf c Is Mapped.EmployeeContact _
      '               Select c
      'Console.WriteLine()
      'Console.WriteLine("****** Externally-mapped inheritance hierarchy ******")
      'For Each contact In contacts
      '  Console.WriteLine("Company name: {0}", contact.CompanyName)
      '  Console.WriteLine("Phone: {0}", contact.Phone)
      '  Console.WriteLine("This is a {0}", contact.GetType())
      '  Console.WriteLine()
      'Next
      '' demonstrate use of an externally-mapped stored procedure
      'Console.WriteLine()
      'Console.WriteLine("****** Externally-mapped stored procedure ******")
      'For Each result In nw.CustomerOrderHistory("ALFKI")
      '  serializer.Serialize(result)
      'Next
      '' demonstrate use of an externally-mapped scalar user defined function
      'Console.WriteLine()
      'Console.WriteLine("****** Externally-mapped scalar UDF ******")
      'Dim totals = From c In nw.Categories _
      '             Select c.CategoryID, TotalUnitPrice = nw.TotalProductUnitPriceByCategory(c.CategoryID)
      'serializer.Serialize(totals)
      '' demonstrate use of an externally-mapped table-valued user-defined function
      'Console.WriteLine()
      'Console.WriteLine("****** Externally-mapped table-valued UDF ******")
      'Dim products = From p In nw.ProductsUnderThisUnitPrice(9.75D) _
      '               Where p.SupplierID = 8 _
      '               Select p
      'serializer.Serialize(products)
    End Sub
  End Class
End Namespace


