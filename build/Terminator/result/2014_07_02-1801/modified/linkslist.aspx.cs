using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.OleDb;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Web.UI;

public partial class Codes_Linkslist : System.Web.UI.Page
{
  protected AzzierScreen screen;
  protected RadGrid grdlinkslist;
  protected string wherestr = "";
  protected string tablename = "Links";
  protected string filename = "";
  protected string module = "";
  protected string linkid = "";
  protected NameValueCollection m_msg = new NameValueCollection();

  protected void Page_Init(object sender, EventArgs e)
  {
    RetrieveMessage();
    UserRights.CheckAccess('');
    
    Session.LCID = Convert.ToInt32(Session["LCID"]);

    if (Request.QueryString["module"] != null)
      module = Request.QueryString["module"].ToString();
    if (Request.QueryString["recordnum"] != null)
      filename = Request.QueryString["recordnum"].ToString();

    Validation v = new Validation();

    wherestr = v.AddLinqConditions("", filename, "", tablename,null,null,"query");

    screen = new AzzierScreen("codes/linkslist.aspx", "MainForm", MainControlsPanel.Controls);

    grdlinkslist = new RadGrid();
    grdlinkslist.ID = "grdacctlist";
    grdlinkslist.ClientSettings.Scrolling.AllowScroll = true;
    grdlinkslist.ClientSettings.Scrolling.SaveScrollPosition = true;
    grdlinkslist.ClientSettings.Scrolling.UseStaticHeaders = true;
    grdlinkslist.MasterTableView.TableLayout = GridTableLayout.Fixed;
    grdlinkslist.PagerStyle.Visible = true;
    grdlinkslist.PagerStyle.AlwaysVisible = true;
    grdlinkslist.Skin = "Outlook";

    grdlinkslist.Attributes.Add("rules", "all");

    grdlinkslist.AutoGenerateColumns = false;
    grdlinkslist.AllowPaging = true;
    grdlinkslist.PageSize = 100;
    grdlinkslist.AllowSorting = true;
    grdlinkslist.MasterTableView.AllowMultiColumnSorting = true;
    grdlinkslist.AllowFilteringByColumn = true;
    grdlinkslist.MasterTableView.CommandItemDisplay = GridCommandItemDisplay.Top;
    grdlinkslist.MasterTableView.DataKeyNames = new string[] { "Counter" };
    grdlinkslist.MasterTableView.ClientDataKeyNames = new string[] { "Counter" };
    grdlinkslist.ClientSettings.Selecting.AllowRowSelect = true;
    grdlinkslist.ClientSettings.ClientEvents.OnRowSelected = "getGridSelectedItems";
    grdlinkslist.ClientSettings.EnableRowHoverStyle = true;

    grdlinkslist.MasterTableView.CommandItemTemplate = new InsertFormItemTemplate("Account Detail", null, "return EditAccount('')", 1, "Admin");


    grdlinkslist.ClientSettings.DataBinding.SelectMethod = "LookupDataAndCount?wherestr=" + wherestr + "&module=" + module + "&linkid" + linkid;
    grdlinkslist.ClientSettings.DataBinding.Location = "../InternalServices/ServiceLinks.svc";
    grdlinkslist.ClientSettings.DataBinding.SortParameterType = GridClientDataBindingParameterType.Linq;
    grdlinkslist.ClientSettings.DataBinding.FilterParameterType = GridClientDataBindingParameterType.Linq;
    //grdacctlist.MasterTableView.VirtualItemCount = 10;

    GridButtonColumn gridbutcol = new GridButtonColumn();
    gridbutcol.UniqueName = "EditAccount";
    gridbutcol.HeaderText = "Edit";
    gridbutcol.ImageUrl = "~/Images2/Edit.gif";
    gridbutcol.HeaderStyle.Width = 20;
    gridbutcol.ButtonType = GridButtonColumnType.ImageButton;
    grdlinkslist.MasterTableView.Columns.Add(gridbutcol);

    screen.SetGridColumns("linkslist", grdlinkslist);

    grdlinkslist.ItemCreated += new GridItemEventHandler(grdlinkslist_ItemCreated);

    grdlinkslist.ItemDataBound += new GridItemEventHandler(grdlinkslist_ItemDataBound);

    MainControlsPanel.Controls.Add(grdlinkslist);

    screen.LoadScreen();
  }

  protected void Page_Load(object sender, EventArgs e)
  {
    
  }

  protected void RadAjaxManager1_AjaxRequest(object sender, AjaxRequestEventArgs e)
  {
    
  }

  protected void grdlinkslist_ItemCreated(object sender, GridItemEventArgs e)
  {
    if (e.Item is GridCommandItem)
    {
        if (e.Item.FindControl("InitInsertButton") != null)
        {
          e.Item.FindControl("InitInsertButton").Visible = false;
        }
    }

    
    screen.GridItemCreated(e, "codes/linkslist.aspx", "MainForm", "linkslist", grdlinkslist);
  }
  
  protected void grdlinkslist_ItemDataBound(object sender, GridItemEventArgs e)
  {
    screen.GridItemDataBound(e, "codes/linkslist.aspx", "MainForm", "linkslist");

    if (e.Item is GridCommandItem)
    {
      Button addButton = e.Item.FindControl("addFormButton") as Button;
      if (addButton != null)
      {
        addButton.Visible = false;
      }
      LinkButton lnkButton = (LinkButton)e.Item.FindControl("InitInsertButton");
      if (lnkButton != null)
      {
        lnkButton.Visible = false;
      }
    }
    if (e.Item is GridDataItem)
    {
      GridDataItem dataItem = e.Item as GridDataItem;
      ImageButton button = dataItem["EditAccount"].Controls[0] as ImageButton;
      button.OnClientClick = "EditLinks(" + dataItem.ItemIndex + "); return false;";
    }

  }

  private void RetrieveMessage()
  {
    SystemMessage msg = new SystemMessage("codes/acctlist.aspx");
    msg.SetJsMessage(litMessage);
  }
}