using System;
using System.Data.Linq.Mapping;

namespace Remotion.Data.Linq.UnitTests.LinqToSqlAdapter
{
  [Table (Name = "dbo.Contacts")]
  [InheritanceMapping (Code = "Unknown", Type = typeof (ContactTestClass), IsDefault = true)]
  [InheritanceMapping (Code = "Employee", Type = typeof (EmployeeContact))]
  [InheritanceMapping (Code = "Supplier", Type = typeof (SupplierContact))]
  [InheritanceMapping (Code = "Customer", Type = typeof (CustomerContact))]
  [InheritanceMapping (Code = "Shipper", Type = typeof (ShipperContact))]
  public class ContactTestClass
  {
    #region inner classes
    public abstract class FullContact : ContactTestClass
    {
    }

    public class CustomerContact : FullContact
    {
    }

    public class ShipperContact : ContactTestClass
    {
    }

    public class SupplierContact : FullContact
    {
      private string _HomePage;
      //private ShipperContact _ShipperContact;

      [Column (Storage = "_HomePage", DbType = "NText", UpdateCheck = UpdateCheck.Never)]
      public string HomePage
      {
        get
        {
          return this._HomePage;
        }
        set
        {
          this._HomePage = value;
        }
      }

      //TODO remove if still unneeded
      //[Column (Storage = "_ShipperContact", DbType = "NText", UpdateCheck = UpdateCheck.Never)]
      //public ShipperContact ShipperContact
      //{
      //  get
      //  {
      //    return this._ShipperContact;
      //  }
      //  set
      //  {
      //    this._ShipperContact = value;
      //  }
      //}

      public override bool Equals (object obj)
      {
        if (obj == null || GetType() != obj.GetType())
        {
          return false;
        }
        if(!_HomePage.Equals(((SupplierContact)obj).HomePage))
        {
          return false;
        }

        return base.Equals (obj);
      }

      public override int GetHashCode ()
      {
        throw new NotImplementedException();
      }
    }

    public class EmployeeContact : ContactTestClass
    {
    }

    #endregion

    private int _ContactID;
    private string _ContactType;

    public override bool Equals (object obj)
    {
      if (obj == null || GetType () != obj.GetType ())
      {
        return false;
      }

      if (_ContactID != ((ContactTestClass) obj)._ContactID)
      {
        return false;
      }

      if (!_ContactType.Equals(((ContactTestClass) obj)._ContactType))
      {
        return false;
      }
      return true;
    }

    public override int GetHashCode ()
    {
      throw new NotImplementedException ();
    }


    [Column (Storage = "_ContactID", AutoSync = AutoSync.OnInsert, DbType = "Int NOT NULL IDENTITY", IsPrimaryKey = true, IsDbGenerated = true)]
    public int ContactID
    {
      get
      {
        return this._ContactID;
      }
      set { this._ContactID = value; }
    }

    [Column (Storage = "_ContactType", DbType = "NVarChar(50)", IsDiscriminator = true)]
    public string ContactType
    {
      get
      {
        return this._ContactType;
      }
      set { this._ContactType = value; }
    }
  }

}
