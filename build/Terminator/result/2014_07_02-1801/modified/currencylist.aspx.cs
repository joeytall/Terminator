using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Web.UI;
using System.Collections.Specialized;

public partial class Codes_currencylist : System.Web.UI.Page
{
    protected AzzierScreen screen;
    protected RadGrid grdcurrencylist;
    protected string mode = "";
    protected string runtimefilter = "";
    protected string designtimefilter = "";
    protected string fieldlist = "";
    protected string referer = "";
    protected bool found = false;
    protected string wherestr = "";
    protected string controlid = "";
    protected string fieldid = "";
    protected string tablename = "v_currchangerate";
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

        screen = new AzzierScreen("codes/currencylist.aspx", "MainForm", MainControlsPanel.Controls);

        //string connstring = Application["ConnString"].ToString();
        //LocListSqlDataSource.ConnectionString = connstring;
        //if (wherestr == "")
        //    LocListSqlDataSource.SelectCommand = "Select * From " + tablename + " Order By location";
        //else
        //    LocListSqlDataSource.SelectCommand = "Select * From " + tablename + " " + wherestr + " Order By location";

        grdcurrencylist = new RadGrid();
        grdcurrencylist.ID = "grdcurrencylist";
        grdcurrencylist.ClientSettings.Scrolling.AllowScroll = true;
        grdcurrencylist.ClientSettings.Scrolling.SaveScrollPosition = true;
        grdcurrencylist.ClientSettings.Scrolling.UseStaticHeaders = true;
        grdcurrencylist.ClientSettings.EnableRowHoverStyle = true;
        //grdloclist.ClientSettings.ClientEvents.OnScroll = "HandleScrolling";
        grdcurrencylist.MasterTableView.TableLayout = GridTableLayout.Fixed;
        //grdloclist.PagerStyle.Visible = false;
        grdcurrencylist.PagerStyle.Visible = true;
        grdcurrencylist.PagerStyle.AlwaysVisible = true;
        grdcurrencylist.Skin = "Outlook";

        grdcurrencylist.Attributes.Add("rules", "all");
        //grdloclist.DataSourceID = "LocListSqlDataSource";
        grdcurrencylist.AutoGenerateColumns = false;
        grdcurrencylist.AllowPaging = true;
        grdcurrencylist.PageSize = 100;
        grdcurrencylist.AllowSorting = true;
        grdcurrencylist.MasterTableView.AllowMultiColumnSorting = true;
        grdcurrencylist.AllowFilteringByColumn = true;
        grdcurrencylist.MasterTableView.CommandItemDisplay = GridCommandItemDisplay.Top;
        grdcurrencylist.MasterTableView.DataKeyNames = new string[] { "Currency" };

        grdcurrencylist.ClientSettings.Selecting.AllowRowSelect = true;
        grdcurrencylist.ClientSettings.ClientEvents.OnRowSelected = "getGridSelectedItems";
        //grdloclist.ClientSettings.Scrolling.FrozenColumnsCount = 1;
        //grdloclist.PagerStyle.Mode = GridPagerMode.NextPrevAndNumeric;

        grdcurrencylist.MasterTableView.CommandItemTemplate = new MultiFunctionItemTemplate("Currency");
        grdcurrencylist.ItemCreated += new GridItemEventHandler(grdcurrencylist_ItemCreated);

        grdcurrencylist.ClientSettings.DataBinding.FilterParameterType = GridClientDataBindingParameterType.Linq;
        grdcurrencylist.ClientSettings.DataBinding.SortParameterType = GridClientDataBindingParameterType.Linq;

        screen.SetGridColumns("currencylist", grdcurrencylist);
        MainControlsPanel.Controls.Add(grdcurrencylist);

        screen.LoadScreen();
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        hidFieldId.Value = fieldid;
        hidControlId.Value = AzzierData.ActualFieldName(tablename, controlid);
        //hidControlId.Value = controlid;

        grdcurrencylist.ClientSettings.DataBinding.SelectMethod = "GetCurrencyAndRate?where=" + wherestr;
        grdcurrencylist.ClientSettings.DataBinding.Location = "../InternalServices/ServicePO.svc";
    }


    protected void grdcurrencylist_ItemCreated(object sender, GridItemEventArgs e)
    {
      screen.GridItemCreated(e, "codes/currencylist.aspx", "MainForm", "results", grdcurrencylist);
    }
  
    protected void RadAjaxManager1_AjaxRequest(object sender, AjaxRequestEventArgs e)
    {
    }

    private void RetrieveMessage()
    {
        SystemMessage msg = new SystemMessage("codes/currencylist.aspx");
        m_msg = msg.GetSystemMessage();
        //msg.SetJsMessage(litMessage);
    }
}