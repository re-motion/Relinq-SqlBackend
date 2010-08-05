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

Imports System.Data.Linq.SqlClient

Namespace LinqSamples101
  Public Class GroupSqlMethods
    Inherits Executor
    '<Category("SqlMethods")> _
    ' <Title("SqlMethods - Like")> _
    ' <Description("This sample uses SqlMethods to filter for Customers with CustomerID that starts with 'C'.")> _
    Public Sub LinqToSqlSqlMethods01()


      Dim q = From c In db.Customers _
            Where SqlMethods.Like (c.CustomerID, "C%") _
            Select c

      serializer.Serialize (q)

    End Sub

    'This sample uses SqlMethods to find all orders which shipped within 10 days the order created")> _
    Public Sub LinqToSqlSqlMethods02()

      Dim orderQuery = From o In db.Orders _
            Where SqlMethods.DateDiffDay (o.OrderDate, o.ShippedDate) < 10

      serializer.Serialize (orderQuery)
    End Sub
  End Class
End Namespace
