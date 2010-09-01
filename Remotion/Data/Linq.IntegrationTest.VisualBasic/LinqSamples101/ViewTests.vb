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
  Public Class ViewTests
    Inherits TestBase
    'This sample uses Select and Where to return a sequence of invoices
    'where the shipping city is London.
    <Test()>
    <Ignore("Bug or missing feature in Relinq - see MappingResolver.ResolveSimpleTableInfo(...): if sortedMembers don't contain any primary key => primaryColumn == null => null pointer thrown by SqlEntityDefinitionExpression")>
    Public Sub LinqToSqlView01()
      Dim shipToLondon = From inv In DB.Invoices _
            Where inv.ShipCity = "London" _
            Select inv.OrderID, inv.ProductName, inv.Quantity, inv.CustomerName

      TestExecutor.Execute(shipToLondon, MethodBase.GetCurrentMethod())
    End Sub

    'This sample uses Select to query QuarterlyOrders.
    <Test()>
    <Ignore("Bug or missing feature in Relinq - see MappingResolver.ResolveSimpleTableInfo(...): if sortedMembers don't contain any primary key => primaryColumn == null => null pointer thrown by SqlEntityDefinitionExpression")>
    Public Sub LinqToSqlView02()
      'WORKAROUND: changed Quarterly_Orders to QuarterlyOrders
      Dim quarterlyOrders = From qo In DB.QuarterlyOrders _
            Select qo

      TestExecutor.Execute(quarterlyOrders, MethodBase.GetCurrentMethod())
    End Sub
  End Class
End Namespace
