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
using System.Collections;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using Remotion.Utilities;

namespace Remotion.Linq.IntegrationTests.Common.Utilities
{
  /// <summary>
  /// Provides functionality to serialize the result of LINQ tests into a human-readable and machine-readable format for comparison between tests.
  /// </summary>
  public class TestResultSerializer
  {
    public const string DefaultSpacer = "  ";

    private readonly TextWriter _textWriter;
    private readonly string _spacer;
    private readonly int _level;
    private readonly Func<MemberInfo, bool> _memberFilter;

    /// <summary>
    /// TestResultSerializer including custom spacer, level of indention and a member-filter
    /// </summary>
    /// <param name="textWriter">TextWriter to which serialisation outpu will be written</param>
    /// <param name="spacer">String to be used for indention</param>
    /// <param name="level">level of indention</param>
    /// <param name="memberFilter">function that evaluates to true for members that will be serialized</param>
    public TestResultSerializer (TextWriter textWriter, string spacer, int level, Func<MemberInfo, bool> memberFilter)
    {
      ArgumentUtility.CheckNotNull (nameof(textWriter), textWriter);
      ArgumentUtility.CheckNotNull (nameof(memberFilter), memberFilter);

      _textWriter = textWriter;
      _spacer = spacer;
      _level = level;
      _memberFilter = memberFilter;
    }

    /// <summary>
    /// TestResultSerializer including custom spacer and level of indention
    /// </summary>
    /// <param name="textWriter">TextWriter to which serialisation outpu will be written</param>
    /// <param name="spacer">String to be used for indention</param>
    /// <param name="level">level of indention</param>
    public TestResultSerializer (TextWriter textWriter, string spacer, int level)
      : this (textWriter, spacer, level, delegate { return true; } )
    {}

    /// <summary>
    /// TestResultSerializer including a member-filter
    /// </summary>
    /// <param name="textWriter">TextWriter to which serialisation outpu will be written</param>
    /// <param name="memberFilter">function that evaluates to true for members that will be serialized</param>
    public TestResultSerializer (TextWriter textWriter, Func<MemberInfo, bool> memberFilter)
      : this (textWriter, DefaultSpacer, 0, memberFilter)
    { }

    /// <summary>
    /// Standard TestResultSerializer
    /// </summary>
    /// <param name="textWriter">TextWriter to which serialisation outpu will be written</param>
    public TestResultSerializer (TextWriter textWriter)
      : this (textWriter, DefaultSpacer, 0)
    {}

    public void Serialize (object value)
    {
      WriteSpacing();

      SerializeWithoutSpacing(value);
    }

    public void Serialize (object value, string name)
    {
      ArgumentUtility.CheckNotNull (nameof(name), name);

      WriteSpacing();

      _textWriter.Write (name);
      _textWriter.Write (": ");

      SerializeWithoutSpacing(value);
    }

    public void Serialize (object value, MethodBase currentMethod)
    {
      ArgumentUtility.CheckNotNull (nameof(currentMethod), currentMethod);

      _textWriter.Write (currentMethod.Name);
      _textWriter.WriteLine (":");
      CreateIndentedSerializer().Serialize (value);
    }

    private void SerializeWithoutSpacing (object value)
    {
      if (value == null)
        _textWriter.WriteLine ("null");
      // DateOnly and DateTime are in the same fields in Net Framework/.NET, to enable tests we need to standardize the results of serialization
      // of all cases where time is irrelevant for the dateTime property
      else if (value is DateTime dateTime && dateTime.ToLongTimeString() == "00:00:00")
        SerializeDateTimeWithNoTime(dateTime);
      else if (value is string)
        SerializeString ((string) value);
      else if (value is IEnumerable)
        SerializeEnumerable ((IEnumerable) value);
      else if (value is ValueType)
        _textWriter.WriteLine (value);
      else
        SerializeComplexValue (value);
    }

    private void SerializeDateTimeWithNoTime (DateTime dateTime)
    {
      _textWriter.WriteLine(dateTime.ToShortDateString());
    }

    private void SerializeString (string value)
    {
      Assertion.DebugAssert (value != null, "should be handled by caller");

      var escapedValue = value.Replace ("'", "''");
      _textWriter.WriteLine ("'" + escapedValue + "'");
    }

    private void SerializeEnumerable (IEnumerable value)
    {
      Assertion.DebugAssert (value != null, "should be handled by caller");

      _textWriter.Write ("Enumerable");
      _textWriter.WriteLine (" {");
      TestResultSerializer elementSerializer = CreateIndentedSerializer();
      
      foreach (var element in value)
        elementSerializer.Serialize (element);

      WriteSpacing();
      _textWriter.WriteLine ("}");
    }

    private void SerializeComplexValue (object value)
    {
      Assertion.DebugAssert (value != null, "should be handled by caller");
      WriteTypeName(value);
      _textWriter.WriteLine();

      MemberInfo[] members = value.GetType().GetMembers (BindingFlags.Public | BindingFlags.Instance);
      
      Array.Sort (members, (m1, m2) => m1.Name.CompareTo(m2.Name));

      var memberValueSerializer = CreateIndentedSerializer();
      foreach (var memberInfo in members)
      {
        object memberValue;
        if (TryGetValue (value, memberInfo, out memberValue) && _memberFilter(memberInfo))
          memberValueSerializer.Serialize (memberValue, memberInfo.Name);
      }
    }

    private static bool TryGetValue (object instance, MemberInfo memberInfo, out object value)
    {
      switch (memberInfo.MemberType)
      {
        case MemberTypes.Property:
          value = ((PropertyInfo) memberInfo).GetValue (instance, null);
          return true;
        case MemberTypes.Field:
          value = ((FieldInfo) memberInfo).GetValue (instance);
          return true;
        default:
          value = null;
          return false;
      }
    }

    private void WriteSpacing ()
    {
      for (int i = 0; i < _level; ++i)
        _textWriter.Write (_spacer);
    }

    private void WriteTypeName (object value)
    {
      _textWriter.Write (IsAnonymousType (value.GetType()) ? MakeAnonymousTypeID (value.GetType()) : value.GetType().Name);
    }

    private static bool IsAnonymousType (Type type)
    {
      return type.IsDefined (typeof (CompilerGeneratedAttribute), false);
    }

    private static string MakeAnonymousTypeID (Type type)
    {
      return "AnonymousType";
    }

    private TestResultSerializer CreateIndentedSerializer ()
    {
      return new TestResultSerializer (_textWriter, _spacer, _level + 1, _memberFilter); 
    }

    
  }
}