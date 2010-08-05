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
  Public Class GroupUserDefinedFunctions
    Inherits Executor
    '<Category("User-Defined Functions")> _
    '   <Title("Scalar Function - Select")> _
    '   <Description("This sample demonstrates using a scalar user-defined function in a projection.")> _
    Public Sub LinqToSqlUserDefined01()
      Dim catQuery = From category In db.Categories _
                     Select category.CategoryID, _
                            TotalUnitPrice = db.TotalProductUnitPriceByCategory(category.CategoryID)

      serializer.Serialize(catQuery)
    End Sub

    '<Category("User-Defined Functions")> _
    '<Title("Scalar Function - Where")> _
    '<Description("This sample demonstrates using a scalar user-defined function in a Where clause.")> _
    Public Sub LinqToSqlUserDefined02()

      Dim prodQuery = From prod In db.Products _
                      Where prod.UnitPrice = db.MinUnitPriceByCategory(prod.CategoryID)

      serializer.Serialize(prodQuery)
    End Sub

    '<Category("User-Defined Functions")> _
    '<Title("Table-Valued Function")> _
    '<Description("This sample demonstrates selecting from a table-valued user-defined function.")> _
    Public Sub LinqToSqlUserDefined03()

      Dim prodQuery = From p In db.ProductsUnderThisUnitPrice(10.25D) _
                      Where Not p.Discontinued

      serializer.Serialize(prodQuery)
    End Sub

    '<Category("User-Defined Functions")> _
    '<Title("Table-Valued Function - Join")> _
    '<Description("This sample demonstrates joining to the results of a table-valued user-defined function.")> _
    Public Sub LinqToSqlUserDefined04()

      Dim q = From category In db.Categories _
              Group Join prod In db.ProductsUnderThisUnitPrice(8.5D) _
                    On category.CategoryID Equals prod.CategoryID _
              Into prods = Group _
              From prod2 In prods _
              Select category.CategoryID, category.CategoryName, _
                     prod2.ProductName, prod2.UnitPrice

      serializer.Serialize(q)
    End Sub
  End Class
End Namespace

