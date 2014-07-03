using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Data;
using System.Data.OleDb;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Web.UI;
using System.Text;

public partial class Codes_itemtypelist : System.Web.UI.Page
{
    protected AzzierScreen screen;
    protected RadGrid grditemtypelist;

    protected string mode = "";
    protected string runtimefilter = "";
    protected string designtimefilter = "";
    protected string tablename = "ItemType";
    protected string fieldlist = "";
    protected string referer = "";
    protected bool found = false;
    protected bool allowedit = true;
    protected string wherestr = "";
    protected string itemlevel = "0";
    protected string filename = "";


    protected string controlid = "";
    protected string fieldid = "";
    protected string totalCount = "";
    private NameValueCollection m_msg = new NameValueCollection();

    protected void Page_Init(object sender, EventArgs e)
    {
        RetrieveMessage();
        UserRights.CheckAccess('');

        UserRights right = new UserRights(Session["Login"].ToString(), "UserRights", "Counter");
        NameValueCollection drRights = right.GetRights(Session["Login"].ToString(), "Codes");

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
        if (Request.QueryString["filename"] != null)
        {
            filename = Request.QueryString["filename"];
            if (Request.QueryString["filename"].ToLower() == "codes/itemtypemain.aspx")
                allowedit = false;
            else allowedit = true;
        }

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
        string filterstr = "";
        filterstr = runtimefilter + "";

        if (designtimefilter + "" != "")
            if (filterstr == "")
                filterstr = designtimefilter + "";
            else
                filterstr = filterstr + "," + designtimefilter + "";

        //wherestr = v.AddConditions(filterstr, filename, controlid, tablename,null,null,mode);
        wherestr = v.AddLinqConditions(filterstr, filename, controlid, tablename, controlid,fieldid, mode);
        
        itemlevel = v.GetFilterValue(filterstr, "itemlevel");
        //Application["EQHierarchy"]

        screen = new AzzierScreen("codes/itemtypelist.aspx", "MainForm", MainControlsPanel.Controls);

        //string eqhierarchy = Application["EQHierarchy"].ToString();
        //string connstring = Application["ConnString"].ToString();

        //ItemTypeListSqlDataSource.ConnectionString = AzzierData.connectString;

        //if (wherestr == "")
        //    ItemTypeListSqlDataSource.SelectCommand = "Select * From " + tablename + " Order By ItemCode";
        //else
        //{
        //    ItemTypeListSqlDataSource.SelectCommand = "Select * From " + tablename + " " + wherestr + " Order By ItemCode";
        //}

        grditemtypelist = new RadGrid();
        grditemtypelist.ID = "grditemtypelist";
        grditemtypelist.ClientSettings.Scrolling.AllowScroll = true;
        grditemtypelist.ClientSettings.Scrolling.ScrollHeight = 300;
        grditemtypelist.ClientSettings.Scrolling.SaveScrollPosition = true;
        grditemtypelist.ClientSettings.Scrolling.UseStaticHeaders = true;
        grditemtypelist.ClientSettings.EnableRowHoverStyle = true;
        grditemtypelist.MasterTableView.TableLayout = GridTableLayout.Fixed;
        //grditemtypelist.PagerStyle.Visible = true;// false;
        grditemtypelist.PagerStyle.AlwaysVisible = true;
        grditemtypelist.Skin = "Outlook";

        grditemtypelist.Attributes.Add("rules", "all");
        //grditemtypelist.DataSourceID = "ItemTypeListSqlDataSource";
        grditemtypelist.AutoGenerateColumns = false;
        grditemtypelist.AllowPaging = true;
        grditemtypelist.PageSize = 100;
        grditemtypelist.AllowSorting = true;
        grditemtypelist.MasterTableView.AllowMultiColumnSorting = true;
        grditemtypelist.AllowFilteringByColumn = true;
        grditemtypelist.MasterTableView.CommandItemDisplay = GridCommandItemDisplay.Top;
        grditemtypelist.MasterTableView.DataKeyNames = new string[] { "Counter" };
        grditemtypelist.MasterTableView.ClientDataKeyNames = new string[] { "Counter" };

        grditemtypelist.ClientSettings.Selecting.AllowRowSelect = true;
        grditemtypelist.ClientSettings.ClientEvents.OnRowSelected = "getGridSelectedItems";

        //if (Session["UserGroup"].ToString() == "Admin" && allowedit)
        if (allowedit)
        {
            GridEditCommandColumn EditColumn = new GridEditCommandColumn();
            EditColumn.HeaderText = "Edit";
            EditColumn.UniqueName = "EditCommand";
            EditColumn.ButtonType = GridButtonColumnType.ImageButton;

            EditColumn.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
            EditColumn.HeaderStyle.Width = 30;
            grditemtypelist.MasterTableView.Columns.Add(EditColumn);
        }

        screen.SetGridColumns("itemtypelist", grditemtypelist);

        if (checkUserRight("AddNew",drRights) && allowedit)
        {
            grditemtypelist.MasterTableView.CommandItemTemplate = new InsertFormItemTemplate("Item Type List", null, "return EditItemType('','" + itemlevel + "')", 1,Session["UserGroup"].ToString());
        }
        else
        {
            grditemtypelist.MasterTableView.CommandItemTemplate = new CodesCommandItem("Item Type List", 0);
        }

       

        grditemtypelist.ItemCreated += new GridItemEventHandler(grditemtypelist_ItemCreated);
        grditemtypelist.ItemDataBound += new GridItemEventHandler(grditemtypelist_ItemDataBound);
        grditemtypelist.ClientSettings.DataBinding.FilterParameterType = GridClientDataBindingParameterType.Linq;
        grditemtypelist.ClientSettings.DataBinding.SortParameterType = GridClientDataBindingParameterType.Linq;
        MainControlsPanel.Controls.Add(grditemtypelist);

        screen.LoadScreen();
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        hidFieldId.Value = fieldid;
        hidControlId.Value = AzzierData.ActualFieldName("itemtype",controlid);

        grditemtypelist.ClientSettings.DataBinding.SelectMethod = "GetItemTypeList?where=" + wherestr;
        grditemtypelist.ClientSettings.DataBinding.Location = "../InternalServices/ServiceInventory.svc";
    }


    protected void grditemtypelist_ItemCreated(object sender, GridItemEventArgs e)
    {
      screen.GridItemCreated(e, "codes/itemtypelist.aspx", "MainForm", "itemtypelist", grditemtypelist);
    }

    protected void grditemtypelist_DeleteCommand(object source, Telerik.Web.UI.GridCommandEventArgs e)
    {
        GridDataItem item = (GridDataItem)e.Item;
        string counter = item.OwnerTableView.DataKeyValues[item.ItemIndex]["Counter"].ToString();

        ModuleoObject obj = new ModuleoObject(Session["Login"].ToString(), tablename, "Counter", counter);
        bool success = obj.Delete();
        if (!success)
        {
            grditemtypelist.Controls.Add(new LiteralControl(m_msg["T2"] + obj.ErrorMessage));
            e.Canceled = true;
        }
    }

    protected void grditemtypelist_ItemDataBound(object sender, GridItemEventArgs e)
    {
      if (e.Item is GridDataItem && !e.Item.IsInEditMode && allowedit && allowedit )
        {
            GridDataItem item = (GridDataItem)e.Item;
            ImageButton btn = (ImageButton)item["EditCommand"].Controls[0];
            btn.ImageUrl = "~/Images/Edit.gif";
            btn.OnClientClick = "return EditItemType('" + item.ItemIndex.ToString() + "'," + itemlevel + ")";
        }

        screen.GridItemDataBound(e, "codes/itemtypelist.aspx", "MainForm", "itemtypelist");
    }


    protected void RadAjaxManager1_AjaxRequest(object sender, AjaxRequestEventArgs e)
    {
    }

    private void RetrieveMessage()
    {
        SystemMessage msg = new SystemMessage("codes/itemtypelist.aspx");
        m_msg = msg.GetSystemMessage();
        msg.SetJsMessage(litMessage);
    }

    private bool checkUserRight(string command, NameValueCollection dr)
    {
        bool right = false;
        switch (command)
        {
            case "AddNew":
                if (dr["urAddNew"] == "1")
                {
                    right = true;
                }
                break;
            case "Edit":
                if (dr["urEdit"] == "1")
                {
                    right = true;
                }
                break;
            case "Delete":
                if (dr["urDelete"] == "1")
                {
                    right = true;
                }
                break;
            default:
                right = false;
                break;
        }
        return right;
    }
}