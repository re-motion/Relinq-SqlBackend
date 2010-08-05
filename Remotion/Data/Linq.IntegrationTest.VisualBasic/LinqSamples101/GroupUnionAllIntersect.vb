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
  Public Class GroupUnionAllIntersect
    Inherits Executor
    '    <Category("UNION ALL/UNION/INTERSECT")> _
    '<Title("Except")> _
    '<Description("This sample uses Except to return a sequence of all countries that " & _
    '             "Customers live in but no Employees live in.")> _
    Public Sub LinqToSqlUnion05()
      Dim countries = (From cust In db.Customers _
                       Select cust.Country).Except(From emp In db.Employees _
                                                   Select emp.Country)

      serializer.Serialize(countries)
    End Sub
  End Class
End Namespace

