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

Imports System.Reflection
Imports Remotion.Data.Linq.IntegrationTests
Imports Remotion.Data.Linq.IntegrationTests.TestDomain.Northwind


Imports System.Data.Linq

Namespace LinqSamples101
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