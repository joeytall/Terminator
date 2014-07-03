using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.OleDb;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Web.UI;
using System.Web.UI.HtmlControls;

public partial class inventory_invpanel : System.Web.UI.Page
{
    protected string mode;
    protected RadGrid grdresults;
    protected RadGrid grdworklist;
    protected RadGrid grdrecentlist;
    //private OleDbConnection conn;
    private bool setPanel;
    private bool setworklist;
    protected int resultcount = 0;
    protected int worklistcount = 0;
    protected int recentlistcount = 0;
    protected string selectedresults;
    protected string selectedworklist;
    protected string selectedrecentlist;
    protected string specsearch = "";
    protected NameValueCollection m_msg = new NameValueCollection();
    protected AzzierScreen screen, screen2, screen3;


    protected void Page_Init(object sender, EventArgs e)
    {
        RetrieveMessage();
        if (Session["Login"] == null)
        {
            //Response.Write("<script>alert('Your session has expired. Please login again.');top.document.location.href='../Login.aspx';</script>");
            Response.Write("<script>alert('" + m_msg["T1"] + "');top.document.location.href='../Login.aspx';</script>");
            Response.End();
        }
        Session.LCID = Convert.ToInt32(Session["LCID"]);
        if (Request.QueryString["showlist"] != null)
            mode = Request.QueryString["showlist"].ToString();
        else
            mode = "worklist";

        if (Request.QueryString["specsearch"] != null)
        {
            specsearch = Request.QueryString["specsearch"];
        }


        string wherestring = "";
        if (Request.QueryString["wherestring"] != null)
        {
            wherestring = Request.QueryString["wherestring"].ToString();
            if (wherestring == "")
                wherestring = " 1=1 ";
        }
        else
            wherestring = " 1=2 ";



        grdworklist = new RadGrid();
        grdresults = new RadGrid();
        grdrecentlist = new RadGrid();



        //Result Grid

        grdresults.ID = "grdresults";
        //grdresults.DataSourceID = "ResultsSqlDataSource";

        screen = new AzzierScreen("inventory/invpanel.aspx", "MainForm", MainControlsPanel.Controls, "edit");

        //ResultsSqlDataSource.ConnectionString = connstring;
        //ResultsSqlDataSource.SelectCommand = "SELECT * FROM Employee WHERE " + wherestring;
        grdresults.Attributes.Add("rules", "all");
        //grdresults.DataSourceID = "ResultsSqlDataSource";
        grdresults.AutoGenerateColumns = false;
        grdresults.AllowPaging = true;
        grdresults.PagerStyle.Position = GridPagerPosition.Bottom;
        
        grdresults.PageSize = 100;
        grdresults.AllowSorting = true;
        grdresults.MasterTableView.AllowMultiColumnSorting = true;
        grdresults.AllowFilteringByColumn = true;
        grdresults.MasterTableView.DataKeyNames = new string[] { "ItemNum" };
        grdresults.MasterTableView.CommandItemDisplay = GridCommandItemDisplay.None;
        grdresults.ClientSettings.EnableRowHoverStyle = true;
        grdresults.ClientSettings.Selecting.AllowRowSelect = true;
        grdresults.AllowMultiRowSelection = true;
        grdresults.ClientSettings.Scrolling.SaveScrollPosition = true;
        //grdresults.ClientSettings.ClientEvents.OnScroll = "HandleScrollingResults";
        grdresults.ClientSettings.Selecting.UseClientSelectColumnOnly = true;
        grdresults.ClientSettings.ClientEvents.OnGridCreated = "updatecounts";
        grdresults.PreRender += new EventHandler(grdresults_PreRender);
        grdresults.ItemEvent += new GridItemEventHandler(grdresults_ItemEvent);
        grdresults.PagerStyle.Visible = true;
        grdresults.ClientSettings.ClientEvents.OnRowClick = "rowClicked";
        grdresults.ClientSettings.Scrolling.AllowScroll = true;
        grdresults.SortingSettings.EnableSkinSortStyles = false;
        grdresults.MasterTableView.ClientDataKeyNames = new string[] { "ItemNum" };

        //grdresults.ClientSettings.ClientEvents.OnGridCreated = "GridCreated";
        GridClientSelectColumn cbColumn = new GridClientSelectColumn();
        grdresults.Columns.Add(cbColumn);
        cbColumn.HeaderStyle.Width = 25;
        screen.SetGridColumns("results", grdresults);
        MainControlsPanel.Controls.Add(grdresults);
        grdresults.PagerStyle.AlwaysVisible = true;
        screen.LoadScreen();

        grdresults.Skin = "Outlook";

        if (specsearch == "true")
        {
            grdresults.ClientSettings.DataBinding.SelectMethod = "SpecSearchDatandCount?wherestring=" + wherestring;
            grdresults.ClientSettings.DataBinding.Location = "../InternalServices/ServiceInventory.svc";
            grdresults.ClientSettings.DataBinding.SortParameterType = GridClientDataBindingParameterType.Linq;
            grdresults.ClientSettings.DataBinding.FilterParameterType = GridClientDataBindingParameterType.Linq;
        }
        else
        {
            grdresults.ClientSettings.DataBinding.SelectMethod = "SearchDataAndCount?wherestring=" + wherestring;
            grdresults.ClientSettings.DataBinding.Location = "../InternalServices/ServiceInventory.svc";
            grdresults.ClientSettings.DataBinding.SortParameterType = GridClientDataBindingParameterType.Linq;
            grdresults.ClientSettings.DataBinding.FilterParameterType = GridClientDataBindingParameterType.Linq;
        }

        // WorkList Grid

        grdworklist.ID = "grdworklist";
        //grdworklist.DataSourceID = "WorkListSqlDataSource";

        //WorkListSqlDataSource.ConnectionString = connstring;
        //WorkListSqlDataSource.SelectCommand = "SELECT Empid,FirstName,LastName FROM Employee WHERE Empid In (Select LinkId From UserWorkList Where UserId='" + Session["Login"] + "' And LinkModule='labour' And LinkType='worklist') And Inactive=0";
        grdworklist.Attributes.Add("rules", "all");
        //grdworklist.GridLines = GridLines.Both;
        //grdworklist.MasterTableView.GridLines = GridLines.Both;
        //grdworklist.DataSourceID = "WorkListSqlDataSource";
        grdworklist.AutoGenerateColumns = false;
        grdworklist.AllowPaging = true;
        grdworklist.PagerStyle.Position = GridPagerPosition.Bottom;
        grdworklist.PageSize = 100;
        grdworklist.AllowSorting = true;
        grdworklist.MasterTableView.AllowMultiColumnSorting = true;
        grdworklist.AllowFilteringByColumn = true;
        grdworklist.MasterTableView.DataKeyNames = new string[] { "ItemNum" };
        grdworklist.MasterTableView.CommandItemDisplay = GridCommandItemDisplay.None;
        grdworklist.ClientSettings.EnableRowHoverStyle = true;
        grdworklist.ClientSettings.Selecting.AllowRowSelect = true;
        grdworklist.ClientSettings.Scrolling.SaveScrollPosition = true;
        //grdworklist.ClientSettings.ClientEvents.OnScroll = "HandleScrollingWorkList";
        grdworklist.PreRender += new EventHandler(grdworklist_PreRender);
        grdworklist.ItemEvent += new GridItemEventHandler(grdworklist_ItemEvent);
        //grdworklist.ItemDataBound +=new GridItemEventHandler(grdworklist_ItemDataBound);
        grdworklist.ClientSettings.ClientEvents.OnRowClick = "rowClicked";
        grdworklist.MasterTableView.ClientDataKeyNames = new string[] { "ItemNum" };
        grdworklist.ClientSettings.Scrolling.AllowScroll = true;
        //grdworklist.ItemCommand += new GridCommandEventHandler(grdworklist_ItemCommand);
        grdworklist.Skin = "Outlook";
        grdworklist.ClientSettings.ClientEvents.OnDataBound = "updatecounts";
        grdworklist.SortingSettings.EnableSkinSortStyles = false;
        screen2 = new AzzierScreen("inventory/invpanel.aspx", "MainForm", MainControlsPanel2.Controls);
        grdworklist.ClientSettings.Selecting.AllowRowSelect = true;
        grdworklist.AllowMultiRowSelection = true;

        grdworklist.ClientSettings.Selecting.UseClientSelectColumnOnly = true;
        grdworklist.PagerStyle.Visible = true;// false;
        GridClientSelectColumn cbColumn2 = new GridClientSelectColumn();
        cbColumn2.HeaderStyle.Width = 25;
        grdworklist.Columns.Add(cbColumn2);

        screen2.SetGridColumns("worklist", grdworklist);
        MainControlsPanel2.Controls.Add(grdworklist);
        screen2.LoadScreenWithoutDirtyLog();


        grdworklist.ClientSettings.DataBinding.SelectMethod = "GetWorkList?wherestring=" + wherestring;
        grdworklist.ClientSettings.DataBinding.Location = "../InternalServices/ServiceInventory.svc";
        grdworklist.ClientSettings.DataBinding.SortParameterType = GridClientDataBindingParameterType.Linq;
        grdworklist.ClientSettings.DataBinding.FilterParameterType = GridClientDataBindingParameterType.Linq;



        // Recent list

        grdrecentlist.ID = "grdrecentlist";
        //grdrecentlist.DataSourceID = "RecentListSqlDataSource";

        //RecentListSqlDataSource.ConnectionString = connstring;
        //RecentListSqlDataSource.SelectCommand = "SELECT Empid,FirstName,LastName FROM Employee WHERE Empid In (Select LinkId From UserWorkList Where UserId='" + Session["Login"] + "' And LinkModule='labour' And LinkType='recent') And Inactive=0";
        grdrecentlist.Attributes.Add("rules", "all");
        //grdrecentlist.GridLines = GridLines.Both;
        //grdrecentlist.MasterTableView.GridLines = GridLines.Both;
        //grdrecentlist.DataSourceID = "RecentListSqlDataSource";
        grdrecentlist.AutoGenerateColumns = false;
        grdrecentlist.AllowPaging = true;
        grdrecentlist.PagerStyle.Position = GridPagerPosition.Bottom;
        grdrecentlist.PageSize = 100;
        grdrecentlist.AllowSorting = true;
        grdrecentlist.MasterTableView.AllowMultiColumnSorting = true;
        grdrecentlist.AllowFilteringByColumn = true;
        grdrecentlist.MasterTableView.DataKeyNames = new string[] { "ItemNum" };
        grdrecentlist.MasterTableView.CommandItemDisplay = GridCommandItemDisplay.None;
        grdrecentlist.ClientSettings.EnableRowHoverStyle = true;
        grdrecentlist.ClientSettings.Selecting.AllowRowSelect = true;
        grdrecentlist.ClientSettings.Scrolling.SaveScrollPosition = true;
        //grdrecentlist.ClientSettings.ClientEvents.OnScroll = "HandleScrollingRecentList";
        grdrecentlist.PreRender += new EventHandler(grdrecentlist_PreRender);
        grdrecentlist.ItemEvent += new GridItemEventHandler(grdrecentlist_ItemEvent);
        screen3 = new AzzierScreen("inventory/invpanel.aspx", "MainForm", MainControlsPanel3.Controls);
        grdrecentlist.ClientSettings.Selecting.AllowRowSelect = true;
        grdrecentlist.AllowMultiRowSelection = true;
        grdrecentlist.PagerStyle.Visible = true;// false;
        grdrecentlist.ClientSettings.ClientEvents.OnRowClick = "rowClicked";
        grdrecentlist.ClientSettings.ClientEvents.OnDataBound = "updatecounts";
        grdrecentlist.ClientSettings.Scrolling.AllowScroll = true;
        grdrecentlist.SortingSettings.EnableSkinSortStyles = false;
        grdrecentlist.MasterTableView.ClientDataKeyNames = new string[] { "ItemNum" };


        //grdresults.ClientSettings.ClientEvents.OnRowDblClick = "getGridSelectedItems";
        //grdresults.ClientSettings.ClientEvents.OnRowSelected  = "getGridSelectedItems";
        grdrecentlist.ClientSettings.Selecting.UseClientSelectColumnOnly = true;
        GridClientSelectColumn cbColumnRecentlist = new GridClientSelectColumn();
        cbColumnRecentlist.HeaderStyle.Width = 25;
        grdrecentlist.Columns.Add(cbColumnRecentlist);
        screen3.SetGridColumns("recentlist", grdrecentlist);
        MainControlsPanel3.Controls.Add(grdrecentlist);

        screen3.LoadScreenWithoutDirtyLog();

        grdrecentlist.Skin = "Outlook";

        
        //grdrecentlist.ClientSettings.DataBinding.SelectMethod = "GetRecentList?wherestring=" + wherestring;
        grdrecentlist.ClientSettings.DataBinding.SelectMethod = "RecentListDataAndCount?wherestring=" + wherestring;
        grdrecentlist.ClientSettings.DataBinding.Location = "../InternalServices/ServiceInventory.svc";
        grdrecentlist.ClientSettings.DataBinding.SortParameterType = GridClientDataBindingParameterType.Linq;
        grdrecentlist.ClientSettings.DataBinding.FilterParameterType = GridClientDataBindingParameterType.Linq;



        grdresults.PreRender += new EventHandler(grdresults_PreRender);
        grdworklist.EnableLinqExpressions = false;
        grdrecentlist.EnableLinqExpressions = false;
        grdresults.EnableLinqExpressions = false;


        grdworklist.GroupingSettings.CaseSensitive = false;
        grdresults.GroupingSettings.CaseSensitive = false;
        grdrecentlist.GroupingSettings.CaseSensitive = false;

        grdresults.Style["Width"] = "98%";
        grdresults.Style["Height"] = "98%";
        grdworklist.Style["Width"] = "98%";
        grdworklist.Style["Height"] = "98%";
        grdrecentlist.Style["Width"] = "98%";
        grdrecentlist.Style["Height"] = "98%";
        grdresults.ClientSettings.DataBinding.ShowEmptyRowsOnLoad = false;
        grdworklist.ClientSettings.DataBinding.ShowEmptyRowsOnLoad = false;
        grdrecentlist.ClientSettings.DataBinding.ShowEmptyRowsOnLoad = false;

        grdrecentlist.ItemCreated += new GridItemEventHandler(grdrecentlist_ItemCreated);
        grdresults.ItemCreated += new GridItemEventHandler(grdresults_ItemCreated);
        grdworklist.ItemCreated += new GridItemEventHandler(grdworklist_ItemCreated);
    }

    protected void grdworklist_ItemCreated(object sender, GridItemEventArgs e)
    {
        screen2.GridItemCreated(e, "inventory/invpanel.aspx", "MainForm", "worklist", grdworklist);
    }

    protected void grdresults_ItemCreated(object sender, GridItemEventArgs e)
    {
        screen.GridItemCreated(e, "inventory/invpanel.aspx", "MainForm", "results", grdresults);
    }

    protected void grdrecentlist_ItemCreated(object sender, GridItemEventArgs e)
    {
        screen3.GridItemCreated(e, "inventory/invpanel.aspx", "MainForm", "recentlist", grdrecentlist);
    }


    protected void grdworklist_PreRender(object sender, EventArgs e)
    {
        if ((setPanel == true) && (setworklist == true))
        {
            MainControlsPanel2.Style.Add("visibility", "visible");
            MainControlsPanel2.Style.Add("display", "block");
            MainControlsPanel.Style.Add("visibility", "hidden");
            MainControlsPanel.Style.Add("display", "none");
            MainControlsPanel3.Style.Add("visibility", "hidden");
            MainControlsPanel3.Style.Add("display", "none");
        }
        //        GetEmpids(WorkListSqlDataSource.SelectCommand, "worklist");
        //if (setworklist == true)
        if (hidMode.Value == "worklist")
        {
            lblListType.Text = "Work List";
        }
    }

    protected void grdresults_PreRender(object sender, EventArgs e)
    {
        if (hidMode.Value == "queryresult")
        {
            lblListType.Text =m_msg["T3"];// "Query Result";
        }

    }

    protected void grdrecentlist_PreRender(object sender, EventArgs e)
    {
        if (hidMode.Value == "recentlist")
        {
            lblListType.Text = m_msg["T2"];
        }

    }
    protected void Page_Load(object sender, EventArgs e)
    {
        litScript.Text = "";

        if (!IsPostBack)
        {
            setPanel = false;
            if (mode != "queryresult")
                setworklist = true;
            if (mode == "queryresult")
            {
                MainControlsPanel.Style.Add("visibility", "visible");
                MainControlsPanel2.Style.Add("visibility", "hidden");
                MainControlsPanel.Style.Add("display", "block");
                MainControlsPanel2.Style.Add("display", "none");
                MainControlsPanel3.Style.Add("visibility", "hidden");
                MainControlsPanel3.Style.Add("display", "none");
                lblListType.Text = m_msg["T3"];
              /*
                if (Request.QueryString["specsearch"] != null)
                {
                    string specsearch = Request.QueryString["specsearch"];
                    if (specsearch == "true")
                    {
                        litScript.Text = "gotomain()";
                    }
                }
               * */
                grdresults.ClientSettings.ClientEvents.OnDataBound = "gotomain";
            }
            else if (mode == "worklist")
            {
                //MainControlsPanel.Visible = false;
                //MainControlsPanel2.Visible = true;
                MainControlsPanel2.Style.Add("visibility", "visible");
                MainControlsPanel.Style.Add("visibility", "hidden");
                MainControlsPanel2.Style.Add("display", "block");
                MainControlsPanel.Style.Add("display", "none");
                MainControlsPanel3.Style.Add("visibility", "hidden");
                MainControlsPanel3.Style.Add("display", "none");
                lblListType.Text = m_msg["T4"];
            }
            else if (mode == "recentlist")
            {
                MainControlsPanel2.Style.Add("visibility", "hidden");
                MainControlsPanel.Style.Add("visibility", "hidden");
                MainControlsPanel2.Style.Add("display", "none");
                MainControlsPanel.Style.Add("display", "none");
                MainControlsPanel3.Style.Add("visibility", "visible");
                MainControlsPanel3.Style.Add("display", "block");
                lblListType.Text = m_msg["T2"];
            }
            hidMode.Value = mode;

            GridSortExpression exp = new GridSortExpression();
            exp.FieldName = "ItemNum";
            exp.SortOrder = GridSortOrder.Ascending;
            grdworklist.MasterTableView.SortExpressions.Add(exp);

            InitToolBar();
            //plhToolBar.Controls.Add(toolbar);

        }
        else
        {
            setPanel = true;
            //if (Request.Form["__EVENTTARGET"] == "addworklist")
            //{
            //    AddToWorkList(null, null);
            //}
            //if (Request.Form["__EVENTTARGET"] == "removeworklist")
            //{
            //    DeleteWorkList(null, null);
            //}
        }

        //trblbnAddList.Attributes.Add("onmouseover", "imageChangeover('tbrimgAddList','../img/addlist32_over.png')");
        //trblbnAddList.Attributes.Add("onmouseout", "imageChangeover('tbrimgAddList','../img/addlist32.png')");
    }

    private void InitToolBar()
    {
        int iconwidth = 55;
        int screenwidth = Convert.ToInt32(Session["ScreenWidth"].ToString());
        int panelwidth = (int)Math.Floor(screenwidth * 0.33) - 32;
        if (Session["INVFrameControlPanel"] != null)
        {
          panelwidth = (int)Math.Floor(screenwidth * (Convert.ToDecimal(Session["INVFrameControlPanel"].ToString()) / 100)) - 32;
        }
        int totaliconnum = (int)Math.Floor((decimal)panelwidth / iconwidth);
        RadToolBarDropDown toolbarcombobox = new RadToolBarDropDown();

        string connstring = Application["ConnString"].ToString();
        OleDbConnection conn = new OleDbConnection(connstring);
        conn.Open();
        string sql = "SELECT * FROM UserToolbar WHERE UserId='" + Session["Login"].ToString() + "' AND UserModule='InventoryPanel' ORDER BY orderby";
        OleDbDataAdapter da = new OleDbDataAdapter(sql, conn);
        DataTable dt = new DataTable();
        da.Fill(dt);
        da.Dispose();

        if (dt.Rows.Count == 0)
        {
          sql = "SELECT * FROM UserToolbar WHERE UserId='DEFAULT' AND UserModule='InventoryPanel' ORDER BY orderby";
          da = new OleDbDataAdapter(sql, conn);
          da.Fill(dt);
          da.Dispose();
        }

        NameValueCollection drRights;
        UserRights right = new UserRights(Session["Login"].ToString(), "UserRights", "Counter");
        drRights = right.GetRights(Session["Login"].ToString(), "Labour");

        int iconnum = 0;
        //toolbar = new RadToolBar();
        toolbar.Height = Unit.Pixel(60);
        toolbar.OnClientButtonClicking = "ToolbarClicked";
        toolbar.AutoPostBack = false;
        //toolbar.ButtonClick +=new RadToolBarEventHandler(toolbar_ButtonClick);
        toolbar.Width = Unit.Percentage(100);
        foreach (DataRow dr in dt.Rows)
        {
            string commandname = dr["CommandName"].ToString().ToLower();
            //if (!ShowToolbarButtonRights(commandname, drRights))
            //        continue;

            RadToolBarButton btn = new RadToolBarButton();
            btn.ImagePosition = ToolBarImagePosition.AboveText;
            btn.Text = dr["LabelText"].ToString();
            btn.ToolTip = dr["ToolTip"].ToString();
            btn.BackColor = System.Drawing.Color.Transparent;
            btn.CommandName = commandname;
            btn.Font.Size = 9;
            btn.ImageUrl = dr["imageurl"].ToString();
            btn.HoveredImageUrl = dr["hoverurl"].ToString();
            //setup images
          /*
            switch (commandname)
            {
                case "addworklist":
                
                    break;
                case "queryresult":
                    
                    break;
                case "worklist":
                    
                    break;
                case "recentlist":
                    
                    break;
                case "removeworklist":
                    
                    break;
                case "searchspec":
                    break;
            }
           * */

            if (iconnum >= totaliconnum - 1)
            {

                if (iconnum == totaliconnum - 1)
                {
                    toolbar.Items.Add(toolbarcombobox);
                }
                btn.ForeColor = System.Drawing.Color.Black;
                toolbarcombobox.Buttons.Add(btn);
            }
            else
            {
                btn.Width = Unit.Pixel(iconwidth);
                btn.ForeColor = System.Drawing.Color.FloralWhite;
                toolbar.Items.Add(btn);
            }
            iconnum++;
        }
    }

    protected void RadAjaxManager1_AjaxRequest(object sender, AjaxRequestEventArgs e)
    {
        AzzierScreen screen = new AzzierScreen();
    }

    protected void grdworklist_ItemEvent(object sender, GridItemEventArgs e)
    {
        if (e.EventInfo is GridInitializePagerItem)
        {
            worklistcount = (e.EventInfo as GridInitializePagerItem).PagingManager.DataSourceCount;
        }
    }

    protected void grdresults_ItemEvent(object sender, GridItemEventArgs e)
    {
        if (e.EventInfo is GridInitializePagerItem)
        {
            resultcount = (e.EventInfo as GridInitializePagerItem).PagingManager.DataSourceCount;
        }
    }

    protected void grdrecentlist_ItemEvent(object sender, GridItemEventArgs e)
    {
        if (e.EventInfo is GridInitializePagerItem)
        {
            recentlistcount = (e.EventInfo as GridInitializePagerItem).PagingManager.DataSourceCount;
        }
    }

    private void RetrieveMessage()
    {
        SystemMessage msg = new SystemMessage("inventory/invpanel.aspx");
        m_msg = msg.GetSystemMessage();
        msg.SetJsMessage(litMessage);
    }
}