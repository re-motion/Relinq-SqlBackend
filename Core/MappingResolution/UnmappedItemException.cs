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
using System.Runtime.Serialization;

namespace Remotion.Linq.SqlBackend.MappingResolution
{
  /// <summary>
  /// The <see cref="UnmappedItemException"/> is thrown if the <see cref="IMappingResolver"/> implementation 
  /// is unable to associate an part of the query model with the mapping.
  /// </summary>
  [Serializable]
  public sealed class UnmappedItemException : Exception
  {
#if NETFRAMEWORK 
    [NonSerialized]
    private UnmappedItemExceptionState _state;
#endif

    public UnmappedItemException (string message)
        : this (message, null)
    {
    }

    public UnmappedItemException (string message, Exception innerException)
        : base (message, innerException)
    {
#if NETFRAMEWORK 
      _state = new UnmappedItemExceptionState();
      SerializeObjectState += (exception, eventArgs) => eventArgs.AddSerializedState (_state);
#endif
    }

#pragma warning disable SYSLIB0051
    private UnmappedItemException (SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
#pragma warning restore

#if NETFRAMEWORK 
    [Serializable]
    private struct UnmappedItemExceptionState : ISafeSerializationData
    {
      void ISafeSerializationData.CompleteDeserialization
          (object obj)
      {
        UnmappedItemException exception = (UnmappedItemException) obj;
        exception._state = this;
      }
    }
#endif
  }
}