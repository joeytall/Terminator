using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Web.UI;
using System.Collections.Specialized;

public partial class Codes_TermList : System.Web.UI.Page
{
  protected AzzierScreen screen;
  protected RadGrid grdtermlist;
  protected string mode = "";
  protected string runtimefilter = "";
  protected string designtimefilter = "";
  protected string fieldlist = "";
  protected string referer = "";
  protected bool found = false;
  protected string wherestr = "";
  protected string controlid = "";
  protected string fieldid = "";
  protected string tablename = "terms";
  protected string TotalCount = "";
  protected NameValueCollection m_msg = new NameValueCollection();

  protected void Page_Init(object sender, EventArgs e)
  {
    if (Session["Login"] == null)
    {
      Response.Write("<script>alert('" + m_msg["T1"] + "');top.document.location.href='../Login.aspx';</script>");
      Response.End();
    }

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
      {
        filterstr = designtimefilter;
      }
      else
      {
        filterstr = filterstr + "," + designtimefilter;
      }
    }
    wherestr = v.AddLinqConditions(filterstr, filename, controlid, tablename, null, null, mode);

    screen = new AzzierScreen("codes/termlist.aspx", "MainForm", MainControlsPanel.Controls);

    string connstring = Application["ConnString"].ToString();
    TermListSqlDataSource.ConnectionString = connstring;
    if (wherestr == "")
      TermListSqlDataSource.SelectCommand = "Select * From " + tablename + " Order By code";
    else
      TermListSqlDataSource.SelectCommand = "Select * From " + tablename + " " + wherestr + " Order By code";

    grdtermlist = new RadGrid();
    grdtermlist.ID = "grdtermlist";
    grdtermlist.ClientSettings.Scrolling.AllowScroll = true;
    grdtermlist.ClientSettings.Scrolling.SaveScrollPosition = true;
    grdtermlist.ClientSettings.Scrolling.UseStaticHeaders = true;
    grdtermlist.ClientSettings.EnableRowHoverStyle = true;
    grdtermlist.MasterTableView.TableLayout = GridTableLayout.Fixed;
    grdtermlist.PagerStyle.Visible = true;
    grdtermlist.PagerStyle.AlwaysVisible = true;
    grdtermlist.Skin = "Outlook";

    grdtermlist.Attributes.Add("rules", "all");
    //grdtermlist.DataSourceID = "TermListSqlDataSource";
    grdtermlist.AutoGenerateColumns = false;
    grdtermlist.AllowPaging = true;
    grdtermlist.PageSize = 100;
    grdtermlist.AllowSorting = true;
    grdtermlist.MasterTableView.AllowMultiColumnSorting = true;
    grdtermlist.AllowFilteringByColumn = true;
    grdtermlist.MasterTableView.CommandItemDisplay = GridCommandItemDisplay.Top;
    grdtermlist.MasterTableView.DataKeyNames = new string[] { "Code" };
    grdtermlist.MasterTableView.ClientDataKeyNames = new string[] { "Code" };

    grdtermlist.ClientSettings.Selecting.AllowRowSelect = true;
    grdtermlist.ClientSettings.ClientEvents.OnRowSelected = "getGridSelectedItems";
    grdtermlist.ClientSettings.Scrolling.FrozenColumnsCount = 1;
    grdtermlist.PagerStyle.Mode = GridPagerMode.NextPrevAndNumeric;

    grdtermlist.ItemDataBound += new GridItemEventHandler(grdtermlist_ItemDataBound);
    grdtermlist.ItemCreated += new GridItemEventHandler(grdtermlist_ItemCreated);

    GridEditCommandColumn EditColumn = new GridEditCommandColumn();
    EditColumn.HeaderText = "Edit";
    EditColumn.UniqueName = "EditCommand";
    EditColumn.ButtonType = GridButtonColumnType.ImageButton;

    EditColumn.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
    EditColumn.HeaderStyle.Width = 30;
    grdtermlist.MasterTableView.Columns.Add(EditColumn);
    grdtermlist.MasterTableView.EditMode = GridEditMode.InPlace;

    if (checkUserRight("AddNew", drRights))
    {
      grdtermlist.MasterTableView.CommandItemTemplate = new InsertFormItemTemplate("Terms", null, "return editterm('')", 1, Session["UserGroup"].ToString());
    }
    else
    {
      grdtermlist.MasterTableView.CommandItemTemplate = new CodesCommandItem("Terms", 0);
    }

    grdtermlist.ClientSettings.DataBinding.FilterParameterType = GridClientDataBindingParameterType.Linq;
    grdtermlist.ClientSettings.DataBinding.SortParameterType = GridClientDataBindingParameterType.Linq;
    screen.SetGridColumns("termlist", grdtermlist);
    MainControlsPanel.Controls.Add(grdtermlist);

    screen.LoadScreen();
  }

  protected void Page_Load(object sender, EventArgs e)
  {
    hidFieldId.Value = fieldid;
    hidControlId.Value = AzzierData.ActualFieldName(tablename, controlid);
    grdtermlist.ClientSettings.DataBinding.SelectMethod = "GetTermList?where=" + wherestr;
    grdtermlist.ClientSettings.DataBinding.Location = "../InternalServices/ServicePO.svc";
  }

  protected void RadAjaxManager1_AjaxRequest(object sender, AjaxRequestEventArgs e)
  {
  }

  protected void grdtermlist_ItemDataBound(object sender, GridItemEventArgs e)
  {
    if ((e.Item is GridEditableItem) && !(e.Item.IsInEditMode))
    {
      GridEditableItem item = (GridEditableItem)e.Item;

      ImageButton btn = (ImageButton)item["EditCommand"].Controls[0];
      btn.ImageUrl = "~/Images/Edit.gif";
      btn.OnClientClick = "return editterm('" + item.ItemIndex.ToString() + "')";
    }
    screen.GridItemDataBound(e, "codes/termlist.aspx", "MainForm", "terms");
  }

  protected void grdtermlist_ItemCreated(object sender, GridItemEventArgs e)
  {
    screen.GridItemCreated(e, "codes/termlist.aspx", "MainForm", "results", grdtermlist);
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