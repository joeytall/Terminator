using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Web.UI;
using System.Collections.Specialized;
using System.Data;

public partial class Codes_Storelist : System.Web.UI.Page
{
    protected AzzierScreen screen;
    protected RadGrid grdstorelist;
    protected string mode = "";
    protected string runtimefilter = "";
    protected string designtimefilter = "";
    protected string fieldlist = "";
    protected string referer = "";
    protected bool found = false;
    protected string controlid = "";
    protected string fieldid = "";
    protected string totalCount = "";
    protected string tablename = "";
    protected string wherestr = "";
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

        string wherestr = v.AddLinqConditions(filterstr, filename, controlid, tablename,null,null,mode);

        screen = new AzzierScreen("codes/storelist.aspx", "MainForm", MainControlsPanel.Controls);

        string connstring = Application["ConnString"].ToString();
        StoreListSqlDataSource.ConnectionString = connstring;

        //string itemnum = GetItemNum(runtimefilter);
      /*
        if (referer == "inventory/storemain.aspx")
        {
          if (wherestr == "")
            wherestr = "Where Storeroom Not In (Select Storeroom From InvMain Where Itemnum='" + itemnum + "' Group By Storeroom) ";
          else
            wherestr = wherestr + " And (Storeroom Not In (Select Storeroom From InvMain Where Itemnum='" + itemnum + "' Group By Storeroom)) "; 
        }
       * */
        if (wherestr == "")
            StoreListSqlDataSource.SelectCommand = "Select * From " + tablename + " Order By storeroom";
        else
            StoreListSqlDataSource.SelectCommand = "Select * From " + tablename + " " + wherestr + " Order By storeroom";

        grdstorelist = new RadGrid();
        grdstorelist.ID = "grdstorelist";
        grdstorelist.ClientSettings.Scrolling.AllowScroll = true;
        grdstorelist.ClientSettings.Scrolling.ScrollHeight = 100;
        grdstorelist.ClientSettings.Scrolling.SaveScrollPosition = true;
        grdstorelist.ClientSettings.Scrolling.UseStaticHeaders = true;
        grdstorelist.ClientSettings.EnableRowHoverStyle = true;
        grdstorelist.MasterTableView.TableLayout = GridTableLayout.Fixed;
        grdstorelist.PagerStyle.Visible = true;// false;
        grdstorelist.PagerStyle.AlwaysVisible = true;
        grdstorelist.Skin = "Outlook";

        grdstorelist.Attributes.Add("rules", "all");
        //grdstorelist.DataSourceID = "StoreListSqlDataSource";
        grdstorelist.AutoGenerateColumns = false;
        grdstorelist.AllowPaging = true;
        grdstorelist.PageSize = 100;
        grdstorelist.AllowSorting = true;
        grdstorelist.MasterTableView.AllowMultiColumnSorting = true;
        grdstorelist.AllowFilteringByColumn = true;
        grdstorelist.MasterTableView.CommandItemDisplay = GridCommandItemDisplay.Top;
        grdstorelist.MasterTableView.DataKeyNames = new string[] { "storeroom" };
        grdstorelist.MasterTableView.CommandItemSettings.ShowAddNewRecordButton = false;

        grdstorelist.ClientSettings.Selecting.AllowRowSelect = true;
        grdstorelist.ClientSettings.ClientEvents.OnRowSelected = "getGridSelectedItems";
        grdstorelist.ClientSettings.Scrolling.FrozenColumnsCount = 1;
        grdstorelist.ItemCreated+=new GridItemEventHandler(grdstorelist_ItemCreated);
        grdstorelist.ClientSettings.DataBinding.FilterParameterType = GridClientDataBindingParameterType.Linq;
        grdstorelist.ClientSettings.DataBinding.SortParameterType = GridClientDataBindingParameterType.Linq;
        grdstorelist.MasterTableView.CommandItemTemplate = new MultiFunctionItemTemplate("Storeroom");

        screen.SetGridColumns("storelist", grdstorelist);

        MainControlsPanel.Controls.Add(grdstorelist);

        screen.LoadScreen();
    }

  /*
    private string GetItemNum(string str)
    {
      string itemnum = "";
      string[] filterlist = str.Split(',');
      for (int i = 0; i < filterlist.Length; i++)
      {
        string filter = filterlist[i];
        int pos = filter.IndexOf('^');
        if (pos >= 0)
        {
          if (filter.Substring(0, pos).ToLower() == "itemnum")
            return filter.Substring(pos + 1);
        }
      }
      return itemnum;
    }
  */
    protected void Page_Load(object sender, EventArgs e)
    {
        hidFieldId.Value = fieldid;
        hidControlId.Value = AzzierData.ActualFieldName(tablename,controlid);
        grdstorelist.ClientSettings.DataBinding.SelectMethod = "LookupStoreRoomDataAndCount?where=" + wherestr;
        grdstorelist.ClientSettings.DataBinding.Location = "../InternalServices/ServiceInventory.svc";
    }

    protected void RadAjaxManager1_AjaxRequest(object sender, AjaxRequestEventArgs e)
    {
    }

    protected void grdstorelist_ItemCreated(object sender, GridItemEventArgs e)
    {
      screen.GridItemCreated(e, "codes/storelist.aspx", "MainForm", "storelist", grdstorelist);
    }

    

    private void RetrieveMessage()
    {
        SystemMessage msg = new SystemMessage("codes/storelist.aspx");
        m_msg = msg.GetSystemMessage();
        msg.SetJsMessage(litMessage);
    }
}