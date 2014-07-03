using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Web.UI;
using System.Collections.Specialized;

public partial class LaborTypelist : System.Web.UI.Page
{
    protected AzzierScreen screen;
    protected RadGrid grdlabtypelist;

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

        wherestr = v.AddLinqConditions(filterstr, filename, controlid, "labortype",null,null,mode);

        screen = new AzzierScreen("codes/labtypelist.aspx", "MainForm", MainControlsPanel.Controls);

        string connstring = Application["ConnString"].ToString();
        LabTypeListSqlDataSource.ConnectionString = connstring;
        if (wherestr == "")
            LabTypeListSqlDataSource.SelectCommand = "Select * From labortype Order By labtype";
        else
            LabTypeListSqlDataSource.SelectCommand = "Select * From labortype " + wherestr + " Order By labtype";

        grdlabtypelist = new RadGrid();
        grdlabtypelist.ID = "grdlabtypelist";
        grdlabtypelist.ClientSettings.Scrolling.AllowScroll = true;
        grdlabtypelist.ClientSettings.Scrolling.ScrollHeight = 300;
        grdlabtypelist.ClientSettings.Scrolling.SaveScrollPosition = true;
        grdlabtypelist.ClientSettings.Scrolling.UseStaticHeaders = true;
        grdlabtypelist.ClientSettings.EnableRowHoverStyle = true;
        grdlabtypelist.MasterTableView.TableLayout = GridTableLayout.Fixed;
        grdlabtypelist.PagerStyle.Visible = true; // false;
        grdlabtypelist.PagerStyle.AlwaysVisible = true;
        grdlabtypelist.Skin = "Outlook";

        grdlabtypelist.Attributes.Add("rules", "all");
        //grdlabtypelist.DataSourceID = "LabTypeListSqlDataSource";
        grdlabtypelist.AutoGenerateColumns = false;
        grdlabtypelist.AllowPaging = true;
        grdlabtypelist.PageSize = 100;
        grdlabtypelist.AllowSorting = true;
        grdlabtypelist.MasterTableView.AllowMultiColumnSorting = true;
        grdlabtypelist.AllowFilteringByColumn = true;
        grdlabtypelist.MasterTableView.CommandItemDisplay = GridCommandItemDisplay.Top;
        grdlabtypelist.MasterTableView.DataKeyNames = new string[] { "LabType" };
        grdlabtypelist.MasterTableView.ClientDataKeyNames = new string[] { "LabType" };

        grdlabtypelist.ClientSettings.Selecting.AllowRowSelect = true;
        grdlabtypelist.ClientSettings.ClientEvents.OnRowSelected = "getGridSelectedItems";
        grdlabtypelist.ClientSettings.Scrolling.FrozenColumnsCount = 1;

        //if (Session["UserGroup"].ToString().ToLower() == "admin")
        //{
        GridEditCommandColumn EditColumn = new GridEditCommandColumn();
        EditColumn.HeaderText = "Edit";
        EditColumn.UniqueName = "EditCommand";
        EditColumn.ButtonType = GridButtonColumnType.ImageButton;

        EditColumn.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
        EditColumn.HeaderStyle.Width = 30;
        grdlabtypelist.MasterTableView.Columns.Add(EditColumn);
        grdlabtypelist.MasterTableView.EditMode = GridEditMode.InPlace;
        //}

        screen.SetGridColumns("labtypelist", grdlabtypelist);

        grdlabtypelist.ItemCreated += new GridItemEventHandler(grdlabtypelist_ItemCreated);
        grdlabtypelist.ItemDataBound += new GridItemEventHandler(grdlabtypelist_ItemDataBound);
        grdlabtypelist.ClientSettings.DataBinding.FilterParameterType = GridClientDataBindingParameterType.Linq;
        grdlabtypelist.ClientSettings.DataBinding.SortParameterType = GridClientDataBindingParameterType.Linq; 

        if (checkUserRight("AddNew", drRights))
        {
            grdlabtypelist.MasterTableView.CommandItemTemplate = new InsertFormItemTemplate("Labor Type ", null, "return EditLaborType('','')", 1, Session["UserGroup"].ToString());
        }
        else
        {
            grdlabtypelist.MasterTableView.CommandItemTemplate = new CodesCommandItem("Labor Type ", 0);
        }

        MainControlsPanel.Controls.Add(grdlabtypelist);

        screen.LoadScreen();
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        hidFieldId.Value = fieldid;
        hidControlId.Value = AzzierData.ActualFieldName("labortype", controlid);
        grdlabtypelist.ClientSettings.DataBinding.SelectMethod = "GetLaborTypeList?wherestring=" + wherestr;
        grdlabtypelist.ClientSettings.DataBinding.Location = "../InternalServices/ServiceLabour.svc";
    }

    protected void RadAjaxManager1_AjaxRequest(object sender, AjaxRequestEventArgs e)
    {
    }

    protected void grdlabtypelist_ItemCreated(object sender, GridItemEventArgs e)
    {
      screen.GridItemCreated(e, "codes/labtypelist.aspx", "MainForm", "labtypelist", grdlabtypelist);
    }

    protected void grdlabtypelist_DeleteCommand(object source, Telerik.Web.UI.GridCommandEventArgs e)
    {
        GridDataItem item = (GridDataItem)e.Item;
        string tmp_craft = item.OwnerTableView.DataKeyValues[item.ItemIndex]["labtype"].ToString();

        ModuleoObject obj = new ModuleoObject(Session["Login"].ToString(), "labortype", "labtype", tmp_craft);
        bool success = obj.Delete();
        if (!success)
        {
            grdlabtypelist.Controls.Add(new LiteralControl(m_msg["T3"] + obj.ErrorMessage));
            e.Canceled = true;
        }
    }

    private void grdlabtypelist_ItemDataBound(object sender, Telerik.Web.UI.GridItemEventArgs e)
    {
        if (e.Item is GridDataItem && !e.Item.IsInEditMode)
        {
            //if (Session["UserGroup"].ToString().ToLower() == "admin")
            //{
            GridDataItem item = (GridDataItem)e.Item;
            ImageButton btn = (ImageButton)item["EditCommand"].Controls[0];
            btn.ImageUrl = "~/Images/Edit.gif";
            btn.OnClientClick = "return EditLaborType('" + item.ItemIndex.ToString() + "','')";
            //}
        }

        screen.GridItemDataBound(e, "codes/labtypelist.aspx", "MainForm", "labtypelist");
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