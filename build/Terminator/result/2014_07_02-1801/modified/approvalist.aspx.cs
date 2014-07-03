using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Web.UI;
using System.Collections.Specialized;

public partial class Codes_Approvalist : System.Web.UI.Page
{
    protected AzzierScreen screen;
    protected RadGrid grdapprovalist;

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

        wherestr = v.AddLinqConditions(filterstr, filename, controlid, "approve",null,null,mode);

        screen = new AzzierScreen("codes/approvalist.aspx", "MainForm", MainControlsPanel.Controls);

        string connstring = Application["ConnString"].ToString();
        ApprovalListSqlDataSource.ConnectionString = connstring;
        //if (wherestr == "")
        ApprovalListSqlDataSource.SelectCommand = "Select * From approve where module='workorder' Order By amount";
        //else
        //   ApprovalListSqlDataSource.SelectCommand = "Select * From approve " + wherestr + " Order By counter";

        if (referer == "PO")
            ApprovalListSqlDataSource.SelectCommand = "Select * From approve where module='purchase' Order By amount";
        if (referer == "PROJ")
            ApprovalListSqlDataSource.SelectCommand = "Select * From approve where module='project' Order By amount";

        grdapprovalist = new RadGrid();
        grdapprovalist.ID = "grdapprovalist";
        grdapprovalist.ClientSettings.Scrolling.AllowScroll = true;
        grdapprovalist.ClientSettings.Scrolling.ScrollHeight = 300;
        grdapprovalist.ClientSettings.Scrolling.SaveScrollPosition = true;
        grdapprovalist.ClientSettings.Scrolling.UseStaticHeaders = true;
        grdapprovalist.ClientSettings.EnableRowHoverStyle = true;
        grdapprovalist.MasterTableView.TableLayout = GridTableLayout.Fixed;
        grdapprovalist.PagerStyle.Visible = true;// false;
        grdapprovalist.PagerStyle.AlwaysVisible = true;
        grdapprovalist.Skin = "Outlook";

        grdapprovalist.Attributes.Add("rules", "all");
        //grdapprovalist.DataSourceID = "ApprovalListSqlDataSource";
        grdapprovalist.AutoGenerateColumns = false;
        grdapprovalist.AllowPaging = true;
        grdapprovalist.PageSize = 100;
        grdapprovalist.AllowSorting = true;
        grdapprovalist.MasterTableView.AllowMultiColumnSorting = true;
        grdapprovalist.AllowFilteringByColumn = true;
        grdapprovalist.MasterTableView.CommandItemDisplay = GridCommandItemDisplay.Top;
        grdapprovalist.MasterTableView.DataKeyNames = new string[] { "Counter" };
        grdapprovalist.MasterTableView.ClientDataKeyNames = new string[] { "Counter" };


        grdapprovalist.ClientSettings.Selecting.AllowRowSelect = true;
        //grdapprovalist.ClientSettings.ClientEvents.OnRowSelected = "getGridSelectedItems";
        grdapprovalist.ClientSettings.Scrolling.FrozenColumnsCount = 1;

        
            GridEditCommandColumn EditColumn = new GridEditCommandColumn();
            EditColumn.HeaderText = "Edit";
            EditColumn.UniqueName = "EditCommand";
            EditColumn.ButtonType = GridButtonColumnType.ImageButton;

            EditColumn.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
            EditColumn.HeaderStyle.Width = 30;
            grdapprovalist.MasterTableView.Columns.Add(EditColumn);
            grdapprovalist.MasterTableView.EditMode = GridEditMode.InPlace;
        

        screen.SetGridColumns("approvalist", grdapprovalist);

            GridButtonColumn DeleteColumn = new GridButtonColumn();
            DeleteColumn.HeaderText = "Delete";
            DeleteColumn.UniqueName = "DeleteButton";
            DeleteColumn.CommandName = "Delete";
            DeleteColumn.ButtonType = GridButtonColumnType.ImageButton;
            DeleteColumn.ImageUrl = "~/Images2/Delete.gif";
            DeleteColumn.Text = "Delete";
            DeleteColumn.ConfirmText = m_msg["T2"];
            DeleteColumn.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
            DeleteColumn.HeaderStyle.Width = 30;

            grdapprovalist.MasterTableView.Columns.Add(DeleteColumn);

        grdapprovalist.DeleteCommand += new GridCommandEventHandler(grdapprovalist_DeleteCommand);
        grdapprovalist.ItemDataBound += new GridItemEventHandler(grdapprovalist_ItemDataBound);

            grdapprovalist.MasterTableView.CommandItemTemplate = new InsertFormItemTemplate("Approve ", null, "return EditApprove('','"+referer+"')", 1, Session["UserGroup"].ToString());

//            grdapprovalist.MasterTableView.CommandItemTemplate = new CodesCommandItem("Approve ", 0);

            grdapprovalist.ClientSettings.DataBinding.FilterParameterType = GridClientDataBindingParameterType.Linq;
            grdapprovalist.ClientSettings.DataBinding.SortParameterType = GridClientDataBindingParameterType.Linq;
        MainControlsPanel.Controls.Add(grdapprovalist);

        screen.LoadScreen();
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        hidFieldId.Value = fieldid;
        hidControlId.Value = controlid;
        grdapprovalist.ClientSettings.DataBinding.SelectMethod = "GetApproveList?where=" + wherestr;
        grdapprovalist.ClientSettings.DataBinding.Location = "../InternalServices/ServiceGeneral.svc";
    }

    protected void grdapprovalist_DeleteCommand(object source, Telerik.Web.UI.GridCommandEventArgs e)
    {
        GridDataItem item = (GridDataItem)e.Item;
        string tmp_key = item.OwnerTableView.DataKeyValues[item.ItemIndex]["counter"].ToString();

        ModuleoObject obj = new ModuleoObject(Session["Login"].ToString(), "approve", "counter", tmp_key);
        bool success = obj.Delete();
        if (!success)
        {
            grdapprovalist.Controls.Add(new LiteralControl(m_msg["T3"] + obj.ErrorMessage));
            e.Canceled = true;
        }
    }

    private void grdapprovalist_ItemDataBound(object sender, Telerik.Web.UI.GridItemEventArgs e)
    {
        if (e.Item is GridDataItem && !e.Item.IsInEditMode)
        {
              GridDataItem item = (GridDataItem)e.Item;
              ImageButton btn = (ImageButton)item["EditCommand"].Controls[0];
              btn.ImageUrl = "~/Images/Edit.gif";
              btn.OnClientClick = "return EditApprove('" + item.ItemIndex.ToString() + "','"+referer+"')";
        }
        screen.GridItemDataBound(e, "codes/approvalist.aspx", "MainForm", "approvalist");
    }

    private void RetrieveMessage()
    {
        SystemMessage msg = new SystemMessage("codes/craftlist.aspx");
        m_msg = msg.GetSystemMessage();
        msg.SetJsMessage(litMessage);
    }
}