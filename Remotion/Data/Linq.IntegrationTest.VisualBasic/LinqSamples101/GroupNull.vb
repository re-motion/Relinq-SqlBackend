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

  Public Class GroupNull
    Inherits Executor

    '<Category("NULL")> _
    '<Title("Handling NULL (Nothing in VB)")> _
    '<Description("This sample uses the Nothing value to find Employees " & _
    '         "that do not report to another Employee.")> _
    Public Sub LinqToSqlNull01()
      Dim empQuery = From emp In db.Employees _
                     Where emp.ReportsTo Is Nothing

      serializer.Serialize(empQuery)
    End Sub

    '<Category("NULL")> _
    '<Title("Nullable(Of T).HasValue")> _
    '<Description("This sample uses Nullable(Of T).HasValue to find Employees " & _
    '             "that do not report to another Employee.")> _
    Public Sub LinqToSqlNull02()
      Dim empQuery = From emp In db.Employees _
                     Where Not emp.ReportsTo.HasValue _
                     Select emp

      serializer.Serialize(empQuery)
    End Sub

    '<Category("NULL")> _
    '<Title("Nullable(Of T).Value")> _
    '<Description("This sample uses Nullable(Of T).Value for Employees " & _
    '             "that report to another Employee to return the " & _
    '             "EmployeeID number of that employee.  Note that " & _
    '             "the .Value is optional.")> _
    Public Sub LinqToSqlNull03()
      Dim empQuery = From emp In db.Employees _
                     Where emp.ReportsTo.HasValue _
                     Select emp.FirstName, emp.LastName, ReportsTo = emp.ReportsTo.Value

      serializer.Serialize(empQuery)
    End Sub

  End Class
End Namespace
