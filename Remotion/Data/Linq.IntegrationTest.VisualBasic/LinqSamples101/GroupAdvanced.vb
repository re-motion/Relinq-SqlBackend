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
  Public Class GroupAdvanced
    Inherits Executor
    '<Category("Advanced")> _
    '<Title("Dynamic query - Select")> _
    '<Description("This sample builds a query dynamically to return the contact name of each customer.")> _
    Public Sub LinqToSqlAdvanced01()
      Dim param = Expression.Parameter(GetType(Customer), "c")
      Dim selector = Expression.Property(param, GetType(Customer).GetProperty("ContactName"))
      Dim pred = Expression.Lambda(selector, param)

      Dim custs = db.Customers
      Dim expr = Expression.Call(GetType(Queryable), "Select", New Type() {GetType(Customer), GetType(String)}, Expression.Constant(custs), pred)
      Dim query = custs.AsQueryable().Provider.CreateQuery(Of String)(expr)

      Dim cmd = db.GetCommand(query)
      serializer.Serialize("Generated T-SQL:")
      serializer.Serialize(cmd.CommandText)
      serializer.Serialize(Environment.NewLine)


      serializer.Serialize(query)
    End Sub

    '<Category("Advanced")> _
    '<Title("Dynamic query - Where")> _
    '<Description("This sample builds a query dynamically to filter for Customers in London.")> _
    Public Sub LinqToSqlAdvanced02()

      Dim custs = db.Customers
      Dim param = Expression.Parameter(GetType(Customer), "c")
      Dim right = Expression.Constant("London")
      Dim left = Expression.Property(param, GetType(Customer).GetProperty("City"))
      Dim filter = Expression.Equal(left, right)
      Dim pred = Expression.Lambda(filter, param)

      Dim expr = Expression.Call(GetType(Queryable), "Where", New Type() {GetType(Customer)}, Expression.Constant(custs), pred)
      Dim query = db.Customers.AsQueryable().Provider.CreateQuery(Of Customer)(expr)
      serializer.Serialize(query)
    End Sub


    '<Category("Advanced")> _
    '<Title("Dynamic query - OrderBy")> _
    '<Description("This sample builds a query dynamically to filter for Customers in London" & _
    '             " and order them by ContactName.")> _
    Public Sub LinqToSqlAdvanced03()

      Dim param = Expression.Parameter(GetType(Customer), "c")

      Dim left = Expression.Property(param, GetType(Customer).GetProperty("City"))
      Dim right = Expression.Constant("London")
      Dim filter = Expression.Equal(left, right)
      Dim pred = Expression.Lambda(filter, param)

      Dim custs As IQueryable = db.Customers

      Dim expr = Expression.Call(GetType(Queryable), "Where", _
                                 New Type() {GetType(Customer)}, _
                                 Expression.Constant(custs), pred)

      expr = Expression.Call(GetType(Queryable), "OrderBy", _
                             New Type() {GetType(Customer), GetType(String)}, _
                             custs.Expression, _
                             Expression.Lambda(Expression.Property(param, "ContactName"), param))


      Dim query = db.Customers.AsQueryable().Provider.CreateQuery(Of Customer)(expr)

      serializer.Serialize(query)
    End Sub

    '<Category("Advanced")> _
    '<Title("Dynamic query - Union")> _
    '<Description("This sample dynamically builds a Union to return a sequence of all countries where either " & _
    '             "a customer or an employee live.")> _
    Public Sub LinqToSqlAdvanced04()

      Dim custs = db.Customers
      Dim param1 = Expression.Parameter(GetType(Customer), "c")
      Dim left1 = Expression.Property(param1, GetType(Customer).GetProperty("City"))
      Dim pred1 = Expression.Lambda(left1, param1)

      Dim employees = db.Employees
      Dim param2 = Expression.Parameter(GetType(Employee), "e")
      Dim left2 = Expression.Property(param2, GetType(Employee).GetProperty("City"))
      Dim pred2 = Expression.Lambda(left2, param2)

      Dim expr1 = Expression.Call(GetType(Queryable), "Select", New Type() {GetType(Customer), GetType(String)}, Expression.Constant(custs), pred1)
      Dim expr2 = Expression.Call(GetType(Queryable), "Select", New Type() {GetType(Employee), GetType(String)}, Expression.Constant(employees), pred2)

      Dim custQuery1 = db.Customers.AsQueryable().Provider.CreateQuery(Of String)(expr1)
      Dim empQuery1 = db.Employees.AsQueryable().Provider.CreateQuery(Of String)(expr2)

      Dim finalQuery = custQuery1.Union(empQuery1)

      serializer.Serialize(finalQuery)
    End Sub

    '<Category("Advanced")> _
    '<Title("Nested in FROM")> _
    '<Description("This sample uses OrderByDescending and Take to return the " & _
    '             "discontinued products of the top 10 most expensive products.")> _
    Public Sub LinqToSqlAdvanced06()
      Dim prods = From prod In db.Products.OrderByDescending(Function(p) p.UnitPrice) _
                  Take 10 _
                  Where prod.Discontinued

      serializer.Serialize(prods)
    End Sub
  End Class
End Namespace
