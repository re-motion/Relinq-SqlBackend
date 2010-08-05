Option Infer On
Option Strict On

Imports System.IO
Imports System.Linq
Imports System.Data.Linq.Mapping
Imports System.Windows.Forms

Namespace LinqSamples101
  Public Class GroupExternalMapping
    Inherits Executor

    'TODO: OutOfMemoryException due to circular dependency
    '<Category("External Mapping")> _
    '<Title("Load and use an External Mapping")> _
    '<Description("This sample demonstrates how to create a data context that uses an external XML mapping source.")> _
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


