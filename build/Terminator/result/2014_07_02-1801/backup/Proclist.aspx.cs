using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Web.UI;
using System.Collections.Specialized;

public partial class Codes_Proclist : System.Web.UI.Page
{
    protected AzzierScreen screen;
    protected RadGrid grdproclist;
    protected string mode = "";
    protected string runtimefilter = "";
    protected string designtimefilter = "";
    protected string fieldlist = "";
    protected string referer = "";
    protected bool found = false;
    protected string wherestr = "";
    protected string controlid = "";
    protected string fieldid = "";
    protected string tablename = "procedures";
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

        screen = new AzzierScreen("codes/proclist.aspx", "MainForm", MainControlsPanel.Controls);

        string connstring = Application["ConnString"].ToString();
        ProcListSqlDataSource.ConnectionString = connstring;
        if (wherestr == "")
            ProcListSqlDataSource.SelectCommand = "Select * From " + tablename + " Order By procnum";
        else
            ProcListSqlDataSource.SelectCommand = "Select * From " + tablename + " " + wherestr + " Order By procnum";

        grdproclist = new RadGrid();
        grdproclist.ID = "grdproclist";
        grdproclist.ClientSettings.Scrolling.AllowScroll = true;
        grdproclist.ClientSettings.Scrolling.SaveScrollPosition = true;
        grdproclist.ClientSettings.Scrolling.UseStaticHeaders = true;
        grdproclist.ClientSettings.EnableRowHoverStyle = true;
        grdproclist.MasterTableView.TableLayout = GridTableLayout.Fixed;
        grdproclist.PagerStyle.Visible = true;// false;
        grdproclist.PagerStyle.AlwaysVisible = true;
        grdproclist.Skin = "Outlook";

        grdproclist.Attributes.Add("rules", "all");
        //grdproclist.DataSourceID = "ProcListSqlDataSource";
        grdproclist.AutoGenerateColumns = false;
        grdproclist.AllowPaging = true;
        grdproclist.PageSize = 100;
        grdproclist.AllowSorting = true;
        grdproclist.MasterTableView.AllowMultiColumnSorting = true;
        grdproclist.AllowFilteringByColumn = true;
        grdproclist.MasterTableView.CommandItemDisplay = GridCommandItemDisplay.Top;
        grdproclist.MasterTableView.DataKeyNames = new string[] { "procnum" };

        grdproclist.ClientSettings.Selecting.AllowRowSelect = true;
        grdproclist.ClientSettings.ClientEvents.OnRowSelected = "getGridSelectedItems";
        grdproclist.ClientSettings.Scrolling.FrozenColumnsCount = 1;
        grdproclist.PagerStyle.Mode = GridPagerMode.NextPrevAndNumeric;

        grdproclist.MasterTableView.CommandItemTemplate = new MultiFunctionItemTemplate("Procedures");
        grdproclist.ItemCreated += new GridItemEventHandler(grdproclist_ItemCreated);
        grdproclist.ClientSettings.DataBinding.FilterParameterType = GridClientDataBindingParameterType.Linq;
        grdproclist.ClientSettings.DataBinding.SortParameterType = GridClientDataBindingParameterType.Linq;
        screen.SetGridColumns("proclist", grdproclist);
        MainControlsPanel.Controls.Add(grdproclist);

        screen.LoadScreen();
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        hidFieldId.Value = fieldid;
        hidControlId.Value = AzzierData.ActualFieldName("Procedures",controlid);
        grdproclist.ClientSettings.DataBinding.SelectMethod = "LookupDataAndCount?where=" + wherestr;
        grdproclist.ClientSettings.DataBinding.Location = "../InternalServices/ServiceProc.svc";
    }

    protected void RadAjaxManager1_AjaxRequest(object sender, AjaxRequestEventArgs e)
    {
    }

    protected void grdproclist_ItemCreated(object sender, GridItemEventArgs e)
    {
      screen.GridItemCreated(e, "codes/proclist.aspx", "MainForm", "results", grdproclist);
    }

    private void RetrieveMessage()
    {
        SystemMessage msg = new SystemMessage("codes/proclist.aspx");
        m_msg = msg.GetSystemMessage();
        //msg.SetJsMessage(litMessage);
    }
}