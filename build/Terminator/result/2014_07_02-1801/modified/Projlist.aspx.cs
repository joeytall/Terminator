using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Web.UI;
using System.Collections.Specialized;

public partial class Codes_Projlist : System.Web.UI.Page
{
    protected AzzierScreen screen;
    protected RadGrid grdprojlist;
    protected string mode = "";
    protected string runtimefilter = "";
    protected string designtimefilter = "";
    protected string fieldlist = "";
    protected string referer = "";
    protected bool found = false;
    protected string wherestr = "";
    protected string controlid = "";
    protected string fieldid = "";
    protected string tablename = "projects";
    protected string TotalCount = "";
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

        screen = new AzzierScreen("codes/projlist.aspx", "MainForm", MainControlsPanel.Controls);

        string connstring = Application["ConnString"].ToString();
        ProjListSqlDataSource.ConnectionString = connstring;
        if (wherestr == "")
            ProjListSqlDataSource.SelectCommand = "Select * From " + tablename + " Order By projectid";
        else
            ProjListSqlDataSource.SelectCommand = "Select * From " + tablename + " " + wherestr + " Order By projectid";

        grdprojlist = new RadGrid();
        grdprojlist.ID = "grdprojlist";
        grdprojlist.ClientSettings.Scrolling.AllowScroll = true;
        grdprojlist.ClientSettings.Scrolling.SaveScrollPosition = true;
        grdprojlist.ClientSettings.Scrolling.UseStaticHeaders = true;
        grdprojlist.ClientSettings.EnableRowHoverStyle = true;
        grdprojlist.MasterTableView.TableLayout = GridTableLayout.Fixed;
        grdprojlist.PagerStyle.Visible = true;// false;
        grdprojlist.PagerStyle.AlwaysVisible = true;
        grdprojlist.Skin = "Outlook";

        grdprojlist.Attributes.Add("rules", "all");
        //grdprojlist.DataSourceID = "ProjListSqlDataSource";
        grdprojlist.AutoGenerateColumns = false;
        grdprojlist.AllowPaging = true;
        grdprojlist.PageSize = 100;
        grdprojlist.AllowSorting = true;
        grdprojlist.MasterTableView.AllowMultiColumnSorting = true;
        grdprojlist.AllowFilteringByColumn = true;
        grdprojlist.MasterTableView.CommandItemDisplay = GridCommandItemDisplay.Top;
        grdprojlist.MasterTableView.DataKeyNames = new string[] { "ProjectId" };
        grdprojlist.MasterTableView.ClientDataKeyNames = new string[] { "ProjectId" };

        grdprojlist.ClientSettings.Selecting.AllowRowSelect = true;
        grdprojlist.ClientSettings.ClientEvents.OnRowSelected = "getGridSelectedItems";
        grdprojlist.ClientSettings.Scrolling.FrozenColumnsCount = 1;

        grdprojlist.MasterTableView.CommandItemTemplate = new MultiFunctionItemTemplate("Projects");
        grdprojlist.ItemCreated += new GridItemEventHandler(grdprojlist_ItemCreated);
        grdprojlist.ClientSettings.DataBinding.FilterParameterType = GridClientDataBindingParameterType.Linq;
        grdprojlist.ClientSettings.DataBinding.SortParameterType = GridClientDataBindingParameterType.Linq; 

        screen.SetGridColumns("projlists", grdprojlist);
        MainControlsPanel.Controls.Add(grdprojlist);

        screen.LoadScreen();
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        hidFieldId.Value = fieldid;
        hidControlId.Value = AzzierData.ActualFieldName("projects",controlid);
        grdprojlist.ClientSettings.DataBinding.SelectMethod = "GetProjectList?wherestring=" + wherestr;
        grdprojlist.ClientSettings.DataBinding.Location = "../InternalServices/ServiceProj.svc";
    }

    protected void RadAjaxManager1_AjaxRequest(object sender, AjaxRequestEventArgs e)
    {
    }

    protected void grdprojlist_ItemCreated(object sender, GridItemEventArgs e)
    {
      screen.GridItemCreated(e, "codes/projlist.aspx", "MainForm", "results", grdprojlist);
    }

    private void RetrieveMessage()
    {
        SystemMessage msg = new SystemMessage("codes/projlist.aspx");
        m_msg = msg.GetSystemMessage();
        //msg.SetJsMessage(litMessage);
    }
}