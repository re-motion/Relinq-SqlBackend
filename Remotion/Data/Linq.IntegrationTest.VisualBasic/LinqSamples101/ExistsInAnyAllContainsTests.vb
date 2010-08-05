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
  Public Class ExistsInAnyAllContainsTests
    Inherits TestBase
    'This sample uses the Any operator to return only Customers that have no Orders.
    <Test()>
    Public Sub LinqToSqlExists01()
      Dim custQuery = From cust In DB.Customers _
            Where Not cust.Orders.Any()

      TestExecutor.Execute(custQuery, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses Any to return only Categories that have
    'at least one Discontinued product.")> _
    <Test()>
    Public Sub LinqToSqlExists02()
      Dim prodQuery = From cust In db.Categories _
            Where (From prod In cust.Products Where prod.Discontinued).Any()

      TestExecutor.Execute(prodQuery, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses All to return Customers whom all of their orders " & _
    'have been shipped to their own city or whom have no orders.")> _
    <Test()>
    Public Sub LinqToSqlExists03()
      Dim ordQuery = From cust In db.Customers _
            Where cust.Orders.All(Function(ord) ord.ShipCity = cust.City)

      TestExecutor.Execute(ordQuery, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses Contain to find which Customer contains an order with OrderID 10248.")> _
    <Test()>
    Public Sub LinqToSqlExists04()

      Dim order = (From o In db.Orders _
            Where o.OrderID = 10248).First()

      Dim q = db.Customers.Where(Function(p) p.Orders.Contains(order)).ToList()

      TestExecutor.Execute(New With {order, q}, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses Contains to find customers whose city is Seattle, London, Paris or Vancouver.")> _
    <Test()>
    Public Sub LinqToSqlExists05()
      Dim cities = New String() {"Seattle", "London", "Vancouver", "Paris"}

      Dim q = db.Customers.Where(Function(p) cities.Contains(p.City)).ToList()

      TestExecutor.Execute(q, MethodBase.GetCurrentMethod())
    End Sub
  End Class
End Namespace
