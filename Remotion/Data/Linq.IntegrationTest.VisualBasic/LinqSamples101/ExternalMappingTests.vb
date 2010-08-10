' This file is part of the re-motion Core Framework (www.re-motion.org)
' Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
' 
' The re-motion Core Framework is free software; you can redistribute it 
' and/or modify it under the terms of the GNU Lesser General Public License 
' as published by the Free Software Foundation; either version 2.1 of the 
' License, or (at your option) any later version.
' 
' re-motion is distributed in the hope that it will be useful, 
' but WITHOUT ANY WARRANTY; without even the implied warranty of 
' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
' GNU Lesser General Public License for more details.
' 
' You should have received a copy of the GNU Lesser General Public License
' along with re-motion; if not, see http://www.gnu.org/licenses.
' 
Option Infer On
Option Strict On

Imports NUnit.Framework

Imports Remotion.Data.Linq.IntegrationTests


Namespace LinqSamples101
  Public Class ExternalMappingTests
    Inherits TestBase
    'TODO: OutOfMemoryException due to circular dependency (Circular Dependency (Order -> Customer -> Order -> Customer...)
    'This sample demonstrates how to create a data context that uses an external XML mapping source.")> _
    '<Test>
    'Public Sub LinqToSqlExternal01()
    '  ' load the mapping source
    '  Dim path2 = Path.Combine(Application.StartupPath, "..\..\..\Linq.IntegrationTests\TestDomain\Northwind\NorthwindMapped.map")
    '  Dim mappingSource As XmlMappingSource = XmlMappingSource.FromXml(File.ReadAllText(path2))
    '  'Notice that each type in the NorthwindMapped.map file contains the fully-qualified
    '  'name (i.e. it *includes* the Root Namespace).  So for example, the following element in the
    '  'mapping file:
    '  '    <Type Name="Mapped.AddressSplit">
    '  '
    '  'becomes:
    '  '    <Type Name="SampleQueries.Mapped.AddressSplit">
    '  '
    '  'since SampleQueries is the Root Namespace defined for this project
    '  ' create context using mapping source
    '  Dim nw As New Mapped.NorthwindMapped(db.Connection, mappingSource)
    '  ' demonstrate use of an externally-mapped entity 
    '  Console.WriteLine("****** Externally-mapped entity ******")
    '  Dim order As Mapped.Order = nw.Orders.First()
    '  serializer.Serialize(order)
    '  ' demonstrate use of an externally-mapped inheritance hierarchy
    '  Dim contacts = From c In nw.Contacts _
    '                 Where TypeOf c Is Mapped.EmployeeContact _
    '                 Select c
    '  Console.WriteLine()
    '  Console.WriteLine("****** Externally-mapped inheritance hierarchy ******")
    '  For Each contact In contacts
    '    Console.WriteLine("Company name: {0}", contact.CompanyName)
    '    Console.WriteLine("Phone: {0}", contact.Phone)
    '    Console.WriteLine("This is a {0}", contact.GetType())
    '    Console.WriteLine()
    '  Next
    '  ' demonstrate use of an externally-mapped stored procedure
    '  Console.WriteLine()
    '  Console.WriteLine("****** Externally-mapped stored procedure ******")
    '  For Each result In nw.CustomerOrderHistory("ALFKI")
    '    serializer.Serialize(result)
    '  Next
    '  ' demonstrate use of an externally-mapped scalar user defined function
    '  Console.WriteLine()
    '  Console.WriteLine("****** Externally-mapped scalar UDF ******")
    '  Dim totals = From c In nw.Categories _
    '               Select c.CategoryID, TotalUnitPrice = nw.TotalProductUnitPriceByCategory(c.CategoryID)
    '  serializer.Serialize(totals)
    '  ' demonstrate use of an externally-mapped table-valued user-defined function
    '  Console.WriteLine()
    '  Console.WriteLine("****** Externally-mapped table-valued UDF ******")
    '  Dim products = From p In nw.ProductsUnderThisUnitPrice(9.75D) _
    '                 Where p.SupplierID = 8 _
    '                 Select p
    '  serializer.Serialize(products)
    'End Sub
  End Class
End Namespace


