using System;
using System.Data.Linq.Mapping;

// TODO Review: Move the test classes (ContactTestClass, PersonTestClass, DataContextTestClass) to a TestDomain namespace
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

      public override bool Equals (object obj)
      {
        if (obj == null || GetType() != obj.GetType())
        {
          return false;
        }
        if(_HomePage != ((SupplierContact)obj).HomePage)
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
      private byte[] _Password;
      private System.Data.Linq.Binary _Photo;

      [Column (Storage = "_Password", DbType = "NText", UpdateCheck = UpdateCheck.Never)]
      public byte[] Password
      {
        get
        {
          return this._Password;
        }
        set
        {
          this._Password = value;
        }
      }

      [Column (Storage = "_Photo", DbType = "Image", CanBeNull = true, UpdateCheck = UpdateCheck.Never)]
      public System.Data.Linq.Binary Photo
      {
        get
        {
          return this._Photo;
        }
        set
        {
          _Photo = value;
        }
      }

      public override bool Equals (object obj)
      {
        if (obj == null || GetType () != obj.GetType ())
          return false;

        if (_Password != ((EmployeeContact) obj).Password)
          return false;

        if (_Photo != ((EmployeeContact) obj).Photo)
          return false;

        return base.Equals (obj);
      }

      public override int GetHashCode ()
      {
        throw new NotImplementedException ();
      }
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


      if (_ContactType != ((ContactTestClass) obj)._ContactType)
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
