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

public partial class inventory_invmain : System.Web.UI.Page
{
    AzzierScreen screen;
    private string connstring;
    protected string querymode = "";
    protected string m_itemnum, m_olditemnum = "";
    protected RadGrid grdinvmain;
    private RadGrid grdlot;
    private RadGrid grdposition;

    Items objItems;
    NameValueCollection nvcitems;
    protected bool isquery = false;
    protected int statuscode = 0;
    protected NameValueCollection m_msg = new NameValueCollection();
    protected NameValueCollection m_rights;
    protected int m_allowedit = 0;
    protected string m_vendor = "";
    protected string t_duplicate;

    protected string defaultmethod = "MIXED";
    protected string defaultissueprice = "AVGPRICE";
    protected string defaultpurchaseprice = "QUOTEDPRICE";
    protected string oldissuemethod = "";

    protected void Page_Init(object sender, EventArgs e)
    {
        RetrieveMessage();
        UserRights.CheckAccess('');
        UserRights r = new UserRights(Session["Login"].ToString(), "UserRights", "counter");
        m_rights = r.GetRights(Session["Login"].ToString(), "inventory");

        m_allowedit = Convert.ToInt16(m_rights["urEdit"].ToString());

        Session.LCID = Convert.ToInt32(Session["LCID"]);
        if (Request.QueryString["mode"] != null)
        {
          querymode = Request.QueryString["mode"].ToString();
          if (querymode=="")
            querymode = "query";
        }
        else
          querymode = "query";

        if (Request.QueryString["itemnum"] != null)
        {
            m_itemnum = Request.QueryString["itemnum"].ToString();
            m_olditemnum = Request.QueryString["itemnum"].ToString();
            HidOldItemNum.Value = Request.QueryString["itemnum"].ToString();
        }
        else
            m_itemnum = "";


        if (querymode == "query")
            isquery = true;

        if (querymode == "edit")
        {
            objItems = new Items(Session["Login"].ToString(), "Items", "ItemNum", m_itemnum);
            m_vendor = objItems.ModuleData["vendor"];
            nvcitems = objItems.ModuleData;
            oldissuemethod = objItems.ModuleData["issuemethod"];
            if (nvcitems["ItemNum"].ToString() == "")
            {
              querymode = "query";
              isquery = true;
              m_itemnum = "";
            }
            UserWorkList list = new UserWorkList();
            list.AddToRecentList(Session["Login"].ToString(), "Inventory", m_itemnum);
        }
        else if (querymode == "duplicate" && m_olditemnum.Length>0)
        {
            objItems = new Items(Session["Login"].ToString(), "Items", "ItemNum", m_olditemnum);  

            nvcitems = objItems.ModuleData;
            UserWorkList list = new UserWorkList();
            list.AddToRecentList(Session["Login"].ToString(), "Inventory", m_olditemnum);
            //litScript1.Text = "clearoldempval()";
        }
        else
        {
            objItems = new Items(Session["Login"].ToString(), "Items", "ItemNum");
            nvcitems = objItems.ModuleData;
            statuscode = 1;
        }
        hidMode.Value = querymode;
        connstring = Application["ConnString"].ToString();
        InitScreen();
    }

    private void InitScreen()
    {
        screen = new AzzierScreen("inventory/invmain.aspx", "MainForm", MainControlsPanel.Controls, querymode);
        Session.LCID = Convert.ToInt16(Session["LCID"]);
        screen.LCID = Session.LCID;
        if (querymode == "edit")
        {
          InitGrid();
        }
        
        screen.LoadScreen();

        if (!isquery)
            screen.SetValidationControls();
    }

    protected void grdinvmain_DetailTableDataBind(object source, Telerik.Web.UI.GridDetailTableDataBindEventArgs e)
    {
      GridDataItem dataItem = (GridDataItem)e.DetailTableView.ParentItem;
      switch (e.DetailTableView.Name)
      {
        case "position":
          {
            string itemnum = dataItem.GetDataKeyValue("ItemNum").ToString();
            string storeroom = dataItem.GetDataKeyValue("Storeroom").ToString();
            e.DetailTableView.DataSource = GetDataTable("SELECT * FROM v_inventoryposition WHERE ItemNum = '" + itemnum + "' And Storeroom='" + storeroom + "'");
            break;
          }

        case "lot":
          {
            string itemnum = dataItem.GetDataKeyValue("ItemNum").ToString();
            string storeroom = dataItem.GetDataKeyValue("Storeroom").ToString();
            string position = dataItem.GetDataKeyValue("Position").ToString();
            e.DetailTableView.DataSource = GetDataTable("SELECT * FROM v_inventorylot WHERE ItemNum = '" + itemnum + "' And Storeroom='" + storeroom + "' And Position='" + position + "' And Inacrive=0");
            break;
          }
      }
    }

    public DataTable GetDataTable(string query)
    {
      String ConnString = Application["ConnString"].ToString();
      OleDbConnection conn = new OleDbConnection(ConnString);
      OleDbDataAdapter adapter = new OleDbDataAdapter();
      adapter.SelectCommand = new OleDbCommand(query, conn);

      DataTable myDataTable = new DataTable();

      conn.Open();
      try
      {
        adapter.Fill(myDataTable);
      }
      finally
      {
        conn.Close();
      }

      return myDataTable;
    }

    private void InitGrid()
    {
      grdinvmain = new RadGrid();
      grdinvmain.ID = "grdinvmain";
      grdinvmain.DataSourceID = "InvMainSQLDataSource";
      grdinvmain.PageSize = 100;
      grdinvmain.AllowPaging = true;
      grdinvmain.AllowSorting = true;
      grdinvmain.MasterTableView.AllowMultiColumnSorting = true;
      grdinvmain.MasterTableView.AutoGenerateColumns = false;
      string sql = "Select * From v_InventoryStoreroom Where itemnum='" + m_itemnum + "'";
      if (Application["usedivision"].ToString().ToLower() == "yes")
      {
        if (Session["AllDivision"].ToString() != "")
        {
          sql = sql + " And (Division is null Or  Division in (" + Session["AllDivision"].ToString() + "))";
        }
        else
          sql = sql + " And (Division is null)";
      }
      InvMainSqlDataSource.SelectCommand = sql;
      
      InvMainSqlDataSource.ConnectionString = connstring;
      grdinvmain.ClientSettings.EnableAlternatingItems = false;
      InvPositionSqlDataSource.SelectCommand = "Select * From v_InventoryPosition";
      InvPositionSqlDataSource.ConnectionString = connstring;

      InvLotSqlDataSource.SelectCommand = "Select * From v_InventoryLot";
      InvLotSqlDataSource.ConnectionString = connstring;

      GridHyperLinkColumn IssueColumn = new GridHyperLinkColumn();
      IssueColumn.HeaderText = "Issue";
      IssueColumn.UniqueName = "IssueCommand";
      IssueColumn.ItemStyle.HorizontalAlign = HorizontalAlign.Left;
      IssueColumn.HeaderStyle.Width = 30;
      grdinvmain.MasterTableView.Columns.Add(IssueColumn);

      GridEditCommandColumn EditColumn = new GridEditCommandColumn();
      EditColumn.HeaderText = "Edit";
      EditColumn.UniqueName = "EditCommand";
      EditColumn.ButtonType = GridButtonColumnType.ImageButton;
      EditColumn.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
      EditColumn.HeaderStyle.Width = 30;
      grdinvmain.MasterTableView.Columns.Add(EditColumn);
      grdinvmain.MasterTableView.DataKeyNames = new string[] { "Counter","ItemNum","Storeroom","Division" };
      screen.SetGridColumns("invmain", grdinvmain);
      //grdinvmain.MasterTableView.CommandItemTemplate = new InsertFormItemTemplate("Inventory", null, "return editstoremain('');", m_allowedit);
      //grdinvmain.MasterTableView.CommandItemTemplate = new MultiFunctionItemTemplate(1,true,"Inventory",null,"return editstoremain('','" + m_itemnum + "');",m_allowedit,"",false);
      grdinvmain.MasterTableView.CommandItemDisplay = GridCommandItemDisplay.Top;
      grdinvmain.ItemDataBound+=new GridItemEventHandler(grdinvmain_ItemDataBound);
      grdinvmain.DetailTableDataBind+=new GridDetailTableDataBindEventHandler(grdinvmain_DetailTableDataBind);
      grdinvmain.MasterTableView.HierarchyLoadMode = GridChildLoadMode.Conditional;
      grdposition = new RadGrid();
      grdposition.AutoGenerateColumns = false;
      grdposition.DataSourceID = "InvPositionSqlDataSource";
      
      screen.SetGridColumns("position", grdposition);
      InvPositionSqlDataSource.SelectCommand = "Select * From v_InventoryPosition where itemnum=? and storeroom=?";

      grdlot = new RadGrid();
      grdlot.AutoGenerateColumns = false;
      grdlot.DataSourceID = "InvLotSqlDataSource";
      screen.SetGridColumns("lot", grdlot);
      InvLotSqlDataSource.SelectCommand = "Select * From v_InventoryLot where itemnum=? and storeroom=? and position=?";

      GridTableView positionview = new GridTableView();
      positionview.Name = "position";
      positionview.AutoGenerateColumns = false;
      //positionview.SkinID = "Telerik";
      positionview.HeaderStyle.CssClass = "InnerHeaderStyle";
      positionview.ItemStyle.CssClass = "InnerItemStyle";
      positionview.AlternatingItemStyle.CssClass = "InnerItemStyle";

      positionview.HierarchyLoadMode = GridChildLoadMode.Conditional;
      for (int i = 0; i < grdposition.Columns.Count; i++)
      {
        positionview.Columns.Add(grdposition.Columns[i]);
      }
      
      //positionview.DataSourceID = "InvPositionSqlDataSource";
      positionview.DataKeyNames = new string[] {"ItemNum","Storeroom","Position"};

      positionview.ShowHeadersWhenNoRecords = true;
      positionview.AutoGenerateColumns = false;

      GridTableView lotview = new GridTableView();
      lotview.Name = "lot";
      lotview.AutoGenerateColumns = false;
      lotview.HeaderStyle.CssClass = "MostInnerHeaderStyle";
      lotview.ItemStyle.CssClass = "MostInnerItemStyle";
      lotview.AlternatingItemStyle.CssClass = "MostInnerItemStyle";
      for (int i = 0; i < grdlot.Columns.Count; i++)
      {
        lotview.Columns.Add(grdlot.Columns[i]);
      }
      grdinvmain.MasterTableView.CommandItemTemplate = new InsertFormItemTemplate("Items", null, "return addnewitem()", 1, Session["UserGroup"].ToString());
      
      //lotview.DataSourceID = "InvLotSqlDataSource";
      lotview.DataKeyNames = new string[] { "ItemNum", "Storeroom", "Position", "LotNum", "Counter" };
      
      positionview.DetailTables.Add(lotview);
      grdinvmain.MasterTableView.DetailTables.Add(positionview);

      //grdinvmain.MasterTableView.DetailTables[0].DetailTables.Add(lotview);

      MainControlsPanel.Controls.Add(grdinvmain);

    }

    protected void grdinvmain_ItemDataBound(object sender, GridItemEventArgs e)
    {
      if (e.Item is GridDataItem && !e.Item.IsInEditMode)
      {
        GridDataItem item = (GridDataItem)e.Item;
        if (item.OwnerTableView.Name != "position" && item.OwnerTableView.Name != "lot")
        {
          ImageButton btn = item["EditCommand"].Controls[0] as ImageButton;
          if (btn != null)
          {
            btn.ImageUrl = "~/Images2/Edit.gif";
            btn.OnClientClick = "editstoremain('" + item.OwnerTableView.DataKeyValues[item.ItemIndex]["Counter"].ToString() + "','" + m_itemnum + "');return false;";
          }

          HyperLink h = item["IssueCommand"].Controls[0] as HyperLink;
          if (h != null)
          {
            Division d = new Division();
            string div1 = item["Division"].Text;
            string div = item.OwnerTableView.DataKeyValues[item.ItemIndex]["Division"].ToString();
            if (!d.Editable("'" + div + "'"))
            {
              h.Visible = false;
            }
            else
            {
              string itemnum = item.OwnerTableView.DataKeyValues[item.ItemIndex]["ItemNum"].ToString();
              string storeroom = item.OwnerTableView.DataKeyValues[item.ItemIndex]["Storeroom"].ToString();
              h.ImageUrl = "../images/inventory/issue_24.png";
              h.NavigateUrl = "javascript:issue('" + itemnum + "','" + storeroom + "')";
            }
          }
        }
      }
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!Page.IsPostBack)
        {
            string autonum = "";
            if (Request.QueryString["numbering"]!=null)
            {
              autonum = Request.QueryString["numbering"].ToString().ToLower();
            }
           
            if ((querymode == "new" && autonum == "auto") || querymode == "duplicate")
            {
              if (querymode == "duplicate")
              {
                screen.PopulateScreen("Items", nvcitems);
              } 
              NewNumber n = new NewNumber();
              TextBox t = MainControlsPanel.FindControl("txtitemnum") as TextBox;
              if (t != null)
                t.Text = n.GetNextNumber("Inventory");
            }
            if (querymode == "edit")
            {
                screen.PopulateScreen("Items", nvcitems);
                //HidDivision.Value = nvcitems["division"];
            }
            

            RadioButtonList r = MainControlsPanel.FindControl("rblinactive") as RadioButtonList;
            
            if (r != null)
            {
              r.Style.Add("valign", "top");
              r.RepeatDirection = RepeatDirection.Horizontal;
              ListItem litm1 = new ListItem("Yes", "1");
              r.Items.Add(litm1);
              ListItem litm2 = new ListItem("No", "0");
              r.Items.Add(litm2);
              if (querymode == "query")
              {
                ListItem litm3 = new ListItem("All", "2");
                r.Items.Add(litm3);
                r.SelectedIndex = 1;
              }
              else if (querymode == "edit")
              {
                if (nvcitems["inactive"].ToString() == "1")
                  r.SelectedIndex = 0;
                else
                  r.SelectedIndex = 1;
              }
              else
              {
                r.SelectedIndex = 1;
              }
            }

            r = MainControlsPanel.FindControl("rblserialized") as RadioButtonList;
            if (r != null)
            {
              r.RepeatDirection = RepeatDirection.Horizontal;
              ListItem litm1 = new ListItem("Yes", "1");
              r.Items.Add(litm1);
              ListItem litm2 = new ListItem("No", "0");
              r.Items.Add(litm2);
              if (querymode == "query")
              {
                ListItem litm3 = new ListItem("All", "2");
                r.Items.Add(litm3);
                r.SelectedIndex = 2;
              }
              else if (querymode == "edit")
              {
                if (nvcitems["serialized"].ToString() == "1")
                  r.SelectedIndex = 0;
                else
                  r.SelectedIndex = 1;
                r.Attributes.Add("onclick", "roradiobutton('rblserialized'," + r.SelectedIndex.ToString() + ")");
              }
              else
              {
                r.SelectedIndex = 1;
              }
            }

            r = MainControlsPanel.FindControl("rblstocked") as RadioButtonList;
            if (r != null)
            {
              r.RepeatDirection = RepeatDirection.Horizontal;
              ListItem litm1 = new ListItem("Yes", "1");
              r.Items.Add(litm1);
              ListItem litm2 = new ListItem("No", "0");
              r.Items.Add(litm2);
              if (querymode == "query")
              {
                ListItem litm3 = new ListItem("All", "2");
                r.Items.Add(litm3);
                r.SelectedIndex = 2;
              }
              else if (querymode == "edit")
              {
                if (nvcitems["stocked"].ToString() == "1")
                  r.SelectedIndex = 0;
                else
                  r.SelectedIndex = 1;
                r.Attributes.Add("onclick", "roradiobutton('rblstocked'," + r.SelectedIndex.ToString() + ")");
              }
              else
              {
                r.SelectedIndex = 1;
              }
            }

            InitialComboBoxes();
        }
        else
        {
            if (Request.Form["__EVENTTARGET"] == "Query")
            {
                Query();
            }
        }

        ucHeader1.Mode = querymode;
        ucHeader1.TabName = "Main";
        ucHeader1.ModuleData = nvcitems;

        string oper = "";
        if (querymode == "query")
        {
            oper = "Query";
        }
        else if (querymode == "new")
        {
            oper = "New";
        }
        else if (querymode == "duplicate")
        {
            oper = "Duplicate";
        }
        else if (querymode == "edit")
        {
            oper = "Edit";
        }
        ucHeader1.OperationLabel = oper;
        
        if (querymode == "edit" )
        {
          if (objItems.VendorCount == 0)
          {
            if (!Page.IsPostBack)
            {
              //litScript1.Text = "setTimeout('AddVendor();',300);";
              RadAjaxManager1.ResponseScripts.Add("AddVendor()");
            }
            else
              litScript1.Text = "";
          }
        }
        
    }

    private void InitialComboBoxes()
    {
      InitialComboBox("issueprice");
      InitialComboBox("purchaseprice");
      InitialComboBox("issuemethod");
              
    }

    private void InitialComboBox(string field)
    {
      RadComboBox rcb = MainControlsPanel.FindControl("cbb" + field) as RadComboBox;
      if (rcb != null)
      {
        if (querymode == "query")
        {
          RadComboBoxItem i = new RadComboBoxItem("All", "");
          i.Selected = true;
          rcb.Items.Add(i);
        }

        if (field == "issueprice")
        {
          

          RadComboBoxItem i = new RadComboBoxItem("AvgPrice", "AVGPRICE");
          if (nvcitems["issueprice"] == "AVGPRICE")
            i.Selected = true;
          rcb.Items.Add(i);

          i = new RadComboBoxItem("LastPrice", "LASTPRICE");
          if (nvcitems["issueprice"] == "LASTPRICE")
            i.Selected = true;
          rcb.Items.Add(i);

          i = new RadComboBoxItem("QuotedPrice", "QUOTEDPRICE");
          if (nvcitems["issueprice"] == "QUOTEDPRICE")
            i.Selected = true;
          rcb.Items.Add(i);

          i = new RadComboBoxItem("LotPrice", "LOTPRICE");
          if (nvcitems["issueprice"] == "LOTPRICE")
            i.Selected = true;
          rcb.Items.Add(i);

          i = new RadComboBoxItem("FixPrice", "FIXPRICE");
          if (nvcitems["issueprice"] == "FIXPRICE")
            i.Selected = true;
          rcb.Items.Add(i);

        }

        if (field == "issuemethod")
        {

          RadComboBoxItem i = new RadComboBoxItem("Mixed", "MIXED");
          if (nvcitems["issuemethod"] == "MIXED")
            i.Selected = true;
          rcb.Items.Add(i);

          i = new RadComboBoxItem("FIFO", "FIFO");
          if (nvcitems["issuemethod"] == "FIFO")
            i.Selected = true;
          rcb.Items.Add(i);

          i = new RadComboBoxItem("LIFO", "LIFO");
          if (nvcitems["issuemethod"] == "LIFO")
            i.Selected = true;
          rcb.Items.Add(i);
        }

        if (field == "purchaseprice")
        {
          RadComboBoxItem i = new RadComboBoxItem("QuotedPrice", "QUOTEDPRICE");
          if (nvcitems["purchaseprice"] == "QUOTEDPRICE")
            i.Selected = true;
          rcb.Items.Add(i);

          i = new RadComboBoxItem("LastPrice", "LASTPRICE");
          if (nvcitems["purchaseprice"] == "LASTPRICE")
            i.Selected = true;
          rcb.Items.Add(i);
        }
      }
    }

    private void Query()
    {
        NameValueCollection nvc;
        nvc = screen.CollectFormValues("Items", true);
        Items objItem = new Items(Session["Login"].ToString(), "Items", "ItemNum");

        string wherestring = "", wherestringlinq = "";
        //string[] empids = objEmpolyee.Query(nvc, ref wherestring);
        objItem.CreateLinqCondition(nvc, ref wherestring,ref wherestringlinq);
        string jscript = "";
        jscript += "var wherestring = '" + Server.UrlEncode(wherestringlinq) + "';var obj=parent.controlpanel.document.location.href='invpanel.aspx?showlist=queryresult&wherestring='+wherestring;";
        litScript1.Text = jscript;
    }

    private void RetrieveMessage()
    {
        SystemMessage msg = new SystemMessage("inventory/invmain.aspx");
        m_msg = msg.GetSystemMessage();
        msg.SetJsMessage(litMessage);
    }
}