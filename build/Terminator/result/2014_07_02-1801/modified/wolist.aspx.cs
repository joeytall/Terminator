using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Web.UI;
using System.Collections.Specialized;


public partial class Codes_wolist : System.Web.UI.Page
{
    protected AzzierScreen screen;
    protected RadGrid grdwolist;
    protected string mode = "";
    protected string runtimefilter = "";
    protected string designtimefilter = "";
    protected string fieldlist = "";
    protected string referer = "";
    protected bool found = false;
    protected string wherestr = "";
    protected string controlid = "";
    protected string fieldid = "";
    protected string tablename = "Location";
    protected string TotalCount = "";
    protected string filterstr = "", filename = "";
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
        if (Request.QueryString["filename"] != null)
            filename = Request.QueryString["filename"].ToString();

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

        filterstr = runtimefilter;
        if (designtimefilter != "")
        {
          if (filterstr == "")
            filterstr = designtimefilter;
          else
            filterstr = filterstr + "," + designtimefilter;
        }

        Validation v = new Validation();

        wherestr = v.AddLinqConditions(filterstr, filename, controlid, tablename,null,null,mode);

        screen = new AzzierScreen("codes/wolist.aspx", "MainForm", MainControlsPanel.Controls);

        grdwolist = new RadGrid();
        grdwolist.ID = "grdwolist";
        grdwolist.ClientSettings.Scrolling.AllowScroll = true;
        grdwolist.ClientSettings.Scrolling.SaveScrollPosition = true;
        grdwolist.ClientSettings.Scrolling.UseStaticHeaders = true;
        grdwolist.ClientSettings.EnableRowHoverStyle = true;
        grdwolist.MasterTableView.TableLayout = GridTableLayout.Fixed;
        grdwolist.PagerStyle.Visible = true;
        grdwolist.PagerStyle.AlwaysVisible = true;
        grdwolist.Skin = "Outlook";

        grdwolist.Attributes.Add("rules", "all");

        grdwolist.AutoGenerateColumns = false;
        grdwolist.AllowPaging = true;
        grdwolist.PageSize = 100;
        grdwolist.AllowSorting = true;
        grdwolist.MasterTableView.AllowMultiColumnSorting = true;
        grdwolist.AllowFilteringByColumn = true;
        grdwolist.MasterTableView.CommandItemDisplay = GridCommandItemDisplay.Top;
        grdwolist.MasterTableView.DataKeyNames = new string[] { "WoNum" };

        grdwolist.ClientSettings.Selecting.AllowRowSelect = true;
        grdwolist.ClientSettings.ClientEvents.OnRowSelected = "getGridSelectedItems";

        grdwolist.MasterTableView.CommandItemTemplate = new MultiFunctionItemTemplate("WorkOrder");

        grdwolist.ClientSettings.DataBinding.FilterParameterType = GridClientDataBindingParameterType.Linq;
        grdwolist.ClientSettings.DataBinding.SortParameterType = GridClientDataBindingParameterType.Linq;

        screen.SetGridColumns("wolist", grdwolist);
        MainControlsPanel.Controls.Add(grdwolist);

        screen.LoadScreen();
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        hidFieldId.Value = fieldid;
        hidControlId.Value = AzzierData.ActualFieldName("workorder",controlid);

        grdwolist.ClientSettings.DataBinding.SelectMethod = "SearchDataAndCount?wherestring=" + wherestr;
        grdwolist.ClientSettings.DataBinding.Location = "../InternalServices/ServiceWO.svc";
    }

    protected void grdloclist_ItemCreated(object sender, GridItemEventArgs e)
    {
        screen.GridItemCreated(e, "codes/wolist.aspx", "MainForm", "results", grdwolist);
    }

    private void RetrieveMessage()
    {
        SystemMessage msg = new SystemMessage("codes/loclist.aspx");
        m_msg = msg.GetSystemMessage();
    }
}