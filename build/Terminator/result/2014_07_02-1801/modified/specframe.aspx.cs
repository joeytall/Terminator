using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.OleDb;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Web.UI;

public partial class Codes_SpecFrame : System.Web.UI.Page
{
    AzzierScreen screen;
    protected RadGrid grdspeclist;

    protected string mode = "";
    protected string runtimefilter = "";
    protected string designtimefilter = "";
    protected string fieldlist = "";
    protected string referer = "";
    protected bool found = false;
    protected string wherestr = "";
    protected string controlid = "";
    protected string fieldid = "";
    protected string tablename = "Specification";
    protected string filterstr = "";
    protected string filename = "";
    protected string totalCount = "";
    protected string employee = "";
    protected string linktype = "";
    protected string linkid = "";
    protected string type = "";
    protected string typefilter = "1";
    protected string istemplate = "0";
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

        string[] filters = filterstr.Split(',');
        for (int i = 0; i < filters.Length; i++)
        {
          string filter = filters[i];
          string[] values = filter.Split('^');
          if (values[0].ToLower() == "linktype")
            linktype = values[1];
          if (values[0].ToLower() == "linkid")
            linkid = values[1];
          if (values[0].ToLower() == "istemplate")
            istemplate = values[1];
        }

        ModuleoObject obj;
        if (istemplate == "0")
        {
          if (linktype.ToLower() == "equipment")
          {
            obj = new ModuleoObject(Session["Login"].ToString(), "Equipment", "Equipment", linkid);
            type = obj.ModuleData["MrType"];
          }
          if (linktype.ToLower() == "location")
          {
            obj = new ModuleoObject(Session["Login"].ToString(), "Location", "Location", linkid);
            type = obj.ModuleData["LocType"];
          }
          if (linktype.ToLower() == "inventory")
          {
            obj = new ModuleoObject(Session["Login"].ToString(), "Items", "Items", linkid);
            type = obj.ModuleData["Category"];
          }
        }

        //Validation v = new Validation();

        //wherestr = v.AddLinqConditions(filterstr, filename, controlid, tablename);

        screen = new AzzierScreen("codes/specframe.aspx", "MainForm", MainControlsPanel.Controls);

        grdspeclist = new RadGrid();
        grdspeclist.ID = "grdspeclist";
        grdspeclist.ClientSettings.Scrolling.AllowScroll = true;
        grdspeclist.ClientSettings.Scrolling.SaveScrollPosition = true;
        grdspeclist.ClientSettings.Scrolling.UseStaticHeaders = true;
        grdspeclist.ClientSettings.EnableRowHoverStyle = true;
        grdspeclist.MasterTableView.TableLayout = GridTableLayout.Fixed;
        grdspeclist.PagerStyle.Visible = true;// false;
        grdspeclist.PagerStyle.AlwaysVisible = true;
        grdspeclist.Skin = "Outlook";

        grdspeclist.Attributes.Add("rules", "all");
        grdspeclist.AutoGenerateColumns = false;
        grdspeclist.AllowPaging = true;
        grdspeclist.PageSize = 100;
        grdspeclist.AllowSorting = true;
        grdspeclist.MasterTableView.AllowMultiColumnSorting = true;
        grdspeclist.AllowFilteringByColumn = true;
        grdspeclist.MasterTableView.CommandItemDisplay = GridCommandItemDisplay.Top;
        grdspeclist.MasterTableView.DataKeyNames = new string[] { "SpecTag" };
        grdspeclist.MasterTableView.ClientDataKeyNames = new string[] { "SpecTag" };

        grdspeclist.MasterTableView.CommandItemSettings.ShowRefreshButton = false;

        grdspeclist.ClientSettings.Selecting.AllowRowSelect = true;
        grdspeclist.ClientSettings.ClientEvents.OnRowSelected = "mygetGridSelectedItems";
        //grdemplist.ClientSettings.Scrolling.FrozenColumnsCount = 1;

        grdspeclist.MasterTableView.CommandItemTemplate = new MultiFunctionItemTemplate("Attribute");
        grdspeclist.ItemCreated += new GridItemEventHandler(grdspeclist_ItemCreated);

        grdspeclist.ClientSettings.DataBinding.SortParameterType = GridClientDataBindingParameterType.Linq;
        grdspeclist.ClientSettings.DataBinding.FilterParameterType = GridClientDataBindingParameterType.Linq;

        screen.SetGridColumns("speclist", grdspeclist);

        MainControlsPanel.Controls.Add(grdspeclist);

        screen.LoadScreen();
    }

    protected void Page_Load(object sender, EventArgs e)
    {
      if (!Page.IsPostBack)
      {
        hidFieldId.Value = fieldid;
        hidControlId.Value = AzzierData.ActualFieldName("specification",controlid);

        hidScreenH.Value = screen.Height.ToString();
        hidScreenW.Value = screen.Width.ToString();

        employee = hidEmployee.Value;

        grdspeclist.ClientSettings.DataBinding.SelectMethod = "SpecificationLookup?typefilter=" + typefilter + "&linktype=" + linktype + "&linkid=" + linkid + "&type=" + type;
        grdspeclist.ClientSettings.DataBinding.Location = "../InternalServices/ServiceMeter.svc";

        RadioButtonList r = (RadioButtonList)MainControlsPanel.FindControl("rbltype");

        if (r != null)
        {
          //r.Attributes.Add("onclick", "javascript:rebind()");

          r.RepeatDirection = RepeatDirection.Horizontal;
          ListItem litm1 = new ListItem("Yes", "1");
          //litm1.Attributes.Add("onclick", "rebind");
          litm1.Selected = true;

          r.Items.Add(litm1);
          ListItem litm2 = new ListItem("No", "0");
          //litm2.Attributes.Add("onclick", "rebind()");
          r.Items.Add(litm2);
          //r.SelectedIndexChanged += new EventHandler(Type_SelectedIndexChanged);
          r.AutoPostBack = true;
        }
      }
      else
      {
        RadioButtonList r = (RadioButtonList)MainControlsPanel.FindControl("rbltype");
        if (r != null)
        {
          string v = r.SelectedValue;
          grdspeclist.ClientSettings.DataBinding.SelectMethod = "SpecificationLookup?typefilter=" + v + "&linktype=" + linktype + "&linkid=" + linkid + "&type=" + type;
          grdspeclist.ClientSettings.DataBinding.Location = "../InternalServices/ServiceMeter.svc";
        }
      }
    }

    private void Type_SelectedIndexChanged(object sender, EventArgs e)
    {
      RadioButtonList r = (RadioButtonList)MainControlsPanel.FindControl("rbltype");

    }

    protected void grdspeclist_ItemCreated(object sender, GridItemEventArgs e)
    {
      screen.GridItemCreated(e, "codes/specframe.aspx", "MainForm", "results", grdspeclist);
    }

    private void RetrieveMessage()
    {
        SystemMessage msg = new SystemMessage("codes/emplist.aspx");
        m_msg = msg.GetSystemMessage();
        msg.SetJsMessage(litMessage);
    }
}