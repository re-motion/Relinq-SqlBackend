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
  Public Class GroupView
    Inherits Executor
    '<Category("View")> _
    ' <Title("Query - Anonymous Type")> _
    ' <Description("This sample uses Select and Where to return a sequence of invoices " & _
    '              "where the shipping city is London.")> _
    Public Sub LinqToSqlView01()
      Dim shipToLondon = From inv In db.Invoices _
                         Where inv.ShipCity = "London" _
                         Select inv.OrderID, inv.ProductName, inv.Quantity, inv.CustomerName

      serializer.Serialize(shipToLondon)
    End Sub

    '<Category("View")> _
    '<Title("Query - Identity mapping")> _
    '<Description("This sample uses Select to query QuarterlyOrders.")> _
    Public Sub LinqToSqlView02()
      'WORKAROUND: changed Quarterly_Orders to QuarterlyOrders
      Dim quarterlyOrders = From qo In db.QuarterlyOrders _
                            Select qo

      serializer.Serialize(quarterlyOrders)
    End Sub
  End Class
End Namespace
