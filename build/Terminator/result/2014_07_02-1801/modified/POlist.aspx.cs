using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Web.UI;
using System.Collections.Specialized;

public partial class Codes_POlist : System.Web.UI.Page
{
    protected AzzierScreen screen;
    protected RadGrid grdpolist;

    protected string mode = "";
    protected string runtimefilter = "";
    protected string designtimefilter = "";
    protected string fieldlist = "";
    protected string referer = "";
    protected bool found = false;
    protected string wherestr = "";
    protected string controlid = "";
    protected string fieldid = "";
    protected string tablename = "po";
    protected string TotalCount = "";
    protected NameValueCollection m_msg = new NameValueCollection();

    protected void Page_Init(object sender, EventArgs e)
    {
        UserRights.CheckAccess('');

        RetrieveMessage();

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

        wherestr = v.AddLinqConditions(filterstr, filename, controlid, tablename,null,null,mode);

        screen = new AzzierScreen("codes/polist.aspx", "MainForm", MainControlsPanel.Controls);

        grdpolist = new RadGrid();
        grdpolist.ID = "grdpolist";
        grdpolist.ClientSettings.Scrolling.AllowScroll = true;
        grdpolist.ClientSettings.Scrolling.SaveScrollPosition = true;
        grdpolist.ClientSettings.Scrolling.UseStaticHeaders = true;
        grdpolist.ClientSettings.EnableRowHoverStyle = true;
        grdpolist.MasterTableView.TableLayout = GridTableLayout.Fixed;
        grdpolist.PagerStyle.Visible = true;
        grdpolist.PagerStyle.AlwaysVisible = true;
        grdpolist.Skin = "Outlook";

        grdpolist.Attributes.Add("rules", "all");
        grdpolist.AutoGenerateColumns = false;
        grdpolist.AllowPaging = true;
        grdpolist.PageSize = 100;
        grdpolist.AllowSorting = true;
        grdpolist.MasterTableView.AllowMultiColumnSorting = true;
        grdpolist.AllowFilteringByColumn = true;
        grdpolist.MasterTableView.CommandItemDisplay = GridCommandItemDisplay.Top;
        grdpolist.MasterTableView.DataKeyNames = new string[] { "PoNum" };
      
        grdpolist.ClientSettings.Selecting.AllowRowSelect = true;
        grdpolist.ClientSettings.ClientEvents.OnRowSelected = "getGridSelectedItems";
        grdpolist.PagerStyle.Mode = GridPagerMode.NextPrevAndNumeric;

        //grdpmlist.ItemDataBound += new GridItemEventHandler(RadGrid1_ItemDataBound);
        grdpolist.MasterTableView.CommandItemTemplate = new MultiFunctionItemTemplate("PO");
        grdpolist.ItemCreated += new GridItemEventHandler(grdpolist_ItemCreated);

        grdpolist.ClientSettings.DataBinding.FilterParameterType = GridClientDataBindingParameterType.Linq;
        grdpolist.ClientSettings.DataBinding.SortParameterType = GridClientDataBindingParameterType.Linq;

        screen.SetGridColumns("polist", grdpolist);
        MainControlsPanel.Controls.Add(grdpolist);

        screen.LoadScreen();
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        hidFieldId.Value = fieldid;
        hidControlId.Value = AzzierData.ActualFieldName("PO",controlid);

        grdpolist.ClientSettings.DataBinding.SelectMethod = "SearchListData?wherestring=" + wherestr;
        grdpolist.ClientSettings.DataBinding.Location = "../InternalServices/ServicePO.svc";
    }

    protected void RadAjaxManager1_AjaxRequest(object sender, AjaxRequestEventArgs e)
    {
    }

    protected void grdpolist_ItemCreated(object sender, GridItemEventArgs e)
    {
      screen.GridItemCreated(e, "codes/polist.aspx", "MainForm", "polist", grdpolist);
    }

    private void RetrieveMessage()
    {
        SystemMessage msg = new SystemMessage("codes/pmlist.aspx");
        m_msg = msg.GetSystemMessage();
    }
}