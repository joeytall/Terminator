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

public partial class inventory_serializedeqplist : System.Web.UI.Page
{
    AzzierScreen screen;
    private string connstring;
    protected string m_itemnum;
    protected RadGrid grdeqplist;

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
        screen = new AzzierScreen("inventory/serializedeqplist.aspx", "MainForm", MainControlsPanel.Controls);
        Session.LCID = Convert.ToInt16(Session["LCID"]);
        screen.LCID = Session.LCID;
        InitGrid();
        screen.LoadScreen();
    }

    private void InitGrid()
    {
      grdeqplist = new RadGrid();
      grdeqplist.ID = "grdeqplist";
      grdeqplist.PageSize = 100;
      grdeqplist.AllowPaging = true;
      grdeqplist.AllowSorting = true;
      grdeqplist.MasterTableView.AllowMultiColumnSorting = true;
      grdeqplist.MasterTableView.AutoGenerateColumns = false;
      grdeqplist.ClientSettings.EnableAlternatingItems = false;

      grdeqplist.MasterTableView.DataKeyNames = new string[] { "Equipment" };
      screen.SetGridColumns("eqplist", grdeqplist);
      grdeqplist.MasterTableView.CommandItemTemplate = new MultiFunctionItemTemplate("Equipment");
      grdeqplist.MasterTableView.CommandItemDisplay = GridCommandItemDisplay.Top;

      Validation v = new Validation();
      string wherestring = v.AddLinqConditions("itemnum^" + m_itemnum, "inventory/serializedeqplist.aspx", "eqplist", "Equipment", null, null, "query");

      grdeqplist.ClientSettings.DataBinding.SelectMethod = "SearchDataAndCount?wherestring=" + wherestring;
      grdeqplist.ClientSettings.DataBinding.Location = "../InternalServices/ServiceEqpt.svc";
      grdeqplist.ClientSettings.DataBinding.SortParameterType = GridClientDataBindingParameterType.Linq;
      grdeqplist.ClientSettings.DataBinding.FilterParameterType = GridClientDataBindingParameterType.Linq;
      grdeqplist.ClientSettings.ClientEvents.OnRowClick = "RowClick";

      MainControlsPanel.Controls.Add(grdeqplist);

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
        ucHeader1.TabName = "Serialized";
        ucHeader1.ModuleData = nvcitems;
        ucHeader1.OperationLabel = "Serialzied";

    }

    private void RetrieveMessage()
    {
        SystemMessage msg = new SystemMessage("inventory/invmain.aspx");
        m_msg = msg.GetSystemMessage();
        msg.SetJsMessage(litMessage);
    }
}