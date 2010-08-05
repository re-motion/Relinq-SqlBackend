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
  Public Class PagingTests
    Inherits TestBase
    'This sample uses the Skip and Take operators to do paging by
    'skipping the first 50 records and then returning the next 10, thereby
    'providing the data for page 6 of the Products table.
    <Test()>
    Public Sub LinqToSqlPaging01()
      Dim productPage = From cust In db.Customers _
            Order By cust.ContactName _
            Skip 50 _
            Take 10

      TestExecutor.Execute(productPage, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses a Where clause and the Take operator to do paging by,
    'first filtering to get only the ProductIDs above 50 (the last ProductID
    'from page 5), then ordering by ProductID, and finally taking the first 10 results,
    'thereby providing the data for page 6 of the Products table.
    'Note that this method only works when ordering by a unique key.
    <Test()>
    Public Sub LinqToSqlPaging02()
      Dim productPage = From prod In db.Products _
            Where prod.ProductID > 50 _
            Select prod _
            Order By prod.ProductID _
            Take 10

      TestExecutor.Execute(productPage, MethodBase.GetCurrentMethod())
    End Sub
  End Class
End Namespace

