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


Namespace LinqSamples101
  Public Class GroupVBSamplesOnly
    Inherits Executor

    'This sample selects 2 columns and returns the data from the database.")> _
    Public Sub LinqToSqlFirst01()

      'Instead of returning the entire Customers table, just return the
      'CompanyName and Country
      Dim londonCustomers = From cust In db.Customers _
                            Select cust.CompanyName, cust.Country

      'Execute the query and print out the results
      For Each custRow In londonCustomers
        serializer.Serialize("Company: " & custRow.CompanyName & vbTab & _
                          "Country: " & custRow.Country)
      Next
    End Sub

    'This sample uses a method mapped to the 'ProductsUnderThisUnitPrice' function " & _
    '             "in Northwind database to return products with unit price less than $10.00. " & _
    '             "Methods can be created by dragging database functions from the Server " & _
    '             "Explorer onto the O/R Designer which can be accessed by double-clicking " & _
    '             "on the .DBML file in the Solution Explorer.")> _
    Public Sub LinqToSqlStoredProc06()
      Dim cheapProducts = db.ProductsUnderThisUnitPrice(10D)

      serializer.Serialize(cheapProducts)
    End Sub

    'This sample queries against a collection of products returned by " & _
    '             "'ProductsUnderThisUnitPrice' method. The method was created from the database  " & _
    '             "function 'ProductsUnderThisUnitPrice' in Northwind database. ")> _
    Public Sub LinqToSqlStoredProc07()
      Dim cheapProducts = From prod In db.ProductsUnderThisUnitPrice(10D) _
                          Where prod.Discontinued = True

      serializer.Serialize(cheapProducts)
    End Sub

  End Class

End Namespace

