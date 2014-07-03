using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Web.UI;
using System.Collections.Specialized;

public partial class Codes_RouteList : System.Web.UI.Page
{
    protected AzzierScreen screen;
    protected RadGrid grdroutelist;
    protected string mode = "";
    protected string runtimefilter = "";
    protected string designtimefilter = "";
    protected string fieldlist = "";
    protected string referer = "";
    protected bool found = false;
    protected string wherestr = "";
    protected string controlid = "";
    protected string fieldid = "";
    protected string tablename = "Route";
    protected string TotalCount = "";
    protected string filterstr = "", filename = "";
    protected NameValueCollection m_msg = new NameValueCollection();

    protected void Page_Init(object sender, EventArgs e)
    {
        UserRights.CheckAccess('');

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
          {
            filterstr = designtimefilter;
          }
          else
          {
            filterstr = filterstr + "," + designtimefilter;
          }
        }

        Validation v = new Validation();

        wherestr = v.AddLinqConditions(filterstr, filename, controlid, tablename,null,null,mode);

        screen = new AzzierScreen("codes/routelist.aspx", "MainForm", MainControlsPanel.Controls);

        //string connstring = Application["ConnString"].ToString();
        //LocListSqlDataSource.ConnectionString = connstring;
        //if (wherestr == "")
        //    LocListSqlDataSource.SelectCommand = "Select * From " + tablename + " Order By location";
        //else
        //    LocListSqlDataSource.SelectCommand = "Select * From " + tablename + " " + wherestr + " Order By location";

        grdroutelist = new RadGrid();
        grdroutelist.ID = "grdroutelist";
        grdroutelist.ClientSettings.Scrolling.AllowScroll = true;
        grdroutelist.ClientSettings.Scrolling.SaveScrollPosition = true;
        grdroutelist.ClientSettings.Scrolling.UseStaticHeaders = true;
        grdroutelist.ClientSettings.EnableRowHoverStyle = true;
        //grdloclist.ClientSettings.ClientEvents.OnScroll = "HandleScrolling";
        grdroutelist.MasterTableView.TableLayout = GridTableLayout.Fixed;
        //grdloclist.PagerStyle.Visible = false;
        grdroutelist.PagerStyle.Visible = true;
        grdroutelist.PagerStyle.AlwaysVisible = true;
        grdroutelist.Skin = "Outlook";

        grdroutelist.Attributes.Add("rules", "all");
        //grdloclist.DataSourceID = "LocListSqlDataSource";
        grdroutelist.AutoGenerateColumns = false;
        grdroutelist.AllowPaging = true;
        grdroutelist.PageSize = 100;
        grdroutelist.AllowSorting = true;
        grdroutelist.MasterTableView.AllowMultiColumnSorting = true;
        grdroutelist.AllowFilteringByColumn = true;
        grdroutelist.MasterTableView.CommandItemDisplay = GridCommandItemDisplay.Top;
        grdroutelist.MasterTableView.DataKeyNames = new string[] { "RouteName" };

        grdroutelist.ClientSettings.Selecting.AllowRowSelect = true;
        grdroutelist.ClientSettings.ClientEvents.OnRowSelected = "getGridSelectedItems";
        //grdloclist.ClientSettings.Scrolling.FrozenColumnsCount = 1;
        //grdloclist.PagerStyle.Mode = GridPagerMode.NextPrevAndNumeric;

        grdroutelist.MasterTableView.CommandItemTemplate = new MultiFunctionItemTemplate("Route");
        grdroutelist.ItemCreated += new GridItemEventHandler(grdroutelist_ItemCreated);

        grdroutelist.ClientSettings.DataBinding.FilterParameterType = GridClientDataBindingParameterType.Linq;
        grdroutelist.ClientSettings.DataBinding.SortParameterType = GridClientDataBindingParameterType.Linq;

        screen.SetGridColumns("routelist", grdroutelist);
        MainControlsPanel.Controls.Add(grdroutelist);

        screen.LoadScreen();
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        hidFieldId.Value = fieldid;
        hidControlId.Value = AzzierData.ActualFieldName("Route", "RouteName");
        //hidControlId.Value = controlid;

        grdroutelist.ClientSettings.DataBinding.SelectMethod = "SearchDataAndCount?wherestring=" + wherestr;
        grdroutelist.ClientSettings.DataBinding.Location = "../InternalServices/ServiceRoute.svc";
    }


    protected void grdroutelist_ItemCreated(object sender, GridItemEventArgs e)
    {
      screen.GridItemCreated(e, "codes/routelist.aspx", "MainForm", "routelist", grdroutelist);
    }
  
    protected void RadAjaxManager1_AjaxRequest(object sender, AjaxRequestEventArgs e)
    {
    }

    private void RetrieveMessage()
    {
        SystemMessage msg = new SystemMessage("codes/loclist.aspx");
        m_msg = msg.GetSystemMessage();
        //msg.SetJsMessage(litMessage);
    }
}