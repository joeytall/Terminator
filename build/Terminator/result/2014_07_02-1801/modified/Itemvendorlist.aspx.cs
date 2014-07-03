using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Web.UI;
using System.Data;
using System.Data.OleDb;
using System.Collections.Specialized;

public partial class Codes_Itemvendorlist : System.Web.UI.Page
{
    protected AzzierScreen screen;
    protected RadGrid grditemvendorlist;

    protected string mode = "";
    protected string runtimefilter = "";
    protected string designtimefilter = "";
    protected string fieldlist = "";
    protected string referer = "";
    protected bool found = false;
    protected string wherestr = "";
    protected string controlid = "";
    protected string fieldid = "";
    protected string totalCount = "";
    protected string tablename = "",m_vendor="";
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
        if (runtimefilter.Trim().Length != 0)
        {
            m_vendor = runtimefilter.Split('^')[1].ToString();
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

        screen = new AzzierScreen("codes/itemvendorlist.aspx", "MainForm", MainControlsPanel.Controls);
        
        grditemvendorlist = new RadGrid();
        grditemvendorlist.ID = "grditemvendorlist";
        grditemvendorlist.ClientSettings.Scrolling.AllowScroll = true;
        grditemvendorlist.ClientSettings.Scrolling.ScrollHeight = 100;
        grditemvendorlist.ClientSettings.Scrolling.SaveScrollPosition = true;
        grditemvendorlist.ClientSettings.Scrolling.UseStaticHeaders = true;
        grditemvendorlist.ClientSettings.EnableRowHoverStyle = true;
        grditemvendorlist.MasterTableView.TableLayout = GridTableLayout.Fixed;
        grditemvendorlist.PagerStyle.Visible = false;
        grditemvendorlist.Skin = "Outlook";

        grditemvendorlist.Attributes.Add("rules", "all");
        //grditemvendorlist.DataSourceID = "ItemListSqlDataSource";
        grditemvendorlist.AutoGenerateColumns = false;
        grditemvendorlist.AllowPaging = true;
        grditemvendorlist.PageSize = 100;
        grditemvendorlist.AllowSorting = true;
        grditemvendorlist.MasterTableView.AllowMultiColumnSorting = true;
        grditemvendorlist.AllowFilteringByColumn = true;
        grditemvendorlist.MasterTableView.CommandItemDisplay = GridCommandItemDisplay.Top;
        grditemvendorlist.MasterTableView.DataKeyNames = new string[] { "itemnum" };
        grditemvendorlist.MasterTableView.CommandItemSettings.ShowAddNewRecordButton = false;

        grditemvendorlist.ClientSettings.Selecting.AllowRowSelect = true;
        grditemvendorlist.ClientSettings.ClientEvents.OnRowSelected = "getGridSelectedItems";
        grditemvendorlist.ClientSettings.Scrolling.FrozenColumnsCount = 1;

        grditemvendorlist.MasterTableView.CommandItemTemplate = new MultiFunctionItemTemplate("Items");

        grditemvendorlist.ClientSettings.DataBinding.SortParameterType = GridClientDataBindingParameterType.Linq;
        grditemvendorlist.ClientSettings.DataBinding.FilterParameterType = GridClientDataBindingParameterType.Linq;

        screen.SetGridColumns("itemvendorlist", grditemvendorlist);

        MainControlsPanel.Controls.Add(grditemvendorlist);

        screen.LoadScreen();
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        hidFieldId.Value = fieldid;
        hidControlId.Value = AzzierData.ActualFieldName("v_itemvendorlist",controlid);

        grditemvendorlist.ClientSettings.DataBinding.SelectMethod = "ItemsVendorLookup?vendor="+m_vendor+ "&filter="+hidFilterByVendor.Value.ToString();

        grditemvendorlist.ClientSettings.DataBinding.Location = "../InternalServices/ServicePO.svc";
    }

    protected void RadAjaxManager1_AjaxRequest(object sender, AjaxRequestEventArgs e)
    {
    }

    private void RetrieveMessage()
    {
      SystemMessage msg = new SystemMessage("codes/itemlist.aspx");
      m_msg = msg.GetSystemMessage();
      msg.SetJsMessage(litMessage);
    }
}