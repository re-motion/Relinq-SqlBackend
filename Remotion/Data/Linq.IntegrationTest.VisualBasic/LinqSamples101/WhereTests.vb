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


Namespace LinqSamples101
    Public Class WhereTests
        Inherits TestBase

        'This sample uses a Where clause to filter for Customers in London.
        <Test()>
        Public Sub LinqToSqlWhere01()

            'Only return customers from London
      Dim londonCustomers = From cust In DB.Customers _
            Where cust.City = "London" _
            Select cust.CompanyName, cust.City, cust.Country


            TestExecutor.Execute(londonCustomers, MethodBase.GetCurrentMethod())
        End Sub


        'This sample uses a Where clause to filter for Employees hired
        '"during or after 1994.
        <Test()>
        Public Sub LinqToSqlWhere02()
            Dim hiredAfter1994 = From emp In db.Employees _
                  Where emp.HireDate >= #1/1/1994# _
                  Select emp

            TestExecutor.Execute(hiredAfter1994, MethodBase.GetCurrentMethod())
        End Sub

        'This sample uses a Where clause to filter for Products that have stock below their
        'reorder level and are not discontinued.
        <Test()>
        Public Sub LinqToSqlWhere03()
            Dim needToOrder = From prod In db.Products _
                  Where prod.UnitsInStock <= prod.ReorderLevel _
                        AndAlso Not prod.Discontinued _
                  Select prod

            TestExecutor.Execute(needToOrder, MethodBase.GetCurrentMethod())
        End Sub

        'This sample uses a Where clause to filter out Products that are either
        'discontinued or that have a UnitPrice greater than 10.
        <Test()>
        Public Sub LinqToSqlWhere04()
            Dim prodQuery = From prod In db.Products _
                  Where prod.UnitPrice > 10.0# OrElse prod.Discontinued

            TestExecutor.Execute(prodQuery, MethodBase.GetCurrentMethod())
        End Sub

        'This sample uses two Where clauses to filter out Products that are discontinued 
        'and with UnitPrice greater than 10
        <Test()>
        Public Sub LinqToSqlWhere05()

            Dim prodQuery = From prod In db.Products _
                  Where prod.UnitPrice > 10D _
                  Where prod.Discontinued

            TestExecutor.Execute(prodQuery, MethodBase.GetCurrentMethod())
        End Sub


        'This sample uses First to select the first Shipper in the table.
        <Test()>
        Public Sub LinqToSqlWhere06()
            Dim shipper = db.Shippers.First()

            TestExecutor.Execute(shipper, MethodBase.GetCurrentMethod())
        End Sub


        'This sample uses Take to select the first Customer with CustomerID 'BONAP'.
        <Test()>
        Public Sub LinqToSqlWhere07()
            Dim customer = From cust In db.Customers _
                  Where cust.CustomerID = "BONAP" _
                  Take 1

            TestExecutor.Execute(customer, MethodBase.GetCurrentMethod())
        End Sub

        'This sample uses First to select an Order with freight greater than 10.00.
        <Test()>
        Public Sub LinqToSqlWhere08()
            Dim firstOrd = (From ord In db.Orders _
                  Where ord.Freight > 10D _
                  Select ord).First()

            TestExecutor.Execute(firstOrd, MethodBase.GetCurrentMethod())
        End Sub
    End Class
End Namespace
