using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.OleDb;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Web.UI;
using System.Text;
using System.Linq;

public partial class Codes_Districtslist : System.Web.UI.Page
{
    protected AzzierScreen screen;
    protected RadGrid grddistrictlist;

    protected string mode = "";
    protected string runtimefilter = "";
    protected string designtimefilter = "";
    protected string tablename = "districts";
    protected string fieldlist = "";
    protected string referer = "";
    protected bool found = false;
    protected bool allowedit = true;
    protected string wherestr = "";
    protected string eqlevel = "";
    protected string filename = "";
    protected string districtcode = "";

    protected string controlid = "";
    protected string fieldid = "";
    protected string totalCount = "";
    private NameValueCollection m_msg = new NameValueCollection();

    protected void Page_Init(object sender, EventArgs e)
    {
        RetrieveMessage();
        if (Session["Login"] == null)
        {
            Response.Write("<html><script type=\"text/javascript\">alert('" + m_msg["T1"] + "');top.document.location.href='../login.aspx';</script></html>");
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
        if (Request.QueryString["tablename"] != null)
            tablename = Request.QueryString["tablename"].ToString();
        if (Request.QueryString["filename"] != null)
        {
            filename = Request.QueryString["filename"];
            if (Request.QueryString["filename"].ToLower() == "codes/districtmain.aspx")
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

        wherestr = v.AddLinqConditions(filterstr, filename, controlid, tablename,null,null,mode);
        //eqlevel = v.GetFilterValue(filterstr, "eqlevel");
        districtcode = v.GetFilterValue(filterstr, "districtcode");

        screen = new AzzierScreen("codes/districtlist.aspx", "MainForm", MainControlsPanel.Controls);

        string connstring = Application["ConnString"].ToString();

        DistrictListSqlDataSource.ConnectionString = Application["ConnString"].ToString();

        if (wherestr == "")
            DistrictListSqlDataSource.SelectCommand = "Select * From " + tablename + " Order By district";
        else
        {
            DistrictListSqlDataSource.SelectCommand = "Select * From " + tablename + " " + wherestr + " Order By district";
        }

        grddistrictlist = new RadGrid();
        grddistrictlist.ID = "grddistrictlist";
        grddistrictlist.ClientSettings.Scrolling.AllowScroll = true;
        grddistrictlist.ClientSettings.Scrolling.ScrollHeight = 300;
        grddistrictlist.ClientSettings.Scrolling.SaveScrollPosition = true;
        grddistrictlist.ClientSettings.Scrolling.UseStaticHeaders = true;
        grddistrictlist.ClientSettings.EnableRowHoverStyle = true;
        grddistrictlist.MasterTableView.TableLayout = GridTableLayout.Fixed;
        grddistrictlist.PagerStyle.Visible = true;// false;
        grddistrictlist.PagerStyle.AlwaysVisible = true;

        grddistrictlist.Skin = "Outlook";

        grddistrictlist.Attributes.Add("rules", "all");
        //grddistrictlist.DataSourceID = "DistrictListSqlDataSource";
        grddistrictlist.AutoGenerateColumns = false;
        grddistrictlist.AllowPaging = true;
        grddistrictlist.PageSize = 100;
        grddistrictlist.AllowSorting = true;
        grddistrictlist.MasterTableView.AllowMultiColumnSorting = true;
        grddistrictlist.AllowFilteringByColumn = true;
        grddistrictlist.MasterTableView.CommandItemDisplay = GridCommandItemDisplay.Top;
        grddistrictlist.MasterTableView.DataKeyNames = new string[] { "District" };
        grddistrictlist.MasterTableView.ClientDataKeyNames = new string[] { "District" };

        grddistrictlist.ClientSettings.Selecting.AllowRowSelect = true;
        grddistrictlist.ClientSettings.ClientEvents.OnRowSelected = "getGridSelectedItems";

        if (allowedit)
        {
            GridEditCommandColumn EditColumn = new GridEditCommandColumn();
            EditColumn.HeaderText = "Edit";
            EditColumn.UniqueName = "EditCommand";
            EditColumn.ButtonType = GridButtonColumnType.ImageButton;

            EditColumn.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
            EditColumn.HeaderStyle.Width = 30;
            grddistrictlist.MasterTableView.Columns.Add(EditColumn);
        }

        screen.SetGridColumns("districtlist", grddistrictlist);

        if (checkUserRight("AddNew", drRights) && allowedit)
        {
            grddistrictlist.MasterTableView.CommandItemTemplate = new InsertFormItemTemplate("Districts ", null, "return EditDistrict('','" + districtcode + "')", 1, Session["UserGroup"].ToString());
        }
        else
        {
            grddistrictlist.MasterTableView.CommandItemTemplate = new CodesCommandItem("Districts ", 0);
        }

        grddistrictlist.ItemDataBound += new GridItemEventHandler(grddistrictlist_ItemDataBound);
        grddistrictlist.ItemCreated += new GridItemEventHandler(grddistrictlist_ItemCreated);
        grddistrictlist.ClientSettings.DataBinding.FilterParameterType = GridClientDataBindingParameterType.Linq;
        grddistrictlist.ClientSettings.DataBinding.SortParameterType = GridClientDataBindingParameterType.Linq;
        MainControlsPanel.Controls.Add(grddistrictlist);

        screen.LoadScreen();
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        hidFieldId.Value = fieldid;
        hidControlId.Value = AzzierData.ActualFieldName("districts",controlid);

        grddistrictlist.ClientSettings.DataBinding.SelectMethod = "GetDistrictsList?where=" + wherestr;
        grddistrictlist.ClientSettings.DataBinding.Location = "../InternalServices/ServiceLoc.svc";
    }

    protected void grddistrictlist_ItemCreated(object sender, GridItemEventArgs e)
    {
        screen.GridItemCreated(e, "codes/districtlist.aspx", "MainForm", "results", grddistrictlist);
    }

    protected void grddistrictlist_ItemDataBound(object sender, GridItemEventArgs e)
    {
        if (e.Item is GridDataItem && !e.Item.IsInEditMode)
        {
            if (allowedit)
            {
                GridDataItem item = (GridDataItem)e.Item;
                ImageButton btn = (ImageButton)item["EditCommand"].Controls[0];
                btn.ImageUrl = "~/Images/Edit.gif";
                btn.OnClientClick = "return EditDistrict('" + item.ItemIndex.ToString() + "','" + districtcode + "')";
            }
        }

        screen.GridItemDataBound(e, "codes/districtlist.aspx", "MainForm", "districtlist");
    }

    private void RetrieveMessage()
    {
        SystemMessage msg = new SystemMessage("codes/eqtypelist.aspx");
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