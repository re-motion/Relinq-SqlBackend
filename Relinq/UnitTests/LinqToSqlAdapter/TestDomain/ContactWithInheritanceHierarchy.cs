// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// as published by the Free Software Foundation; either version 2.1 of the 
// License, or (at your option) any later version.
// 
// re-motion is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-motion; if not, see http://www.gnu.org/licenses.
// 
using System;
using System.Data.Linq.Mapping;

namespace Remotion.Linq.UnitTests.LinqToSqlAdapter.TestDomain
{
  [Table (Name = "dbo.Contacts")]
  [InheritanceMapping (Code = "Unknown", Type = typeof (ContactWithInheritanceHierarchy), IsDefault = true)]
  [InheritanceMapping (Code = "Employee", Type = typeof (EmployeeContact))]
  [InheritanceMapping (Code = "Supplier", Type = typeof (SupplierContact))]
  [InheritanceMapping (Code = "Customer", Type = typeof (CustomerContact))]
  [InheritanceMapping (Code = "Shipper", Type = typeof (ShipperContact))]
  public class ContactWithInheritanceHierarchy
  {
    #region inner classes

    public abstract class FullContact : ContactWithInheritanceHierarchy
    {
    }

    public class CustomerContact : FullContact
    {
    }

    public class ShipperContact : ContactWithInheritanceHierarchy
    {
    }

    public class SupplierContact : FullContact
    {
      private string _HomePage;
      //private ShipperContact _ShipperContact;

      [Column (Storage = "_HomePage", DbType = "NText", UpdateCheck = UpdateCheck.Never)]
      public string HomePage
      {
        get { return this._HomePage; }
        set { this._HomePage = value; }
      }

      public override bool Equals (object obj)
      {
        if (obj == null || GetType() != obj.GetType())
          return false;
        if (_HomePage != ((SupplierContact) obj).HomePage)
          return false;

        return base.Equals (obj);
      }

      public override int GetHashCode ()
      {
        throw new NotImplementedException();
      }
    }

    public class EmployeeContact : ContactWithInheritanceHierarchy
    {
      private byte[] _Password;
      private System.Data.Linq.Binary _Photo;

      [Column (Storage = "_Password", DbType = "NText", UpdateCheck = UpdateCheck.Never)]
      public byte[] Password
      {
        get { return this._Password; }
        set { this._Password = value; }
      }

      [Column (Storage = "_Photo", DbType = "Image", CanBeNull = true, UpdateCheck = UpdateCheck.Never, Name = "PhotoColumn")]
      public System.Data.Linq.Binary Photo
      {
        get { return this._Photo; }
        set { _Photo = value; }
      }

      public override bool Equals (object obj)
      {
        if (obj == null || GetType() != obj.GetType())
          return false;

        if (_Password != ((EmployeeContact) obj).Password)
          return false;

        if (_Photo != ((EmployeeContact) obj).Photo)
          return false;

        return base.Equals (obj);
      }

      public override int GetHashCode ()
      {
        throw new NotImplementedException();
      }
    }

    #endregion

    private int _ContactID;
    private string _ContactType;

    public override bool Equals (object obj)
    {
      if (obj == null || GetType() != obj.GetType())
        return false;

      if (_ContactID != ((ContactWithInheritanceHierarchy) obj)._ContactID)
        return false;


      if (_ContactType != ((ContactWithInheritanceHierarchy) obj)._ContactType)
        return false;
      return true;
    }

    public override int GetHashCode ()
    {
      throw new NotImplementedException();
    }


    [Column (Storage = "_ContactID", AutoSync = AutoSync.OnInsert, DbType = "Int NOT NULL IDENTITY", IsPrimaryKey = true, IsDbGenerated = true)]
    public int ContactID
    {
      get { return this._ContactID; }
      set { this._ContactID = value; }
    }

    [Column (Storage = "_ContactType", DbType = "NVarChar(50)", IsDiscriminator = true)]
    public string ContactType
    {
      get { return this._ContactType; }
      set { this._ContactType = value; }
    }
  }
}