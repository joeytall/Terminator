using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Web.UI;
using System.Collections.Specialized;

public partial class Codes_PositionList : System.Web.UI.Page
{
    protected AzzierScreen screen;
    protected RadGrid grdpositionlist;
    protected string mode = "";
    protected string runtimefilter = "";
    protected string designtimefilter = "";
    protected string fieldlist = "";
    protected string referer = "";
    protected bool found = false;
    protected string wherestr = "";
    protected string controlid = "";
    protected string fieldid = "";
    protected string tablename = "v_InventoryPosition";
    protected string TotalCount = "";
    protected string filterstr = "", filename = "";
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
            filename = Request.QueryString["filename"].ToString();

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

        screen = new AzzierScreen("codes/positionlist.aspx", "MainForm", MainControlsPanel.Controls);

        //string connstring = Application["ConnString"].ToString();
        //LocListSqlDataSource.ConnectionString = connstring;
        //if (wherestr == "")
        //    LocListSqlDataSource.SelectCommand = "Select * From " + tablename + " Order By location";
        //else
        //    LocListSqlDataSource.SelectCommand = "Select * From " + tablename + " " + wherestr + " Order By location";

        grdpositionlist = new RadGrid();
        grdpositionlist.ID = "grdpositionlist";
        grdpositionlist.ClientSettings.Scrolling.AllowScroll = true;
        grdpositionlist.ClientSettings.Scrolling.SaveScrollPosition = true;
        grdpositionlist.ClientSettings.Scrolling.UseStaticHeaders = true;
        grdpositionlist.ClientSettings.EnableRowHoverStyle = true;
        //grdloclist.ClientSettings.ClientEvents.OnScroll = "HandleScrolling";
        grdpositionlist.MasterTableView.TableLayout = GridTableLayout.Fixed;
        //grdloclist.PagerStyle.Visible = false;
        grdpositionlist.PagerStyle.Visible = true;
        grdpositionlist.PagerStyle.AlwaysVisible = true;
        grdpositionlist.Skin = "Outlook";

        grdpositionlist.Attributes.Add("rules", "all");
        //grdloclist.DataSourceID = "LocListSqlDataSource";
        grdpositionlist.AutoGenerateColumns = false;
        grdpositionlist.AllowPaging = true;
        grdpositionlist.PageSize = 100;
        grdpositionlist.AllowSorting = true;
        grdpositionlist.MasterTableView.AllowMultiColumnSorting = true;
        grdpositionlist.AllowFilteringByColumn = true;
        grdpositionlist.MasterTableView.CommandItemDisplay = GridCommandItemDisplay.Top;
        grdpositionlist.MasterTableView.DataKeyNames = new string[] { "position" };

        grdpositionlist.ClientSettings.Selecting.AllowRowSelect = true;
        grdpositionlist.ClientSettings.ClientEvents.OnRowSelected = "getGridSelectedItems";

        grdpositionlist.MasterTableView.CommandItemTemplate = new MultiFunctionItemTemplate("Position");
        grdpositionlist.ItemCreated += new GridItemEventHandler(grdloclist_ItemCreated);

        grdpositionlist.ClientSettings.DataBinding.FilterParameterType = GridClientDataBindingParameterType.Linq;
        grdpositionlist.ClientSettings.DataBinding.SortParameterType = GridClientDataBindingParameterType.Linq;

        screen.SetGridColumns("positionlist", grdpositionlist);
        MainControlsPanel.Controls.Add(grdpositionlist);

        screen.LoadScreen();
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        hidFieldId.Value = fieldid;
        hidControlId.Value = AzzierData.ActualFieldName("V_InventoryPosition", controlid);
        //hidControlId.Value = controlid;

        grdpositionlist.ClientSettings.DataBinding.SelectMethod = "GetPositionList?wherestring=" + wherestr;
        grdpositionlist.ClientSettings.DataBinding.Location = "../InternalServices/ServiceInventory.svc";
    }


    protected void grdloclist_ItemCreated(object sender, GridItemEventArgs e)
    {
      screen.GridItemCreated(e, "codes/positionlist.aspx", "MainForm", "positionlist", grdpositionlist);
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