using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Data.OleDb;
using Telerik.Web.UI;
using System.Configuration;
using System.Linq;

public partial class inventory_invhistory : System.Web.UI.Page
{
    AzzierScreen screen;
    private string connstring;
    protected string querymode = "edit";
    protected string m_itemnum, m_olditemnum = "";
    protected RadGrid grditemhistory;
    protected RadGrid grdstoreroomhistory;

    Items objItems;
    NameValueCollection nvcitems;
    protected int statuscode = 0;
    protected NameValueCollection m_msg = new NameValueCollection();
    protected NameValueCollection m_rights;
    protected int m_allowedit = 0;
    protected string m_vendor = "";
    protected string filterstr = "";
    protected string wherestring = "";
    protected string wherestring2 = "";


    protected void Page_Init(object sender, EventArgs e)
    {
        RetrieveMessage();
        UserRights.CheckAccess('');
        UserRights r = new UserRights(Session["Login"].ToString(), "UserRights", "counter");
        m_rights = r.GetRights(Session["Login"].ToString(), "inventory");

        m_allowedit = Convert.ToInt16(m_rights["urEdit"].ToString());

        Session.LCID = Convert.ToInt32(Session["LCID"]);


        if (Request.QueryString["itemnum"] != null)
        {
          m_itemnum = Request.QueryString["itemnum"].ToString();
        }
        else
        {
          Response.Write("<script>alert('Illegal Access');document.location.href='invmain.aspx';</script>");
          Response.End();
        }


        
        objItems = new Items(Session["Login"].ToString(), "Items", "ItemNum", m_itemnum);
        m_vendor = objItems.ModuleData["vendor"];
        nvcitems = objItems.ModuleData;
        
        hidMode.Value = querymode;
        connstring = Application["ConnString"].ToString();
        InitScreen();
    }

    private void InitScreen()
    {
        screen = new AzzierScreen("inventory/invhistory.aspx", "MainForm", MainControlsPanel.Controls, querymode);
        Session.LCID = Convert.ToInt16(Session["LCID"]);
        screen.LCID = Session.LCID;
        InitGrid();
        screen.LoadScreen();
    }

    public DataTable GetDataTable(string query)
    {
      String ConnString = Application["ConnString"].ToString();
      OleDbConnection conn = new OleDbConnection(ConnString);
      OleDbDataAdapter adapter = new OleDbDataAdapter();
      adapter.SelectCommand = new OleDbCommand(query, conn);

      DataTable myDataTable = new DataTable();

      conn.Open();
      try
      {
        adapter.Fill(myDataTable);
      }
      finally
      {
        conn.Close();
      }

      return myDataTable;
    }

    private void InitGrid()
    {

      Validation v = new Validation();
      filterstr = "linktable^items,linkid^" + m_itemnum;
      wherestring = v.AddLinqConditions(filterstr, "", "", "HistoryLog");

      grditemhistory = new RadGrid();
      grditemhistory.ID = "grditemhistory";
      grditemhistory.ClientSettings.Scrolling.AllowScroll = true;
      grditemhistory.ClientSettings.Scrolling.SaveScrollPosition = true;
      grditemhistory.ClientSettings.Scrolling.UseStaticHeaders = true;
      grditemhistory.ClientSettings.EnableRowHoverStyle = true;

      grditemhistory.MasterTableView.TableLayout = GridTableLayout.Fixed;
      //grdloclist.PagerStyle.Visible = false;
      grditemhistory.PagerStyle.Visible = true;
      grditemhistory.PagerStyle.AlwaysVisible = true;
      grditemhistory.Skin = "Outlook";

      grditemhistory.AutoGenerateColumns = false;
      grditemhistory.AllowPaging = true;
      grditemhistory.PageSize = 100;
      grditemhistory.AllowSorting = true;
      grditemhistory.MasterTableView.AllowMultiColumnSorting = true;
      grditemhistory.AllowFilteringByColumn = true;
      grditemhistory.MasterTableView.CommandItemDisplay = GridCommandItemDisplay.Top;
      grditemhistory.MasterTableView.DataKeyNames = new string[] { "Counter" };
      //string sql = "Select * From HsitoryLog Where linktable='Items' And Linkid='" + m_itemnum + "'";
      //ItemHistorySqlDataSource.SelectCommand = sql;

      //ItemHistorySqlDataSource.ConnectionString = connstring;
      grditemhistory.ClientSettings.EnableAlternatingItems = true;

      grditemhistory.PagerStyle.Visible = true;
      grditemhistory.PagerStyle.AlwaysVisible = true;

      grditemhistory.MasterTableView.DataKeyNames = new string[] { "Counter"};
      grditemhistory.MasterTableView.CommandItemTemplate = new CodesCommandItem("Item History");
      grditemhistory.MasterTableView.CommandItemDisplay = GridCommandItemDisplay.Top;

      grditemhistory.ClientSettings.DataBinding.FilterParameterType = GridClientDataBindingParameterType.Linq;
      grditemhistory.ClientSettings.DataBinding.SortParameterType = GridClientDataBindingParameterType.Linq;

      grditemhistory.ItemDataBound+=new GridItemEventHandler(grditemhistory_ItemDataBound);
      screen.SetGridColumns("itemhistory", grditemhistory);
      MainControlsPanel.Controls.Add(grditemhistory);

      grdstoreroomhistory = new RadGrid();
      grdstoreroomhistory.ID = "grdtransactions";
      grdstoreroomhistory.ClientSettings.Scrolling.AllowScroll = true;
      grdstoreroomhistory.ClientSettings.Scrolling.SaveScrollPosition = true;
      grdstoreroomhistory.ClientSettings.Scrolling.UseStaticHeaders = true;
      grdstoreroomhistory.ClientSettings.EnableRowHoverStyle = true;

      grdstoreroomhistory.MasterTableView.TableLayout = GridTableLayout.Fixed;
      //grdloclist.PagerStyle.Visible = false;
      grdstoreroomhistory.PagerStyle.Visible = true;
      grdstoreroomhistory.PagerStyle.AlwaysVisible = true;
      grdstoreroomhistory.Skin = "Outlook";

      grdstoreroomhistory.AutoGenerateColumns = false;
      grdstoreroomhistory.AllowPaging = true;
      grdstoreroomhistory.PageSize = 100;
      grdstoreroomhistory.AllowSorting = true;
      grdstoreroomhistory.MasterTableView.AllowMultiColumnSorting = true;
      grdstoreroomhistory.AllowFilteringByColumn = true;
      grdstoreroomhistory.MasterTableView.CommandItemDisplay = GridCommandItemDisplay.Top;
      grdstoreroomhistory.MasterTableView.DataKeyNames = new string[] { "Counter" };
      //string sql = "Select * From HsitoryLog Where linktable='Items' And Linkid='" + m_itemnum + "'";
      //ItemHistorySqlDataSource.SelectCommand = sql;

      //ItemHistorySqlDataSource.ConnectionString = connstring;
      grdstoreroomhistory.ClientSettings.EnableAlternatingItems = true;

      grdstoreroomhistory.PagerStyle.Visible = true;
      grdstoreroomhistory.PagerStyle.AlwaysVisible = true;

      grdstoreroomhistory.MasterTableView.DataKeyNames = new string[] { "Counter" };
      grdstoreroomhistory.MasterTableView.CommandItemTemplate = new CodesCommandItem("Item Transaction History");
      grdstoreroomhistory.MasterTableView.CommandItemDisplay = GridCommandItemDisplay.Top;

      grdstoreroomhistory.ClientSettings.DataBinding.FilterParameterType = GridClientDataBindingParameterType.Linq;
      grdstoreroomhistory.ClientSettings.DataBinding.SortParameterType = GridClientDataBindingParameterType.Linq;

      grdstoreroomhistory.ItemDataBound += new GridItemEventHandler(grditemhistory_ItemDataBound);
      screen.SetGridColumns("transactions", grdstoreroomhistory);
      MainControlsPanel.Controls.Add(grdstoreroomhistory);
      filterstr = "itemnum^" + m_itemnum;
      wherestring2 = v.AddLinqConditions(filterstr, "", "", "v_InventoryTransDetail");



      
    }

    protected void grditemhistory_ItemDataBound(object sender, GridItemEventArgs e)
    {
      screen.GridItemDataBound(e, "inventory/invhistory.aspx", "MainForm", "itemhistory");
    }

     protected void Page_Load(object sender, EventArgs e)
    {
      grditemhistory.ClientSettings.DataBinding.SelectMethod = "GetHistory?where=" + wherestring;
      grditemhistory.ClientSettings.DataBinding.Location = "../InternalServices/ServiceHistory.svc";
      grdstoreroomhistory.ClientSettings.DataBinding.SelectMethod = "GetTransactionHistory?where=" + wherestring2;
      grdstoreroomhistory.ClientSettings.DataBinding.Location = "../InternalServices/ServiceInventory.svc";
      if (!Page.IsPostBack)
      {
          screen.PopulateScreen("Items", nvcitems);
      }

      ucHeader1.Mode = querymode;
      ucHeader1.TabName = "History";
      ucHeader1.ModuleData = nvcitems;

    }

    private void RetrieveMessage()
    {
        SystemMessage msg = new SystemMessage("inventory/invmain.aspx");
        m_msg = msg.GetSystemMessage();
        msg.SetJsMessage(litMessage);
    }
}