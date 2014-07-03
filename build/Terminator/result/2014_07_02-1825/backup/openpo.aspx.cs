using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Data.OleDb;
using Telerik.Web.UI;
using System.Configuration;
using System.Linq;

public partial class inventory_openpo : System.Web.UI.Page
{
    AzzierScreen screen;
    private string connstring;
    protected string m_itemnum;
    protected RadGrid grdpolist;

    Items objItems;
    NameValueCollection nvcitems;
    protected int statuscode = 0;
    protected NameValueCollection m_msg = new NameValueCollection();

    protected void Page_Init(object sender, EventArgs e)
    {
        RetrieveMessage();
        if (Session["Login"] == null)
        {
            Response.Write("<script>alert('" + m_msg["T1"] + "');top.document.location.href='../Login.aspx';</script>");
            Response.End();
        }
        Session.LCID = Convert.ToInt32(Session["LCID"]);


        if (Request.QueryString["itemnum"] != null)
        {
          m_itemnum = Request.QueryString["itemnum"].ToString();
        }
        else
        {
          Response.Write("<script>alert('Illegal Access');document.location.href='invmain.aspx';</script>");
          Response.End();
        }


        
        objItems = new Items(Session["Login"].ToString(), "Items", "ItemNum", m_itemnum);
        nvcitems = objItems.ModuleData;
        
        connstring = Application["ConnString"].ToString();
        InitScreen();
    }

    private void InitScreen()
    {
        screen = new AzzierScreen("inventory/openpo.aspx", "MainForm", MainControlsPanel.Controls);
        Session.LCID = Convert.ToInt16(Session["LCID"]);
        screen.LCID = Session.LCID;
        InitGrid();
        screen.LoadScreen();
    }

    private void InitGrid()
    {
      grdpolist = new RadGrid();
      grdpolist.ID = "grdpolist";
      grdpolist.PageSize = 100;
      grdpolist.AllowPaging = true;
      grdpolist.AllowSorting = true;
      grdpolist.MasterTableView.AllowMultiColumnSorting = true;
      grdpolist.MasterTableView.AutoGenerateColumns = false;
      grdpolist.ClientSettings.EnableAlternatingItems = false;

      grdpolist.MasterTableView.DataKeyNames = new string[] { "PoNum" };
      screen.SetGridColumns("polist", grdpolist);
      grdpolist.MasterTableView.CommandItemTemplate = new MultiFunctionItemTemplate("Open PO List");
      grdpolist.MasterTableView.CommandItemDisplay = GridCommandItemDisplay.Top;

      Validation v = new Validation();
      string wherestring = v.AddLinqConditions("itemnum^" + m_itemnum + ",statuscode^>0,statuscode^<300", "inventory/openpo.aspx", "polist", "v_ReceivingPOLine", null, null, "query");

      grdpolist.ClientSettings.DataBinding.SelectMethod = "ReceivingLineQuery?wherestring=" + wherestring;
      grdpolist.ClientSettings.DataBinding.Location = "../InternalServices/ServiceInventory.svc";
      grdpolist.ClientSettings.DataBinding.SortParameterType = GridClientDataBindingParameterType.Linq;
      grdpolist.ClientSettings.DataBinding.FilterParameterType = GridClientDataBindingParameterType.Linq;
      grdpolist.ClientSettings.ClientEvents.OnRowClick = "RowClick";

      MainControlsPanel.Controls.Add(grdpolist);

    }

    protected void grdeqplist_ItemDataBound(object sender, GridItemEventArgs e)
    {
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!Page.IsPostBack)
        {
            screen.PopulateScreen("Items", nvcitems);
        }

        ucHeader1.Mode = "edit";
        ucHeader1.TabName = "Open PO";
        ucHeader1.ModuleData = nvcitems;
        ucHeader1.OperationLabel = "Open PO List";
    }

    private void RetrieveMessage()
    {
        SystemMessage msg = new SystemMessage("inventory/invmain.aspx");
        m_msg = msg.GetSystemMessage();
        msg.SetJsMessage(litMessage);
    }
}