using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Web.UI;
using System.Collections.Specialized;

public partial class Codes_measurementlist : System.Web.UI.Page
{
    protected AzzierScreen screen;
    protected RadGrid grdmeasurementlist;
    protected NameValueCollection m_msg;

    protected string mode = "";
    protected string runtimefilter = "";
    protected string designtimefilter = "";
    protected string fieldlist = "";
    protected string referer = "";
    protected string controlid = "";
    protected string fieldid = "";
    protected string wherestr = "";
    

    protected void Page_Init(object sender, EventArgs e)
    {
        RetrieveMessage();
        if (Session["Login"] == null)
        {
            Response.Write("<script>alert('" + m_msg["T1"] + "');top.document.location.href='../Login.aspx';</script>");
            Response.End();
        }

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
        wherestr = v.AddLinqConditions(filterstr, filename, controlid, "v_LastMeasurementReadingDetail", null, null, mode);

        screen = new AzzierScreen("codes/measurementlist.aspx", "MainForm", MainControlsPanel.Controls);

        grdmeasurementlist = new RadGrid();
        grdmeasurementlist.ID = "grdmeasurementlist";
        grdmeasurementlist.ClientSettings.Scrolling.AllowScroll = true;
        grdmeasurementlist.ClientSettings.Scrolling.SaveScrollPosition = true;
        grdmeasurementlist.ClientSettings.Scrolling.UseStaticHeaders = true;
        grdmeasurementlist.ClientSettings.EnableRowHoverStyle = true;
        grdmeasurementlist.MasterTableView.TableLayout = GridTableLayout.Fixed;
        grdmeasurementlist.PagerStyle.Visible = true;
        grdmeasurementlist.PagerStyle.AlwaysVisible = true;
        grdmeasurementlist.Skin = "Outlook";

        grdmeasurementlist.Attributes.Add("rules", "all");
        grdmeasurementlist.AutoGenerateColumns = false;
        grdmeasurementlist.AllowPaging = true;
        grdmeasurementlist.PageSize = 100;
        grdmeasurementlist.AllowSorting = true;
        grdmeasurementlist.MasterTableView.AllowMultiColumnSorting = true;
        grdmeasurementlist.AllowFilteringByColumn = true;
        grdmeasurementlist.MasterTableView.CommandItemDisplay = GridCommandItemDisplay.Top;
        grdmeasurementlist.MasterTableView.DataKeyNames = new string[] { "Counter" };

        grdmeasurementlist.ClientSettings.Selecting.AllowRowSelect = true;
        grdmeasurementlist.ClientSettings.ClientEvents.OnRowSelected = "getGridSelectedItems";
        grdmeasurementlist.ClientSettings.Scrolling.FrozenColumnsCount = 1;

        screen.SetGridColumns("measurementlist", grdmeasurementlist);

        grdmeasurementlist.ItemCreated += new GridItemEventHandler(grdmeasurementlist_ItemCreated);

        grdmeasurementlist.MasterTableView.CommandItemTemplate = new CodesCommandItem("Measurement", 0);

        grdmeasurementlist.ClientSettings.DataBinding.SortParameterType = GridClientDataBindingParameterType.Linq;
        grdmeasurementlist.ClientSettings.DataBinding.FilterParameterType = GridClientDataBindingParameterType.Linq;
        MainControlsPanel.Controls.Add(grdmeasurementlist);

        screen.LoadScreen();


    }

    protected void Page_Load(object sender, EventArgs e)
    {

        hidFieldId.Value = fieldid;
        hidControlId.Value = AzzierData.ActualFieldName("v_LastMeasurementReadingDetail", controlid);
        //grdmeasurementlist.DataSource = new DataTable();
        grdmeasurementlist.ClientSettings.DataBinding.SelectMethod = "MeasurementLookup?wherestr=" + wherestr;
        grdmeasurementlist.ClientSettings.DataBinding.Location = "../InternalServices/ServiceMeter.svc";
        
    }

    protected void grdmeasurementlist_ItemCreated(object sender, GridItemEventArgs e)
    {
        screen.GridItemCreated(e, "codes/measurementlist.aspx", "MainForm", "results", grdmeasurementlist);
    }


    private void RetrieveMessage()
    {
        SystemMessage msg = new SystemMessage("codes/craftlist.aspx");
        m_msg = msg.GetSystemMessage();
        msg.SetJsMessage(litMessage);
    }


}