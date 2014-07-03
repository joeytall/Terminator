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

public partial class inventory_stagingissue : System.Web.UI.Page
{
  protected NameValueCollection m_msg = new NameValueCollection();
  protected AzzierScreen screen;
  protected RadGrid grdstaginglist;

    protected void Page_Init(object sender, EventArgs e)
    {
      RetrieveMessage();
      UserRights.CheckAccess('');
      Session.LCID = Convert.ToInt32(Session["LCID"]);

      screen = new AzzierScreen("inventory/stagingissue.aspx", "MainForm", MainControlsPanel.Controls);
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
      grdstaginglist = new RadGrid();
      grdstaginglist.ID = "grdstaginglist";
      grdstaginglist.DataSourceID = "RequestSQLDataSource";
      grdstaginglist.PageSize = 100;
      grdstaginglist.AllowPaging = true;
      grdstaginglist.AllowFilteringByColumn = true;
      grdstaginglist.AllowSorting = true;
      grdstaginglist.MasterTableView.AllowMultiColumnSorting = true;
      grdstaginglist.MasterTableView.AutoGenerateColumns = false;
      grdstaginglist.ItemDataBound += new GridItemEventHandler(grdstaginglist_ItemDataBound);
      grdstaginglist.PreRender += new EventHandler(grdstaginglist_PreRender);
      grdstaginglist.ItemCreated += new GridItemEventHandler(grdstaginglist_ItemCreated);
      grdstaginglist.MasterTableView.EditMode = GridEditMode.InPlace;
      grdstaginglist.ShowFooter = true;
      grdstaginglist.AllowMultiRowEdit = true;
      grdstaginglist.ClientSettings.Scrolling.AllowScroll = true;
      grdstaginglist.ClientSettings.Scrolling.UseStaticHeaders = true;
      grdstaginglist.FooterStyle.HorizontalAlign = HorizontalAlign.Right;
      //grdstaginglist.ClientSettings.ClientEvents.OnRowSelecting = "RowSelect";
      grdstaginglist.MasterTableView.ClientDataKeyNames = new string[] { "BatchNum", "QtyInIssueUnit","IssuedQty" };

      string sql = "Select * From v_StagingIssue ";
      if (Application["usedivision"].ToString().ToLower() == "yes")
      {
        if (Session["EditableDivision"].ToString() != "")
        {
          sql = sql + " Where (Division is null Or  Division in (" + Session["EditableDivision"].ToString() + "))";
        }
        else
          sql = sql + " Where (Division is null)";
      }
      
      RequestSqlDataSource.ConnectionString = Application["ConnString"].ToString();
      

      RequestSqlDataSource.SelectCommand = sql;
/*
      GridClientSelectColumn reservecol = new GridClientSelectColumn();
      reservecol.UniqueName = "Reserve";
      reservecol.HeaderStyle.Width = 30;
      grdstaginglist.ClientSettings.Selecting.AllowRowSelect = true;
      grdstaginglist.AllowMultiRowSelection = true;   
      grdstaginglist.Columns.Add(reservecol);
      */
      screen.SetGridColumns("staginglist", grdstaginglist);
      for (int i = 0; i < grdstaginglist.Columns.Count; i++)
      {
        GridBoundColumn col = grdstaginglist.Columns[i] as GridBoundColumn;
        if (col != null)
        {
          if (col.UniqueName.ToLower() != "qtytoissue")
            col.ReadOnly = true;
        }
        else
          col.AllowFiltering = false;
      }
      
      MainControlsPanel.Controls.Add(grdstaginglist);

    }

    protected void grdstaginglist_ItemCreated(object sender, GridItemEventArgs e)
    {
      screen.GridItemCreated(e, "inventory/stagingissue.aspx", "MainForm", "staginglist", grdstaginglist);
    }

    private void grdstaginglist_PreRender(object sender, System.EventArgs e)
    {
      foreach (GridItem item in grdstaginglist.MasterTableView.Items)
      {
        if (item is GridEditableItem)
        {
          GridEditableItem editableItem = item as GridDataItem;
          editableItem.Edit = true;
        }
      }
      grdstaginglist.Rebind();
    }

    protected void grdstaginglist_ItemDataBound(object sender, GridItemEventArgs e)
    {
      if (e.Item is GridEditableItem && e.Item.IsInEditMode)
      {
        GridEditableItem editedItem = (GridEditableItem)e.Item;
        DataRowView item = (DataRowView)editedItem.DataItem;
        string serialized = item["Serialized"].ToString();
        TextBox t = editedItem["QtyToIssue"].Controls[0] as TextBox;
        if (t != null)
        {
          t.Attributes.Add("onchange", "checkqty(" + editedItem.ItemIndex.ToString() + ")");
          t.Text = (Convert.ToDecimal(item["QtyInIssueUnit"]) - Convert.ToDecimal(item["IssuedQty"])).ToString();
        }
      }

      screen.GridItemDataBound(e, "inventory/stagingissue.aspx", "MainForm","staginglist");
    }

    private void RetrieveMessage()
    {
     // SystemMessage msg = new SystemMessage("workorder/batchclose.aspx");
        SystemMessage msg = new SystemMessage("pm/pmseason.aspx");
      m_msg = msg.GetSystemMessage();
      msg.SetJsMessage(litMessage);

    }

}