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


Namespace LinqSamples101
  Public Class GroupExistsInAnyAllContains
    Inherits Executor
    '<Category("EXISTS/IN/ANY/ALL")> _
    ' <Title("Any - Simple")> _
    ' <Description("This sample uses the Any operator to return only Customers that have no Orders.")> _
    Public Sub LinqToSqlExists01()
      Dim custQuery = From cust In db.Customers _
            Where Not cust.Orders.Any()

      serializer.Serialize(custQuery)
    End Sub

    '<Category("EXISTS/IN/ANY/ALL")> _
    '<Title("Any - Conditional")> _
    '<Description("This sample uses Any to return only Categories that have " & _
    '             "at least one Discontinued product.")> _
    Public Sub LinqToSqlExists02()
      Dim prodQuery = From cust In db.Categories _
            Where (From prod In cust.Products Where prod.Discontinued).Any()

      serializer.Serialize(prodQuery)
    End Sub

    '<Category("EXISTS/IN/ANY/ALL")> _
    '<Title("All - Conditional")> _
    '<Description("This sample uses All to return Customers whom all of their orders " & _
    '             "have been shipped to their own city or whom have no orders.")> _
    Public Sub LinqToSqlExists03()
      Dim ordQuery = From cust In db.Customers _
            Where cust.Orders.All(Function(ord) ord.ShipCity = cust.City)

      serializer.Serialize(ordQuery)
    End Sub

    '<Category("Exists/In/Any/All/Contains")> _
    '<Title("Contains - One Object")> _
    '<Description("This sample uses Contain to find which Customer contains an order with OrderID 10248.")> _
    Public Sub LinqToSqlExists04()

      Dim order = (From o In db.Orders _
            Where o.OrderID = 10248).First()

      Dim q = db.Customers.Where(Function(p) p.Orders.Contains(order)).ToList()

      For Each cust In q
        For Each ord In cust.Orders

          serializer.Serialize(String.Format("Customer {0} has OrderID {1}.", _
                                               cust.CustomerID, ord.OrderID))
        Next
      Next
    End Sub

    '<Category("Exists/In/Any/All/Contains")> _
    '<Title("Contains - Multiple values")> _
    '<Description("This sample uses Contains to find customers whose city is Seattle, London, Paris or Vancouver.")> _
    Public Sub LinqToSqlExists05()
      Dim cities = New String() {"Seattle", "London", "Vancouver", "Paris"}

      Dim q = db.Customers.Where(Function(p) cities.Contains(p.City)).ToList()

      serializer.Serialize(q)
    End Sub
  End Class
End Namespace
