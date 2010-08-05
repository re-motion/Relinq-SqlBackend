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
  Public Class GroupCompiledQuery
    Inherits Executor
    '<Category("Compiled Query")> _
    ' <Title("Compiled Query - 1")> _
    ' <Description("This sample create a compiled query and then use it to retrieve customers of the input city")> _
    Public Sub LinqToSqlCompileQuery01()

      '' Create compiled query
      Dim fn = System.Data.Linq.CompiledQuery.Compile( _
              Function(db2 As Northwind, city As String) _
                  From c In db2.Customers _
                  Where c.City = city _
                  Select c)


      serializer.Serialize("****** Call compiled query to retrieve customers from London ******")
      Dim LonCusts = fn(db, "London")
      serializer.Serialize(LonCusts)

      serializer.Serialize(Environment.NewLine)

      serializer.Serialize("****** Call compiled query to retrieve customers from Seattle ******")
      Dim SeaCusts = fn(db, "Seattle")
      serializer.Serialize(SeaCusts)

    End Sub

  End Class
End Namespace
