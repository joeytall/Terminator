using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Web.UI;
using System.Collections.Specialized;

public partial class Codes_Phaselist : System.Web.UI.Page
{
    protected AzzierScreen screen;

    protected RadGrid grdphaselist;

    protected string mode = "";
    protected string runtimefilter = "";
    protected string designtimefilter = "";
    protected string fieldlist = "";
    protected string referer = "";
    protected bool found = false;
    protected string wherestr = "";
    protected string controlid = "";
    protected string fieldid = "";
    protected string tablename = "phase";
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

        string wherestr = v.AddConditions(filterstr, filename, controlid, tablename,null,null,mode);

        screen = new AzzierScreen("codes/phaselist.aspx", "MainForm", MainControlsPanel.Controls);

        string connstring = Application["ConnString"].ToString();
        PhaseListSqlDataSource.ConnectionString = connstring;
        if (wherestr == "")
            PhaseListSqlDataSource.SelectCommand = "Select * From " + tablename + " Order By phase";
        else
            PhaseListSqlDataSource.SelectCommand = "Select * From " + tablename + " " + wherestr + " Order By phase";

        grdphaselist = new RadGrid();
        grdphaselist.ID = "grdphaselist";
        grdphaselist.ClientSettings.Scrolling.AllowScroll = true;
        grdphaselist.ClientSettings.Scrolling.SaveScrollPosition = true;
        grdphaselist.ClientSettings.Scrolling.UseStaticHeaders = true;
        grdphaselist.ClientSettings.EnableRowHoverStyle = true;
        grdphaselist.MasterTableView.TableLayout = GridTableLayout.Fixed;
        grdphaselist.PagerStyle.Visible = true;// false;
        grdphaselist.PagerStyle.AlwaysVisible = true;
        grdphaselist.Skin = "Outlook";

        grdphaselist.Attributes.Add("rules", "all");
        grdphaselist.DataSourceID = "PhaseListSqlDataSource";
        grdphaselist.AutoGenerateColumns = false;
        grdphaselist.AllowPaging = true;
        grdphaselist.PageSize = 100;
        grdphaselist.AllowSorting = true;
        grdphaselist.MasterTableView.AllowMultiColumnSorting = true;
        grdphaselist.AllowFilteringByColumn = true;
        grdphaselist.MasterTableView.CommandItemDisplay = GridCommandItemDisplay.Top;
        grdphaselist.MasterTableView.DataKeyNames = new string[] { "phase" };

        grdphaselist.ClientSettings.Selecting.AllowRowSelect = true;
        grdphaselist.ClientSettings.ClientEvents.OnRowSelected = "getGridSelectedItems";
        grdphaselist.ClientSettings.Scrolling.FrozenColumnsCount = 1;

        grdphaselist.MasterTableView.CommandItemTemplate = new MultiFunctionItemTemplate("Phase");
        grdphaselist.ItemCreated += new GridItemEventHandler(grdphaselist_ItemCreated);

        screen.SetGridColumns("phaselist", grdphaselist);
        MainControlsPanel.Controls.Add(grdphaselist);

        screen.LoadScreen();
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        hidFieldId.Value = fieldid;
        hidControlId.Value = AzzierData.ActualFieldName("phase",controlid);
    }

    protected void RadAjaxManager1_AjaxRequest(object sender, AjaxRequestEventArgs e)
    {
    }

    protected void grdphaselist_ItemCreated(object sender, GridItemEventArgs e)
    {
      screen.GridItemCreated(e, "codes/phaselist.aspx", "MainForm", "results", grdphaselist);
    }

    private void RetrieveMessage()
    {
        SystemMessage msg = new SystemMessage("codes/phaselist.aspx");
        m_msg = msg.GetSystemMessage();
        //msg.SetJsMessage(litMessage);
    }
}