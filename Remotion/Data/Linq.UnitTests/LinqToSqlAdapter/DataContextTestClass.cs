using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Text;


namespace Remotion.Data.Linq.UnitTests.LinqToSqlAdapter
{
  [System.Data.Linq.Mapping.DatabaseAttribute (Name = "DATACONTEXT")]
  class DataContextTestClass : System.Data.Linq.DataContext
  {
    private static System.Data.Linq.Mapping.MappingSource mappingSource = new AttributeMappingSource ();

    #region constructors

    static DataContextTestClass ()
    {

    }

    public DataContextTestClass (string connection)
      : base (connection, mappingSource)
    {

    }

    #endregion

    [Table (Name = "dbo.Customers")]
    public partial class Customer //: INotifyPropertyChanging, INotifyPropertyChanged
    {
      private string _CustomerID;
      private string _CompanyName;
      private EntitySet<Order> _Orders = new EntitySet<Order>();

      [Column (Storage = "_CustomerID", DbType = "NChar(5) NOT NULL", CanBeNull = false, IsPrimaryKey = true)]
      public string CustomerID
      {
        get
        {
          return this._CustomerID;
        }
        set
        {
          if ((this._CustomerID != value))
          {
            //this.OnCustomerIDChanging (value);
            //this.SendPropertyChanging ();
            this._CustomerID = value;
            //this.SendPropertyChanged ("CustomerID");
            //this.OnCustomerIDChanged ();
          }
        }
      }

      [Column (Storage = "_CompanyName", DbType = "NVarChar(40) NOT NULL", CanBeNull = false)]
      public string CompanyName
      {
        get
        {
          return this._CompanyName;
        }
        set
        {
          if ((this._CompanyName != value))
          {
            //this.OnCompanyNameChanging (value);
            //this.SendPropertyChanging ();
            this._CompanyName = value;
            //this.SendPropertyChanged ("CompanyName");
            //this.OnCompanyNameChanged ();
          }
        }
      }

      [Association (Name = "Customer_Order", Storage = "_Orders", OtherKey = "CustomerID")]
      public EntitySet<Order> Orders
      {
        get
        {
          return this._Orders;
        }
        set
        {
          this._Orders.Assign (value);
        }
      }


    }
    [Table (Name = "dbo.Region")]
    public partial class Region //: INotifyPropertyChanging, INotifyPropertyChanged
    {
      private int _RegionID;

      private string _RegionDescription;

      private EntitySet<Territory> _Territories = new EntitySet<Territory> ();


      [Column (Storage = "_RegionID", DbType = "Int NOT NULL", IsPrimaryKey = true)]
      public int RegionID
      {
        get
        {
          return this._RegionID;
        }
        set
        {
          if ((this._RegionID != value))
          {
            //this.OnRegionIDChanging (value);
            //this.SendPropertyChanging ();
            this._RegionID = value;
            //this.SendPropertyChanged ("RegionID");
            //this.OnRegionIDChanged ();
          }
        }
      }

      [Column (Storage = "_RegionDescription", DbType = "NChar(50) NOT NULL", CanBeNull = false)]
      public string RegionDescription
      {
        get
        {
          return this._RegionDescription;
        }
        set
        {
          if ((this._RegionDescription != value))
          {
            //this.OnRegionDescriptionChanging (value);
            //this.SendPropertyChanging ();
            this._RegionDescription = value;
            //this.SendPropertyChanged ("RegionDescription");
            //this.OnRegionDescriptionChanged ();
          }
        }
      }

      [Association (Name = "Region_Territory", Storage = "_Territories", OtherKey = "RegionID")]
      public EntitySet<Territory> Territories
      {
        get
        {
          return this._Territories;
        }
        set
        {
          this._Territories.Assign (value);
        }
      }

    }

    [Table (Name = "dbo.Territories")]
    public partial class Territory //: INotifyPropertyChanging, INotifyPropertyChanged
    {


    }

    [Table (Name = "dbo.Orders")]
    public partial class Order// : INotifyPropertyChanging, INotifyPropertyChanged
    {
      private int _OrderID;
      private string _CustomerID;

      private EntityRef<Customer> _Customer;

      [Column (Storage = "_OrderID", AutoSync = AutoSync.OnInsert, DbType = "Int NOT NULL IDENTITY", IsPrimaryKey = true, IsDbGenerated = true)]
      public int OrderID
      {
        get
        {
          return this._OrderID;
        }
        set
        {
          if ((this._OrderID != value))
          {
            //this.OnOrderIDChanging (value);
            //this.SendPropertyChanging ();
            this._OrderID = value;
            //this.SendPropertyChanged ("OrderID");
            //this.OnOrderIDChanged ();
          }
        }
      }

      [Column (Storage = "_CustomerID", DbType = "NChar(5)")]
      public string CustomerID
      {
        get
        {
          return this._CustomerID;
        }
        set
        {
          if ((this._CustomerID != value))
          {
            //if (this._Customer.HasLoadedOrAssignedValue)
            //{
              //throw new System.Data.Linq.ForeignKeyReferenceAlreadyHasValueException ();
            //}
            //this.OnCustomerIDChanging (value);
            //this.SendPropertyChanging ();
            this._CustomerID = value;
            //this.SendPropertyChanged ("CustomerID");
            //this.OnCustomerIDChanged ();
          }
        }
      }

      [Association (Name = "Customer_Order", Storage = "_Customer", ThisKey = "CustomerID", IsForeignKey = true)]
      public Customer Customer
      {
        get
        {
          return this._Customer.Entity;
        }
        set
        {
          Customer previousValue = this._Customer.Entity;
          if (((previousValue != value)
                || (this._Customer.HasLoadedOrAssignedValue == false)))
          {
            //this.SendPropertyChanging ();
            if ((previousValue != null))
            {
              this._Customer.Entity = null;
              previousValue.Orders.Remove (this);
            }
            this._Customer.Entity = value;
            if ((value != null))
            {
              value.Orders.Add (this);
              this._CustomerID = value.CustomerID;
            }
            else
            {
              this._CustomerID = default (string);
            }
            //this.SendPropertyChanged ("Customer");
          }
        }
      }
    }

  } //DataContextTestClass
}