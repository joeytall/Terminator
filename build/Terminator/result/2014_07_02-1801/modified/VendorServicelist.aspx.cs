using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Web.UI;
using System.Collections.Specialized;

public partial class Codes_VendorServicelist : System.Web.UI.Page
{
    protected AzzierScreen screen;
    protected RadGrid grdvendorservicelist;
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
        filterstr = designtimefilter;
        if (runtimefilter != "")
        {
          if (filterstr != "")
            filterstr = filterstr + "," + runtimefilter;
          else
            filterstr = runtimefilter;
        }
        wherestr = v.AddLinqConditions(filterstr, filename, controlid, tablename,null,null,mode);

        screen = new AzzierScreen("codes/VendorServicelist.aspx", "MainForm", MainControlsPanel.Controls);

        string connstring = Application["ConnString"].ToString();
        VendorServicelistSqlDataSource.ConnectionString = connstring;
        if (wherestr == "")
            VendorServicelistSqlDataSource.SelectCommand = "Select * From " + tablename;
        else
            VendorServicelistSqlDataSource.SelectCommand = "Select * From " + tablename + " " + wherestr + " Order By ServiceCode";

        grdvendorservicelist = new RadGrid();
        grdvendorservicelist.ID = "grdvendorservicelist";
        grdvendorservicelist.ClientSettings.Scrolling.AllowScroll = true;
        grdvendorservicelist.ClientSettings.Scrolling.SaveScrollPosition = true;
        grdvendorservicelist.ClientSettings.Scrolling.UseStaticHeaders = true;
        grdvendorservicelist.ClientSettings.EnableRowHoverStyle = true;
        grdvendorservicelist.MasterTableView.TableLayout = GridTableLayout.Fixed;
        grdvendorservicelist.PagerStyle.Visible = true;// false;
        grdvendorservicelist.PagerStyle.AlwaysVisible = true;
        grdvendorservicelist.Skin = "Outlook";
        grdvendorservicelist.Attributes.Add("rules", "all");
        //grdvendorservicelist.DataSourceID = "VendorServicelistSqlDataSource";
        grdvendorservicelist.AutoGenerateColumns = false;
        grdvendorservicelist.AllowPaging = true;
        grdvendorservicelist.PageSize = 100;
        grdvendorservicelist.AllowSorting = true;
        grdvendorservicelist.MasterTableView.AllowMultiColumnSorting = true;
        grdvendorservicelist.AllowFilteringByColumn = true;
        grdvendorservicelist.MasterTableView.CommandItemDisplay = GridCommandItemDisplay.Top;
        grdvendorservicelist.MasterTableView.DataKeyNames = new string[] { "ServiceCode" };
        grdvendorservicelist.ClientSettings.Selecting.AllowRowSelect = true;
        grdvendorservicelist.ClientSettings.ClientEvents.OnRowSelected = "getGridSelectedItems";
        grdvendorservicelist.PagerStyle.Mode = GridPagerMode.NextPrevAndNumeric;
        grdvendorservicelist.MasterTableView.CommandItemTemplate = new MultiFunctionItemTemplate("Vendor Service");

        grdvendorservicelist.ClientSettings.DataBinding.FilterParameterType = GridClientDataBindingParameterType.Linq;
        grdvendorservicelist.ClientSettings.DataBinding.SortParameterType = GridClientDataBindingParameterType.Linq; 
        screen.SetGridColumns("VendorServicelist", grdvendorservicelist);
        MainControlsPanel.Controls.Add(grdvendorservicelist);
        screen.LoadScreen();
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        hidFieldId.Value = fieldid;
        hidControlId.Value = AzzierData.ActualFieldName(tablename,controlid);
        grdvendorservicelist.ClientSettings.DataBinding.SelectMethod = "GetVendorServiceLinq?wherestring=" + wherestr;
        grdvendorservicelist.ClientSettings.DataBinding.Location = "../InternalServices/ServiceVendor.svc";
    }

    protected void RadAjaxManager1_AjaxRequest(object sender, AjaxRequestEventArgs e)
    {
    }

    private void RetrieveMessage()
    {
        SystemMessage msg = new SystemMessage("codes/vendorServicelist.aspx");
        m_msg = msg.GetSystemMessage();
        msg.SetJsMessage(litMessage);
    }

}