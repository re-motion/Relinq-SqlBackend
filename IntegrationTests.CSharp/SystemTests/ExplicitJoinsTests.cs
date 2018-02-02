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

namespace Remotion.Linq.IntegrationTests.CSharp.SystemTests
{
  [TestFixture]
  public class ExplicitJoinsTests : TestBase
  {
    [Test]
    public void ExplicitJoin ()
    {
      var query =
          from o in DB.Orders
          join c in DB.Customers on o.Customer equals c
          where o.CustomerID == "QUEEN"
          select o;

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void ExplicitJoinWithInto_Once ()
    {
      var query =
          from o in DB.Orders
          join od in DB.OrderDetails on o equals od.Order into odo
          from ode in odo
          select ode;

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void ExplicitJoinWithInto_Twice ()
    {
      var query =
          from o in DB.Orders
          join od in DB.OrderDetails on o equals od.Order into god
          join c in DB.Customers on o.Customer equals c into goc
          from odo in god
          from oc in goc
          select odo;

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void ExplicitJoinWithInto_InSubstatement_Once ()
    {
      var query =
          from o in DB.Orders
          where o.OrderID ==
                (from so in DB.Orders
                  join si in DB.OrderDetails on so equals si.Order into goi
                  from oi in goi
                  select oi.Order.OrderID
                    ).First()
          select o;

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void ExplicitJoinWithInto_InSubstatement_Twice ()
    {
      var query =
          from o in DB.Orders
          where o.OrderID ==
                (from so in DB.Orders
                  join si in DB.OrderDetails on so equals si.Order into goi
                  join si in DB.Customers on so.Customer equals si into goc
                  from oi in goi
                  from oc in goc
                  select oi.Order.OrderID).OrderBy (x => x).First()
          select o;

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void ExplicitJoinWithInto_InTwoSubStatements ()
    {
      var query =
          from o in DB.Orders
          where o.OrderID ==
                (from so in DB.Orders
                  join si in DB.OrderDetails on so equals si.Order into goi
                  from oi in goi
                  select oi.Order.OrderID).First()
                && o.Customer.ContactName ==
                (from so in DB.Orders
                  join sc in DB.Customers on so.Customer equals sc into goc
                  from oc in goc
                  select oc.ContactName).First()
          select o;

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void ExplicitJoinWithInto_InSameStatementAndInSubStatement ()
    {
      var query =
          from o in DB.Orders
          join d in DB.OrderDetails on o equals d.Order into god
          from od in god
          where o.ShipCity ==
                (from so in DB.Orders
                  join sd in DB.OrderDetails on so equals sd.Order into gda
                  from da in gda
                  select so.ShipCity).First()
          select od;

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }

    [Test]
    public void ExplicitJoinWithInto_WithOrderBy ()
    {
      var query =
          from o in DB.Orders
          join d in DB.OrderDetails.OrderBy (od => od.Quantity) on o equals d.Order into god
          from od in god
          select od;

      TestExecutor.Execute (query, MethodBase.GetCurrentMethod());
    }
  }
}