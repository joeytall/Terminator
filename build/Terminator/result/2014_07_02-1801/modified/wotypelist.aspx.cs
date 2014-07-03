using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.OleDb;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Web.UI;

public partial class Codes_WOTypelist : System.Web.UI.Page
{
  protected AzzierScreen screen;
  protected RadGrid grdwotypelist;

  protected string mode = "";
  protected string runtimefilter = "";
  protected string designtimefilter = "";
  protected string fieldlist = "";
  protected string referer = "";
  protected bool found = false;
  protected string wherestr = "";
  protected string controlid = "";
  protected string fieldid = "";
  protected string totalCount = "";
  protected string tablename = "WorkType";
  protected string filename = "";
  protected NameValueCollection m_msg = new NameValueCollection();
  NameValueCollection drRights;

  protected void Page_Init(object sender, EventArgs e)
  {
    RetrieveMessage();
    UserRights.CheckAccess('');

    Session.LCID = Convert.ToInt32(Session["LCID"]);

    if (Request.QueryString["mode"] != null)
      mode = Request.QueryString["mode"].ToString();
    if (Request.QueryString["runtimefilter"] != null)
      runtimefilter = Request.QueryString["runtimefilter"].ToString();
    if (Request.QueryString["designtimefilter"] != null)
      designtimefilter = Request.QueryString["designtimefilter"].ToString();
    if (Request.QueryString["fieldlist"] != null)
      fieldlist = Request.QueryString["fieldlist"].ToString();
    if (Request.QueryString["referer"] != null)
      referer = Request.QueryString["referer"].ToString();
    if (Request.QueryString["tablename"] != null)
      tablename = Request.QueryString["tablename"].ToString();
    if (Request.QueryString["filename"] != null)
      filename = Request.QueryString["filename"].ToString();

    UserRights right = new UserRights(Session["Login"].ToString(), "UserRights", "Counter");
     drRights = right.GetRights(Session["Login"].ToString(), "Codes");

    if (fieldlist != "")
    {
      string[] fields = fieldlist.Split(',');
      string[] list = fields[0].Split('^');
      if (list.Length >= 2)
      {
        fieldid = list[1].ToString();
        controlid = list[0].ToString();
      }
    }

    Validation v = new Validation();

    string filterstrlinq = "";
    runtimefilter = runtimefilter ?? "";
    designtimefilter = designtimefilter ?? "";
    if (runtimefilter != "")
    {
      if (designtimefilter != "")
      {
        filterstrlinq = runtimefilter + "," + designtimefilter;
      }
      else
      {
        filterstrlinq = runtimefilter;
      }
    }
    else
    {
      if (designtimefilter != "")
      {
        filterstrlinq = designtimefilter;
      }
    }

    wherestr = v.AddLinqConditions(filterstrlinq, filename, controlid, tablename,null,null,mode);

    screen = new AzzierScreen("codes/wotypelist.aspx", "MainForm", MainControlsPanel.Controls);

    grdwotypelist = new RadGrid();
    grdwotypelist.ID = "grdwotypelist";
    grdwotypelist.ClientSettings.Scrolling.AllowScroll = true;
    grdwotypelist.ClientSettings.Scrolling.SaveScrollPosition = true;
    grdwotypelist.ClientSettings.Scrolling.UseStaticHeaders = true;
    grdwotypelist.MasterTableView.TableLayout = GridTableLayout.Fixed;
    grdwotypelist.PagerStyle.Visible = true;
    grdwotypelist.PagerStyle.AlwaysVisible = true;
    grdwotypelist.Skin = "Outlook";

    grdwotypelist.Attributes.Add("rules", "all");

    grdwotypelist.AutoGenerateColumns = false;
    grdwotypelist.AllowPaging = true;
    grdwotypelist.PageSize = 100;
    grdwotypelist.AllowSorting = true;
    grdwotypelist.MasterTableView.AllowMultiColumnSorting = true;
    grdwotypelist.AllowFilteringByColumn = true;
    grdwotypelist.MasterTableView.CommandItemDisplay = GridCommandItemDisplay.Top;
    grdwotypelist.MasterTableView.DataKeyNames = new string[] { "WOType" };
    grdwotypelist.MasterTableView.ClientDataKeyNames = new string[] { "WOType" };
    grdwotypelist.ClientSettings.Selecting.AllowRowSelect = true;
    grdwotypelist.ClientSettings.ClientEvents.OnRowSelected = "getGridSelectedItems";
    grdwotypelist.ClientSettings.EnableRowHoverStyle = true;

    if (drRights["urAddNew"] == "1")
    {
      grdwotypelist.MasterTableView.CommandItemTemplate = new InsertFormItemTemplate("WOType", null, "return EditWOType('')", 1, "Admin");
    }
    else
    {
      //grdacctlist.MasterTableView.CommandItemTemplate = new InsertFormItemTemplate("Account Detail", null, "return EditAccount('')", 1, "");
      grdwotypelist.MasterTableView.CommandItemTemplate = new InsertFormItemTemplate("WOType");
    }

    grdwotypelist.ClientSettings.DataBinding.SelectMethod = "GetWorkTypeList?wherestring=" + wherestr;
    grdwotypelist.ClientSettings.DataBinding.Location = "../InternalServices/ServiceWO.svc";
    grdwotypelist.ClientSettings.DataBinding.SortParameterType = GridClientDataBindingParameterType.Linq;
    grdwotypelist.ClientSettings.DataBinding.FilterParameterType = GridClientDataBindingParameterType.Linq;
    //grdacctlist.MasterTableView.VirtualItemCount = 10;

    if (drRights["urEdit"] == "1")
    {
      GridButtonColumn gridbutcol = new GridButtonColumn();
      gridbutcol.UniqueName = "Edit";
      gridbutcol.ImageUrl = "~/Images/Edit.gif";
      gridbutcol.HeaderStyle.Width = 30;
      gridbutcol.ButtonType = GridButtonColumnType.ImageButton;
      grdwotypelist.MasterTableView.Columns.Add(gridbutcol);
    }

    screen.SetGridColumns("wotypelist", grdwotypelist);

    grdwotypelist.ItemCreated += new GridItemEventHandler(grdwotypelist_ItemCreated);
    /*
    grdacctlist.DeleteCommand += new GridCommandEventHandler(grdacctlist_DeleteCommand);
    grdacctlist.InsertCommand += new GridCommandEventHandler(grdacctlist_InsertCommand);
    grdacctlist.UpdateCommand += new GridCommandEventHandler(grdacctlist_UpdateCommand);
     * */
    grdwotypelist.ItemDataBound += new GridItemEventHandler(grdwotypelist_ItemDataBound);

    MainControlsPanel.Controls.Add(grdwotypelist);

    screen.LoadScreen();
  }

  protected void Page_Load(object sender, EventArgs e)
  {
    hidFieldId.Value = fieldid;
    hidControlId.Value = AzzierData.ActualFieldName("WorkType",controlid);
  }

  protected void grdwotypelist_ItemCreated(object sender, GridItemEventArgs e)
  {
    if (e.Item is GridCommandItem)
    {
      /*
      if (righ)
        if (e.Item.FindControl("InitInsertButton") != null)
        {
          e.Item.FindControl("InitInsertButton").Visible = false;
        }
       * */
    }
    /*
    GridDataItem dataItem = e.Item as GridDataItem;
    if (dataItem != null && referer == "Admin")
    {
      ImageButton button = dataItem["Edit"].Controls[0] as ImageButton;
      //button.OnClientClick = "EditAccount(" + dataItem.OwnerTableView.DataKeyValues[dataItem.ItemIndex]["Account"].ToString() + "); return false;"; 
      button.OnClientClick = "EditWOType(" + dataItem.ItemIndex + "); return false;";
      //int i = e.Item.
    }
     * */

    screen.GridItemCreated(e, "codes/wotypelist.aspx", "MainForm", "results", grdwotypelist);
  }
  
  protected void grdwotypelist_ItemDataBound(object sender, GridItemEventArgs e)
  {
    screen.GridItemDataBound(e, "codes/wotypelist.aspx", "MainForm", "wotypelist");

    if (drRights["urEdit"] == "1" && e.Item is GridDataItem)
    {
      GridDataItem dataItem = e.Item as GridDataItem;
      ImageButton button = dataItem["Edit"].Controls[0] as ImageButton;
      button.OnClientClick = "EditWOType(" + dataItem.ItemIndex + "); return false;";
    }
  }

  private void RetrieveMessage()
  {
    SystemMessage msg = new SystemMessage("codes/acctlist.aspx");
    //m_msg = msg.GetSystemMessage();
    //SystemMessage msg = new SystemMessage();
    msg.SetJsMessage(litMessage);
  }
}