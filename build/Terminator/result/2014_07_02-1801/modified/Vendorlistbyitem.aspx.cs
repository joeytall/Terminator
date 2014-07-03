using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Web.UI;
using System.Collections.Specialized;

public partial class Codes_vendorlistbyitem : System.Web.UI.Page
{
    protected AzzierScreen screen;
    protected RadGrid grdvendorlist;
    protected string mode = "edit";
    protected string addnew = "";
    protected string runtimefilter = "";
    protected string designtimefilter = "";
    protected string fieldlist = "";
    protected string referer = "";
    protected bool found = false;
    protected string wherestr = "";
    protected string controlid = "";
    protected string fieldid = "";
    protected string TotalCount = "";
    protected string filterstr = "", filename = "";
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
        if (Request.QueryString["filename"].ToString() != null)
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

        wherestr = v.AddLinqConditions(filterstr, filename, controlid,"itemvendor",null,null,mode);

        screen = new AzzierScreen("codes/vendorlistbyitem.aspx", "MainForm", MainControlsPanel.Controls);

        grdvendorlist = new RadGrid();
        grdvendorlist.ID = "grdvendorlist";
        grdvendorlist.ClientSettings.Scrolling.AllowScroll = true;
        grdvendorlist.ClientSettings.Scrolling.SaveScrollPosition = true;
        grdvendorlist.ClientSettings.Scrolling.UseStaticHeaders = true;
        grdvendorlist.ClientSettings.EnableRowHoverStyle = true;
        //grdvendorlist.ClientSettings.ClientEvents.OnScroll = "HandleScrolling";
        grdvendorlist.MasterTableView.TableLayout = GridTableLayout.Fixed;
        grdvendorlist.PagerStyle.Visible = true;
        grdvendorlist.PagerStyle.AlwaysVisible = true;
        grdvendorlist.Skin = "Outlook";
        grdvendorlist.Attributes.Add("rules", "all");
        //grdvendorlist.DataSourceID = "vendorListSqlDataSource";
        grdvendorlist.AutoGenerateColumns = false;
        grdvendorlist.AllowPaging = true;
        grdvendorlist.PageSize = 100;
        grdvendorlist.AllowSorting = true;
        grdvendorlist.MasterTableView.AllowMultiColumnSorting = true;
        grdvendorlist.AllowFilteringByColumn = true;
        grdvendorlist.MasterTableView.CommandItemDisplay = GridCommandItemDisplay.Top;
        grdvendorlist.MasterTableView.DataKeyNames = new string[]{"Vendor"};  //  { "CompanyCode" };  //  CompanyCode
        grdvendorlist.ClientSettings.Selecting.AllowRowSelect = true;
        //grdvendorlist.ClientSettings.ClientEvents.OnRowSelected = "RowSelected_test__";// "getGridSelectedItems";
        grdvendorlist.ClientSettings.ClientEvents.OnRowSelected = "getGridSelectedItems";
        
        //grdvendorlist.ClientSettings.Scrolling.FrozenColumnsCount = 1;
        //grdvendorlist.PagerStyle.Mode = GridPagerMode.NextPrevAndNumeric;
        grdvendorlist.MasterTableView.CommandItemTemplate = new MultiFunctionItemTemplate("Item Vendor List");
        grdvendorlist.ItemCreated += new GridItemEventHandler(grdvendorlist_ItemCreated);

        grdvendorlist.ClientSettings.DataBinding.SelectMethod = "GetItemVendor?where=" + wherestr;
        grdvendorlist.ClientSettings.DataBinding.Location = "../InternalServices/ServiceVendor.svc";
        grdvendorlist.ClientSettings.DataBinding.FilterParameterType = GridClientDataBindingParameterType.Linq;
        grdvendorlist.ClientSettings.DataBinding.SortParameterType = GridClientDataBindingParameterType.Linq;

        screen.SetGridColumns("vendorlist", grdvendorlist);
        MainControlsPanel.Controls.Add(grdvendorlist);
        screen.LoadScreen();
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        hidFieldId.Value = fieldid;
        hidControlId.Value = AzzierData.ActualFieldName("ItemVendor",controlid);
    }

    protected void grdvendorlist_ItemCreated(object sender, GridItemEventArgs e)
    {
      screen.GridItemCreated(e, "codes/vendorlistbyitem.aspx", "MainForm", "results", grdvendorlist);
    }

    private void RetrieveMessage()
    {
        SystemMessage msg = new SystemMessage("codes/vendorlistbyitem.aspx");
        m_msg = msg.GetSystemMessage();
        msg.SetJsMessage(litMessage);
    }
}