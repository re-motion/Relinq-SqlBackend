// Microsoft Public License (Ms-PL)
// 
// This license governs use of the accompanying software. If you use the software, you
// accept this license. If you do not accept the license, do not use the software.
// 
// 1. Definitions
// The terms "reproduce," "reproduction," "derivative works," and "distribution" have the
// same meaning here as under U.S. copyright law.
// A "contribution" is the original software, or any additions or changes to the software.
// A "contributor" is any person that distributes its contribution under this license.
// "Licensed patents" are a contributor's patent claims that read directly on its contribution.
// 
// 2. Grant of Rights
// (A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
// each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, 
// prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
// (B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
// each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, 
// sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.
// 
// 3. Conditions and Limitations
// (A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
// (B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, your patent license from 
// such contributor to the software ends automatically.
// (C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution notices that are present 
// in the software.
// (D) If you distribute any portion of the software in source code form, you may do so only under this license by including a complete copy of 
// this license with your distribution. If you distribute any portion of the software in compiled or object code form, you may only do so under a 
// license that complies with this license.
// (E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees or conditions. 
// You may have additional consumer rights under your local laws which this license cannot change. To the extent permitted under your local laws,
// the contributors exclude the implied warranties of merchantability, fitness for a particular purpose and non-infringement.

using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace Remotion.Linq.IntegrationTests.CSharp.MiscTests
{
  [TestFixture]
  public class DateTimeAddTests : TestBase
  {
    [Test]
    public void AddYears ()
    {
      var query = DB.Orders.Select (
          o => new
          {
            One = o.OrderDate.Value.AddYears (1),
            Two = o.OrderDate.Value.AddYears (-1),
            Three = o.OrderDate.Value.AddYears (1501),
            Four = o.OrderDate.Value.AddYears (-201),
            Five = o.OrderDate.Value.AddYears (8000),
            Six = o.OrderDate.Value.AddYears (-200),
            Seven = o.OrderDate.Value.AddYears (o.OrderID % 1000),
          });
      TestExecutor.Execute (query, MethodBase.GetCurrentMethod ());
    }

    [Test]
    public void AddMonths ()
    {
      var query = DB.Orders.Select (
          o => new
          {
            One = o.OrderDate.Value.AddMonths (1),
            Two = o.OrderDate.Value.AddMonths (-1),
            Three = o.OrderDate.Value.AddMonths (1501),
            Four = o.OrderDate.Value.AddMonths (-1501),
            Five = o.OrderDate.Value.AddMonths (50073),
            Six = o.OrderDate.Value.AddMonths (-2073),
            Seven = o.OrderDate.Value.AddMonths (o.OrderID),
          });
      TestExecutor.Execute (query, MethodBase.GetCurrentMethod ());
    }

    [Test]
    public void AddDays ()
    {
      var query = DB.Orders.Select (
          o => new
          {
            One = o.OrderDate.Value.AddDays (1.5),
            Two = o.OrderDate.Value.AddDays (-1.5),
            Three = o.OrderDate.Value.AddDays (1500.76),
            Four = o.OrderDate.Value.AddDays (-1500.76),
            Five = o.OrderDate.Value.AddDays (365114.7142132),
            Six = o.OrderDate.Value.AddDays (-65114.7142132),
            Seven = o.OrderDate.Value.AddDays (0.0001),
            Eight = o.OrderDate.Value.AddDays (-0.0001),
          });
      TestExecutor.Execute (query, MethodBase.GetCurrentMethod ());
    }

    [Test]
    public void AddHours ()
    {
      var query = DB.Orders.Select (
          o => new
          {
            One = o.OrderDate.Value.AddHours (1.5),
            Two = o.OrderDate.Value.AddHours (-1.5),
            Three = o.OrderDate.Value.AddHours (1500.76),
            Four = o.OrderDate.Value.AddHours (-1500.76),
            Five = o.OrderDate.Value.AddHours (36511474.712),
            Six = o.OrderDate.Value.AddHours (-511474.712),
            Seven = o.OrderDate.Value.AddHours (0.0001),
            Eight = o.OrderDate.Value.AddHours (-0.0001),
          });
      TestExecutor.Execute (query, MethodBase.GetCurrentMethod ());
    }

    [Test]
    public void AddMinutes ()
    {
      var query = DB.Orders.Select (
          o => new
          {
            One = o.OrderDate.Value.AddMinutes (1.5),
            Two = o.OrderDate.Value.AddMinutes (-1.5),
            Three = o.OrderDate.Value.AddMinutes (1500.76),
            Four = o.OrderDate.Value.AddMinutes (-1500.76),
            Five = o.OrderDate.Value.AddMinutes (36511474.712),
            Six = o.OrderDate.Value.AddMinutes (-36511474.712),
            Seven = o.OrderDate.Value.AddMinutes (0.0001),
            Eight = o.OrderDate.Value.AddMinutes (-0.0001),
          });
      TestExecutor.Execute (query, MethodBase.GetCurrentMethod ());
    }

    [Test]
    public void AddSeconds ()
    {
      var query = DB.Orders.Select (
          o => new
          {
            One = o.OrderDate.Value.AddSeconds (1.5),
            Two = o.OrderDate.Value.AddSeconds (-1.5),
            Three = o.OrderDate.Value.AddSeconds (1500.76),
            Four = o.OrderDate.Value.AddSeconds (-1500.76),
            Five = o.OrderDate.Value.AddSeconds (3651147445.712),
            Six = o.OrderDate.Value.AddSeconds (-3651147445.712),
            Seven = o.OrderDate.Value.AddSeconds (0.0001),
            Eight = o.OrderDate.Value.AddSeconds (-0.0001),
          });
      TestExecutor.Execute (query, MethodBase.GetCurrentMethod ());
    }

    [Test]
    public void AddMilliseconds ()
    {
      var query = DB.Orders.Select (
          o => new
          {
            One = o.OrderDate.Value.AddMilliseconds (1.5),
            Two = o.OrderDate.Value.AddMilliseconds (-1.5),
            Three = o.OrderDate.Value.AddMilliseconds (1500.76),
            Four = o.OrderDate.Value.AddMilliseconds (-1500.76),
            Five = o.OrderDate.Value.AddMilliseconds (3651147445123.712),
            Six = o.OrderDate.Value.AddMilliseconds (-3651147445123.712),
            Seven = o.OrderDate.Value.AddMilliseconds (0.0001),
            Eight = o.OrderDate.Value.AddMilliseconds (-0.0001),
          });
      TestExecutor.Execute (query, MethodBase.GetCurrentMethod ());
    }

    [Test]
    public void AddTicks ()
    {
      var query = DB.Orders.Select (
          o => new
          {
            One = o.OrderDate.Value.AddTicks (1),
            Two = o.OrderDate.Value.AddTicks (-1),
            Three = o.OrderDate.Value.AddTicks (1501),
            Four = o.OrderDate.Value.AddTicks (-1501),
            Five = o.OrderDate.Value.AddTicks (507365465231654654L),
            Six = o.OrderDate.Value.AddTicks (-57365465231654654L),
            Seven = o.OrderDate.Value.AddTicks (o.OrderID),
          });
      TestExecutor.Execute (query, MethodBase.GetCurrentMethod ());
    }

    [Test]
    public void Add_TimeSpan ()
    {
      var query = DB.Orders.Select (
          o => new
          {
            One = o.OrderDate.Value.Add (TimeSpan.FromDays (1.5)),
            Two = o.OrderDate.Value.Add (TimeSpan.FromDays (-1.5)),
            Three = o.OrderDate.Value.Add (TimeSpan.FromDays (1500.76)),
            Four = o.OrderDate.Value.Add (TimeSpan.FromDays (-1500.76)),
            Five = o.OrderDate.Value.Add (TimeSpan.FromDays (365114.7)),
            Six = o.OrderDate.Value.Add (TimeSpan.FromDays (-65114.7)),
            Seven = o.OrderDate.Value.Add (TimeSpan.FromDays (0.0001)),
            Eight = o.OrderDate.Value.Add (TimeSpan.FromDays (-0.0001)),
            Nine = o.OrderDate.Value.Add (TimeSpan.FromMilliseconds (1.5)),
            Ten = o.OrderDate.Value.Add (TimeSpan.FromMilliseconds (-1.5)),
            Eleven = o.OrderDate.Value.Add (TimeSpan.FromMilliseconds (1500.76)),
            Twelve = o.OrderDate.Value.Add (TimeSpan.FromMilliseconds (-1500.76)),
            Thirteen = o.OrderDate.Value.Add (TimeSpan.FromMilliseconds (54987987987987.7)),
            Fourteen = o.OrderDate.Value.Add (TimeSpan.FromMilliseconds (-4987987987987.7)),
            Fifteen = o.OrderDate.Value.Add (TimeSpan.FromMilliseconds (0.0001)),
            Sixteen = o.OrderDate.Value.Add (TimeSpan.FromMilliseconds (-0.0001)),
          });
      TestExecutor.Execute (query, MethodBase.GetCurrentMethod ());
    }
  }
}