using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Web.UI;
using System.Data;
using System.Data.OleDb;
using System.Collections.Specialized;

public partial class Codes_inventorystoreroom : System.Web.UI.Page
{
  protected AzzierScreen screen;
  protected RadGrid grditeminvlist;

  protected string mode = "";
  protected string runtimefilter = "";
  protected string designtimefilter = "";
  protected string fieldlist = "";
  protected string referer = "";
  protected bool found = false;
  protected string wherestr = "", wherestrlinq = "";
  protected string controlid = "";
  protected string fieldid = "";
  protected string totalCount = "";
  protected string tablename = "";
  protected NameValueCollection m_msg = new NameValueCollection();

  protected void Page_Init(object sender, EventArgs e)
  {
    RetrieveMessage();
    if (Session["Login"] == null)
    {
      Response.Write("<script>alert('" + m_msg["T1"] + "');top.document.location.href='../Login.aspx';</script>");
      Response.End();
    }

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
    filterstr = runtimefilter + "";
    if (designtimefilter + "" != "")
      if (filterstr == "")
        filterstr = designtimefilter + "";
      else
        filterstr = filterstr + "^" + designtimefilter + "";

    wherestrlinq = v.AddLinqConditions(filterstr, filename, controlid, tablename, null, null, mode);

    screen = new AzzierScreen("codes/inventorystoreroom.aspx", "MainForm", MainControlsPanel.Controls);

    grditeminvlist = new RadGrid();
    grditeminvlist.ID = "grditeminvlist";
    grditeminvlist.ClientSettings.Scrolling.AllowScroll = true;
    grditeminvlist.ClientSettings.Scrolling.ScrollHeight = 100;
    grditeminvlist.ClientSettings.Scrolling.SaveScrollPosition = true;
    grditeminvlist.ClientSettings.Scrolling.UseStaticHeaders = true;
    grditeminvlist.ClientSettings.EnableRowHoverStyle = true;
    grditeminvlist.MasterTableView.TableLayout = GridTableLayout.Fixed;
    grditeminvlist.PagerStyle.Visible = true;// false;
    grditeminvlist.PagerStyle.AlwaysVisible = true;
    grditeminvlist.Skin = "Outlook";

    grditeminvlist.Attributes.Add("rules", "all");
    grditeminvlist.AutoGenerateColumns = false;
    grditeminvlist.AllowPaging = true;
    grditeminvlist.PageSize = 100;
    grditeminvlist.AllowSorting = true;
    grditeminvlist.MasterTableView.AllowMultiColumnSorting = true;
    grditeminvlist.AllowFilteringByColumn = true;
    grditeminvlist.MasterTableView.CommandItemDisplay = GridCommandItemDisplay.Top;
    grditeminvlist.MasterTableView.DataKeyNames = new string[] { "itemnum", "storeroom" };
    grditeminvlist.MasterTableView.CommandItemSettings.ShowAddNewRecordButton = false;

    grditeminvlist.ClientSettings.Selecting.AllowRowSelect = true;
    grditeminvlist.ClientSettings.ClientEvents.OnRowSelected = "getGridSelectedItems";
    grditeminvlist.MasterTableView.ClientDataKeyNames = new string[] { "itemnum", "storeroom" };

    grditeminvlist.MasterTableView.CommandItemTemplate = new MultiFunctionItemTemplate("Inventory Storeroom");

    //grditeminvlist.ItemEvent += new GridItemEventHandler(grditemlist_ItemEvent);
    //grditeminvlist.PreRender += new EventHandler(grditeminvlist_PreRender);

    screen.SetGridColumns("iteminvlist", grditeminvlist);

    MainControlsPanel.Controls.Add(grditeminvlist);

    screen.LoadScreen();

    grditeminvlist.ClientSettings.DataBinding.SortParameterType = GridClientDataBindingParameterType.Linq;
    grditeminvlist.ClientSettings.DataBinding.FilterParameterType = GridClientDataBindingParameterType.Linq;
  }

  protected void Page_Load(object sender, EventArgs e)
  {
    hidFieldId.Value = fieldid;
    hidControlId.Value = AzzierData.ActualFieldName(tablename, controlid);

    string isinventory = "";
    isinventory = hidIsInventory.Value;

    //RadAjaxManager1.ResponseScripts.Add("alert("+isinventory+")");
    grditeminvlist.ClientSettings.DataBinding.SelectMethod = "ItemInvList?wherestring=" + wherestrlinq + "&isinventory=1";
    grditeminvlist.ClientSettings.DataBinding.Location = "../InternalServices/ServiceInventory.svc";
  }

  protected void RadAjaxManager1_AjaxRequest(object sender, AjaxRequestEventArgs e)
  { }

  private void RetrieveMessage()
  {
    SystemMessage msg = new SystemMessage("codes/itemlist.aspx");
    m_msg = msg.GetSystemMessage();
    msg.SetJsMessage(litMessage);
  }
}