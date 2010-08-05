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
Imports System.Data.Linq.SqlClient

Namespace LinqSamples101
  Public Class GroupSqlMethods
    Inherits Executor
    '<Category("SqlMethods")> _
    ' <Title("SqlMethods - Like")> _
    ' <Description("This sample uses SqlMethods to filter for Customers with CustomerID that starts with 'C'.")> _
    Public Sub LinqToSqlSqlMethods01()


      Dim q = From c In db.Customers _
              Where SqlMethods.Like(c.CustomerID, "C%") _
              Select c

      serializer.Serialize(q)

    End Sub

    '<Category("SqlMethods")> _
    '<Title("SqlMethods - DateDiffDay")> _
    '<Description("This sample uses SqlMethods to find all orders which shipped within 10 days the order created")> _
    Public Sub LinqToSqlSqlMethods02()

      Dim orderQuery = From o In db.Orders _
                       Where SqlMethods.DateDiffDay(o.OrderDate, o.ShippedDate) < 10

      serializer.Serialize(orderQuery)
    End Sub

  End Class
End Namespace
