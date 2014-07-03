using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Web.UI;
using System.Collections.Specialized;

public partial class Codes_loclist : System.Web.UI.Page
{
    protected AzzierScreen screen;
    protected RadGrid grdloclist;
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

        screen = new AzzierScreen("codes/loclist.aspx", "MainForm", MainControlsPanel.Controls);

        //string connstring = Application["ConnString"].ToString();
        //LocListSqlDataSource.ConnectionString = connstring;
        //if (wherestr == "")
        //    LocListSqlDataSource.SelectCommand = "Select * From " + tablename + " Order By location";
        //else
        //    LocListSqlDataSource.SelectCommand = "Select * From " + tablename + " " + wherestr + " Order By location";

        grdloclist = new RadGrid();
        grdloclist.ID = "grdloclist";
        grdloclist.ClientSettings.Scrolling.AllowScroll = true;
        grdloclist.ClientSettings.Scrolling.SaveScrollPosition = true;
        grdloclist.ClientSettings.Scrolling.UseStaticHeaders = true;
        grdloclist.ClientSettings.EnableRowHoverStyle = true;
        //grdloclist.ClientSettings.ClientEvents.OnScroll = "HandleScrolling";
        grdloclist.MasterTableView.TableLayout = GridTableLayout.Fixed;
        //grdloclist.PagerStyle.Visible = false;
        grdloclist.PagerStyle.Visible = true;
        grdloclist.PagerStyle.AlwaysVisible = true;
        grdloclist.Skin = "Outlook";

        grdloclist.Attributes.Add("rules", "all");
        //grdloclist.DataSourceID = "LocListSqlDataSource";
        grdloclist.AutoGenerateColumns = false;
        grdloclist.AllowPaging = true;
        grdloclist.PageSize = 100;
        grdloclist.AllowSorting = true;
        grdloclist.MasterTableView.AllowMultiColumnSorting = true;
        grdloclist.AllowFilteringByColumn = true;
        grdloclist.MasterTableView.CommandItemDisplay = GridCommandItemDisplay.Top;
        grdloclist.MasterTableView.DataKeyNames = new string[] { "Location" };

        grdloclist.ClientSettings.Selecting.AllowRowSelect = true;
        grdloclist.ClientSettings.ClientEvents.OnRowSelected = "getGridSelectedItems";
        //grdloclist.ClientSettings.Scrolling.FrozenColumnsCount = 1;
        //grdloclist.PagerStyle.Mode = GridPagerMode.NextPrevAndNumeric;

        grdloclist.MasterTableView.CommandItemTemplate = new MultiFunctionItemTemplate("Location");
        grdloclist.ItemCreated += new GridItemEventHandler(grdloclist_ItemCreated);

        grdloclist.ClientSettings.DataBinding.FilterParameterType = GridClientDataBindingParameterType.Linq;
        grdloclist.ClientSettings.DataBinding.SortParameterType = GridClientDataBindingParameterType.Linq;

        screen.SetGridColumns("loclist", grdloclist);
        MainControlsPanel.Controls.Add(grdloclist);

        screen.LoadScreen();
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        hidFieldId.Value = fieldid;
        hidControlId.Value = AzzierData.ActualFieldName("location", "location");
        //hidControlId.Value = controlid;

        grdloclist.ClientSettings.DataBinding.SelectMethod = "LookupDataAndCount?where=" + wherestr;
        grdloclist.ClientSettings.DataBinding.Location = "../InternalServices/ServiceLoc.svc";
    }


    protected void grdloclist_ItemCreated(object sender, GridItemEventArgs e)
    {
      screen.GridItemCreated(e, "codes/loclist.aspx", "MainForm", "results", grdloclist);
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