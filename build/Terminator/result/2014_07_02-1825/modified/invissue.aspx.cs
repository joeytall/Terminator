using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections.Specialized;
using Telerik.Web.UI;
using System.DirectoryServices.ActiveDirectory;
using System.Data;

public partial class inventory_invissue : System.Web.UI.Page
{
  protected NameValueCollection m_msg = new NameValueCollection();
  protected string m_itemnum;
  protected string m_storeroom;
  protected string m_reservecounter = "";
  protected Boolean serialized = false;
  protected AzzierScreen screen;
  protected double m_qtyonhand = 0;  // all stock minus reserved ones
  protected NameValueCollection m_rights;
  protected Int16 m_allowedit;
  protected ModuleoObject objInvLot;
  protected NameValueCollection nvc;
  protected ModuleoObject objinvstore;
  protected RadGrid grdinvissue;
  protected string m_issuemethod = "";
  protected string m_issueprice = "";
  protected Single m_fixprice = 0;
  protected Single m_reserved = 0;
  protected string m_linecounter = "";
  protected string m_wonum = "";
  protected string m_reqnum = "";
  protected string m_equipment = "";
  protected string m_location = "";
  protected string m_draccount = "";
  protected string m_craccount = "";
  protected string m_empid = "";

    protected void Page_Init(object sender, EventArgs e)
    {
      RetrieveMessage();
      UserRights.CheckAccess('');
      Session.LCID = Convert.ToInt32(Session["LCID"]);
      if (Request.QueryString["itemnum"] != null)
      {
        m_itemnum = Request.QueryString["itemnum"].ToString();
      }
      if (Request.QueryString["storeroom"] != null)
      {
        m_storeroom = Request.QueryString["storeroom"].ToString();
      }

      if (Request.QueryString["wonum"] != null)
      {
        m_wonum = Request.QueryString["wonum"].ToString();
      }

      if (Request.QueryString["linecounter"] != null)
      {
        m_linecounter = Request.QueryString["linecounter"].ToString();
      }

      if (Request.QueryString["equipment"] != null)
      {
        m_equipment = Request.QueryString["equipment"].ToString();
      }

      if (Request.QueryString["location"] != null)
      {
        m_location = Request.QueryString["location"].ToString();
      }

      if (Request.QueryString["draccount"] != null)
      {
        m_draccount = Request.QueryString["account"].ToString();
      }

      if (Request.QueryString["empid"] != null)
      {
        m_empid = Request.QueryString["empid"].ToString();
      }

      if (Request.QueryString["reservecounter"] != null)
      {
        m_reservecounter = Request.QueryString["reservecounter"].ToString();
      }

      if (m_itemnum == "" || m_storeroom == "")
      {
        Response.Write("<script>alert('" + m_msg["T2"] + "');top.document.location.href='../Login.aspx';</script>");
        Response.End();
      }



      UserRights r = new UserRights(Session["Login"].ToString(), "UserRights", "counter");
      m_rights = r.GetRights(Session["Login"].ToString(), "workorder");
      m_allowedit = Convert.ToInt16(m_rights["urEdit"].ToString());
      
      Inventory i = new Inventory(Session["Login"].ToString(), m_itemnum, m_storeroom);
      objinvstore = i.InventoryStore;
      m_issueprice = objinvstore.ModuleData["issueprice"];
      serialized = (objinvstore.ModuleData["Serialized"] == "1");
      screen = new AzzierScreen("inventory/invissue.aspx", "MainForm", MainControlsPanel.Controls);
      InitGrid();
      screen.LoadScreen();
      screen.SetValidationControls();
    }

    protected void Page_Load(object sender, EventArgs e)
    {
      if (!Page.IsPostBack)
      {
        screen.PopulateScreen("v_inventorystoreroom", objinvstore.ModuleData);

        hidchargeback.Value = "0";

        CheckBox c = MainControlsPanel.FindControl("chkchargeback") as CheckBox;
        if (c != null)
        {
          c.Checked = false;
          //c.CheckedChanged = "togglechargeback()";
          c.Attributes.Add("onchange", "togglechargeback()");

        }

        c = MainControlsPanel.FindControl("chkserialized") as CheckBox;
        if (c != null)
        {
          c.Attributes.Add("onclick", "return false;");
          c.Attributes.Add("onkeydown", "return false;");
        }

        
        //TextBox t = MainControlsPanel.FindControl("txtissueto") as TextBox;
        //if (t != null)
        //{
        //  t.Attributes.Add("onchange", "alert('hi');");
        //}


        TextBox t = MainControlsPanel.FindControl("txtissuedate") as TextBox;
        if (t != null)
        {
          t.Text = DateTime.Today.ToShortDateString();
        }

        /*
        t = MainControlsPanel.FindControl("txtdraccount") as TextBox;
        if (t != null)
        {
          t.Text = objinvstore.ModuleData["DrAccount"];
        }

        t = MainControlsPanel.FindControl("txtcraccount") as TextBox;
        if (t != null)
        {
          t.Text = objinvstore.ModuleData["CrAccount"];
        }
         * */

        RadComboBox r = MainControlsPanel.FindControl("cbbissuetype") as RadComboBox;
        if (r != null)
        {
          RadComboBoxItem item = new RadComboBoxItem("WORKORDER", "WORKORDER");
          r.Items.Add(item);

          //item = new RadComboBoxItem("Item Request", "ITEMREQUEST");
          //r.Items.Add(item);

          item = new RadComboBoxItem("EQUIPMENT", "EQUIPMENT");
          r.Items.Add(item);

          item = new RadComboBoxItem("LOCATION", "LOCATION");
          r.Items.Add(item);

          item = new RadComboBoxItem("EMPLOYEE", "EMPLOYEE");
          r.Items.Add(item);

          item = new RadComboBoxItem("ACCOUNT", "ACCOUNT");
          r.Items.Add(item);

          r.Height = Unit.Pixel(130);

          r.OnClientSelectedIndexChanged = "issuetypechanged";
        }

        if (m_reservecounter != "") // calling from issue reserved page
        {
          ModuleoObject objreserve = new ModuleoObject(Session["Login"].ToString(), "InvReserves", "Counter", m_reservecounter);
          ModuleoObject objchargeto = null;
          if (objreserve.ModuleData["LinkType"] == "W")
          {
            objchargeto = new ModuleoObject(Session["Login"].ToString(), "Workorder", "WONum", objreserve.ModuleData["ChargeTo"]);
            m_wonum = objchargeto.ModuleData["WONum"];
            m_equipment = objchargeto.ModuleData["Equipment"];
            m_location = objchargeto.ModuleData["Location"];
            m_draccount = objchargeto.ModuleData["DrAccount"];
            m_craccount = objchargeto.ModuleData["crAccount"];
            
            r.SelectedIndex = 0;
          }
          if (objreserve.ModuleData["LinkType"] == "R")
          {
            objchargeto = new ModuleoObject(Session["Login"].ToString(), "ItemRequest", "ReqNum", objreserve.ModuleData["ChargeTo"]);
            m_draccount = objchargeto.ModuleData["DrAccount"];
            m_craccount = objchargeto.ModuleData["crAccount"];
            RadComboBoxItem reqitem = new RadComboBoxItem("REQUISITION", "REQUISITION");
            reqitem.Selected = true;
            r.Items.Add(reqitem);
            m_reqnum = objchargeto.ModuleData["ReqNum"];
          }
          screen.SetTextControlReadonly("txtwonum", MainControlsPanel);
          screen.SetTextControlReadonly("txtwolinecounter", MainControlsPanel);
          screen.SetTextControlReadonly("txtequipment", MainControlsPanel);
          screen.SetTextControlReadonly("txtlocation", MainControlsPanel);
          screen.SetTextControlReadonly("txtempid", MainControlsPanel);
          screen.SetTextControlReadonly("txtreqnum", MainControlsPanel);
          screen.SetTextControlReadonly("txtreqlinecounter", MainControlsPanel);
          r.Enabled = false;
        }
      }
      SetValue();
    }

    

    private void SetValue()
    {
      SetTextBoxText("txtwonum", m_wonum);
      SetTextBoxText("txtequipment", m_equipment);
      SetTextBoxText("txtlocation", m_location);
      //SetTextBoxText("txtdraccount", m_draccount);
      SetTextBoxText("txtempid", m_empid);
      SetTextBoxText("txtreqnum", m_reqnum);
      SetTextBoxText("txtdraccount", m_draccount);
      SetTextBoxText("txtcraccount", m_craccount);
      SetTextBoxText("txtreqnum", m_reqnum);

      if (m_linecounter != "")
      {
        if (m_wonum != "")
          SetTextBoxText("txtwolinecounter", m_linecounter);
        if (m_reqnum != "")
          SetTextBoxText("txtreqlinecounter", m_linecounter);
      }
    }

    private void SetTextBoxText(string controlid, string text)
    {
      TextBox t = MainControlsPanel.FindControl(controlid) as TextBox;
      if (t != null)
        t.Text = text;
    }

    private void InitGrid()
    {
      grdinvissue = new RadGrid();
      grdinvissue.ID = "grdinvissue";
      grdinvissue.DataSourceID = "InvLotSQLDataSource";
      grdinvissue.PageSize = 100;
      grdinvissue.AllowPaging = true;
      //grdinvissue.AllowSorting = true;
      //grdinvissue.MasterTableView.AllowMultiColumnSorting = true;
      grdinvissue.MasterTableView.AutoGenerateColumns = false;
      grdinvissue.ItemDataBound += new GridItemEventHandler(grdinvissue_ItemDataBound);
      grdinvissue.PreRender += new EventHandler(grdinvissue_PreRender);
      grdinvissue.MasterTableView.EditMode = GridEditMode.InPlace;
      grdinvissue.ShowFooter = true;
      grdinvissue.AllowMultiRowEdit = true;
      grdinvissue.ClientSettings.Scrolling.AllowScroll = true;
      grdinvissue.ClientSettings.Scrolling.UseStaticHeaders = true;
      grdinvissue.FooterStyle.HorizontalAlign = HorizontalAlign.Right;

//      string sql = "Select * From v_invissue(null) Where itemnum='" + m_itemnum + "' And Storeroom='" + m_storeroom + "' and inactive=0";
      string sql = "Select * From v_inventoryissue Where itemnum='" + m_itemnum + "' And Storeroom='" + m_storeroom + "' and inactive=0";
      
      InvLotSqlDataSource.ConnectionString = Application["ConnString"].ToString();
      //if (Application["usedivision"].ToString() == "1")
      //{
      //  if (Session["AllDivision"].ToString() != "")
      //  {
      //    sql = sql + " And (Division is null Or  Division in (" + Session["EditableDivision"].ToString() + "))";  // only allow issue editable division lots
      //  }
      //  else
      //    sql = sql + " And (Division is null)";
      //}

      if (objinvstore.ModuleData["IssueMethod"] == "MIXED")  // mixed
      {
        sql = sql + " Order By Position";
        m_issuemethod = "Mixed";
      }
      if (objinvstore.ModuleData["IssueMethod"] == "FIFO")  // FIFO
      {
        sql = sql + " Order By ReceiveDate";
        m_issuemethod = "FIFO";
      }
      if (objinvstore.ModuleData["IssueMethod"] == "LIFO")  // LIFO
      {
        sql = sql + " Order By ReceiveDate Desc";
        m_issuemethod = "LIFO";
      }

      InvLotSqlDataSource.SelectCommand = sql;
      screen.SetGridColumns("invissue", grdinvissue);
      GridBoundColumn stkcol = new GridBoundColumn();
      stkcol.DataField = "StockLevel";
      stkcol.Display = false;
      stkcol.HeaderText = "oldstock";
      stkcol.UniqueName = "oldstock";
      grdinvissue.Columns.Add(stkcol);

      for (int i = 0; i < grdinvissue.Columns.Count; i++)
      {
        GridBoundColumn col = grdinvissue.Columns[i] as GridBoundColumn;
        if (!(col.UniqueName.ToLower() == "price" || col.UniqueName.ToLower() == "tax1" || col.UniqueName.ToLower() == "tax2" || col.UniqueName.ToLower() == "quantity" || col.UniqueName.ToLower() == "addcost"
              || col.UniqueName.ToLower() == "totalcost" || col.UniqueName.ToLower() == "oldstock"))
          col.ReadOnly = true;

        if (col.UniqueName.ToLower() == "serialnum")
        {
          if (!serialized)
            col.Display = false;
        }
      }

      
      grdinvissue.MasterTableView.ClientDataKeyNames = new string[] { "Counter" };
      grdinvissue.MasterTableView.DataKeyNames = new string[] { "Counter" };
      MainControlsPanel.Controls.Add(grdinvissue);

    }

    private void grdinvissue_PreRender(object sender, System.EventArgs e)
    {
      foreach (GridItem item in grdinvissue.MasterTableView.Items)
      {
        if (item is GridEditableItem)
        {
          GridEditableItem editableItem = item as GridDataItem;
          editableItem.Edit = true;
        }
      }
      grdinvissue.Rebind();
    }

    protected void grdinvissue_ItemDataBound(object sender, GridItemEventArgs e)
    {
      if (e.Item is GridEditableItem && e.Item.IsInEditMode)
      {
        GridEditableItem editedItem = (GridEditableItem)e.Item;
        DataRowView item = (DataRowView)editedItem.DataItem;
        
        if (m_issueprice == "AVGPRICE")
        {
          (editedItem["Price"].Controls[0] as TextBox).Text = item["AvgPrice"].ToString();          
        }
        else if (m_issueprice == "LASTPRICE")
        {
          (editedItem["Price"].Controls[0] as TextBox).Text = item["LastPrice"].ToString();          
        }
        else if (m_issueprice == "QUOTEDPRICE")
        {
          (editedItem["Price"].Controls[0] as TextBox).Text = item["QuotedPrice"].ToString();
        }
        else if (m_issueprice == "LOTPRICE")
        {
          (editedItem["Price"].Controls[0] as TextBox).Text = item["Cost"].ToString();
        }
        else if (m_issueprice == "FIXPRICE")
        {
          (editedItem["Price"].Controls[0] as TextBox).Text = item["FixPrice"].ToString();
        }


        
      }
      

      screen.GridItemDataBound(e, "inventory/invissue.aspx", "MainForm","invissue");
    }

    private void RetrieveMessage()
    {
     // SystemMessage msg = new SystemMessage("workorder/batchclose.aspx");
        SystemMessage msg = new SystemMessage("pm/pmseason.aspx");
      m_msg = msg.GetSystemMessage();
      msg.SetJsMessage(litMessage);

    }

}