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

public partial class Codes_ItemInvlist : System.Web.UI.Page
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
  protected string storeroomfield = "txtstoreroom";
  protected NameValueCollection m_msg = new NameValueCollection();

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

    if (fieldlist != "")
    {
      string[] fields = fieldlist.Split(',');
      for (int i = 0; i < fields.Length; i++)
      {
        string[] list = fields[i].Split('^');
        if (list.Length >= 2)
        {
          if (i == 0)
          {
            fieldid = list[1].ToString();
            controlid = list[0].ToString();
          }

          if (list[0].ToLower() == "store" || list[0].ToLower() == "storeroom")
            storeroomfield = list[1];
        }
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

    screen = new AzzierScreen("codes/iteminvlist.aspx", "MainForm", MainControlsPanel.Controls);

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
    grditeminvlist.ClientSettings.ClientEvents.OnRowSelected = "getGridSelectedItemsInvList";
    grditeminvlist.ClientSettings.Scrolling.FrozenColumnsCount = 1;
    grditeminvlist.MasterTableView.ClientDataKeyNames = new string[] { "itemnum", "storeroom" };

    grditeminvlist.MasterTableView.CommandItemTemplate = new MultiFunctionItemTemplate("Inventory List");

    //grditeminvlist.ItemEvent += new GridItemEventHandler(grditemlist_ItemEvent);
    //grditeminvlist.PreRender += new EventHandler(grditeminvlist_PreRender);

    screen.SetGridColumns("iteminvlist", grditeminvlist);

    MainControlsPanel.Controls.Add(grditeminvlist);

    screen.LoadScreen();


    CheckBox chkispart = (CheckBox) MainControlsPanel.FindControl("chkispart");
    if (chkispart != null)
    {
        chkispart.Attributes.Add("OnChange","onCheckedChanged()");
    }
    RadioButtonList r = MainControlsPanel.FindControl("rbliteminv") as RadioButtonList;
    
    if (r != null)
    {
      r.RepeatDirection = RepeatDirection.Horizontal;
      ListItem l = new ListItem("Yes", "1");
      l.Selected = true;
      r.Items.Add(l);
      l = new ListItem("No", "0");
      r.Items.Add(l);
    }

    if (referer == "inventory/transfer.aspx")
    {
      if (r!=null)
        r.Visible = false;
      HyperLink h = MainControlsPanel.FindControl("lbliteminv") as HyperLink;
      if (h!=null)
        h.Visible = false;
    }


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
    string equipment = "";
    string mode = "";
    if (Request.Form["__EVENTTARGET"] != null)
    {
        if (Request.Form["__EVENTTARGET"].ToString() != "")
        {
            mode = Request.Form["__EVENTTARGET"].ToString();
            if (Request.Form["__EVENTARGUMENT"] != null)
            {
                if (Request.Form["__EVENTARGUMENT"].ToString() != "")
                {
                    equipment = Request.Form["__EVENTARGUMENT"].ToString();
                }
            }
        }
    }
    if (mode == "ispartrefresh")
    {
        grditeminvlist.ClientSettings.DataBinding.SelectMethod = "PartInvList?wherestring=" + wherestrlinq + "&isinventory=" + isinventory + "&equipment=" + equipment;
        grditeminvlist.ClientSettings.DataBinding.Location = "../InternalServices/ServiceInventory.svc";
    }
    else
    {
        grditeminvlist.ClientSettings.DataBinding.SelectMethod = "ItemInvList?wherestring=" + wherestrlinq + "&isinventory=" + isinventory;
        grditeminvlist.ClientSettings.DataBinding.Location = "../InternalServices/ServiceInventory.svc";
    }
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