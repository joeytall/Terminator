using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Web.UI;
using System.Collections.Specialized;

public partial class Codes_Meterlist : System.Web.UI.Page
{
    protected AzzierScreen screen;
    protected RadGrid grdmeterlist;

    protected string mode = "";
    protected string addnew = "";
    protected string runtimefilter = "";
    protected string designtimefilter = "";
    protected string fieldlist = "";
    protected string clear = "yes";
    protected string referer = "";
    protected bool found = false;
    protected bool hasopener = false;
    protected string wherestr = "";
    protected string controlid = "";
    protected string fieldid = "";

    protected string TotalCount = "";
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

        //string wherestr = v.AddConditions(filterstr, filename, controlid, "craft");
        wherestr = v.AddLinqConditions(filterstr, filename, controlid,"v_AllMaxReading",null,null,mode);

        screen = new AzzierScreen("codes/meterlist.aspx", "MainForm", MainControlsPanel.Controls);

        grdmeterlist = new RadGrid();
        grdmeterlist.ID = "grdmeterlist";
        grdmeterlist.ClientSettings.Scrolling.AllowScroll = true;
        grdmeterlist.ClientSettings.Scrolling.SaveScrollPosition = true;
        grdmeterlist.ClientSettings.Scrolling.UseStaticHeaders = true;
        grdmeterlist.ClientSettings.EnableRowHoverStyle = true;
        grdmeterlist.MasterTableView.TableLayout = GridTableLayout.Fixed;
        grdmeterlist.PagerStyle.Visible = true;
        grdmeterlist.PagerStyle.AlwaysVisible = true;
        grdmeterlist.Skin = "Outlook";

        grdmeterlist.Attributes.Add("rules", "all");
        grdmeterlist.AutoGenerateColumns = false;
        grdmeterlist.AllowPaging = true;
        grdmeterlist.PageSize = 100;
        grdmeterlist.AllowSorting = true;
        grdmeterlist.MasterTableView.AllowMultiColumnSorting = true;
        grdmeterlist.AllowFilteringByColumn = true;
        grdmeterlist.MasterTableView.CommandItemDisplay = GridCommandItemDisplay.Top;
        grdmeterlist.MasterTableView.DataKeyNames = new string[] { "Counter" };

        grdmeterlist.ClientSettings.Selecting.AllowRowSelect = true;
        grdmeterlist.ClientSettings.ClientEvents.OnRowSelected = "getGridSelectedItems";
        grdmeterlist.ClientSettings.Scrolling.FrozenColumnsCount = 1;

        screen.SetGridColumns("meterlist", grdmeterlist);

        grdmeterlist.ItemCreated += new GridItemEventHandler(grdmeterlist_ItemCreated);

        grdmeterlist.MasterTableView.CommandItemTemplate = new CodesCommandItem("Meter", 0);

        grdmeterlist.ClientSettings.DataBinding.SortParameterType = GridClientDataBindingParameterType.Linq;
        grdmeterlist.ClientSettings.DataBinding.FilterParameterType = GridClientDataBindingParameterType.Linq;
        MainControlsPanel.Controls.Add(grdmeterlist);

        screen.LoadScreen();
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        hidFieldId.Value = fieldid;
        hidControlId.Value = AzzierData.ActualFieldName("Meter",controlid);

        grdmeterlist.ClientSettings.DataBinding.SelectMethod = "MeterLookup?wherestr=" + wherestr;
        grdmeterlist.ClientSettings.DataBinding.Location = "../InternalServices/ServiceMeter.svc";

        //CalRadwinSize();
    }

    protected void RadAjaxManager1_AjaxRequest(object sender, AjaxRequestEventArgs e)
    {
        //grdcraftlist.PageSize = 100 + grdcraftlist.PageSize;
        //grdcraftlist.Rebind();
    }

    protected void grdmeterlist_ItemCreated(object sender, GridItemEventArgs e)
    {
      screen.GridItemCreated(e, "codes/meterlist.aspx", "MainForm", "results", grdmeterlist);
    }
    
    private void RetrieveMessage()
    {
        SystemMessage msg = new SystemMessage("codes/craftlist.aspx");
        m_msg = msg.GetSystemMessage();
        msg.SetJsMessage(litMessage);
    }
}
