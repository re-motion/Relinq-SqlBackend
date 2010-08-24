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
      // TODO Review: Use a using block to close the connection at the end of the test
      
      NorthwindConnectionManager manager = new NorthwindConnectionManager();
      IDbConnection connection = manager.Open();

      // TODO Review: Refactor to use Assert.That instead of Assert.AreEqual/IsTrue

      // TODO Review: Compare against ConfigurationManager.ConnectionStrings["Northwind"]
      Assert.AreEqual (connection.ConnectionString, "Data Source=localhost;Initial Catalog=Northwind; Integrated Security=SSPI;");
      Assert.IsTrue (connection.State==ConnectionState.Open);
    }
  }
}