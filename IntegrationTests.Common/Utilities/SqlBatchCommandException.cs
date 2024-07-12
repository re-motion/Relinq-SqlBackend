// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (c) rubicon IT GmbH, www.rubicon.eu
// 
// re-linq is free software; you can redistribute it and/or modify it under 
// the terms of the GNU Lesser General Public License as published by the 
// Free Software Foundation; either version 2.1 of the License, 
// or (at your option) any later version.
// 
// re-linq is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-linq; if not, see http://www.gnu.org/licenses.
// 
using System;

namespace Remotion.Linq.IntegrationTests.Common.Utilities
{
#if NETFRAMEWORK
  [Serializable]
#endif
  public class SqlBatchCommandException : Exception
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="SqlBatchCommandException"/>  class with a specified error message.
    /// </summary>
    /// <param name="message">The error message string. </param>
    public SqlBatchCommandException (string message)
      : base (message)
    {
    }

    /// <summary>
    ///   Initializes a new instance of the <see cref="SqlBatchCommandException"/> class with a specified 
    ///   error message and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message string. </param>
    /// <param name="innerException">The inner exception reference. </param>
    public SqlBatchCommandException (string message, Exception innerException)
      : base (message, innerException)
    {
    }
  }
}