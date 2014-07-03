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

public partial class inventory_invvendor : System.Web.UI.Page
{
    AzzierScreen screen;
    private string connstring;
    protected string querymode = "edit";
    protected string m_itemnum, m_olditemnum = "";
    protected RadGrid grditemvendor;

    Items objItems;
    NameValueCollection nvcitems;
    protected int statuscode = 0;
    protected NameValueCollection m_msg = new NameValueCollection();
    protected NameValueCollection m_rights;
    protected int m_allowedit = 0;
    protected string m_vendor = "";


    protected void Page_Init(object sender, EventArgs e)
    {
        RetrieveMessage();
        if (Session["Login"] == null)
        {
            Response.Write("<script>alert('" + m_msg["T1"] + "');top.document.location.href='../Login.aspx';</script>");
            Response.End();
        }
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
        screen = new AzzierScreen("inventory/invvendor.aspx", "MainForm", MainControlsPanel.Controls, querymode);
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
      grditemvendor = new RadGrid();
      grditemvendor.ID = "grditemvendor";
      grditemvendor.DataSourceID = "VendorSQLDataSource";
      grditemvendor.PageSize = 100;
      grditemvendor.AllowPaging = true;
      grditemvendor.AllowSorting = true;
      grditemvendor.MasterTableView.AllowMultiColumnSorting = true;
      grditemvendor.MasterTableView.AutoGenerateColumns = false;
      string sql = "Select * From ItemVendor Where itemnum='" + m_itemnum + "'";
      VendorSqlDataSource.SelectCommand = sql;

      VendorSqlDataSource.ConnectionString = connstring;
      grditemvendor.ClientSettings.EnableAlternatingItems = false;

      GridEditCommandColumn EditColumn = new GridEditCommandColumn();
      EditColumn.HeaderText = "Edit";
      EditColumn.UniqueName = "EditCommand";
      EditColumn.ButtonType = GridButtonColumnType.ImageButton;
      EditColumn.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
      EditColumn.HeaderStyle.Width = 30;
      grditemvendor.MasterTableView.Columns.Add(EditColumn);
      grditemvendor.MasterTableView.DataKeyNames = new string[] { "Counter","ItemNum","Vendor" };
      screen.SetGridColumns("itemvendor", grditemvendor);
      //grdinvmain.MasterTableView.CommandItemTemplate = new InsertFormItemTemplate("Inventory", null, "return editstoremain('');", m_allowedit);
      grditemvendor.MasterTableView.CommandItemTemplate = new MultiFunctionItemTemplate(1,true,"Inventory",null,"return edititemvendor('' );",m_allowedit,"",false);
      grditemvendor.MasterTableView.CommandItemDisplay = GridCommandItemDisplay.Top;
      grditemvendor.ItemDataBound+=new GridItemEventHandler(grditemvendor_ItemDataBound);
      
      MainControlsPanel.Controls.Add(grditemvendor);

    }

    protected void grditemvendor_ItemDataBound(object sender, GridItemEventArgs e)
    {
      if (e.Item is GridDataItem && !e.Item.IsInEditMode)
      {
        GridDataItem item = (GridDataItem)e.Item;
        ImageButton btn = item["EditCommand"].Controls[0] as ImageButton;
        if (btn != null)
        {
          btn.ImageUrl = "~/Images2/Edit.gif";
          btn.OnClientClick = "edititemvendor('" + item.OwnerTableView.DataKeyValues[item.ItemIndex]["Counter"].ToString() + "','" + m_itemnum + "');return false;";
        }
      }
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!Page.IsPostBack)
        {
            screen.PopulateScreen("Items", nvcitems);
        }

        ucHeader1.Mode = querymode;
        ucHeader1.TabName = "Vendors";
        ucHeader1.ModuleData = nvcitems;

    }

    private void RetrieveMessage()
    {
        SystemMessage msg = new SystemMessage("inventory/invmain.aspx");
        m_msg = msg.GetSystemMessage();
        msg.SetJsMessage(litMessage);
    }
}