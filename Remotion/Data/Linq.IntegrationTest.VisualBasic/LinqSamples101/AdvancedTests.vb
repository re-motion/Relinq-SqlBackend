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

Imports System.Linq.Expressions
Imports Remotion.Data.Linq.IntegrationTests
Imports Remotion.Data.Linq.IntegrationTests.TestDomain.Northwind
Imports System.Reflection

Namespace LinqSamples101
  Public Class AdvancedTests
    Inherits TestBase

    'This sample builds a query dynamically to return the contact name of each customer.
    <Test()>
    Public Sub LinqToSqlAdvanced01()
      Dim param = Expression.Parameter(GetType(Customer), "c")
      Dim selector = Expression.Property(param, GetType(Customer).GetProperty("ContactName"))
      Dim pred = Expression.Lambda(selector, param)

      Dim custs = db.Customers
      Dim _
        expr = _
          Expression.Call(GetType(Queryable), "Select", New Type() {GetType(Customer), GetType(String)}, _
                           Expression.Constant(custs), pred)
      Dim query = custs.AsQueryable().Provider.CreateQuery(Of String)(expr)

      Dim cmd = DB.GetCommand(query)

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    'This sample builds a query dynamically to filter for Customers in London.
    <Test()>
    Public Sub LinqToSqlAdvanced02()

      Dim custs = db.Customers
      Dim param = Expression.Parameter(GetType(Customer), "c")
      Dim right = Expression.Constant("London")
      Dim left = Expression.Property(param, GetType(Customer).GetProperty("City"))
      Dim filter = Expression.Equal(left, right)
      Dim pred = Expression.Lambda(filter, param)

      Dim _
        expr = _
          Expression.Call(GetType(Queryable), "Where", New Type() {GetType(Customer)}, Expression.Constant(custs), _
                           pred)
      Dim query = DB.Customers.AsQueryable().Provider.CreateQuery(Of Customer)(expr)

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub


    'This sample builds a query dynamically to filter for Customers in London and order them by ContactName.
    <Test()>
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

      TestExecutor.Execute(query, MethodBase.GetCurrentMethod())
    End Sub

    'This sample dynamically builds a Union to return a sequence of all countries where either a customer or an employee live.
    <Test()>
    Public Sub LinqToSqlAdvanced04()

      Dim custs = db.Customers
      Dim param1 = Expression.Parameter(GetType(Customer), "c")
      Dim left1 = Expression.Property(param1, GetType(Customer).GetProperty("City"))
      Dim pred1 = Expression.Lambda(left1, param1)

      Dim employees = db.Employees
      Dim param2 = Expression.Parameter(GetType(Employee), "e")
      Dim left2 = Expression.Property(param2, GetType(Employee).GetProperty("City"))
      Dim pred2 = Expression.Lambda(left2, param2)

      Dim _
        expr1 = _
          Expression.Call(GetType(Queryable), "Select", New Type() {GetType(Customer), GetType(String)}, _
                           Expression.Constant(custs), pred1)
      Dim _
        expr2 = _
          Expression.Call(GetType(Queryable), "Select", New Type() {GetType(Employee), GetType(String)}, _
                           Expression.Constant(employees), pred2)

      Dim custQuery1 = db.Customers.AsQueryable().Provider.CreateQuery(Of String)(expr1)
      Dim empQuery1 = db.Employees.AsQueryable().Provider.CreateQuery(Of String)(expr2)

      Dim finalQuery = custQuery1.Union(empQuery1)

      TestExecutor.Execute(finalQuery, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses OrderByDescending and Take to return the discontinued products of the top 10 most expensive products.
    <Test()>
    Public Sub LinqToSqlAdvanced06()
      Dim prods = From prod In DB.Products.OrderByDescending(Function(p) p.UnitPrice) _
            Take 10 _
            Where prod.Discontinued

      TestExecutor.Execute(prods, MethodBase.GetCurrentMethod())
    End Sub
  End Class
End Namespace
