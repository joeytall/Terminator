using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Web.UI;
using System.Collections.Specialized;

public partial class Codes_PMlist : System.Web.UI.Page
{
    protected AzzierScreen screen;
    protected RadGrid grdpmlist;

    protected string mode = "";
    protected string runtimefilter = "";
    protected string designtimefilter = "";
    protected string fieldlist = "";
    protected string referer = "";
    protected bool found = false;
    protected string wherestr = "";
    protected string controlid = "";
    protected string fieldid = "";
    protected string tablename = "pm";
    protected string TotalCount = "";
    protected NameValueCollection m_msg = new NameValueCollection();

    protected void Page_Init(object sender, EventArgs e)
    {
        if (Session["Login"] == null)
        {
            //Response.Write("<html><script type=\"text/javascript\">alert('Your session has expired. Please login again.');top.document.location.href='../login.aspx';</script></html>");
            Response.Write("<script>alert('" + m_msg["T1"] + "');top.document.location.href='../Login.aspx';</script>");
            Response.End();
        }

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

        string wherestr = v.AddLinqConditions(filterstr, filename, controlid, tablename,null,null,mode);

        screen = new AzzierScreen("codes/pmlist.aspx", "MainForm", MainControlsPanel.Controls);

        string connstring = Application["ConnString"].ToString();
        PMListSqlDataSource.ConnectionString = connstring;
        if (wherestr == "")
            PMListSqlDataSource.SelectCommand = "Select * From " + tablename + " Order By pmnum";
        else
            PMListSqlDataSource.SelectCommand = "Select * From " + tablename + " " + wherestr + " Order By pmnum";

        grdpmlist = new RadGrid();
        grdpmlist.ID = "grdpmlist";
        grdpmlist.ClientSettings.Scrolling.AllowScroll = true;
        grdpmlist.ClientSettings.Scrolling.SaveScrollPosition = true;
        grdpmlist.ClientSettings.Scrolling.UseStaticHeaders = true;
        grdpmlist.ClientSettings.EnableRowHoverStyle = true;
        grdpmlist.MasterTableView.TableLayout = GridTableLayout.Fixed;
        grdpmlist.PagerStyle.Visible = true;
        grdpmlist.PagerStyle.AlwaysVisible = true;
        grdpmlist.Skin = "Outlook";

        grdpmlist.Attributes.Add("rules", "all");
        //grdpmlist.DataSourceID = "PMListSqlDataSource";
        grdpmlist.AutoGenerateColumns = false;
        grdpmlist.AllowPaging = true;
        grdpmlist.PageSize = 100;
        grdpmlist.AllowSorting = true;
        grdpmlist.MasterTableView.AllowMultiColumnSorting = true;
        grdpmlist.AllowFilteringByColumn = true;
        grdpmlist.MasterTableView.CommandItemDisplay = GridCommandItemDisplay.Top;
        grdpmlist.MasterTableView.DataKeyNames = new string[] { "pmnum" };

        grdpmlist.ClientSettings.Selecting.AllowRowSelect = true;
        grdpmlist.ClientSettings.ClientEvents.OnRowSelected = "getGridSelectedItems";
        grdpmlist.PagerStyle.Mode = GridPagerMode.NextPrevAndNumeric;

        //grdpmlist.ItemDataBound += new GridItemEventHandler(RadGrid1_ItemDataBound);
        grdpmlist.MasterTableView.CommandItemTemplate = new MultiFunctionItemTemplate("PM");
        grdpmlist.ItemCreated += new GridItemEventHandler(grdpmlist_ItemCreated);
        grdpmlist.ClientSettings.DataBinding.FilterParameterType = GridClientDataBindingParameterType.Linq;
        grdpmlist.ClientSettings.DataBinding.SortParameterType = GridClientDataBindingParameterType.Linq;
        screen.SetGridColumns("pmlist", grdpmlist);
        MainControlsPanel.Controls.Add(grdpmlist);

        screen.LoadScreen();
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        hidFieldId.Value = fieldid;
        hidControlId.Value = AzzierData.ActualFieldName("PM",controlid);
        grdpmlist.ClientSettings.DataBinding.SelectMethod = "LookupDataAndCount?where=" + wherestr;
        grdpmlist.ClientSettings.DataBinding.Location = "../InternalServices/ServicePM.svc";
    }

    protected void RadAjaxManager1_AjaxRequest(object sender, AjaxRequestEventArgs e)
    {
    }

    protected void grdpmlist_ItemCreated(object sender, GridItemEventArgs e)
    {
      screen.GridItemCreated(e, "codes/pmlist.aspx", "MainForm", "results", grdpmlist);
    }

    private void RetrieveMessage()
    {
        SystemMessage msg = new SystemMessage("codes/pmlist.aspx");
        m_msg = msg.GetSystemMessage();
    }
}