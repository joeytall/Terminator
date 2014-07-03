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

public partial class inventory_checkrequesteditem : System.Web.UI.Page
{
  protected NameValueCollection m_msg = new NameValueCollection();
  protected AzzierScreen screen;
  protected RadGrid grdrequestlist;

    protected void Page_Init(object sender, EventArgs e)
    {
      RetrieveMessage();
      UserRights.CheckAccess('');
      Session.LCID = Convert.ToInt32(Session["LCID"]);

      screen = new AzzierScreen("inventory/checkrequesteditem.aspx", "MainForm", MainControlsPanel.Controls);
      InitGrid();
      screen.LoadScreen();
      screen.SetValidationControls();
    }

    protected void Page_Load(object sender, EventArgs e)
    {
      if (!Page.IsPostBack)
      {
      }
      hidnonselrows.Value = "";
    }

    private void InitGrid()
    {
      grdrequestlist = new RadGrid();
      grdrequestlist.ID = "grdrequestlist";
      grdrequestlist.DataSourceID = "RequestSQLDataSource";
      grdrequestlist.PageSize = 100;
      grdrequestlist.AllowPaging = true;
      grdrequestlist.AllowFilteringByColumn = true;
      grdrequestlist.AllowSorting = true;
      grdrequestlist.MasterTableView.AllowMultiColumnSorting = true;
      grdrequestlist.MasterTableView.AutoGenerateColumns = false;
      grdrequestlist.ItemDataBound += new GridItemEventHandler(grdrequestlist_ItemDataBound);
      grdrequestlist.PreRender += new EventHandler(grdrequestlist_PreRender);
      grdrequestlist.MasterTableView.EditMode = GridEditMode.InPlace;
      grdrequestlist.ShowFooter = true;
      grdrequestlist.AllowMultiRowEdit = true;
      grdrequestlist.ClientSettings.Scrolling.AllowScroll = true;
      grdrequestlist.ClientSettings.Scrolling.UseStaticHeaders = true;
      grdrequestlist.FooterStyle.HorizontalAlign = HorizontalAlign.Right;
      grdrequestlist.ClientSettings.ClientEvents.OnRowSelecting = "RowSelect";
      grdrequestlist.MasterTableView.ClientDataKeyNames = new string[] { "ItemNum", "WONum", "IsService","Quantity" };

      string sql = "Select * From v_checkrequesteditem Where IsService=0 And ItemNum Is Not Null";
      if (Application["usedivision"].ToString().ToLower() == "yes")
      {
        if (Session["EditableDivision"].ToString() != "")
        {
          sql = sql + " And (Division is null Or  Division in (" + Session["EditableDivision"].ToString() + "))";
        }
        else
          sql = sql + " And (Division is null)";
      }
      
      RequestSqlDataSource.ConnectionString = Application["ConnString"].ToString();
      

      RequestSqlDataSource.SelectCommand = sql;

      GridClientSelectColumn reservecol = new GridClientSelectColumn();
      //reservecol.HeaderText = "";
      reservecol.UniqueName = "Reserve";
      reservecol.HeaderStyle.Width = 30;
      grdrequestlist.ClientSettings.Selecting.AllowRowSelect = true;
      grdrequestlist.AllowMultiRowSelection = true;   
      grdrequestlist.Columns.Add(reservecol);

      screen.SetGridColumns("requestlist", grdrequestlist);
      for (int i = 0; i < grdrequestlist.Columns.Count; i++)
      {
        GridBoundColumn col = grdrequestlist.Columns[i] as GridBoundColumn;
        if (col != null)
        {
          if (col.UniqueName.ToLower() != "storeroom" && col.UniqueName.ToLower() != "Reserve")
            col.ReadOnly = true;
        }
      }
      /*
      GridHyperLinkColumn newpocol = new GridHyperLinkColumn();
      newpocol.HeaderStyle.Width = 60;
      newpocol.ImageUrl = "../images/issue_24.png";
      newpocol.HeaderText = "New PO";
      newpocol.UniqueName = "NEWPO";
      newpocol.AllowFiltering = false;
      grdrequestlist.Columns.Add(newpocol);

      GridHyperLinkColumn addpocol = new GridHyperLinkColumn();
      addpocol.HeaderStyle.Width = 60;
      addpocol.ImageUrl = "../images/issue_24.png";
      addpocol.HeaderText = "To PO";
      addpocol.UniqueName = "AddPO";
      addpocol.AllowFiltering = false;
      grdrequestlist.Columns.Add(addpocol);
      */

      GridBoundColumn itemcol = new GridBoundColumn();
      itemcol.HeaderStyle.Width = 60;
      itemcol.UniqueName = "myitemnum";
      itemcol.DataField = "ItemNum";
      itemcol.Display = false;
      grdrequestlist.Columns.Add(itemcol);
      MainControlsPanel.Controls.Add(grdrequestlist);

    }

    private void grdrequestlist_PreRender(object sender, System.EventArgs e)
    {
      foreach (GridItem item in grdrequestlist.MasterTableView.Items)
      {
        if (item is GridEditableItem)
        {
          GridEditableItem editableItem = item as GridDataItem;
          editableItem.Edit = true;
        }
      }
      grdrequestlist.Rebind();
    }

    protected void grdrequestlist_ItemDataBound(object sender, GridItemEventArgs e)
    {

      if (e.Item is GridFilteringItem)
      {
        GridFilteringItem filteringItem = e.Item as GridFilteringItem;
        //set dimensions for the filter textbox  
        for (int i = 0; i < grdrequestlist.Columns.Count;i++ )
        {
          string columnname = grdrequestlist.Columns[i].UniqueName;
          if (filteringItem[columnname] != null)
          {
            if (filteringItem[columnname].HasControls())
            {
              TextBox t = filteringItem[columnname].Controls[0] as TextBox;
              if (t != null)
              {
                if (Convert.ToInt16(grdrequestlist.Columns[i].HeaderStyle.Width.Value) > 45)
                  t.Width = Convert.ToInt16(grdrequestlist.Columns[i].HeaderStyle.Width.Value - 45);
                else
                  t.Width = 0;
              }
            }
          }
        }
      }

      if (e.Item is GridEditableItem && e.Item.IsInEditMode)
      {
        GridEditableItem editedItem = (GridEditableItem)e.Item;
        DataRowView item = (DataRowView)editedItem.DataItem;

        //(editedItem["NEWPO"].Controls[0] as HyperLink).NavigateUrl = "javascript:createpo('" + item["reqtype"].ToString() + "','" + item["isservice"].ToString() + "','" + item["Counter"].ToString() + "')";
        //(editedItem["AddPO"].Controls[0] as HyperLink).NavigateUrl = "javascript:addtopo('" + item["reqtype"].ToString() + "','" + item["isservice"].ToString() + "','" + item["Counter"].ToString() + "')";
        
        if (item["IsService"].ToString() == "1" || item["ItemNum"].ToString() == "")
        {
          if (editedItem["Reserve"] != null)
          {
            TableCell cell = editedItem["Reserve"];
            cell.BackColor = System.Drawing.Color.LightGray;

            (editedItem["Reserve"].Controls[0] as CheckBox).Enabled = false;
            //(item["SelectColumn"].Controls[0] as CheckBox).BackColor = System.Drawing.Color.Gray;

            //SelEstLabour.Disabled = true;
            hidnonselrows.Value += editedItem.ItemIndex.ToString() + ",";
          }
        }
      }
      

      screen.GridItemDataBound(e, "inventory/checkrequesteditem.aspx", "MainForm","requestlist");
    }

    private void RetrieveMessage()
    {
     // SystemMessage msg = new SystemMessage("workorder/batchclose.aspx");
        SystemMessage msg = new SystemMessage("pm/pmseason.aspx");
      m_msg = msg.GetSystemMessage();
      msg.SetJsMessage(litMessage);

    }

}