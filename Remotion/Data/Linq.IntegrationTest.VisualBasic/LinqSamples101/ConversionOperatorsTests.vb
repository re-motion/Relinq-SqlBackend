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

Imports System.Reflection
Imports Remotion.Data.Linq.IntegrationTests
Imports Remotion.Data.Linq.IntegrationTests.TestDomain.Northwind

Namespace LinqSamples101
  Public Class ConversionOperatorsTests
    Inherits TestBase
    
    'This sample uses ToArray so that the client-side IEnumerable(Of T) implementation of Where is used, instead of the default Query(Of T) 
    'implementation which would be converted to SQL and executed " & _
    'on the server.  This is necessary because the where clause " & _
    'references a user-defined client-side method, isValidProduct, " & _
    'which cannot be converted to SQL.")> _
    Public Sub LinqToSqlConversions01()
      Dim prodQuery = From prod In DB.Products.AsEnumerable() _
            Where isValidProduct(prod)

      TestExecutor.Execute(prodQuery, MethodBase.GetCurrentMethod())
    End Sub

    Private Function isValidProduct (ByVal prod As Product) As Boolean
      Return (prod.ProductName.LastIndexOf ("C") = 0)
    End Function

    'This sample uses ToArray to immediately evaluate a query into an array " & _
    '             "and get the 3rd element.")> _
    Public Sub LinqToSqlConversions02()
      Dim londonCustomers = From cust In db.Customers _
            Where cust.City = "London"

      Dim custArray = londonCustomers.ToArray()
      TestExecutor.Execute(custArray(3), MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses ToList to immediately evaluate a query into a List(Of T).")> _
    Public Sub LinqToSqlConversions03()
      Dim hiredAfter1994 = From emp In db.Employees _
            Where emp.HireDate >= #1/1/1994#

      Dim qList = hiredAfter1994.ToList()
      TestExecutor.Execute(qList, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses ToDictionary to immediately evaluate a query and " & _
    '             "a key expression into an Dictionary(Of K, T).")> _
    Public Sub LinqToSqlConversion04()
      Dim prodQuery = From prod In db.Products _
            Where prod.UnitsInStock <= prod.ReorderLevel _
                  AndAlso Not prod.Discontinued

      Dim qDictionary = prodQuery.ToDictionary (Function(prod) prod.ProductID)

      TestExecutor.Execute(qDictionary, MethodBase.GetCurrentMethod())
    End Sub
  End Class
End Namespace

