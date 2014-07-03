using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Web.UI;
using System.Collections.Specialized;

public partial class ManufacturerList : System.Web.UI.Page
{
    protected AzzierScreen screen;
    protected RadGrid grdmanufacturerlist;

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
        if (Session["Login"] == null)
        {
            Response.Write("<script>alert('" + m_msg["T1"] + "');top.document.location.href='../Login.aspx';</script>");
            Response.End();
        }

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

        wherestr = v.AddLinqConditions(filterstr, filename, controlid, "manufacturer",null,null,mode);

        screen = new AzzierScreen("codes/manufacturerlist.aspx", "MainForm", MainControlsPanel.Controls);

        string connstring = Application["ConnString"].ToString();
        ManufacturerlistSqlDataSource.ConnectionString = connstring;
        if (wherestr == "")
            ManufacturerlistSqlDataSource.SelectCommand = "Select * From manufacturer Order By companycode";
        else
            ManufacturerlistSqlDataSource.SelectCommand = "Select * From manufacturer " + wherestr + " Order By companycode";

        grdmanufacturerlist = new RadGrid();
        grdmanufacturerlist.ID = "grdmanufacturerlist";
        grdmanufacturerlist.ClientSettings.Scrolling.AllowScroll = true;
        grdmanufacturerlist.ClientSettings.Scrolling.ScrollHeight = 300;
        grdmanufacturerlist.ClientSettings.Scrolling.SaveScrollPosition = true;
        grdmanufacturerlist.ClientSettings.Scrolling.UseStaticHeaders = true;
        grdmanufacturerlist.ClientSettings.EnableRowHoverStyle = true;
        grdmanufacturerlist.MasterTableView.TableLayout = GridTableLayout.Fixed;
        grdmanufacturerlist.PagerStyle.Visible = true;// false;
        grdmanufacturerlist.PagerStyle.AlwaysVisible = true;
        grdmanufacturerlist.Skin = "Outlook";

        grdmanufacturerlist.Attributes.Add("rules", "all");
        //grdmanufacturerlist.DataSourceID = "ManufacturerlistSqlDataSource";
        grdmanufacturerlist.AutoGenerateColumns = false;
        grdmanufacturerlist.AllowPaging = true;
        grdmanufacturerlist.PageSize = 100;
        grdmanufacturerlist.AllowSorting = true;
        grdmanufacturerlist.MasterTableView.AllowMultiColumnSorting = true;
        grdmanufacturerlist.AllowFilteringByColumn = true;
        grdmanufacturerlist.MasterTableView.CommandItemDisplay = GridCommandItemDisplay.Top;
        grdmanufacturerlist.MasterTableView.DataKeyNames = new string[] { "CompanyCode" };
        grdmanufacturerlist.MasterTableView.ClientDataKeyNames = new string[] { "CompanyCode" };

        grdmanufacturerlist.ClientSettings.Selecting.AllowRowSelect = true;
        grdmanufacturerlist.ClientSettings.ClientEvents.OnRowSelected = "getGridSelectedItems";
        grdmanufacturerlist.ClientSettings.Scrolling.FrozenColumnsCount = 1;

        //if (Session["UserGroup"].ToString().ToLower() == "admin")
        //{
        GridEditCommandColumn EditColumn = new GridEditCommandColumn();
        EditColumn.HeaderText = "Edit";
        EditColumn.UniqueName = "EditCommand";
        EditColumn.ButtonType = GridButtonColumnType.ImageButton;
        EditColumn.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
        EditColumn.HeaderStyle.Width = 30;

        grdmanufacturerlist.MasterTableView.Columns.Add(EditColumn);
        //}

        screen.SetGridColumns("manufacturerlist", grdmanufacturerlist);


        grdmanufacturerlist.ItemCreated += new GridItemEventHandler(grdmanufacturerlist_ItemCreated);
        grdmanufacturerlist.ItemDataBound += new GridItemEventHandler(grdmanufacturerlist_ItemDataBound);
        grdmanufacturerlist.ItemCreated += new GridItemEventHandler(grdmanufacturerlist_ItemCreated);
        grdmanufacturerlist.ClientSettings.DataBinding.FilterParameterType = GridClientDataBindingParameterType.Linq;
        grdmanufacturerlist.ClientSettings.DataBinding.SortParameterType = GridClientDataBindingParameterType.Linq; 

        if (checkUserRight("AddNew", drRights))
        {
            grdmanufacturerlist.MasterTableView.CommandItemTemplate = new InsertFormItemTemplate("manufacturer ", null, "return Editmanufacturer('','')", 1, Session["UserGroup"].ToString());
        }
        else
        {
            grdmanufacturerlist.MasterTableView.CommandItemTemplate = new CodesCommandItem("manufacturer ", 0);
        }

        MainControlsPanel.Controls.Add(grdmanufacturerlist);

        screen.LoadScreen();
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        hidFieldId.Value = fieldid;
        hidControlId.Value = AzzierData.ActualFieldName("manufacturer",controlid);
        grdmanufacturerlist.ClientSettings.DataBinding.SelectMethod = "GetManufacturerList?wherestring=" + wherestr;
        grdmanufacturerlist.ClientSettings.DataBinding.Location = "../InternalServices/ServiceEqpt.svc";
        //CalRadwinSize();
    }

    

    protected void grdmanufacturerlist_ItemCreated(object sender, GridItemEventArgs e)
    {
      screen.GridItemCreated(e, "codes/manufacturelist.aspx", "MainForm", "results", grdmanufacturerlist);
    }

    private void grdmanufacturerlist_ItemDataBound(object sender, Telerik.Web.UI.GridItemEventArgs e)
    {
        if (e.Item is GridDataItem && !e.Item.IsInEditMode)
        {
            //if (Session["UserGroup"].ToString().ToLower() == "admin")
            //{
            GridDataItem item = (GridDataItem)e.Item;
            ImageButton btn = (ImageButton)item["EditCommand"].Controls[0];
            btn.ImageUrl = "~/Images/Edit.gif";
            btn.OnClientClick = "return Editmanufacturer('" + item.ItemIndex.ToString() + "','')";
            //}
        }

        screen.GridItemDataBound(e, "codes/manufacturerlist.aspx", "MainForm", "manufacturerlist");
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