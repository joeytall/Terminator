using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Web.UI;
using System.Collections.Specialized;

public partial class Inventory_IssueBatchlist : System.Web.UI.Page
{
    protected AzzierScreen screen;
    protected RadGrid grdbatchlist;

    protected NameValueCollection m_msg = new NameValueCollection();
    protected void Page_Init(object sender, EventArgs e)
    {
        RetrieveMessage();
        UserRights.CheckAccess('');

        Session.LCID = Convert.ToInt32(Session["LCID"]);

        screen = new AzzierScreen("inventory/issuebatchlist.aspx", "MainForm", MainControlsPanel.Controls);

        grdbatchlist = new RadGrid();
        grdbatchlist.ID = "grdbatchlist";
        grdbatchlist.ClientSettings.Scrolling.AllowScroll = true;
        grdbatchlist.ClientSettings.Scrolling.SaveScrollPosition = true;
        grdbatchlist.ClientSettings.Scrolling.UseStaticHeaders = true;
        grdbatchlist.ClientSettings.EnableRowHoverStyle = true;
        grdbatchlist.MasterTableView.TableLayout = GridTableLayout.Fixed;
        grdbatchlist.PagerStyle.Visible = true;
        grdbatchlist.PagerStyle.AlwaysVisible = true;
        grdbatchlist.Skin = "Outlook";

        grdbatchlist.Attributes.Add("rules", "all");
        grdbatchlist.AutoGenerateColumns = false;
        grdbatchlist.AllowPaging = true;
        grdbatchlist.PageSize = 100;
        grdbatchlist.AllowSorting = true;
        grdbatchlist.MasterTableView.AllowMultiColumnSorting = true;
        grdbatchlist.AllowFilteringByColumn = true;
        grdbatchlist.MasterTableView.CommandItemDisplay = GridCommandItemDisplay.Top;
        grdbatchlist.MasterTableView.DataKeyNames = new string[] { "BatchNum" };
        grdbatchlist.MasterTableView.ClientDataKeyNames = new string[] { "BatchNum" };

        grdbatchlist.ClientSettings.Selecting.AllowRowSelect = true;

        GridEditCommandColumn EditColumn = new GridEditCommandColumn();
        EditColumn.HeaderText = "Edit";
        EditColumn.UniqueName = "EditCommand";
        EditColumn.ButtonType = GridButtonColumnType.ImageButton;

        EditColumn.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
        EditColumn.HeaderStyle.Width = 30;
        grdbatchlist.MasterTableView.Columns.Add(EditColumn);


        screen.SetGridColumns("batchlist", grdbatchlist);
        
        grdbatchlist.ItemCreated += new GridItemEventHandler(grdbatchlist_ItemCreated);

        grdbatchlist.ItemDataBound += new GridItemEventHandler(grdbatchlist_ItemDataBound);

        grdbatchlist.MasterTableView.CommandItemTemplate = new CodesCommandItem("Inventory Issue", 0);


        MainControlsPanel.Controls.Add(grdbatchlist);

        screen.LoadScreen();
    }

    protected void Page_Load(object sender, EventArgs e)
    {
      grdbatchlist.ClientSettings.DataBinding.SelectMethod = "TransBatchQuery";
      grdbatchlist.ClientSettings.DataBinding.Location = "../InternalServices/ServiceInventory.svc";
      grdbatchlist.ClientSettings.DataBinding.SortParameterType = GridClientDataBindingParameterType.Linq;
      grdbatchlist.ClientSettings.DataBinding.FilterParameterType = GridClientDataBindingParameterType.Linq;
    }

    protected void grdbatchlist_ItemCreated(object sender, GridItemEventArgs e)
    {
      screen.GridItemCreated(e, "inventory/issuebatchlist.aspx", "MainForm", "batchlist", grdbatchlist);
    }

    private void grdbatchlist_ItemDataBound(object sender, Telerik.Web.UI.GridItemEventArgs e)
    {
        if (e.Item is GridDataItem && !e.Item.IsInEditMode)
        {
            GridDataItem item = (GridDataItem)e.Item;
            ImageButton btn = (ImageButton)item["EditCommand"].Controls[0];
            btn.ImageUrl = "~/Images2/Edit.gif";
            btn.OnClientClick = "return EditBatch(" + item.ItemIndex.ToString() + ")";
        }
        screen.GridItemDataBound(e, "inventory/issuebatchlist.aspx", "MainForm", "batchlist");        
    }

    private void RetrieveMessage()
    {
        SystemMessage msg = new SystemMessage("codes/craftlist.aspx");
        m_msg = msg.GetSystemMessage();
        msg.SetJsMessage(litMessage);
    }
}