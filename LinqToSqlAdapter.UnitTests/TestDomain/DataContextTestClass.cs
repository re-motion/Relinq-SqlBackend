// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (c) rubicon IT GmbH, www.rubicon.eu
// 
// re-linq is free software; you can redistribute it and/or modify it under 
// the terms of the GNU Lesser General Public License as published by the 
// Free Software Foundation; either version 2.1 of the License, 
// or (at your option) any later version.
// 
// re-linq is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-linq; if not, see http://www.gnu.org/licenses.
// 

using System;
using System.Data.Linq;
using System.Data.Linq.Mapping;

namespace Remotion.Linq.LinqToSqlAdapter.UnitTests.TestDomain
{
  [Database (Name = "DATACONTEXT")]
  internal class DataContextTestClass : DataContext
  {
    private static readonly MappingSource mappingSource = new AttributeMappingSource ();

    #region constructors

    static DataContextTestClass ()
    {
    }

    public DataContextTestClass (string connection)
      : base (connection, mappingSource)
    {
    }

    #endregion

    #region inner classes
    
    [Table (Name = "dbo.Customers")]
    public class Customer
    {
      private string _CustomerID;
      private string _CompanyName;
      private readonly EntitySet<Order> _Orders = new EntitySet<Order> ();

      [Column (Storage = "_CustomerID", DbType = "NChar(5) NOT NULL", CanBeNull = false, IsPrimaryKey = true)]
      public string CustomerID
      {
        get { return _CustomerID; }
        set
        {
          _CustomerID = value;
        }
      }

      [Column (Storage = "_CompanyName", DbType = "NVarChar(40) NOT NULL", CanBeNull = false)]
      public string CompanyName
      {
        get { return _CompanyName; }
        set { _CompanyName = value; }
      }

      [Association (Name = "Customer_Order", Storage = "_Orders", OtherKey = "CustomerID")]
      public EntitySet<Order> Orders
      {
        get { return _Orders; }
        set { _Orders.Assign (value); }
      }

      public override bool Equals (object obj)
      {
        if (!(obj is Customer))
          return false;

        return Equals ((Customer) obj);
      }

      private bool Equals(Customer customer)
      {
        return customer.CompanyName == CompanyName && customer.Orders == Orders && customer.CustomerID == CustomerID;
      }

      public override int GetHashCode ()
      {
        unchecked
        {
          int result = (_CustomerID != null ? _CustomerID.GetHashCode() : 0);
          result = (result * 397) ^ (_CompanyName != null ? _CompanyName.GetHashCode() : 0);
          result = (result * 397) ^ (_Orders != null ? _Orders.GetHashCode() : 0);
          return result;
        }
      }
    }

    [Table (Name = "dbo.Region")]
    public class Region
    {
      private int _RegionID;

      private string _RegionDescription;

      private readonly EntitySet<Territory> _Territories = new EntitySet<Territory> ();


      [Column (Storage = "_RegionID", DbType = "Int NOT NULL", IsPrimaryKey = true)]
      public int RegionID
      {
        get { return _RegionID; }
        set
        {
          _RegionID = value;
        }
      }

      [Column (Storage = "_RegionDescription", DbType = "NChar(50) NOT NULL", CanBeNull = false)]
      public string RegionDescription
      {
        get { return _RegionDescription; }
        set
        {
          _RegionDescription = value;
        }
      }

      [Association (Name = "Region_Territory", Storage = "_Territories", OtherKey = "RegionID")]
      public EntitySet<Territory> Territories
      {
        get { return _Territories; }
        set { _Territories.Assign (value); }
      }
    }

    [Table (Name = "dbo.Territories")]
    public class Territory
    {
      private int? _RegionID;

      // Added to test a join info involving a nullable key
      [Column (Storage = "_RegionID", DbType = "Int NULL")]
      public int? RegionID
      {
        get { return _RegionID; }
        set { _RegionID = value; }
      }
    }

    [Table (Name = "dbo.Orders")]
    public class Order
    {
      private int _OrderID;
      private string _CustomerID;

      private EntityRef<Customer> _Customer;

      [Column (Storage = "_OrderID", AutoSync = AutoSync.OnInsert, DbType = "Int NOT NULL IDENTITY", IsPrimaryKey = true, IsDbGenerated = true)]
      public int OrderID
      {
        get { return _OrderID; }
        set
        {
          if ((_OrderID != value))
          {
            //this.OnOrderIDChanging (value);
            //this.SendPropertyChanging ();
            _OrderID = value;
            //this.SendPropertyChanged ("OrderID");
            //this.OnOrderIDChanged ();
          }
        }
      }

      [Column (Storage = "_CustomerID", DbType = "NChar(5)")]
      public string CustomerID
      {
        get { return _CustomerID; }
        set
        {
          if ((_CustomerID != value))
          {
            //if (this._Customer.HasLoadedOrAssignedValue)
            //{
            //throw new System.Data.Linq.ForeignKeyReferenceAlreadyHasValueException ();
            //}
            //this.OnCustomerIDChanging (value);
            //this.SendPropertyChanging ();
            _CustomerID = value;
            //this.SendPropertyChanged ("CustomerID");
            //this.OnCustomerIDChanged ();
          }
        }
      }

      [Association (Name = "Customer_Order", Storage = "_Customer", ThisKey = "CustomerID", IsForeignKey = true)]
      public Customer Customer
      {
        get { return _Customer.Entity; }
        set
        {
          Customer previousValue = _Customer.Entity;
          if (((previousValue != value)
               || (_Customer.HasLoadedOrAssignedValue == false)))
          {
            //this.SendPropertyChanging ();
            if ((previousValue != null))
            {
              _Customer.Entity = null;
              previousValue.Orders.Remove (this);
            }
            _Customer.Entity = value;
            if ((value != null))
            {
              value.Orders.Add (this);
              _CustomerID = value.CustomerID;
            }
            else
              _CustomerID = default (string);
            //this.SendPropertyChanged ("Customer");
          }
        }
      }
    }

    [Table (Name = "dbo.FakeClassWithoutPrimaryKey")]
    public class FakeClassWithoutPrimaryKey
    {
      private string _Name;

      [Column (Storage = "_Name", DbType = "NVarChar(40) NOT NULL", CanBeNull = false, IsPrimaryKey = false)]
      public string Name
      {
        get { return _Name; }
        set { _Name = value; }
      }
    }

    [Table (Name = "dbo.FakeClassWithoutInheritanceCodeBase")]
    public class FakeClassWithoutInheritanceCodeBase
    {
      private string _BaseName;

      [Column (Storage = "_BaseName", DbType = "NVarChar(40) NOT NULL", CanBeNull = false, IsPrimaryKey = false)]
      public string BaseName
      {
        get { return _BaseName; }
        set { _BaseName = value; }
      }
    }

    [Table (Name = "dbo.FakeClassWithoutInheritanceCode")]
    public class FakeClassWithoutInheritanceCode : FakeClassWithoutInheritanceCodeBase
    {
      private string _Name;

      [Column (Storage = "_Name", DbType = "NVarChar(40) NOT NULL", CanBeNull = false, IsPrimaryKey = false)]
      public string Name
      {
        get { return _Name; }
        set { _Name = value; }
      }
    }

    [Table (Name = "dbo.ClassWithCompoundPrimaryKey")]
    public class ClassWithCompoundPrimaryKey
    {
      private int _Key1;
      private string _Key2;

      [Column (Storage = "_Key1", DbType = "Int NOT NULL", IsPrimaryKey = true)]
      public int Key1
      {
        get
        {
          return this._Key1;
        }
        set { _Key1 = value; }
        }

      [Column (Storage = "_Key2", DbType = "NVarChar(20) NOT NULL", CanBeNull = false, IsPrimaryKey = true)]
      public string Key2
      {
        get
        {
          return _Key2;
        }
        set { _Key2 = value; }
      }
    }

    [Table (Name = "dbo.ClassWithOneToOneRelation_ForeignKeySide")]
    public class ClassWithOneToOneRelation_ForeignKeySide
    {
      private int _ID;
      private int _VirtualSideID;
      private ClassWithOneToOneRelation_VirtualSide _VirtualSide;

      [Column (Storage = "_ID", DbType = "Int NOT NULL", IsPrimaryKey = true)]
      public int ID
      {
        get
        {
          return this._ID;
        }
        set { _ID = value; }
      }

      [Column (Storage = "_VirtualSideID", DbType = "Int")]
      public int VirtualSideID
      {
        get { return _VirtualSideID; }
        set
        {
          _VirtualSideID = value;
        }
      }

      [Association (Name = "OneToOneRelation", Storage = "_VirtualSide", ThisKey = "VirtualSideID", IsForeignKey = true)]
      public ClassWithOneToOneRelation_VirtualSide VirtualSide
      {
        get { return _VirtualSide; }
        set { _VirtualSide = value; }
      }
    }

    [Table (Name = "dbo.ClassWithOneToOneRelation_VirtualSide")]
    public class ClassWithOneToOneRelation_VirtualSide
    {
      private int _ID;
      private int _OtherMember;
      private ClassWithOneToOneRelation_ForeignKeySide _ForeignKeySide;

      [Column (Storage = "_ID", DbType = "Int NOT NULL", IsPrimaryKey = true)]
      public int ID
      {
        get
        {
          return this._ID;
        }
        set { _ID = value; }
      }

      [Association (Name = "OneToOneRelation", Storage = "_ForeignKeySide", IsForeignKey = false)]
      public ClassWithOneToOneRelation_ForeignKeySide ForeignKeySide
      {
        get { return _ForeignKeySide; }
        set { _ForeignKeySide = value; }
      }

      [Column (Storage = "_OtherMember", DbType = "Int NOT NULL")]
      public int OtherMember
      {
        get
        {
          return this._OtherMember;
        }
        set { _OtherMember = value; }
      }
    }

    #endregion
  }
}