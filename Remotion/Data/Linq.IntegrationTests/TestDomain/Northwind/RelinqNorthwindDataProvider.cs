using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Remotion.Data.Linq.IntegrationTests.TestDomain.Northwind
{
  class RelinqNorthwindDataProvider : INorthwindDataProvider // TODO: implement
  {
    public IQueryable<Product> Products
    {
      get { throw new NotImplementedException (); }
    }
  }
}
