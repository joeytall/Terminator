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

public partial class inventory_editissue : System.Web.UI.Page
{
  protected NameValueCollection m_msg = new NameValueCollection();
  protected string m_batchnum = "";
  protected string m_itemnum;
  protected string m_storeroom;
  protected string m_counter = "";
  protected Boolean serialized = false;
  protected AzzierScreen screen;
  protected double m_qtyonhand = 0;  // all stock minus reserved ones
  protected NameValueCollection m_rights;
  protected Int16 m_allowedit;
  protected ModuleoObject objInvLot;
  protected ModuleoObject objbatch;
  protected NameValueCollection nvc;
  protected ModuleoObject objinvstore;
  protected RadGrid grdinvissue;
  protected string m_issuemethod = "";
  protected string m_issueprice = "";
  protected Single m_fixprice = 0;
  protected Single m_reserved = 0;

    protected void Page_Init(object sender, EventArgs e)
    {
      RetrieveMessage();
      UserRights.CheckAccess('');
      Session.LCID = Convert.ToInt32(Session["LCID"]);
      if (Request.QueryString["batchnum"] != null)
      {
        m_batchnum = Request.QueryString["batchnum"].ToString();
      }
      else
      {
        Response.Write("<script>alert('" + m_msg["T2"] + "');top.document.location.href='../Login.aspx';</script>");
        Response.End();
      }

      UserRights r = new UserRights(Session["Login"].ToString(), "UserRights", "counter");
      m_rights = r.GetRights(Session["Login"].ToString(), "workorder");
      m_allowedit = Convert.ToInt16(m_rights["urEdit"].ToString());

      objbatch = new ModuleoObject(Session["Login"].ToString(), "v_InventoryTransBatch", "BatchNum", m_batchnum);
      m_itemnum = objbatch.ModuleData["ItemNum"];
      m_storeroom = objbatch.ModuleData["Storeroom"];
      
      Inventory i = new Inventory(Session["Login"].ToString(), m_itemnum, m_storeroom);
      objinvstore = i.InventoryStore;
      m_issueprice = objinvstore.ModuleData["issueprice"];
      serialized = (objinvstore.ModuleData["serialized"] == "1");

      screen = new AzzierScreen("inventory/editissue.aspx", "MainForm", MainControlsPanel.Controls);
      InitGrid();
      screen.LoadScreen();
      screen.SetValidationControls();
    }

    protected void Page_Load(object sender, EventArgs e)
    {
      if (!Page.IsPostBack)
      {
        screen.PopulateScreen("v_inventorystoreroom", objinvstore.ModuleData);

        hidchargeback.Value = objbatch.ModuleData["Chargeback"];

        CheckBox c = MainControlsPanel.FindControl("chkchargeback") as CheckBox;
        if (c != null)
        {
          if (objbatch.ModuleData["Chargeback"].ToString() == "1")
            c.Checked = true;
          else
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

        TextBox t = MainControlsPanel.FindControl("txtissueto") as TextBox;
        if (t != null)
        {
          t.Attributes.Add("onchange", "issuetochanged()");
        }

        t = MainControlsPanel.FindControl("txtissuedate") as TextBox;
        if (t != null)
        {
          DateFormat objDateFormat = new DateFormat(Session.LCID);
          t.Text = objDateFormat.FormatOutputDate(objbatch.ModuleData["TransDate"].ToString());
          //t.Text = objbatch.ModuleData["TransDate"].ToString();
        }

        t = MainControlsPanel.FindControl("txtissuetype") as TextBox;
        if (t != null)
        {
          if (objbatch.ModuleData["WONum"] != "")
            t.Text = "WORKORDER";
          if (objbatch.ModuleData["ReqNum"] != "")
            t.Text = "ITEM REQUEST";
          if (objbatch.ModuleData["WONum"] == "" && objbatch.ModuleData["Equipment"] != "")
            t.Text = "EQUIPMENT";
          if (objbatch.ModuleData["WONum"] == "" && objbatch.ModuleData["Equipment"] == "" && objbatch.ModuleData["Location"] != "")
            t.Text = "LOCATION";
          if (objbatch.ModuleData["WONum"] == "" && objbatch.ModuleData["Empid"] != "")
            t.Text = "EMPLOYEE";
          if (objbatch.ModuleData["WONum"] == "" && objbatch.ModuleData["Equipment"] == "" && objbatch.ModuleData["Location"] == "" && objbatch.ModuleData["ReqNum"] == "" && objbatch.ModuleData["DrAccount"] != "")
            t.Text = "ACCOUNT";
        }

        t = MainControlsPanel.FindControl("txtwonum") as TextBox;
        if (t != null)
        {
          t.Text = objbatch.ModuleData["WONum"].ToString();
        }

        t = MainControlsPanel.FindControl("txtequipment") as TextBox;
        if (t != null)
        {
          t.Text = objbatch.ModuleData["Equipment"].ToString();
        }

        t = MainControlsPanel.FindControl("txtlocation") as TextBox;
        if (t != null)
        {
          t.Text = objbatch.ModuleData["Location"].ToString();
        }

        t = MainControlsPanel.FindControl("txtempid") as TextBox;
        if (t != null)
        {
          t.Text = objbatch.ModuleData["Empid"].ToString();
        }

        t = MainControlsPanel.FindControl("txtreqnum") as TextBox;
        if (t != null)
        {
          t.Text = objbatch.ModuleData["ReqNum"].ToString();
        }

        t = MainControlsPanel.FindControl("txtdraccount") as TextBox;
        if (t != null)
        {
          t.Text = objbatch.ModuleData["draccount"].ToString();
        }

        t = MainControlsPanel.FindControl("txtcraccount") as TextBox;
        if (t != null)
        {
          t.Text = objbatch.ModuleData["craccount"].ToString();
        }

        t = MainControlsPanel.FindControl("txtissuequantity") as TextBox;
        if (t != null)
        {
          t.Text = objbatch.ModuleData["Quantity"].ToString();
        }
        hidoriginalqty.Value = objbatch.ModuleData["Quantity"].ToString();
      }
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

      //string sql = "Select * From v_invissue(null) Where itemnum='" + m_itemnum + "' And Storeroom='" + m_storeroom + "' and inactive=0";
      string sql = "Select * From v_invissue('" + m_batchnum + "') Where itemnum='" + m_itemnum + "' And Storeroom='" + m_storeroom + "' and (inactive=0 Or TransCounter is not null)";
      
      InvLotSqlDataSource.ConnectionString = Application["ConnString"].ToString();

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
      //InvLotSqlDataSource.SelectCommand = "Select";

      GridBoundColumn stkcol = new GridBoundColumn();
      stkcol.DataField = "StockLevel";
      stkcol.Display = false;
      stkcol.HeaderText = "oldstock";
      stkcol.UniqueName = "oldstock";
      grdinvissue.Columns.Add(stkcol);

      GridBoundColumn qtycol = new GridBoundColumn();
      qtycol.DataField = "Quantity";
      qtycol.Display = false;
      qtycol.UniqueName = "oldquantity";
      grdinvissue.Columns.Add(qtycol);



      for (int i = 0; i < grdinvissue.Columns.Count; i++)
      {
        GridBoundColumn col = grdinvissue.Columns[i] as GridBoundColumn;
        if (!(col.UniqueName.ToLower() == "price" || col.UniqueName.ToLower() == "tax1" || col.UniqueName.ToLower() == "tax2" || col.UniqueName.ToLower() == "quantity" || col.UniqueName.ToLower() == "addcost"
              || col.UniqueName.ToLower() == "totalcost" || col.UniqueName.ToLower() == "oldstock" || col.UniqueName.ToLower() == "oldquantity" || col.UniqueName.ToLower()=="transcounter"))
          col.ReadOnly = true;

        if (col.UniqueName.ToLower() == "serialnum" || col.UniqueName.ToLower() == "equipment")
        {
          if (!serialized)
            col.Display = false;
        }
      }

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

        if (Convert.ToDecimal(item["Quantity"].ToString()) == 0)
        {

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
        else
          (editedItem["Price"].Controls[0] as TextBox).Text = item["Price"].ToString();
        
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