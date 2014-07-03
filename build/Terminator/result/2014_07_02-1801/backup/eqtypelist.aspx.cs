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

public partial class Codes_eqtypelist : System.Web.UI.Page
{
    protected AzzierScreen screen;
    protected RadGrid grdeqtypelist;

    protected string mode = "";
    protected string runtimefilter = "";
    protected string designtimefilter = "";
    protected string tablename = "eqtype";
    protected string fieldlist = "";
    protected string referer = "";
    protected bool found = false;
    protected bool allowedit = true;
    protected string wherestr = "";
    protected string eqlevel = "";
    protected string filename = "";


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
            if (Request.QueryString["filename"].ToLower() == "codes/eqtypemain.aspx")
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
        eqlevel = v.GetFilterValue(filterstr, "eqlevel");
        //Application["EQHierarchy"]

        screen = new AzzierScreen("codes/eqtypelist.aspx", "MainForm", MainControlsPanel.Controls);

        grdeqtypelist = new RadGrid();
        grdeqtypelist.ID = "grdeqtypelist";
        grdeqtypelist.ClientSettings.Scrolling.AllowScroll = true;
        grdeqtypelist.ClientSettings.Scrolling.ScrollHeight = 300;
        grdeqtypelist.ClientSettings.Scrolling.SaveScrollPosition = true;
        grdeqtypelist.ClientSettings.Scrolling.UseStaticHeaders = true;
        grdeqtypelist.ClientSettings.EnableRowHoverStyle = true;
        grdeqtypelist.MasterTableView.TableLayout = GridTableLayout.Fixed;
        grdeqtypelist.PagerStyle.Visible = true;// false;
        grdeqtypelist.PagerStyle.AlwaysVisible = true;
        grdeqtypelist.Skin = "Outlook";

        grdeqtypelist.Attributes.Add("rules", "all");
        //grdeqtypelist.DataSourceID = "EQTypeListSqlDataSource";
        grdeqtypelist.AutoGenerateColumns = false;
        grdeqtypelist.AllowPaging = true;
        grdeqtypelist.PageSize = 100;
        grdeqtypelist.AllowSorting = true;
        grdeqtypelist.MasterTableView.AllowMultiColumnSorting = true;
        grdeqtypelist.AllowFilteringByColumn = true;
        grdeqtypelist.MasterTableView.CommandItemDisplay = GridCommandItemDisplay.Top;
        grdeqtypelist.MasterTableView.DataKeyNames = new string[] { "Counter" };
        grdeqtypelist.MasterTableView.ClientDataKeyNames = new string[] { "Counter" };

        grdeqtypelist.ClientSettings.Selecting.AllowRowSelect = true;
        grdeqtypelist.ClientSettings.ClientEvents.OnRowSelected = "getGridSelectedItems";

        //if (Session["UserGroup"].ToString() == "Admin" && allowedit)
        if (allowedit)
        {
            GridEditCommandColumn EditColumn = new GridEditCommandColumn();
            EditColumn.HeaderText = "Edit";
            EditColumn.UniqueName = "EditCommand";
            EditColumn.ButtonType = GridButtonColumnType.ImageButton;

            EditColumn.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
            EditColumn.HeaderStyle.Width = 30;
            grdeqtypelist.MasterTableView.Columns.Add(EditColumn);
        }

        screen.SetGridColumns("eqtypelist", grdeqtypelist);

        if (checkUserRight("AddNew",drRights) && allowedit)
        {
            grdeqtypelist.MasterTableView.CommandItemTemplate = new InsertFormItemTemplate("EQType Detail", null, "return EditEQType('','" + eqlevel + "')", 1,Session["UserGroup"].ToString());
        }
        else
        {
            grdeqtypelist.MasterTableView.CommandItemTemplate = new CodesCommandItem("EQType Detail", 0);
        }

        grdeqtypelist.ItemCreated += new GridItemEventHandler(grdeqtypelist_ItemCreated);
        //grdeqtypelist.DeleteCommand += new GridCommandEventHandler(grdeqtypelist_DeleteCommand);
        grdeqtypelist.ItemDataBound += new GridItemEventHandler(grdeqtypelist_ItemDataBound);
        grdeqtypelist.ClientSettings.DataBinding.FilterParameterType = GridClientDataBindingParameterType.Linq;
        grdeqtypelist.ClientSettings.DataBinding.SortParameterType = GridClientDataBindingParameterType.Linq; 
        MainControlsPanel.Controls.Add(grdeqtypelist);

        screen.LoadScreen();
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        hidFieldId.Value = fieldid;
        hidControlId.Value = AzzierData.ActualFieldName("eqtype",controlid);
        grdeqtypelist.ClientSettings.DataBinding.SelectMethod = "GetEqTypeList?wherestring=" + wherestr;
        grdeqtypelist.ClientSettings.DataBinding.Location = "../InternalServices/ServiceEqpt.svc";
    }


    protected void grdeqtypelist_ItemCreated(object sender, GridItemEventArgs e)
    {
      screen.GridItemCreated(e, "codes/eqtypelist.aspx", "MainForm", "results", grdeqtypelist);
    }

    protected void grdeqtypelist_DeleteCommand(object source, Telerik.Web.UI.GridCommandEventArgs e)
    {
        GridDataItem item = (GridDataItem)e.Item;
        string counter = item.OwnerTableView.DataKeyValues[item.ItemIndex]["counter"].ToString();

        ModuleoObject obj = new ModuleoObject(Session["Login"].ToString(), tablename, "Counter", counter);
        bool success = obj.Delete();
        if (!success)
        {
            grdeqtypelist.Controls.Add(new LiteralControl(m_msg["T2"] + obj.ErrorMessage));
            e.Canceled = true;
        }
    }

    protected void grdeqtypelist_ItemDataBound(object sender, GridItemEventArgs e)
    {
      if (e.Item is GridDataItem && !e.Item.IsInEditMode && allowedit && allowedit )
        {
            GridDataItem item = (GridDataItem)e.Item;
            ImageButton btn = (ImageButton)item["EditCommand"].Controls[0];
            btn.ImageUrl = "~/Images/Edit.gif";
            btn.OnClientClick = "return EditEQType('" + item.ItemIndex.ToString() + "'," + eqlevel + ")";
        }

        screen.GridItemDataBound(e, "codes/eqtypelist.aspx", "MainForm", "eqtypelist");
    }


    protected void RadAjaxManager1_AjaxRequest(object sender, AjaxRequestEventArgs e)
    {
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