// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System.Data;
using NUnit.Framework;
using Remotion.Data.Linq.IntegrationTests.TestDomain.Northwind;

namespace Remotion.Data.Linq.IntegrationTests.UnitTests
{
  [TestFixture]
  public class NorthwindConnectionManagerTest
  {
    [Test]
    public void Open ()
    {
      NorthwindConnectionManager manager = new NorthwindConnectionManager();
      IDbConnection connection = manager.Open();

      // TODO better solution?
      Assert.AreEqual (connection.ConnectionString, "Data Source=localhost;Initial Catalog=Northwind; Integrated Security=SSPI;");
    }
  }
}