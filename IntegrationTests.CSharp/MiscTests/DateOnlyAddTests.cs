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
  public class DateOnlyAddTests : TestBase
  {
    [Test]
    public void AddYears ()
    {
      var query = DB.Orders.Select (
          o => new
            {
              One = o.RequiredDate.Value.AddYears (1),
              Two = o.RequiredDate.Value.AddYears (-1),
              Three = o.RequiredDate.Value.AddYears (1501),
              Four = o.RequiredDate.Value.AddYears (-201),
              Five = o.RequiredDate.Value.AddYears (8000),
              Six = o.RequiredDate.Value.AddYears (-200),
              Seven = o.RequiredDate.Value.AddYears (o.OrderID % 1000),
            });
      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void AddMonths ()
    {
      var query = DB.Orders.Select (
          o => new
            {
              One = o.RequiredDate.Value.AddMonths (1),
              Two = o.RequiredDate.Value.AddMonths (-1),
              Three = o.RequiredDate.Value.AddMonths (1501),
              Four = o.RequiredDate.Value.AddMonths (-1501),
              Five = o.RequiredDate.Value.AddMonths (50073),
              Six = o.RequiredDate.Value.AddMonths (-2073),
              Seven = o.RequiredDate.Value.AddMonths (o.OrderID),
            });
      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void AddDays ()
    {
      var query = DB.Orders.Select (
          o => new
            {
              One = o.RequiredDate.Value.AddDays (1),
              Two = o.RequiredDate.Value.AddDays (-1),
              Three = o.RequiredDate.Value.AddDays (1501),
              Four = o.RequiredDate.Value.AddDays (-1501),
              Five = o.RequiredDate.Value.AddDays (365114),
              Six = o.RequiredDate.Value.AddDays (-65114),
              Seven = o.RequiredDate.Value.AddDays (-365),
              Eight = o.RequiredDate.Value.AddDays (365),
            });
      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }
  }
}