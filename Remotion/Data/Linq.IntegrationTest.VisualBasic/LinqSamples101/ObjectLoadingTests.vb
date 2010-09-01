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

Imports System.Reflection
Imports Remotion.Data.Linq.IntegrationTests
Imports Remotion.Data.Linq.IntegrationTests.TestDomain.Northwind


Imports System.Data.Linq

Namespace LinqSamples101
  <TestFixture()>
  Public Class ObjectLoadingTests
    Inherits TestBase

    ''WORKAROUND: tests trigger when navigating -> not needed
    'This sample demonstrates how navigating through relationships in
    'retrieved objects can end up triggering new queries to the database
    'if the data was not requested by the original query.
    '<Test>
    'Public Sub LinqToSqlObject01()
    '  Dim custs = From cust In db.Customers _
    '        Where cust.City = "Sao Paulo" _
    '        Select cust

    '  TestExecutor.Execute(custs, MethodBase.GetCurrentMethod())
    'End Sub

    'TODO: not needed (?)
    ''This sample demonstrates how to use LoadWith to request related " & _
    ''             "data during the original query so that additional roundtrips to the " & _
    ''             "database are not required later when navigating through " & _
    ''             "the retrieved objects.")> _
    '<Test>
    'Public Sub LinqToSqlObject02()

    '  Dim db2 = New Northwind(connString)
    '  'db2.Log = Me.OutputStreamWriter

    '  Dim ds = New DataLoadOptions()
    '  ds.LoadWith(Of Customer)(Function(cust) cust.Orders)

    '  db2.LoadOptions = ds

    '  Dim custs = From cust In DB.Customers _
    '        Where cust.City = "Sao Paulo"

    '  For Each cust In custs
    '    For Each ord In cust.Orders
    '      TestExecutor.Execute("CustomerID " & cust.CustomerID & " has an OrderID " & ord.OrderID)
    '    Next
    '  Next


    'End Sub

    'TODO: not needed
    ''This sample demonstrates how navigating through relationships in
    ''retrieved objects can end up triggering new queries to the database
    ''if the data was not requested by the original query. Also this sample shows relationship
    ''objects can be filtered using AssoicateWith when they are deferred loaded.
    '<Test>
    'Public Sub LinqToSqlObject03()

    '  Dim db2 As New Northwind(connString)
    '  'db2.Log = Me.OutputStreamWriter

    '  Dim ds As New DataLoadOptions()
    '  ds.AssociateWith(Of Customer)(Function(p) p.Orders.Where(Function(o) o.ShipVia.Value > 1))

    '  db2.LoadOptions = ds

    '  Dim custs = From cust In db2.Customers _
    '        Where cust.City = "London"


    '  For Each cust In custs
    '    For Each ord In cust.Orders
    '      For Each orderDetail In ord.OrderDetails

    '        TestExecutor.Execute( _
    '                              String.Format( _
    'CustomerID {0} has an OrderID {1} that ShipVia is {2} with ProductID {3} that has name {4}.", _
    '                                             cust.CustomerID, ord.OrderID, ord.ShipVia, orderDetail.ProductID, _
    '                                             orderDetail.Product.ProductName))
    '      Next
    '    Next
    '  Next
    'End Sub

    'TODO: not needed
    ''This sample demonstrates how to use LoadWith to request related " & _
    ''             "data during the original query so that additional roundtrips to the " & _
    ''             "database are not required later when navigating through " & _
    ''             "the retrieved objects. Also this sample shows relationship" & _
    ''             "objects can be ordered by using Assoicate With when they are eager loaded.")> _
    '<Test>
    'Public Sub LinqToSqlObject04()

    '  Dim db2 = New Northwind(connString)
    '  'db2.Log = Me.OutputStreamWriter


    '  Dim ds As New DataLoadOptions()
    '  ds.LoadWith(Of Customer)(Function(cust) cust.Orders)
    '  ds.LoadWith(Of Order)(Function(ord) ord.OrderDetails)

    '  ds.AssociateWith(Of Order)(Function(p) p.OrderDetails.OrderBy(Function(o) o.Quantity))

    '  db2.LoadOptions = ds

    '  Dim custs = From cust In DB.Customers _
    '        Where cust.City = "London"

    '  For Each cust In custs
    '    For Each ord In cust.Orders
    '      For Each orderDetail In ord.OrderDetails
    '        TestExecutor.Execute( _
    '                              String.Format( _
    'CustomerID {0} has an OrderID {1} with ProductID {2} that has quantity {3}.", _
    '                                             cust.CustomerID, ord.OrderID, orderDetail.ProductID, _
    '                                             orderDetail.Quantity))
    '      Next
    '    Next
    '  Next

    'End Sub


    'Private Function isValidProduct(ByVal prod As Product) As Boolean
    'Return (prod.ProductName.LastIndexOf("C") = 0)
    'End Sub

    'WORKAROUND: tests trigger when navigating -> not needed
    ''This sample demonstrates how navigating through relationships in " & _
    ''             "retrieved objects can result in triggering new queries to the database " & _
    ''             "if the data was not requested by the original query.")> _
    '<Test>
    'Public Sub LinqToSqlObject05()
    '  Dim emps = DB.Employees

    '  For Each emp In emps
    '    For Each man In emp.Employees
    '      TestExecutor.Execute("Employee " & emp.FirstName & " reported to Manager " & man.FirstName)
    '    Next
    '  Next
    'End Sub

    'WORKAROUND: tests trigger when navigating -> not needed
    'This sample demonstrates how navigating through Link in " & _
    ''             "retrieved objects can end up triggering new queries to the database " & _
    ''             "if the data type is Link.")> _
    '<Test>
    'Public Sub LinqToSqlObject06()
    '  Dim emps = DB.Employees

    '  For Each emp In emps
    '    TestExecutor.Execute(emp.Notes)
    '  Next

    'End Sub

    'TODO: not needed
    ''This samples overrides the partial method LoadProducts in Category class. When products of a category are being loaded,
    ''LoadProducts is being called to load products that are not discontinued in this category.
    '<Test>
    'Public Sub LinqToSqlObject07()

    '  Dim db2 As New Northwind(connString)

    '  Dim ds As New DataLoadOptions()

    '  ds.LoadWith(Of Category)(Function(p) p.Products)
    '  db2.LoadOptions = ds

    '  Dim q = From c In db2.Categories _
    '        Where c.CategoryID < 3

    '  For Each cat In q
    '    For Each prod In cat.Products
    '      TestExecutor.Execute(String.Format("Category {0} has a ProductID {1} that Discontined = {2}.", _
    '                                           cat.CategoryID, prod.ProductID, prod.Discontinued))
    '    Next
    '  Next

    'End Sub
  End Class
End Namespace