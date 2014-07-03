using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.OleDb;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Web.UI;

public partial class Codes_Emplist : System.Web.UI.Page
{
    AzzierScreen screen;
    protected RadGrid grdemplist;

    protected string mode = "";
    protected string runtimefilter = "";
    protected string designtimefilter = "";
    protected string fieldlist = "";
    protected string referer = "";
    protected bool found = false;
    protected string wherestr = "";
    protected string controlid = "";
    protected string fieldid = "";
    protected string tablename = "Employee";
    protected string filterstr = "";
    protected string filename = "";
    protected string totalCount = "";
    protected string employee = "";
    protected NameValueCollection m_msg = new NameValueCollection();

    protected void Page_Init(object sender, EventArgs e)
    {
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
        if (Request.QueryString["tablename"] != null)
            tablename = Request.QueryString["tablename"].ToString();

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

        screen = new AzzierScreen("codes/emplist.aspx", "MainForm", MainControlsPanel.Controls);

        grdemplist = new RadGrid();
        grdemplist.ID = "grdemplist";
        grdemplist.ClientSettings.Scrolling.AllowScroll = true;
        grdemplist.ClientSettings.Scrolling.SaveScrollPosition = true;
        grdemplist.ClientSettings.Scrolling.UseStaticHeaders = true;
        grdemplist.ClientSettings.EnableRowHoverStyle = true;
        grdemplist.MasterTableView.TableLayout = GridTableLayout.Fixed;
        grdemplist.PagerStyle.Visible = true;// false;
        grdemplist.PagerStyle.AlwaysVisible = true;
        grdemplist.Skin = "Outlook";

        grdemplist.Attributes.Add("rules", "all");
        grdemplist.AutoGenerateColumns = false;
        grdemplist.AllowPaging = true;
        grdemplist.PageSize = 100;
        grdemplist.AllowSorting = true;
        grdemplist.MasterTableView.AllowMultiColumnSorting = true;
        grdemplist.AllowFilteringByColumn = true;
        grdemplist.MasterTableView.CommandItemDisplay = GridCommandItemDisplay.Top;
        grdemplist.MasterTableView.DataKeyNames = new string[] { "Empid" };
        grdemplist.MasterTableView.ClientDataKeyNames = new string[] { "Empid" };

        grdemplist.MasterTableView.CommandItemSettings.ShowRefreshButton = false;

        grdemplist.ClientSettings.Selecting.AllowRowSelect = true;
        grdemplist.ClientSettings.ClientEvents.OnRowSelected = "getGridSelectedItems";

        grdemplist.MasterTableView.CommandItemTemplate = new MultiFunctionItemTemplate("Employee");
        grdemplist.ItemCreated += new GridItemEventHandler(grdemplist_ItemCreated);

        grdemplist.ClientSettings.DataBinding.SortParameterType = GridClientDataBindingParameterType.Linq;
        grdemplist.ClientSettings.DataBinding.FilterParameterType = GridClientDataBindingParameterType.Linq;

        screen.SetGridColumns("emplist", grdemplist);

        MainControlsPanel.Controls.Add(grdemplist);

        screen.LoadScreen();
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        hidFieldId.Value = fieldid;
        hidControlId.Value = AzzierData.ActualFieldName("Employee",controlid);

        hidScreenH.Value = screen.Height.ToString();
        hidScreenW.Value = screen.Width.ToString();

        employee = hidEmployee.Value;
        
        grdemplist.ClientSettings.DataBinding.SelectMethod = "LookupDataAndCount?where=" + wherestr + "&employee=" + employee;
        grdemplist.ClientSettings.DataBinding.Location = "../InternalServices/ServiceLabour.svc";

        RadioButtonList r = (RadioButtonList)MainControlsPanel.FindControl("rblemployee");
        if (r != null)
        {
            r.RepeatDirection = RepeatDirection.Horizontal;
            ListItem litm1 = new ListItem("Employee", "1");
            r.Items.Add(litm1);
            ListItem litm2 = new ListItem("Requester", "0");
            r.Items.Add(litm2);
            ListItem litm3 = new ListItem("All", "-1");
            litm3.Selected = true;
            r.Items.Add(litm3);
        }
    }

    protected void grdemplist_ItemCreated(object sender, GridItemEventArgs e)
    {
      screen.GridItemCreated(e, "codes/emplist.aspx", "MainForm", "results", grdemplist);
    }

    private void RetrieveMessage()
    {
        SystemMessage msg = new SystemMessage("codes/emplist.aspx");
        m_msg = msg.GetSystemMessage();
        msg.SetJsMessage(litMessage);
    }
}