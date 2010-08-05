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

Namespace LinqSamples101

  Public Class GroupObjectIdentity
    Inherits Executor

    '  <Category("Object Identity")> _
    '<Title("Object Caching - 1")> _
    '<Description("This sample demonstrates how, upon executing the same query twice, " & _
    '             "you will receive a reference to the same object in memory each time.")> _
    Public Sub LinqToSqlObjectIdentity01()
      Dim cust1 = db.Customers.First(Function(cust) cust.CustomerID = "BONAP")
      Dim cust2 = (From cust In db.Customers _
                               Where cust.CustomerID = "BONAP").First()

      serializer.Serialize("cust1 and cust2 refer to the same object in memory: " & _
                        Object.ReferenceEquals(cust1, cust2))
    End Sub

    '<Category("Object Identity")> _
    '<Title("Object Caching - 2")> _
    '<Description("This sample demonstrates how, upon executing different queries that " & _
    '             "return the same row from the database, you will receive a " & _
    '             "reference to the same object in memory each time.")> _
    Public Sub LinqToSqlObjectIdentity02()
      Dim cust1 = db.Customers.First(Function(cust) cust.CustomerID = "BONAP")
      Dim cust2 = (From ord In db.Orders _
                   Where ord.Customer.CustomerID = "BONAP").First().Customer

      serializer.Serialize("cust1 and cust2 refer to the same object in memory: " & _
                        Object.ReferenceEquals(cust1, cust2))
    End Sub


  End Class
End Namespace
