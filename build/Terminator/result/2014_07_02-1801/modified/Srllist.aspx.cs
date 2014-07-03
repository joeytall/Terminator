using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Web.UI;
using System.Collections.Specialized;
using System.Data;

public partial class Codes_Srllist : System.Web.UI.Page
{
    protected  AzzierScreen screen;
    protected RadGrid grdsrllist;
    protected string mode = "";
    protected string runtimefilter = "";
    protected string designtimefilter = "";
    protected string fieldlist = "";
    protected string referer = "";
    protected bool found = false;
    protected string wherestr = "";
    protected string controlid = "";
    protected string fieldid = "";
    protected string tablename = "";
    protected string totalCount = "";
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

        string wherestr = v.AddConditions(filterstr, filename, controlid, tablename,null,null,mode);

        screen = new AzzierScreen("codes/srllist.aspx", "MainForm", MainControlsPanel.Controls);

        string connstring = Application["ConnString"].ToString();
        SrlListSqlDataSource.ConnectionString = connstring;
        if (wherestr == "")
          SrlListSqlDataSource.SelectCommand = "SELECT Equipment,SerialNum FROM " + tablename;
        else
          SrlListSqlDataSource.SelectCommand = "SELECT Equipment,SerialNum FROM " + tablename + " " + wherestr + " Order By equipment";

        grdsrllist = new RadGrid();
        grdsrllist.ID = "grdsrllist";
        grdsrllist.ClientSettings.Scrolling.AllowScroll = true;
        grdsrllist.ClientSettings.Scrolling.ScrollHeight = 100;
        grdsrllist.ClientSettings.Scrolling.SaveScrollPosition = true;
        grdsrllist.ClientSettings.Scrolling.UseStaticHeaders = true;
        grdsrllist.ClientSettings.EnableRowHoverStyle = true;
        grdsrllist.MasterTableView.TableLayout = GridTableLayout.Fixed;
        grdsrllist.PagerStyle.Visible = true;// false;
        grdsrllist.PagerStyle.AlwaysVisible = true;
        grdsrllist.Skin = "Outlook";

        grdsrllist.Attributes.Add("rules", "all");
        grdsrllist.DataSourceID = "SrlListSqlDataSource";
        grdsrllist.AutoGenerateColumns = false;
        grdsrllist.AllowPaging = true;
        grdsrllist.PageSize = 100;
        grdsrllist.AllowSorting = true;
        grdsrllist.MasterTableView.AllowMultiColumnSorting = true;
        grdsrllist.AllowFilteringByColumn = true;
        grdsrllist.MasterTableView.CommandItemDisplay = GridCommandItemDisplay.Top;
        grdsrllist.MasterTableView.DataKeyNames = new string[] { "equipment" };
        grdsrllist.MasterTableView.CommandItemSettings.ShowAddNewRecordButton = false;

        grdsrllist.ClientSettings.Selecting.AllowRowSelect = true;
        grdsrllist.ClientSettings.ClientEvents.OnRowSelected = "getGridSelectedItems";
        grdsrllist.ClientSettings.Scrolling.FrozenColumnsCount = 1;

        grdsrllist.MasterTableView.CommandItemTemplate = new MultiFunctionItemTemplate("Serial Number");
        


        screen.SetGridColumns("srllist", grdsrllist);

        MainControlsPanel.Controls.Add(grdsrllist);

        screen.LoadScreen();
    }

    

    protected void Page_Load(object sender, EventArgs e)
    {
        hidFieldId.Value = fieldid;
        hidControlId.Value = AzzierData.ActualFieldName(tablename,controlid);
    }

    protected void RadAjaxManager1_AjaxRequest(object sender, AjaxRequestEventArgs e)
    {
    }


    private void RetrieveMessage()
    {
      SystemMessage msg = new SystemMessage("codes/srllist.aspx");
      m_msg = msg.GetSystemMessage();
      msg.SetJsMessage(litMessage);
    }
}