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
Imports Remotion.Data.Linq.IntegrationTests.Utilities

Namespace LinqSamples101
  Public Class GroupPaging
    Inherits Executor
    '<Category("Paging")> _
    '   <Title("Paging - Index")> _
    '   <Description("This sample uses the Skip and Take operators to do paging by " & _
    '                "skipping the first 50 records and then returning the next 10, thereby " & _
    '                "providing the data for page 6 of the Products table.")> _
    Public Sub LinqToSqlPaging01()
      Dim productPage = From cust In db.Customers _
                        Order By cust.ContactName _
                        Skip 50 _
                        Take 10

      serializer.Serialize(productPage)
    End Sub

    '<Category("Paging")> _
    '<Title("Paging - Ordered Unique Key")> _
    '<Description("This sample uses a Where clause and the Take operator to do paging by, " & _
    '             "first filtering to get only the ProductIDs above 50 (the last ProductID " & _
    '             "from page 5), then ordering by ProductID, and finally taking the first 10 results, " & _
    '             "thereby providing the data for page 6 of the Products table.  " & _
    '             "Note that this method only works when ordering by a unique key.")> _
    Public Sub LinqToSqlPaging02()
      Dim productPage = From prod In db.Products _
                        Where prod.ProductID > 50 _
                        Select prod _
                        Order By prod.ProductID _
                        Take 10

      serializer.Serialize(productPage)
    End Sub
  End Class
End Namespace

