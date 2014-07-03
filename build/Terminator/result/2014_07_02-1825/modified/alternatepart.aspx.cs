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

public partial class inventory_alternatepart : System.Web.UI.Page
{
    AzzierScreen screen;
    private string connstring;
    protected string m_itemnum;
    protected RadGrid grditemlist;

    Items objItems;
    NameValueCollection nvcitems;
    protected int statuscode = 0;
    protected NameValueCollection m_msg = new NameValueCollection();
    protected NameValueCollection m_rights;
    protected int m_allowedit = 0;

    protected void Page_Init(object sender, EventArgs e)
    {
        RetrieveMessage();
        UserRights.CheckAccess('');
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

        UserRights r = new UserRights(Session["Login"].ToString(), "UserRights", "counter");
        m_rights = r.GetRights(Session["Login"].ToString(), "inventory");

        m_allowedit = Convert.ToInt16(m_rights["urEdit"].ToString());
        
        objItems = new Items(Session["Login"].ToString(), "Items", "ItemNum", m_itemnum);
        nvcitems = objItems.ModuleData;
        
        connstring = Application["ConnString"].ToString();
        InitScreen();
    }

    private void InitScreen()
    {
        screen = new AzzierScreen("inventory/alternatepart.aspx", "MainForm", MainControlsPanel.Controls);
        Session.LCID = Convert.ToInt16(Session["LCID"]);
        screen.LCID = Session.LCID;
        InitGrid();
        screen.LoadScreen();
    }

    private void InitGrid()
    {
      grditemlist = new RadGrid();
      grditemlist.ID = "grditemlist";
      grditemlist.PageSize = 100;
      grditemlist.AllowPaging = true;
      grditemlist.AllowSorting = true;
      grditemlist.MasterTableView.AllowMultiColumnSorting = true;
      grditemlist.MasterTableView.AutoGenerateColumns = false;
      grditemlist.ClientSettings.EnableAlternatingItems = false;

      grditemlist.MasterTableView.DataKeyNames = new string[] { "ItemNum" };
      grditemlist.MasterTableView.ClientDataKeyNames = new string[] { "ItemNum" };

      GridEditCommandColumn editcol = new GridEditCommandColumn();
      editcol.UniqueName = "EditCommand";
      editcol.ButtonType = GridButtonColumnType.ImageButton;
      editcol.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
      editcol.HeaderStyle.Width = 30;
      grditemlist.MasterTableView.Columns.Add(editcol);

      screen.SetGridColumns("itemlist", grditemlist);

      //grditemlist.MasterTableView.CommandItemTemplate = new MultiFunctionItemTemplate("Alternate Parts");
      grditemlist.MasterTableView.CommandItemDisplay = GridCommandItemDisplay.Top;

      Validation v = new Validation();
      string wherestring = v.AddLinqConditions("", "inventory/alternatepart.aspx", "itemlist", "Items", null, null, "query");

      grditemlist.ClientSettings.DataBinding.SelectMethod = "GetAlternatePart?wherestring=" + wherestring + "&ItemNum=" + m_itemnum;
      grditemlist.ClientSettings.DataBinding.Location = "../InternalServices/ServiceInventory.svc";
      grditemlist.ClientSettings.DataBinding.SortParameterType = GridClientDataBindingParameterType.Linq;
      grditemlist.ClientSettings.DataBinding.FilterParameterType = GridClientDataBindingParameterType.Linq;
      grditemlist.ClientSettings.ClientEvents.OnRowClick = "RowClick";
      grditemlist.ItemDataBound += new GridItemEventHandler(grditemlist_ItemDataBound);

      grditemlist.MasterTableView.CommandItemTemplate = new MultiFunctionItemTemplate(1, true, "Alternate Parts", null, "editpart('','" + m_itemnum + "');return false;", m_allowedit, "", false);

      MainControlsPanel.Controls.Add(grditemlist);

    }

    protected void grditemlist_ItemDataBound(object sender, GridItemEventArgs e)
    {
      if (e.Item is GridDataItem && !e.Item.IsInEditMode)
      {
        GridDataItem item = (GridDataItem)e.Item;
        ImageButton btn = item["EditCommand"].Controls[0] as ImageButton;
        if (btn != null)
        {
          btn.ImageUrl = "~/Images/Edit.gif";
          btn.OnClientClick = "editpart('" + item.ItemIndex.ToString() + "','" + m_itemnum + "');return false;";
        }
      }
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!Page.IsPostBack)
        {
            screen.PopulateScreen("Items", nvcitems);
        }

        ucHeader1.Mode = "edit";
        ucHeader1.TabName = "Alternate Part";
        ucHeader1.ModuleData = nvcitems;
        ucHeader1.OperationLabel = "Alternate Part";
    }

    private void RetrieveMessage()
    {
        SystemMessage msg = new SystemMessage("inventory/invmain.aspx");
        m_msg = msg.GetSystemMessage();
        msg.SetJsMessage(litMessage);
    }
}