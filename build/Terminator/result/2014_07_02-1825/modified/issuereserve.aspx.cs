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

public partial class inventory_issuereserve : System.Web.UI.Page
{
  protected NameValueCollection m_msg = new NameValueCollection();
  protected AzzierScreen screen;
  protected RadGrid grdinvreserves;
  protected string m_storeroom = "";
  protected string m_deliveryto = "";
  protected string m_pickupby = "";

    protected void Page_Init(object sender, EventArgs e)
    {
      RetrieveMessage();
      UserRights.CheckAccess('');
      Session.LCID = Convert.ToInt32(Session["LCID"]);

      if (Request.QueryString["storeroom"] != null)
        m_storeroom = Request.QueryString["storeroom"].ToString();
      if (Request.QueryString["deliveryto"] != null)
        m_deliveryto = Request.QueryString["deliveryto"].ToString();
      if (Request.QueryString["pickupby"] != null)
        m_pickupby = Request.QueryString["pickupby"].ToString();

      screen = new AzzierScreen("inventory/issuereserve.aspx", "MainForm", MainControlsPanel.Controls);
      InitGrid();
      screen.LoadScreen();
      screen.SetValidationControls();
    }

    protected void Page_Load(object sender, EventArgs e)
    {
      if (!Page.IsPostBack)
      {
        TextBox t = MainControlsPanel.FindControl("txtstoreroom") as TextBox;
        if (t != null)
        {
          t.Text = m_storeroom;
        }

        t = MainControlsPanel.FindControl("txtpickupby") as TextBox;
        if (t != null)
        {
          t.Text = m_pickupby;
        }

        t = MainControlsPanel.FindControl("txtdeliveryto") as TextBox;
        if (t != null)
        {
          t.Text = m_deliveryto;
        }

      }
    }

    private void InitGrid()
    {
      grdinvreserves = new RadGrid();
      grdinvreserves.ID = "grdinvreserves";
      grdinvreserves.DataSourceID = "RequestSQLDataSource";
      grdinvreserves.PageSize = 100;
      grdinvreserves.AllowPaging = true;
      grdinvreserves.AllowFilteringByColumn = true;
      grdinvreserves.AllowSorting = true;
      grdinvreserves.MasterTableView.AllowMultiColumnSorting = true;
      grdinvreserves.MasterTableView.AutoGenerateColumns = false;
      grdinvreserves.ItemDataBound += new GridItemEventHandler(grdinvreserves_ItemDataBound);
      grdinvreserves.PreRender += new EventHandler(grdinvreserves_PreRender);
      grdinvreserves.ItemCreated +=new GridItemEventHandler(grdinvreserves_ItemCreated);
      grdinvreserves.MasterTableView.EditMode = GridEditMode.InPlace;
      grdinvreserves.ShowFooter = true;
      grdinvreserves.AllowMultiRowEdit = true;
      grdinvreserves.ClientSettings.Scrolling.AllowScroll = true;
      grdinvreserves.ClientSettings.Scrolling.UseStaticHeaders = true;
      grdinvreserves.FooterStyle.HorizontalAlign = HorizontalAlign.Right;
      grdinvreserves.ClientSettings.ClientEvents.OnRowSelecting = "RowSelect";
      grdinvreserves.MasterTableView.ClientDataKeyNames = new string[] {"Counter","QtyOnHand","LinkType","ChargeTo","ItemNum","LineCounter" };

      string sql = "Select * From v_reserveditems where storeroom='" + m_storeroom + "'";
      
      RequestSqlDataSource.ConnectionString = Application["ConnString"].ToString();
      RequestSqlDataSource.SelectCommand = sql;

      /*
      GridClientSelectColumn reservecol = new GridClientSelectColumn();
      //reservecol.HeaderText = "";
      reservecol.UniqueName = "Reserve";
      reservecol.HeaderStyle.Width = 30;
      grdinvreserves.ClientSettings.Selecting.AllowRowSelect = true;
      grdinvreserves.AllowMultiRowSelection = true;   
      grdinvreserves.Columns.Add(reservecol);
       * */


      screen.SetGridColumns("invreserves", grdinvreserves);
      for (int i = 0; i < grdinvreserves.Columns.Count; i++)
      {
        GridBoundColumn col = grdinvreserves.Columns[i] as GridBoundColumn;
        col.ReadOnly = true;
      }

      GridBoundColumn issueqtycol = new GridBoundColumn();
      issueqtycol.HeaderStyle.Width = 60;
      issueqtycol.HeaderText = "IssueQty";
      issueqtycol.UniqueName = "IssueQty";
      issueqtycol.DataField = "QtyReserved";
      issueqtycol.AllowFiltering = false;
      issueqtycol.AllowSorting = false;
      grdinvreserves.Columns.Add(issueqtycol);

      GridHyperLinkColumn issuedetialcol = new GridHyperLinkColumn();
      issuedetialcol.HeaderStyle.Width = 60;
      issuedetialcol.ImageUrl = "../images/edit.gif";
      issuedetialcol.HeaderText = "Detail";
      issuedetialcol.UniqueName = "Detail";
      issuedetialcol.AllowFiltering = false;
      issuedetialcol.AllowSorting = false;
      grdinvreserves.Columns.Add(issuedetialcol);

      MainControlsPanel.Controls.Add(grdinvreserves);

    }

    protected void grdinvreserves_ItemCreated(object sender, GridItemEventArgs e)
    {
      screen.GridItemCreated(e, "inventory/issuereserve.aspx", "MainForm", "invreserves", grdinvreserves);
    }

    private void grdinvreserves_PreRender(object sender, System.EventArgs e)
    {
      foreach (GridItem item in grdinvreserves.MasterTableView.Items)
      {
        if (item is GridEditableItem)
        {
          GridEditableItem editableItem = item as GridDataItem;
          editableItem.Edit = true;
        }
      }
      grdinvreserves.Rebind();
    }

    protected void grdinvreserves_ItemDataBound(object sender, GridItemEventArgs e)
    {
      if (e.Item is GridEditableItem && e.Item.IsInEditMode)
      {
        GridEditableItem editedItem = (GridEditableItem)e.Item;
        DataRowView item = (DataRowView)editedItem.DataItem;

        //(editedItem["Detail"].Controls[0] as HyperLink).NavigateUrl = "javascript:detail('" + item["Counter"].ToString() + "','" + item["ReqLineCounter"].ToString() + "')";
        (editedItem["Detail"].Controls[0] as HyperLink).NavigateUrl = "javascript:detail('" + editedItem.ItemIndex.ToString() + "')";
        
        if (editedItem["IssueQty"] != null)
        {
          TextBox t = editedItem["IssueQty"].Controls[0] as TextBox;
          t.Text = "";
        }
      }
      screen.GridItemDataBound(e, "inventory/issuereserve.aspx", "MainForm","invreserves");
    }

    private void RetrieveMessage()
    {
     // SystemMessage msg = new SystemMessage("workorder/batchclose.aspx");
        SystemMessage msg = new SystemMessage("pm/pmseason.aspx");
      m_msg = msg.GetSystemMessage();
      msg.SetJsMessage(litMessage);

    }

}