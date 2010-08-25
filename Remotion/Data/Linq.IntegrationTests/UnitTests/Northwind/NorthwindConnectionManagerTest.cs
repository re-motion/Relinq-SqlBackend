// Copyright (C) 2005 - 2009 rubicon informationstechnologie gmbh
// All rights reserved.
//
using System.Configuration;
using System.Data;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.IntegrationTests.TestDomain.Northwind;

namespace Remotion.Data.Linq.IntegrationTests.UnitTests.Northwind
{
  [TestFixture]
  public class NorthwindConnectionManagerTest
  {
    [Test]
    public void Open ()
    {
      NorthwindConnectionManager manager = new NorthwindConnectionManager();
      using (var connection = manager.Open ())
      {
        Assert.That (connection.ConnectionString,Is.EqualTo (ConfigurationManager.ConnectionStrings["Northwind"].ConnectionString));
        Assert.That (connection.State, Is.EqualTo (ConnectionState.Open));
      }
    }
  }
}