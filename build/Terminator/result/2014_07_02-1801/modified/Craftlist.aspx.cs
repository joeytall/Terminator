using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Web.UI;
using System.Collections.Specialized;

public partial class Codes_Craftlist : System.Web.UI.Page
{
    protected AzzierScreen screen;
    protected RadGrid grdcraftlist;

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
    private string craft = "", craftDESC = "", RATE = "", Inactive = "";

    protected string TotalCount = "";
    protected NameValueCollection m_msg = new NameValueCollection();
    protected void Page_Init(object sender, EventArgs e)
    {
        RetrieveMessage();
        UserRights.CheckAccess('');

        Session.LCID = Convert.ToInt32(Session["LCID"]);

        UserRights right = new UserRights(Session["Login"].ToString(), "UserRights", "Counter");
        NameValueCollection drRights = right.GetRights(Session["Login"].ToString(), "Codes");

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
        if (Request.QueryString["hasopener"] != null)
            hasopener = true;

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

        //string wherestr = v.AddConditions(filterstr, filename, controlid, "craft",null,null,mode);
        string wherestr = v.AddLinqConditions(filterstr, filename, controlid, "craft", null, null, mode);
        craft = v.GetFilterValue(designtimefilter, "craft");
        craftDESC = v.GetFilterValue(designtimefilter, "craftDesc");
        RATE = v.GetFilterValue(designtimefilter, "Rate");
        Inactive = v.GetFilterValue(designtimefilter, "Inactive");

        screen = new AzzierScreen("codes/craftlist.aspx", "MainForm", MainControlsPanel.Controls);

        //string connstring = Application["ConnString"].ToString();
        //CraftListSqlDataSource.ConnectionString = connstring;
        //if (wherestr == "")
        //    CraftListSqlDataSource.SelectCommand = "Select * From craft Order By craft";
        //else
        //    CraftListSqlDataSource.SelectCommand = "Select * From craft " + wherestr + " Order By craft";

        grdcraftlist = new RadGrid();
        grdcraftlist.ID = "grdcraftlist";
        grdcraftlist.ClientSettings.Scrolling.AllowScroll = true;
        grdcraftlist.ClientSettings.Scrolling.ScrollHeight = 300;
        grdcraftlist.ClientSettings.Scrolling.SaveScrollPosition = true;
        grdcraftlist.ClientSettings.Scrolling.UseStaticHeaders = true;
        grdcraftlist.ClientSettings.EnableRowHoverStyle = true;
        grdcraftlist.MasterTableView.TableLayout = GridTableLayout.Fixed;
        //grdcraftlist.PagerStyle.Visible = true;
        grdcraftlist.PagerStyle.AlwaysVisible = true;
        grdcraftlist.Skin = "Outlook";

        grdcraftlist.Attributes.Add("rules", "all");
        //grdcraftlist.DataSourceID = "CraftListSqlDataSource";
        grdcraftlist.AutoGenerateColumns = false;
        grdcraftlist.AllowPaging = true;
        grdcraftlist.PageSize = 100;
        grdcraftlist.AllowSorting = true;
        grdcraftlist.MasterTableView.AllowMultiColumnSorting = true;
        grdcraftlist.AllowFilteringByColumn = true;
        grdcraftlist.MasterTableView.CommandItemDisplay = GridCommandItemDisplay.Top;
        grdcraftlist.MasterTableView.DataKeyNames = new string[] { "Craft" };
        grdcraftlist.MasterTableView.ClientDataKeyNames = new string[] { "Craft" };

        grdcraftlist.ClientSettings.Selecting.AllowRowSelect = true;
        grdcraftlist.ClientSettings.ClientEvents.OnRowSelected = "getGridSelectedItems";
        grdcraftlist.ClientSettings.Scrolling.FrozenColumnsCount = 1;

        //Add edit button column in grid
        GridEditCommandColumn EditColumn = new GridEditCommandColumn();
        EditColumn.HeaderText = "Edit";
        EditColumn.UniqueName = "EditCommand";
        EditColumn.ButtonType = GridButtonColumnType.ImageButton;
        EditColumn.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
        EditColumn.HeaderStyle.Width = 30;
        grdcraftlist.MasterTableView.Columns.Add(EditColumn);
        grdcraftlist.MasterTableView.EditMode = GridEditMode.InPlace;
        

        screen.SetGridColumns("craftlist", grdcraftlist);
        grdcraftlist.ItemCreated += new GridItemEventHandler(grdcraftlist_ItemCreated);
        grdcraftlist.ItemDataBound += new GridItemEventHandler(grdcraftlist_ItemDataBound);
        grdcraftlist.ClientSettings.DataBinding.FilterParameterType = GridClientDataBindingParameterType.Linq;
        grdcraftlist.ClientSettings.DataBinding.SortParameterType = GridClientDataBindingParameterType.Linq;

        if (checkUserRight("AddNew", drRights))
        {
            grdcraftlist.MasterTableView.CommandItemTemplate = new InsertFormItemTemplate("Craft ", null, "return EditCraft('')", 1);
        }
        else
        {
           grdcraftlist.MasterTableView.CommandItemTemplate = new CodesCommandItem("Craft  ", 0);
        }



        MainControlsPanel.Controls.Add(grdcraftlist);

        screen.LoadScreen();
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        hidFieldId.Value = fieldid;
        hidControlId.Value = AzzierData.ActualFieldName("craft",controlid);
        grdcraftlist.ClientSettings.DataBinding.SelectMethod = "GetCraftList?where=" + wherestr;
        grdcraftlist.ClientSettings.DataBinding.Location = "../InternalServices/ServiceGeneral.svc";
    }

    protected void RadAjaxManager1_AjaxRequest(object sender, AjaxRequestEventArgs e)
    {
    }


    protected void grdcraftlist_ItemCreated(object sender, GridItemEventArgs e)
    {
      screen.GridItemCreated(e, "codes/craftlist.aspx", "MainForm", "results", grdcraftlist);
    }

    private void grdcraftlist_ItemDataBound(object sender, Telerik.Web.UI.GridItemEventArgs e)
    {
        if (e.Item is GridDataItem && !e.Item.IsInEditMode)
        {
            //if (Session["UserGroup"].ToString().ToLower() == "admin")
            //{
            GridDataItem item = (GridDataItem)e.Item;
            ImageButton btn = (ImageButton)item["EditCommand"].Controls[0];
            btn.ImageUrl = "~/Images/Edit.gif";
            btn.OnClientClick = "return EditCraft('" + item.ItemIndex.ToString() + "')";
            //}
        }
        screen.GridItemDataBound(e, "codes/craftlist.aspx", "MainForm", "craftlist");        
    }

    private void RetrieveMessage()
    {
        SystemMessage msg = new SystemMessage("codes/craftlist.aspx");
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