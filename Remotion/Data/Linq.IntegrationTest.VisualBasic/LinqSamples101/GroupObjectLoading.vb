' Copyright (c) Microsoft Corporation.  All rights reserved.
Option Infer On
Option Strict On

Imports System.Collections.Generic
Imports System.Data
Imports System.Data.SqlClient
Imports System.IO
Imports System.Linq
Imports System.Linq.Expressions
Imports System.Reflection
Imports Remotion.Data.Linq.IntegrationTests.TestDomain.Northwind
Imports Remotion.Data.Linq.IntegrationTests.Utilities
Imports System.Data.Linq
Imports System.Text

Namespace LinqSamples101

  Public Class GroupObjectLoading
    Inherits Executor

    '  <Category("Object Loading")> _
    '<Title("Deferred Loading - 1")> _
    '<Description("This sample demonstrates how navigating through relationships in " & _
    '             "retrieved objects can end up triggering new queries to the database " & _
    '             "if the data was not requested by the original query.")> _
    Public Sub LinqToSqlObject01()
      Dim custs = From cust In db.Customers _
                  Where cust.City = "Sao Paulo" _
                  Select cust

      For Each cust In custs
        For Each ord In cust.Orders
          serializer.Serialize("CustomerID " & cust.CustomerID & " has an OrderID " & ord.OrderID)
        Next
      Next
    End Sub

    '<Category("Object Loading")> _
    '<Title("LoadWith - Eager Loading - 1")> _
    '<Description("This sample demonstrates how to use LoadWith to request related " & _
    '             "data during the original query so that additional roundtrips to the " & _
    '             "database are not required later when navigating through " & _
    '             "the retrieved objects.")> _
    Public Sub LinqToSqlObject02()

      Dim db2 = New Northwind(connString)
      'db2.Log = Me.OutputStreamWriter

      Dim ds = New DataLoadOptions()
      ds.LoadWith(Of Customer)(Function(cust) cust.Orders)

      db2.LoadOptions = ds

      Dim custs = From cust In db.Customers _
                  Where cust.City = "Sao Paulo"

      For Each cust In custs
        For Each ord In cust.Orders
          serializer.Serialize("CustomerID " & cust.CustomerID & " has an OrderID " & ord.OrderID)
        Next
      Next

    End Sub

    '<Category("Object Loading")> _
    '<Title("Deferred Loading + AssociateWith")> _
    '<Description("This sample demonstrates how navigating through relationships in " & _
    '             "retrieved objects can end up triggering new queries to the database " & _
    '             "if the data was not requested by the original query. Also this sample shows relationship " & _
    '             "objects can be filtered using AssoicateWith when they are deferred loaded.")> _
    Public Sub LinqToSqlObject03()

      Dim db2 As New Northwind(connString)
      'db2.Log = Me.OutputStreamWriter

      Dim ds As New DataLoadOptions()
      ds.AssociateWith(Of Customer)(Function(p) p.Orders.Where(Function(o) o.ShipVia.Value > 1))

      db2.LoadOptions = ds

      Dim custs = From cust In db2.Customers _
                  Where cust.City = "London"



      For Each cust In custs
        For Each ord In cust.Orders
          For Each orderDetail In ord.OrderDetails

            serializer.Serialize(String.Format("CustomerID {0} has an OrderID {1} that ShipVia is {2} with ProductID {3} that has name {4}.", _
                cust.CustomerID, ord.OrderID, ord.ShipVia, orderDetail.ProductID, orderDetail.Product.ProductName))
          Next
        Next
      Next
    End Sub

    '<Category("Object Loading")> _
    '<Title("LoadWith - Eager Loading + Associate With")> _
    '<Description("This sample demonstrates how to use LoadWith to request related " & _
    '             "data during the original query so that additional roundtrips to the " & _
    '             "database are not required later when navigating through " & _
    '             "the retrieved objects. Also this sample shows relationship" & _
    '             "objects can be ordered by using Assoicate With when they are eager loaded.")> _
    Public Sub LinqToSqlObject04()

      Dim db2 = New Northwind(connString)
      'db2.Log = Me.OutputStreamWriter


      Dim ds As New DataLoadOptions()
      ds.LoadWith(Of Customer)(Function(cust) cust.Orders)
      ds.LoadWith(Of Order)(Function(ord) ord.OrderDetails)

      ds.AssociateWith(Of Order)(Function(p) p.OrderDetails.OrderBy(Function(o) o.Quantity))

      db2.LoadOptions = ds

      Dim custs = From cust In db.Customers _
                  Where cust.City = "London"

      For Each cust In custs
        For Each ord In cust.Orders
          For Each orderDetail In ord.OrderDetails
            serializer.Serialize(String.Format("CustomerID {0} has an OrderID {1} with ProductID {2} that has quantity {3}.", _
                cust.CustomerID, ord.OrderID, orderDetail.ProductID, orderDetail.Quantity))
          Next
        Next
      Next

    End Sub


    Private Function isValidProduct(ByVal prod As Product) As Boolean
      Return (prod.ProductName.LastIndexOf("C") = 0)
    End Function

    '<Category("Object Loading")> _
    '<Title("Deferred Loading - (1:M)")> _
    '<Description("This sample demonstrates how navigating through relationships in " & _
    '             "retrieved objects can result in triggering new queries to the database " & _
    '             "if the data was not requested by the original query.")> _
    Public Sub LinqToSqlObject05()
      Dim emps = db.Employees

      For Each emp In emps
        For Each man In emp.Employees
          serializer.Serialize("Employee " & emp.FirstName & " reported to Manager " & man.FirstName)
        Next
      Next
    End Sub

    '<Category("Object Loading")> _
    '<Title("Deferred Loading - (BLOB)")> _
    '<Description("This sample demonstrates how navigating through Link in " & _
    '             "retrieved objects can end up triggering new queries to the database " & _
    '             "if the data type is Link.")> _
    Public Sub LinqToSqlObject06()
      Dim emps = db.Employees

      For Each emp In emps
        serializer.Serialize(emp.Notes)
      Next

    End Sub


    '<Category("Object Loading")> _
    '<Title("Load Override")> _
    '<Description("This samples overrides the partial method LoadProducts in Category class. When products of a category are being loaded, " & _
    '             "LoadProducts is being called to load products that are not discontinued in this category. ")> _
    Public Sub LinqToSqlObject07()

      Dim db2 As New Northwind(connString)

      Dim ds As New DataLoadOptions()

      ds.LoadWith(Of Category)(Function(p) p.Products)
      db2.LoadOptions = ds

      Dim q = From c In db2.Categories _
              Where c.CategoryID < 3

      For Each cat In q
        For Each prod In cat.Products
          serializer.Serialize(String.Format("Category {0} has a ProductID {1} that Discontined = {2}.", _
                            cat.CategoryID, prod.ProductID, prod.Discontinued))
        Next
      Next

    End Sub

  End Class

End Namespace