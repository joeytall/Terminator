using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Web.UI;
using System.Collections.Specialized;

public partial class Codes_tasklist : System.Web.UI.Page
{
    protected AzzierScreen screen;
    protected RadGrid grdtasklist;
    protected string mode = "";
    protected string runtimefilter = "";
    protected string designtimefilter = "";
    protected string fieldlist = "";
    protected string referer = "";
    protected bool found = false;
    protected string wherestr = "";
    protected string controlid = "";
    protected string fieldid = "";
    protected string tablename = "WOTasks";
    protected string TotalCount = "";
    protected string filterstr = "", filename = "";
    protected string m_ordertype = "";
    protected string m_ordernum = "";
    protected string m_estimate = "";
    protected NameValueCollection m_msg = new NameValueCollection();

    protected void Page_Init(object sender, EventArgs e)
    {
        if (Session["Login"] == null)
        {
            //Response.Write("<html><script type=\"text/javascript\">alert('Your session has expired. Please login again.');top.document.location.href='../login.aspx';</script></html>");
            Response.Write("<script>alert('" + m_msg["T1"] + "');top.document.location.href='../Login.aspx';</script>");
            Response.End();
        }

        Session.LCID = Convert.ToInt32(Session["LCID"]);

        RetrieveMessage();
        if (Request.QueryString["referer"] != null)
          referer = Request.QueryString["referer"].ToString();

        if (Request.QueryString["mode"] != null)
            mode = Request.QueryString["mode"].ToString();
        if (Request.QueryString["runtimefilter"] != null)
            runtimefilter = Request.QueryString["runtimefilter"].ToString();
        if (Request.QueryString["designtimefilter"] != null)
            designtimefilter = Request.QueryString["designtimefilter"].ToString();
        if (Request.QueryString["fieldlist"] != null)
            fieldlist = Request.QueryString["fieldlist"].ToString();
        
        if (Request.QueryString["tablename"] != null)
            tablename = Request.QueryString["tablename"].ToString();
        if (Request.QueryString["filename"] != null)
            filename = Request.QueryString["filename"].ToString();
        

        if (referer == "TaskLibrary" || referer == "AddFromLibrary")
        {
          if (Request.QueryString["ordernum"] != null)
            m_ordernum = Request.QueryString["ordernum"].ToString();
          if (Request.QueryString["ordertype"] != null)
            m_ordertype = Request.QueryString["ordertype"].ToString();
          if (Request.QueryString["estimate"] != null)
            m_estimate = Request.QueryString["estimate"].ToString();

          designtimefilter = "ordertype^tasklibrary";
        }
        else
        {
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
        }

        filterstr = runtimefilter;
        if (designtimefilter != "")
        {
          if (filterstr == "")
          {
            filterstr = designtimefilter;
          }
          else
          {
            filterstr = filterstr + "," + designtimefilter;
          }
        }
        

        Validation v = new Validation();

        wherestr = v.AddLinqConditions(filterstr, filename, controlid, tablename,null,null,mode);

        screen = new AzzierScreen("codes/tasklist.aspx", "MainForm", MainControlsPanel.Controls);

        //string connstring = Application["ConnString"].ToString();
        //LocListSqlDataSource.ConnectionString = connstring;
        //if (wherestr == "")
        //    LocListSqlDataSource.SelectCommand = "Select * From " + tablename + " Order By location";
        //else
        //    LocListSqlDataSource.SelectCommand = "Select * From " + tablename + " " + wherestr + " Order By location";

        grdtasklist = new RadGrid();
        grdtasklist.ID = "grdtasklist";
        grdtasklist.ClientSettings.Scrolling.AllowScroll = true;
        grdtasklist.ClientSettings.Scrolling.SaveScrollPosition = true;
        grdtasklist.ClientSettings.Scrolling.UseStaticHeaders = true;
        grdtasklist.ClientSettings.EnableRowHoverStyle = true;
        //grdloclist.ClientSettings.ClientEvents.OnScroll = "HandleScrolling";
        grdtasklist.MasterTableView.TableLayout = GridTableLayout.Fixed;
        //grdloclist.PagerStyle.Visible = false;
        grdtasklist.PagerStyle.Visible = true;
        grdtasklist.PagerStyle.AlwaysVisible = true;
        grdtasklist.Skin = "Outlook";

        grdtasklist.Attributes.Add("rules", "all");
        //grdloclist.DataSourceID = "LocListSqlDataSource";
        grdtasklist.AutoGenerateColumns = false;
        grdtasklist.AllowPaging = true;
        grdtasklist.PageSize = 100;
        grdtasklist.AllowSorting = true;
        grdtasklist.MasterTableView.AllowMultiColumnSorting = true;
        grdtasklist.AllowFilteringByColumn = true;
        grdtasklist.MasterTableView.CommandItemDisplay = GridCommandItemDisplay.Top;
        grdtasklist.MasterTableView.DataKeyNames = new string[] { "Counter" };
        grdtasklist.MasterTableView.ClientDataKeyNames = new string[] { "Counter" };

        grdtasklist.ClientSettings.Selecting.AllowRowSelect = true;
        
          
        //grdloclist.ClientSettings.Scrolling.FrozenColumnsCount = 1;
        //grdloclist.PagerStyle.Mode = GridPagerMode.NextPrevAndNumeric;
        if (referer == "TaskLibrary")
        {
          grdtasklist.MasterTableView.CommandItemTemplate = new MultiFunctionItemTemplate(1, true, "Task Library", null, "return editLibrary('');", 1, "", false, "", "New Task Library");

          GridEditCommandColumn EditColumn = new GridEditCommandColumn();
          EditColumn.UniqueName = "EditCommand";
          EditColumn.ButtonType = GridButtonColumnType.ImageButton;
          EditColumn.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
          EditColumn.HeaderStyle.Width = 30;
          grdtasklist.MasterTableView.Columns.Add(EditColumn);
        }
        else if (referer == "AddFromLibrary")
        {
          grdtasklist.MasterTableView.CommandItemTemplate = new MultiFunctionItemTemplate("Tasks");
          GridClientSelectColumn cbcolumn = new GridClientSelectColumn();
          cbcolumn.UniqueName = "SelectColumn";
          grdtasklist.MasterTableView.Columns.Add(cbcolumn);
        }
        else
        {
          grdtasklist.MasterTableView.CommandItemTemplate = new MultiFunctionItemTemplate("Tasks");
          grdtasklist.ClientSettings.ClientEvents.OnRowSelected = "getGridSelectedItems";
        }

        grdtasklist.ItemCreated += new GridItemEventHandler(grdtasklist_ItemCreated);
        grdtasklist.ItemDataBound+=new GridItemEventHandler(grdtasklist_ItemDataBound);
        grdtasklist.ClientSettings.DataBinding.FilterParameterType = GridClientDataBindingParameterType.Linq;
        grdtasklist.ClientSettings.DataBinding.SortParameterType = GridClientDataBindingParameterType.Linq;

        screen.SetGridColumns("tasklist", grdtasklist);
        MainControlsPanel.Controls.Add(grdtasklist);

        screen.LoadScreen();
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        hidFieldId.Value = fieldid;
        hidControlId.Value = AzzierData.ActualFieldName("wotasks", controlid);
        //hidControlId.Value = controlid;

        grdtasklist.ClientSettings.DataBinding.SelectMethod = "LookupTasks?wherestring=" + wherestr;
        grdtasklist.ClientSettings.DataBinding.Location = "../InternalServices/ServiceWO.svc";

        if (referer != "AddFromLibrary")
        {
          btnsave.Visible = false;
        }
    }


    protected void grdtasklist_ItemCreated(object sender, GridItemEventArgs e)
    {
      screen.GridItemCreated(e, "codes/taskslist.aspx", "MainForm", "tasklist", grdtasklist);
    }

    protected void grdtasklist_ItemDataBound(object sender, GridItemEventArgs e)
    {
      //screen.GridItemDataBound(e, "codes/tasklist.aspx", "MainForm", "tasklist");

      if (e.Item is GridDataItem && !e.Item.IsInEditMode)
      {
        if (referer == "TaskLibrary")
        {
          GridDataItem item = (GridDataItem)e.Item;
          ImageButton btn = (ImageButton)item["EditCommand"].Controls[0];
          btn.ImageUrl = "~/Images/Edit.gif";
          btn.OnClientClick = "return editLibrary('" + item.ItemIndex.ToString() + "')";
        }
      }

      if (e.Item is GridCommandItem)
      {
        /*
        Button addButton = e.Item.FindControl("addFormButton") as Button;
        if (addButton != null)
        {
          addButton.Visible = false;
        }
         
        LinkButton lnkButton = (LinkButton)e.Item.FindControl("InitInsertButton");
        if (lnkButton != null)
        {
          lnkButton.Visible = false;
        }
         * */

      }
    }
  
    protected void RadAjaxManager1_AjaxRequest(object sender, AjaxRequestEventArgs e)
    {
    }

    private void RetrieveMessage()
    {
        SystemMessage msg = new SystemMessage("codes/loclist.aspx");
        m_msg = msg.GetSystemMessage();
        //msg.SetJsMessage(litMessage);
    }
}