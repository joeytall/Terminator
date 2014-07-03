using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Web.UI;
using System.Collections.Specialized;

public partial class Codes_ShiptoList : System.Web.UI.Page
{
  protected AzzierScreen screen;
  protected RadGrid grdshiptolist;
  protected string mode = "";
  protected string runtimefilter = "";
  protected string designtimefilter = "";
  protected string fieldlist = "";
  protected string referer = "";
  protected bool found = false;
  protected string wherestr = "";
  protected string controlid = "";
  protected string fieldid = "";
  protected string tablename = "shipto";
  protected string TotalCount = "";
  protected NameValueCollection m_msg = new NameValueCollection();

  protected void Page_Init(object sender, EventArgs e)
  {
    UserRights.CheckAccess('');

    UserRights right = new UserRights(Session["Login"].ToString(), "UserRights", "Counter");
    NameValueCollection drRights = right.GetRights(Session["Login"].ToString(), "Codes");

    Session.LCID = Convert.ToInt32(Session["LCID"]);

    RetrieveMessage();

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
    string filterstr = "", filename = "";
    filterstr = runtimefilter;
    if (designtimefilter != "")
    {
      if (filterstr == "")
        filterstr = designtimefilter;
      else
        filterstr += "," + designtimefilter;
    }
    else if (referer != "")
    {
        referer = "shipto^" + referer;
        if (filterstr == "")
            filterstr = referer;
        else
            filterstr += "," + referer;
    }
    wherestr = v.AddLinqConditions(filterstr, filename, controlid, tablename, null, null, mode);

    screen = new AzzierScreen("codes/shiptolist.aspx", "MainForm", MainControlsPanel.Controls);

    string connstring = Application["ConnString"].ToString();
    ShiptoListSqlDataSource.ConnectionString = connstring;
    if (wherestr == "")
      ShiptoListSqlDataSource.SelectCommand = "Select * From " + tablename + " Order By shipname";
    else
      ShiptoListSqlDataSource.SelectCommand = "Select * From " + tablename + " " + wherestr + " Order By shipname";

    grdshiptolist = new RadGrid();
    grdshiptolist.ID = "grdshiptolist";
    grdshiptolist.ClientSettings.Scrolling.AllowScroll = true;
    grdshiptolist.ClientSettings.Scrolling.SaveScrollPosition = true;
    grdshiptolist.ClientSettings.Scrolling.UseStaticHeaders = true;
    grdshiptolist.ClientSettings.EnableRowHoverStyle = true;
    grdshiptolist.MasterTableView.TableLayout = GridTableLayout.Fixed;
    grdshiptolist.PagerStyle.Visible = true;// false;
    grdshiptolist.PagerStyle.AlwaysVisible = true;
    grdshiptolist.Skin = "Outlook";

    grdshiptolist.Attributes.Add("rules", "all");
   // grdshiptolist.DataSourceID = "ShiptoListSqlDataSource";
    grdshiptolist.AutoGenerateColumns = false;
    grdshiptolist.AllowPaging = true;
    grdshiptolist.PageSize = 100;
    grdshiptolist.AllowSorting = true;
    grdshiptolist.MasterTableView.AllowMultiColumnSorting = true;
    grdshiptolist.AllowFilteringByColumn = true;
    grdshiptolist.MasterTableView.CommandItemDisplay = GridCommandItemDisplay.Top;
    grdshiptolist.MasterTableView.DataKeyNames = new string[] { "shipname", "shiptype" };
    grdshiptolist.MasterTableView.ClientDataKeyNames = new string[] { "shipname", "shiptype" };

    grdshiptolist.ClientSettings.Selecting.AllowRowSelect = true;
    grdshiptolist.ClientSettings.ClientEvents.OnRowSelected = "getGridSelectedItems";
    grdshiptolist.ClientSettings.Scrolling.FrozenColumnsCount = 1;
    grdshiptolist.PagerStyle.Mode = GridPagerMode.NextPrevAndNumeric;

    grdshiptolist.ItemDataBound += new GridItemEventHandler(grdshiptolist_ItemDataBound);
    grdshiptolist.ItemCreated += new GridItemEventHandler(grdshiptolist_ItemCreated);
    grdshiptolist.ClientSettings.DataBinding.FilterParameterType = GridClientDataBindingParameterType.Linq;
    grdshiptolist.ClientSettings.DataBinding.SortParameterType = GridClientDataBindingParameterType.Linq;
    //if (Session["UserGroup"].ToString().ToLower() == "admin")
    //{
    GridEditCommandColumn EditColumn = new GridEditCommandColumn();
    EditColumn.HeaderText = "Edit";
    EditColumn.UniqueName = "EditCommand";
    EditColumn.ButtonType = GridButtonColumnType.ImageButton;

    EditColumn.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
    EditColumn.HeaderStyle.Width = 30;
    grdshiptolist.MasterTableView.Columns.Add(EditColumn);
    grdshiptolist.MasterTableView.EditMode = GridEditMode.InPlace;
    //}

    if (checkUserRight("AddNew", drRights))
    {
      if (referer == "shipto")
      {
        grdshiptolist.MasterTableView.CommandItemTemplate = new InsertFormItemTemplate("Ship To", null, "return editshipto('','shipto')", 1, Session["UserGroup"].ToString());
      }
      else
      {
        grdshiptolist.MasterTableView.CommandItemTemplate = new InsertFormItemTemplate("Bill To", null, "return editshipto('','billto')", 1, Session["UserGroup"].ToString());
      }
    }
    else
    {
      if (referer == "shipto")
      {
        grdshiptolist.MasterTableView.CommandItemTemplate = new CodesCommandItem("Ship To", 0);
      }
      else
      {
        grdshiptolist.MasterTableView.CommandItemTemplate = new CodesCommandItem("Bill To", 0);
      }
    }

    screen.SetGridColumns("shiptolist", grdshiptolist);
    MainControlsPanel.Controls.Add(grdshiptolist);

    screen.LoadScreen();
  }

  protected void Page_Load(object sender, EventArgs e)
  {
    hidFieldId.Value = fieldid;
    hidControlId.Value = AzzierData.ActualFieldName(tablename, controlid);
    grdshiptolist.ClientSettings.DataBinding.SelectMethod = "GetShipToList?where=" + wherestr;
    grdshiptolist.ClientSettings.DataBinding.Location = "../InternalServices/ServicePO.svc";
  }

  protected void RadAjaxManager1_AjaxRequest(object sender, AjaxRequestEventArgs e)
  {
  }

  protected void grdshiptolist_ItemDataBound(object sender, GridItemEventArgs e)
  {
    if ((e.Item is GridEditableItem) && !(e.Item.IsInEditMode))
    {
      GridEditableItem item = (GridEditableItem)e.Item;

      //if (Session["UserGroup"].ToString().ToLower() == "admin")
      //{
      ImageButton btn = (ImageButton)item["EditCommand"].Controls[0];
      btn.ImageUrl = "~/Images/Edit.gif";
      btn.OnClientClick = "return editshipto('" + item.ItemIndex.ToString() + "','" + referer + "')";
      //}
    }
    screen.GridItemDataBound(e, "codes/shiptolist.aspx", "MainForm", "shipto");
  }

  protected void grdshiptolist_ItemCreated(object sender, GridItemEventArgs e)
  {
    screen.GridItemCreated(e, "codes/shiptolist.aspx", "MainForm", "results", grdshiptolist);
  }

  private void RetrieveMessage()
  {
    SystemMessage msg = new SystemMessage("codes/loclist.aspx");
    m_msg = msg.GetSystemMessage();
  }

  private bool checkUserRight(string command, NameValueCollection dr)
  {
    bool right = false;
    switch (command)
    {
      case "AddNew":
        if (dr["urAddNew"] == "1")
        {
          right = true;
        }
        break;
      case "Edit":
        if (dr["urEdit"] == "1")
        {
          right = true;
        }
        break;
      case "Delete":
        if (dr["urDelete"] == "1")
        {
          right = true;
        }
        break;
      default:
        right = false;
        break;
    }
    return right;
  }
}