' This file is part of the re-motion Core Framework (www.re-motion.org)
' Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
' 
' The re-motion Core Framework is free software; you can redistribute it 
' and/or modify it under the terms of the GNU Lesser General Public License 
' as published by the Free Software Foundation; either version 2.1 of the 
' License, or (at your option) any later version.
' 
' re-motion is distributed in the hope that it will be useful, 
' but WITHOUT ANY WARRANTY; without even the implied warranty of 
' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
' GNU Lesser General Public License for more details.
' 
' You should have received a copy of the GNU Lesser General Public License
' along with re-motion; if not, see http://www.gnu.org/licenses.
' 
Option Infer On
Option Strict On

Imports NUnit.Framework

Imports System.Reflection
Imports Remotion.Data.Linq.IntegrationTests


Namespace LinqSamples101
  <TestFixture()>
  Public Class VBSamplesOnlyTests
    Inherits TestBase

    'This sample selects 2 columns and returns the data from the database.")> _
    <Test()>
    Public Sub LinqToSqlFirst01()

      'Instead of returning the entire Customers table, just return the
      'CompanyName and Country
      Dim londonCustomers = From cust In db.Customers _
                            Select cust.CompanyName, cust.Country

      TestExecutor.Execute(londonCustomers, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses a method mapped to the 'ProductsUnderThisUnitPrice' function
    'in Northwind database to return products with unit price less than $10.00.
    'Methods can be created by dragging database functions from the Server
    'Explorer onto the O/R Designer which can be accessed by double-clicking
    'on the .DBML file in the Solution Explorer.")> _
    <Test()>
    <Ignore()>
    Public Sub LinqToSqlStoredProc06()
      Dim cheapProducts = DB.ProductsUnderThisUnitPrice(10D)

      TestExecutor.Execute(cheapProducts, MethodBase.GetCurrentMethod())
    End Sub

    'This sample queries against a collection of products returned by
    ''ProductsUnderThisUnitPrice' method. The method was created from the database
    'function 'ProductsUnderThisUnitPrice' in Northwind database.
    <Test()>
    <Ignore()>
    Public Sub LinqToSqlStoredProc07()
      Dim cheapProducts = From prod In DB.ProductsUnderThisUnitPrice(10D) _
                          Where prod.Discontinued = True

      TestExecutor.Execute(cheapProducts, MethodBase.GetCurrentMethod())
    End Sub

  End Class

End Namespace

