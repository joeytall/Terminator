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

public partial class Codes_Failurecodelist : System.Web.UI.Page
{
    protected AzzierScreen screen;
    protected RadGrid grdfailurecodelist;

    protected string mode = "";
    protected string runtimefilter = "";
    protected string designtimefilter = "";
    protected string tablename = "FailureCode";
    protected string fieldlist = "";
    protected string referer = "";
    protected bool found = false;
    protected bool allowedit = true;
    protected string wherestr = "";
    protected string failurelevel = "";
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
            if (Request.QueryString["filename"].ToLower() == "codes/failurecodemain.aspx")
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
        failurelevel = v.GetFilterValue(filterstr, "failurelevel");

        screen = new AzzierScreen("codes/failurecodelist.aspx", "MainForm", MainControlsPanel.Controls);

        //string eqhierarchy = Application["EQHierarchy"].ToString();
        string connstring = Application["ConnString"].ToString();

        FailurecodeListSqlDataSource.ConnectionString = AzzierData.connectString;

        if (wherestr == "")
            FailurecodeListSqlDataSource.SelectCommand = "Select * From " + tablename + " Order By FailureCode";
        else
        {
            FailurecodeListSqlDataSource.SelectCommand = "Select * From " + tablename + " " + wherestr + " Order By FailureCode";
        }

        grdfailurecodelist = new RadGrid();
        grdfailurecodelist.ID = "grdfailurecodelist";
        grdfailurecodelist.ClientSettings.Scrolling.AllowScroll = true;
        grdfailurecodelist.ClientSettings.Scrolling.ScrollHeight = 300;
        grdfailurecodelist.ClientSettings.Scrolling.SaveScrollPosition = true;
        grdfailurecodelist.ClientSettings.Scrolling.UseStaticHeaders = true;
        grdfailurecodelist.ClientSettings.EnableRowHoverStyle = true;
        grdfailurecodelist.MasterTableView.TableLayout = GridTableLayout.Fixed;
        grdfailurecodelist.PagerStyle.Visible = true;// false;
        grdfailurecodelist.PagerStyle.AlwaysVisible = true;
        grdfailurecodelist.Skin = "Outlook";

        grdfailurecodelist.Attributes.Add("rules", "all");
        //grdfailurecodelist.DataSourceID = "FailurecodeListSqlDataSource";
        grdfailurecodelist.AutoGenerateColumns = false;
        grdfailurecodelist.AllowPaging = true;
        grdfailurecodelist.PageSize = 100;
        grdfailurecodelist.AllowSorting = true;
        grdfailurecodelist.MasterTableView.AllowMultiColumnSorting = true;
        grdfailurecodelist.AllowFilteringByColumn = true;
        grdfailurecodelist.MasterTableView.CommandItemDisplay = GridCommandItemDisplay.Top;
        grdfailurecodelist.MasterTableView.DataKeyNames = new string[] { "Counter" };
        grdfailurecodelist.MasterTableView.ClientDataKeyNames = new string[] { "Counter" };

        grdfailurecodelist.ClientSettings.Selecting.AllowRowSelect = true;
        grdfailurecodelist.ClientSettings.ClientEvents.OnRowSelected = "getGridSelectedItems";

        //if (Session["UserGroup"].ToString() == "Admin" && allowedit)
        if ( allowedit)
        {
            GridEditCommandColumn EditColumn = new GridEditCommandColumn();
            EditColumn.HeaderText = "Edit";
            EditColumn.UniqueName = "EditCommand";
            EditColumn.ButtonType = GridButtonColumnType.ImageButton;

            EditColumn.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
            EditColumn.HeaderStyle.Width = 30;
            grdfailurecodelist.MasterTableView.Columns.Add(EditColumn);
        }

        screen.SetGridColumns("failurecodelist", grdfailurecodelist);

        //if (Session["UserGroup"].ToString() == "Admin" && allowedit)
        if (checkUserRight("Delete", drRights) && allowedit)
        {
            GridButtonColumn DeleteColumn = new GridButtonColumn();
            DeleteColumn.HeaderText = "Delete";
            DeleteColumn.UniqueName = "DeleteButton";
            DeleteColumn.CommandName = "Delete";
            DeleteColumn.ButtonType = GridButtonColumnType.ImageButton;
            DeleteColumn.ImageUrl = "~/Images2/Delete.gif";
            DeleteColumn.Text = "Delete";
            DeleteColumn.ConfirmText = m_msg["T3"];
            DeleteColumn.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
            DeleteColumn.HeaderStyle.Width = 30;
            grdfailurecodelist.MasterTableView.Columns.Add(DeleteColumn);
        }

        if (checkUserRight("AddNew", drRights) && allowedit)
        {
           grdfailurecodelist.MasterTableView.CommandItemTemplate = new InsertFormItemTemplate("Failurecode Detail", null, "return EditFailurecode('','')", 1, Session["UserGroup"].ToString());
        }
        else
        {
            grdfailurecodelist.MasterTableView.CommandItemTemplate = new CodesCommandItem("Failurecode Detail", 0);
        }

        grdfailurecodelist.ItemCreated += new GridItemEventHandler(grdfailurecodelist_ItemCreated);
        grdfailurecodelist.DeleteCommand += new GridCommandEventHandler(grdfailurecodelist_DeleteCommand);
        //grdfailurecodelist.InsertCommand += new GridCommandEventHandler(grdfailurecodelist_InsertCommand);
        //grdfailurecodelist.UpdateCommand += new GridCommandEventHandler(grdfailurecodelist_UpdateCommand);
        grdfailurecodelist.ItemDataBound += new GridItemEventHandler(grdfailurecodelist_ItemDataBound);
        //grdfailurecodelist.PreRender += new EventHandler(grdfailurecodelist_PreRender);
        //grdfailurecodelist.ItemEvent += new GridItemEventHandler(grdfailurecodelist_ItemEvent);
        grdfailurecodelist.ClientSettings.DataBinding.FilterParameterType = GridClientDataBindingParameterType.Linq;
        grdfailurecodelist.ClientSettings.DataBinding.SortParameterType = GridClientDataBindingParameterType.Linq; 
        MainControlsPanel.Controls.Add(grdfailurecodelist);

        screen.LoadScreen();
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        hidFieldId.Value = fieldid;
        hidControlId.Value = AzzierData.ActualFieldName("failurecode",controlid);
        //Page.ClientScript.RegisterClientScriptBlock(Page.GetType(), "alertMessage", "alert('" + fieldid + ":" + controlid + "')", true);
        grdfailurecodelist.ClientSettings.DataBinding.SelectMethod = "GetFailureCodeList?wherestring=" + wherestr;
        grdfailurecodelist.ClientSettings.DataBinding.Location = "../InternalServices/ServiceWO.svc";
    }

    protected void grdfailurecodelist_ItemEvent(object sender, GridItemEventArgs e)
    {
        //if (e.EventInfo is GridInitializePagerItem)
        //{
        //    int rowCount = (e.EventInfo as GridInitializePagerItem).PagingManager.DataSourceCount;
        //    string countstr = grdfailurecodelist.MasterTableView.Items.Count.ToString();
        //    totalCount = "Failure code " + countstr + "/" + rowCount.ToString();
        //}
    }

    protected void grdfailurecodelist_ItemCreated(object sender, GridItemEventArgs e)
    {
      screen.GridItemCreated(e, "codes/failurecodelist.aspx", "MainForm", "results", grdfailurecodelist);
      /*
        if (e.Item is GridCommandItem)
        {
            if (Session["UserGroup"].ToString() != "Admin")
                e.Item.FindControl("InitInsertButton").Visible = false;
        }
       * */
    }

    protected void grdfailurecodelist_DeleteCommand(object source, Telerik.Web.UI.GridCommandEventArgs e)
    {
        GridDataItem item = (GridDataItem)e.Item;
        string counter = item.OwnerTableView.DataKeyValues[item.ItemIndex]["counter"].ToString();

        ModuleoObject obj = new ModuleoObject(Session["Login"].ToString(), tablename, "Counter", counter);
        bool success = obj.Delete();
        if (!success)
        {
            grdfailurecodelist.Controls.Add(new LiteralControl(m_msg["T2"] + obj.ErrorMessage));
            e.Canceled = true;
        }
    }

    protected void grdfailurecodelist_ItemDataBound(object sender, GridItemEventArgs e)
    {
        if (e.Item is GridDataItem && !e.Item.IsInEditMode && allowedit )
        {
            GridDataItem item = (GridDataItem)e.Item;
            ImageButton btn = (ImageButton)item["EditCommand"].Controls[0];
            btn.ImageUrl = "~/Images/Edit.gif";
            btn.OnClientClick = "return EditFailurecode('" + item.ItemIndex.ToString() + "',0)";
        }

        screen.GridItemDataBound(e, "codes/failurecodelist.aspx", "MainForm", "failurecodelist");
    }

    protected void grdfailurecodelist_PreRender(object sender, EventArgs e)
    {
        //GridItem commandItem = grdfailurecodelist.MasterTableView.GetItems(GridItemType.CommandItem)[0];
        //Label lbl = (Label)commandItem.FindControl("lbltitle");
        //lbl.Text = totalCount;
    }

    protected void RadAjaxManager1_AjaxRequest(object sender, AjaxRequestEventArgs e)
    {
        grdfailurecodelist.PageSize = 100 + grdfailurecodelist.PageSize;
        grdfailurecodelist.Rebind();
    }

    private void RetrieveMessage()
    {
        SystemMessage msg = new SystemMessage("codes/failurecodelist.aspx");
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