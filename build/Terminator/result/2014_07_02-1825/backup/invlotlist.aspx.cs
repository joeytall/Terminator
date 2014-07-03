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

public partial class inventory_invlotlist : System.Web.UI.Page
{
  protected NameValueCollection m_msg = new NameValueCollection();
  protected string m_itemnum;
  protected string m_storeroom;
  protected string m_counter = "";
  protected Boolean serialized = false;
  protected AzzierScreen screen;
  protected double m_qtyonhand = 0;  // all stock minus reserved ones
  protected NameValueCollection m_rights;
  protected Int16 m_allowedit;
  protected ModuleoObject objInvLot;
  protected NameValueCollection nvc;
  protected ModuleoObject objinvstore;
  protected ModuleoObject objinvbatch;
  protected RadGrid grdinvissue;
  protected string m_issuemethod = "";
  protected string m_issueprice = "";
  protected Single m_fixprice = 0;
  protected Single m_reserved = 0;
  protected string m_batchnum = "";
  protected string m_lotinfofield = "";

    protected void Page_Init(object sender, EventArgs e)
    {
      RetrieveMessage();
      if (Session["Login"] == null)
      {
        //Response.Write("<script>alert('Your session has expired. Please login again.');top.document.location.href='../Login.aspx';</script>");
        Response.Write("<script>alert('" + m_msg["T1"] + "');top.document.location.href='../Login.aspx';</script>");
        Response.End();
      }
      Session.LCID = Convert.ToInt32(Session["LCID"]);

      if (Request.QueryString["lotinfofield"] != null)
      {
        m_lotinfofield = Request.QueryString["lotinfofield"].ToString();
      }
      else
      {
        Response.Write("<script>alert('" + m_msg["T2"] + "');top.document.location.href='../Login.aspx';</script>");
        Response.End();
      }

      if (Request.QueryString["batchnum"] != null)
      {
        m_batchnum = Request.QueryString["batchnum"].ToString();
      }

      if (m_batchnum != "")
      {
        objinvbatch = new ModuleoObject(Session["Login"].ToString(), "v_InventoryTransBatch", "BatchNum", m_batchnum);
        m_itemnum = objinvbatch.ModuleData["itemnum"];
        m_storeroom = objinvbatch.ModuleData["storeroom"];
      }
      else
      {
        if (Request.QueryString["itemnum"] != null)
        {
          m_itemnum = Request.QueryString["itemnum"].ToString();
        }
        if (Request.QueryString["storeroom"] != null)
        {
          m_storeroom = Request.QueryString["storeroom"].ToString();
        }
      }

      if (m_itemnum == "" || m_storeroom == "")
      {
        Response.Write("<script>alert('" + m_msg["T2"] + "');top.document.location.href='../Login.aspx';</script>");
        Response.End();
      }

      Inventory i = new Inventory(Session["Login"].ToString(), m_itemnum, m_storeroom);
      objinvstore = i.InventoryStore;
      m_issueprice = objinvstore.ModuleData["issueprice"];
      screen = new AzzierScreen("inventory/invissue.aspx", "MainForm", MainControlsPanel.Controls);
      InitGrid();
      screen.LoadScreen();
      screen.SetValidationControls();
    }

    protected void Page_Load(object sender, EventArgs e)
    {
      if (!Page.IsPostBack)
      {
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

      string sql;
      if (m_batchnum == "")
        sql = "Select * From v_invissue(null) Where itemnum='" + m_itemnum + "' And Storeroom='" + m_storeroom + "' and inactive=0";
      else
        sql = "Select * From v_invissue('" + m_batchnum + "') Where itemnum='" + m_itemnum + "' And Storeroom='" + m_storeroom + "' and (inactive=0 Or Quantity<>0) ";
      
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

      if (objinvstore.ModuleData["IssueMethod"] == "1")  // mixed
      {
        sql = sql + " Order By Position";
        m_issuemethod = "Mixed";
      }
      if (objinvstore.ModuleData["IssueMethod"] == "2")  // FIFO
      {
        sql = sql + " Order By ReceiveDate";
        m_issuemethod = "FIFO";
      }
      if (objinvstore.ModuleData["IssueMethod"] == "3")  // LIFO
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

      if (m_batchnum != "")
      {
        GridBoundColumn qtycol = new GridBoundColumn();
        qtycol.DataField = "Quantity";
        qtycol.Display = false;
        qtycol.HeaderText = "oldqty";
        qtycol.UniqueName = "oldqty";
        grdinvissue.Columns.Add(qtycol);

        GridBoundColumn changedcol = new GridBoundColumn();
        changedcol.DataField = "changed";
        changedcol.Display = false;
        changedcol.HeaderText = "changed";
        changedcol.UniqueName = "changed";
        grdinvissue.Columns.Add(changedcol);
      }

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

        if (m_batchnum!="")
          (editedItem["changed"].Controls[0] as TextBox).Text = "0";   
        
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