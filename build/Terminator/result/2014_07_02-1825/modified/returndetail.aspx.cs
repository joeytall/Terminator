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

public partial class inventory_returndetail : System.Web.UI.Page
{
  protected NameValueCollection m_msg = new NameValueCollection();
  protected string m_batchnum = "";
  protected string m_itemnum;
  protected string m_storeroom;
  protected string m_counter = "";
  protected Boolean serialized = false;
  protected AzzierScreen screen;
  protected NameValueCollection m_rights;
  protected Int16 m_allowedit;
  protected ModuleoObject objInvLot;
  protected ModuleoObject objbatch;
  protected NameValueCollection nvc;
  protected ModuleoObject objinvstore;
  protected RadGrid grdreturnlist;
  protected string m_issuemethod = "";
  

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
      serialized = (objinvstore.ModuleData["serialized"] == "1");

      screen = new AzzierScreen("inventory/returndetail.aspx", "MainForm", MainControlsPanel.Controls);
      InitGrid();
      screen.LoadScreen();
      screen.SetValidationControls();
    }

    protected void Page_Load(object sender, EventArgs e)
    {
      if (!Page.IsPostBack)
      {
        screen.PopulateScreen("v_inventorytransbatch", objbatch.ModuleData);

        CheckBox c = MainControlsPanel.FindControl("chkserialized") as CheckBox;
        if (c != null)
        {
          c.Attributes.Add("onclick", "return false;");
          c.Attributes.Add("onkeydown", "return false;");
        }

        TextBox t = MainControlsPanel.FindControl("txtreturndate") as TextBox;
        t.Text = DateTime.Today.ToShortDateString();

        hidoriginalqty.Value = objbatch.ModuleData["Quantity"].ToString();
      }
    }

    private void InitGrid()
    {
      grdreturnlist = new RadGrid();
      grdreturnlist.ID = "grdreturnlist";
      grdreturnlist.DataSourceID = "InvLotSQLDataSource";
      grdreturnlist.PageSize = 100;
      grdreturnlist.AllowPaging = true;
      //grdinvissue.AllowSorting = true;
      //grdinvissue.MasterTableView.AllowMultiColumnSorting = true;
      grdreturnlist.MasterTableView.AutoGenerateColumns = false;
      grdreturnlist.ItemDataBound += new GridItemEventHandler(grdinvissue_ItemDataBound);
      grdreturnlist.PreRender += new EventHandler(grdinvissue_PreRender);
      grdreturnlist.MasterTableView.EditMode = GridEditMode.InPlace;
      //grdreturnlist.ShowFooter = true;
      grdreturnlist.AllowMultiRowEdit = true;
      grdreturnlist.ClientSettings.Scrolling.AllowScroll = true;
      grdreturnlist.ClientSettings.Scrolling.UseStaticHeaders = true;
      grdreturnlist.FooterStyle.HorizontalAlign = HorizontalAlign.Right;
      grdreturnlist.MasterTableView.ClientDataKeyNames = new string[] {"Counter","IssuedQty"};
      //string sql = "Select * From v_invissue(null) Where itemnum='" + m_itemnum + "' And Storeroom='" + m_storeroom + "' and inactive=0";
      string sql = "Select * From v_ReturnDetail Where batchnum='" + m_batchnum + "'";
      InvLotSqlDataSource.ConnectionString = Application["ConnString"].ToString();

      if (objinvstore.ModuleData["IssueMethod"] == "MIXED")  // mixed
      {
        sql = sql + " Order By Position";
        m_issuemethod = "Mixed";
      }
      if (objinvstore.ModuleData["IssueMethod"] == "FIFO")  // FIFO
      {
        sql = sql + " Order By ReceiveDate Desc";
        m_issuemethod = "FIFO";
      }
      if (objinvstore.ModuleData["IssueMethod"] == "LIFO")  // LIFO
      {
        sql = sql + " Order By ReceiveDate";
        m_issuemethod = "LIFO";
      }

      InvLotSqlDataSource.SelectCommand = sql;
      screen.SetGridColumns("returnlist", grdreturnlist);
      //InvLotSqlDataSource.SelectCommand = "Select";

      for (int i = 0; i < grdreturnlist.Columns.Count; i++)
      {
        GridBoundColumn col = grdreturnlist.Columns[i] as GridBoundColumn;
        if (col.UniqueName.ToLower() != "returnqty")
          col.ReadOnly = true;

        if (col.UniqueName.ToLower() == "serialnum" || col.UniqueName.ToLower() == "equipment")
        {
          if (!serialized)
            col.Display = false;
        }
      }

      MainControlsPanel.Controls.Add(grdreturnlist);

    }

    private void grdinvissue_PreRender(object sender, System.EventArgs e)
    {
      foreach (GridItem item in grdreturnlist.MasterTableView.Items)
      {
        if (item is GridEditableItem)
        {
          GridEditableItem editableItem = item as GridDataItem;
          editableItem.Edit = true;
        }
      }
      grdreturnlist.Rebind();
    }

    protected void grdinvissue_ItemDataBound(object sender, GridItemEventArgs e)
    {
      screen.GridItemDataBound(e, "inventory/returndetail.aspx", "MainForm","returnlist");
    }

    private void RetrieveMessage()
    {
     // SystemMessage msg = new SystemMessage("workorder/batchclose.aspx");
        SystemMessage msg = new SystemMessage("pm/pmseason.aspx");
      m_msg = msg.GetSystemMessage();
      msg.SetJsMessage(litMessage);

    }

}