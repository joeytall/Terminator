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

public partial class Codes_Itemlist : System.Web.UI.Page
{
    protected AzzierScreen screen;
    protected RadGrid grditemlist;

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
    protected string tablename = "";
    protected NameValueCollection m_msg = new NameValueCollection(); 

    protected void Page_Init(object sender, EventArgs e)
    {
        RetrieveMessage();
        if (Session["Login"] == null)
        {
            Response.Write("<script>alert('" + m_msg["T1"] + "');top.document.location.href='../Login.aspx';</script>");
            Response.End();
        }

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
                filterstr = filterstr + "," + designtimefilter + "";
        //Response.Write(filterstr);
        wherestr = v.AddLinqConditions(filterstr, filename, controlid, tablename,null,null,mode);

        screen = new AzzierScreen("codes/itemlist.aspx", "MainForm", MainControlsPanel.Controls);
        
        string connstring = Application["ConnString"].ToString();
        ItemListSqlDataSource.ConnectionString = connstring;

        string fldlist = "*";

        //if (wherestr == "")
        //  ItemListSqlDataSource.SelectCommand = "Select " + fldlist + " From items Order By itemnum";
        //else
        //  ItemListSqlDataSource.SelectCommand = "Select " + fldlist + " From items " + wherestr + " Order By itemnum";

        if (wherestr == "")
            ItemListSqlDataSource.SelectCommand = "Select " + fldlist + " From " + tablename + " Order By itemnum";
        else
            ItemListSqlDataSource.SelectCommand = "Select " + fldlist + " From " + tablename + " " + wherestr + " Order By itemnum";

        grditemlist = new RadGrid();
        grditemlist.ID = "grditemlist";
        grditemlist.ClientSettings.Scrolling.AllowScroll = true;
        grditemlist.ClientSettings.Scrolling.ScrollHeight = 100;
        grditemlist.ClientSettings.Scrolling.SaveScrollPosition = true;
        grditemlist.ClientSettings.Scrolling.UseStaticHeaders = true;
        grditemlist.ClientSettings.EnableRowHoverStyle = true;
        grditemlist.MasterTableView.TableLayout = GridTableLayout.Fixed;
        grditemlist.PagerStyle.Visible = true;
        grditemlist.Skin = "Outlook";

        grditemlist.Attributes.Add("rules", "all");
        //grditemlist.DataSourceID = "ItemListSqlDataSource";
        grditemlist.AutoGenerateColumns = false;
        grditemlist.AllowPaging = true;
        grditemlist.PageSize = 100;
        grditemlist.AllowSorting = true;
        grditemlist.MasterTableView.AllowMultiColumnSorting = true;
        grditemlist.AllowFilteringByColumn = true;
        grditemlist.MasterTableView.CommandItemDisplay = GridCommandItemDisplay.Top;
        grditemlist.MasterTableView.DataKeyNames = new string[] { "itemnum" };
        grditemlist.MasterTableView.CommandItemSettings.ShowAddNewRecordButton = false;

        grditemlist.ClientSettings.Selecting.AllowRowSelect = true;
        grditemlist.ClientSettings.ClientEvents.OnRowSelected = "getGridSelectedItems";

        grditemlist.MasterTableView.CommandItemTemplate = new MultiFunctionItemTemplate("Items");

        grditemlist.ItemCreated += new GridItemEventHandler(grditemlist_ItemCreated);

        screen.SetGridColumns("itemlist", grditemlist);

        grditemlist.ClientSettings.DataBinding.FilterParameterType = GridClientDataBindingParameterType.Linq;
        grditemlist.ClientSettings.DataBinding.SortParameterType = GridClientDataBindingParameterType.Linq;
        
        MainControlsPanel.Controls.Add(grditemlist);

        screen.LoadScreen();
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        hidFieldId.Value = fieldid;
        hidControlId.Value = AzzierData.ActualFieldName(tablename, controlid);
        //CalRadwinSize();
        grditemlist.ClientSettings.DataBinding.SelectMethod = "GetItemList?where=" + wherestr;
        grditemlist.ClientSettings.DataBinding.Location = "../InternalServices/ServiceInventory.svc";

    }

    protected void RadAjaxManager1_AjaxRequest(object sender, AjaxRequestEventArgs e)
    {
    }

    protected void grditemlist_ItemCreated(object sender, GridItemEventArgs e)
    {
      screen.GridItemCreated(e, "codes/itemlist.aspx", "MainForm", "results", grditemlist);
    
    }

    private void RetrieveMessage()
    {
      SystemMessage msg = new SystemMessage("codes/itemlist.aspx");
      m_msg = msg.GetSystemMessage();
      msg.SetJsMessage(litMessage);
    }

}