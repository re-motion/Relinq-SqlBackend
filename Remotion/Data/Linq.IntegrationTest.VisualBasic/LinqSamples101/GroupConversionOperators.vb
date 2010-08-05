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
  Public Class GroupConversionOperators
    Inherits Executor
    '<Category("Conversion Operators")> _
    ' <Title("AsEnumerable")> _
    ' <Description("This sample uses ToArray so that the client-side IEnumerable(Of T) " & _
    '              "implementation of Where is used, instead of the default Query(Of T) " & _
    '              "implementation which would be converted to SQL and executed " & _
    '              "on the server.  This is necessary because the where clause " & _
    '              "references a user-defined client-side method, isValidProduct, " & _
    '              "which cannot be converted to SQL.")> _
    ' <LinkedFunction("isValidProduct")> _
    Public Sub LinqToSqlConversions01()
      Dim prodQuery = From prod In db.Products.AsEnumerable() _
                      Where isValidProduct(prod)

      serializer.Serialize(prodQuery)
    End Sub

    Private Function isValidProduct(ByVal prod As Product) As Boolean
      Return (prod.ProductName.LastIndexOf("C") = 0)
    End Function

    '<Category("Conversion Operators")> _
    '<Title("ToArray")> _
    '<Description("This sample uses ToArray to immediately evaluate a query into an array " & _
    '             "and get the 3rd element.")> _
    Public Sub LinqToSqlConversions02()
      Dim londonCustomers = From cust In db.Customers _
                            Where cust.City = "London"

      Dim custArray = londonCustomers.ToArray()
      serializer.Serialize(custArray(3))
    End Sub

    '<Category("Conversion Operators")> _
    '<Title("ToList")> _
    '<Description("This sample uses ToList to immediately evaluate a query into a List(Of T).")> _
    Public Sub LinqToSqlConversions03()
      Dim hiredAfter1994 = From emp In db.Employees _
                           Where emp.HireDate >= #1/1/1994#

      Dim qList = hiredAfter1994.ToList()
      serializer.Serialize(qList)
    End Sub

    '<Category("Conversion Operators")> _
    '<Title("ToDictionary")> _
    '<Description("This sample uses ToDictionary to immediately evaluate a query and " & _
    '             "a key expression into an Dictionary(Of K, T).")> _
    Public Sub LinqToSqlConversion04()
      Dim prodQuery = From prod In db.Products _
                      Where prod.UnitsInStock <= prod.ReorderLevel _
                            AndAlso Not prod.Discontinued

      Dim qDictionary = prodQuery.ToDictionary(Function(prod) prod.ProductID)

      For Each key In qDictionary.Keys
        serializer.Serialize("Key " & key & ":")
        serializer.Serialize(qDictionary(key))
        serializer.Serialize(Environment.NewLine)
      Next
    End Sub
  End Class
End Namespace

