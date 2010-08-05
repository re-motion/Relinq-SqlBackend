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
  Public Class GroupTopBottom
    Inherits Executor
    '<Category("TOP/BOTTOM")> _
    '  <Title("Take")> _
    '  <Description("This sample uses Take to select the first 5 Employees hired.")> _
    Public Sub LinqToSqlTop01()
      Dim first5Employees = From emp In db.Employees _
                            Order By emp.HireDate _
                            Take 5

      serializer.Serialize(first5Employees)
    End Sub

    '<Category("TOP/BOTTOM")> _
    '<Title("Skip")> _
    '<Description("This sample uses Skip to select all but the 10 most expensive Products.")> _
    Public Sub LinqToSqlTop02()
      Dim expensiveProducts = From prod In db.Products _
                              Order By prod.UnitPrice Descending _
                              Skip 10

      serializer.Serialize(expensiveProducts)
    End Sub
  End Class
End Namespace

