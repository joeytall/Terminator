using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Web.UI;
using System.Collections.Specialized;
using System.Data;

public partial class Codes_Divlist : System.Web.UI.Page
{
    protected RadGrid grddivlist;
    protected AzzierScreen screen;
    protected string mode = "";
    protected string runtimefilter = "";
    protected string designtimefilter = "";
    protected string fieldlist = "";
    protected string referer = "";
    protected bool found = false;
    protected string wherestr = "";
    protected string controlid = "";
    protected string fieldid = "";
    protected string tablename = "UserDivision";
    protected string filename = "";
    protected string totalCount = "";
    protected string wherestrlinq = "";
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

        Validation v = new Validation();

        string filterstrlinq = "";
        runtimefilter = runtimefilter ?? "";
        designtimefilter = designtimefilter ?? "";
        if (runtimefilter != "")
        {
            if (designtimefilter != "")
            {
                filterstrlinq = runtimefilter + "," + designtimefilter;
            }
            else
            {
                filterstrlinq = runtimefilter;
            }
        }
        else
        {
            if (designtimefilter != "")
            {
                filterstrlinq = designtimefilter;
            }
        }

        wherestrlinq = v.AddLinqConditions(filterstrlinq, filename, controlid, tablename,null,null,mode);
        screen = new AzzierScreen("codes/divlist.aspx", "MainForm", MainControlsPanel.Controls);

        grddivlist = new RadGrid();
        grddivlist.ID = "grddivlist";
        grddivlist.ClientSettings.Scrolling.AllowScroll = true;
        grddivlist.ClientSettings.Scrolling.SaveScrollPosition = true;
        grddivlist.ClientSettings.Scrolling.UseStaticHeaders = true;
        grddivlist.ClientSettings.EnableRowHoverStyle = true;
        //grdeqplist.ClientSettings.ClientEvents.OnScroll = "HandleScrolling";
        grddivlist.MasterTableView.TableLayout = GridTableLayout.Fixed;
        grddivlist.PagerStyle.Visible = true;
        grddivlist.PagerStyle.AlwaysVisible = true;
        grddivlist.Skin = "Outlook";

        //grddivlist.Attributes.Add("rules", "all");

        grddivlist.AutoGenerateColumns = false;
        grddivlist.AllowPaging = true;
        grddivlist.PageSize = 100;
        grddivlist.AllowSorting = true;
        grddivlist.MasterTableView.AllowMultiColumnSorting = true;
        grddivlist.AllowFilteringByColumn = true;
        grddivlist.MasterTableView.CommandItemDisplay = GridCommandItemDisplay.Top;
        grddivlist.MasterTableView.DataKeyNames = new string[] { "Division" };
        grddivlist.MasterTableView.CommandItemSettings.ShowAddNewRecordButton = false;

        grddivlist.ClientSettings.Selecting.AllowRowSelect = true;
        grddivlist.ClientSettings.ClientEvents.OnRowSelected = "getGridSelectedItems";

        grddivlist.MasterTableView.CommandItemTemplate = new MultiFunctionItemTemplate("Division");
        grddivlist.ItemCreated += new GridItemEventHandler(grddivlist_ItemCreated);

        
        grddivlist.ClientSettings.DataBinding.SortParameterType = GridClientDataBindingParameterType.Linq;
        grddivlist.ClientSettings.DataBinding.FilterParameterType = GridClientDataBindingParameterType.Linq;
        
        screen.SetGridColumns("divlist", grddivlist);
        MainControlsPanel.Controls.Add(grddivlist);
        screen.LoadScreen();
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        grddivlist.Rebind();

        hidFieldId.Value = fieldid;
        hidControlId.Value = AzzierData.ActualFieldName("userdivision",controlid);
        hidScreenH.Value = screen.Height.ToString();
        hidScreenW.Value = screen.Width.ToString();

        grddivlist.ClientSettings.DataBinding.SelectMethod = "LookupDataAndCount?wherestr=" + wherestrlinq + "&mode=" + mode + "&user=" + Session["Login"].ToString();
        grddivlist.ClientSettings.DataBinding.Location = "../InternalServices/ServiceDivision.svc";
    }


    protected void grddivlist_ItemCreated(object sender, GridItemEventArgs e)
    {
      screen.GridItemCreated(e, "codes/divlist.aspx", "MainForm", "results", grddivlist);
    }

    protected void RetrieveMessage()
    {
        SystemMessage msg = new SystemMessage("codes/divlist.aspx");
        m_msg = msg.GetSystemMessage();
        msg.SetJsMessage(litMessage);
    }
}