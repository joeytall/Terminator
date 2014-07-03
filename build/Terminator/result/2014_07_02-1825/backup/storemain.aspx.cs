using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections.Specialized;
using Telerik.Web.UI;

public partial class inventory_storemain : System.Web.UI.Page
{
  protected NameValueCollection m_msg = new NameValueCollection();
  protected string m_itemnum = "";
  protected string m_storeroom = "";
  protected string m_position;
  protected string m_counter = "";
  protected string m_mode = "";
  protected Boolean serialized = false;
  protected AzzierScreen screen;
  protected RadGrid grdinvlot;

  protected NameValueCollection m_rights;
  protected Int16 m_allowedit;
  protected ModuleoObject objInventory;
  protected NameValueCollection nvc;

    protected void Page_Init(object sender, EventArgs e)
    {
      RetrieveMessage();
      if (Session["Login"] == null)
      {
        //Response.Write("<script>alert('Your session has expired. Please login again.');top.document.location.href='../Login.aspx';</script>");
        Response.Write("<script>alert('" + m_msg["T1"] + "');top.document.location.href='../Login.aspx';</script>");
        Response.End();
      }
      Session.LCID = Convert.ToInt32(Session["LCID"]);
      if (Request.QueryString["itemnum"] != null)
      {
        m_itemnum = Request.QueryString["itemnum"].ToString();
      }

      if (Request.QueryString["storeroom"] != null)
      {
        m_storeroom = Request.QueryString["storeroom"].ToString();
      } 

      if (Request.QueryString["counter"] != null)
      {
        m_counter = Request.QueryString["counter"].ToString();
        m_mode = "edit";
      }
      else
      {
        if (m_itemnum != "" || m_storeroom != "")
          m_mode = "new";
        else
        {
          Response.Write("<script>alert('" + m_msg["T2"] + "');top.document.location.href='../Login.aspx';</script>");
          Response.End();
        }
      }

      UserRights r = new UserRights(Session["Login"].ToString(), "UserRights", "counter");
      m_rights = r.GetRights(Session["Login"].ToString(), "Inventory");
      m_allowedit = Convert.ToInt16(m_rights["urEdit"].ToString());
      
      if (m_counter != "")
      {
        objInventory = new ModuleoObject(Session["Login"].ToString(), "v_InventoryStoreroom", "Counter", m_counter);
        nvc = objInventory.ModuleData;
        if (nvc["qtyonhand"] == "")
          nvc["qtyonhand"] = "0";
        if (nvc["lastprice"] == "")
          nvc["lastprice"] = "0";
        if (nvc["avgprice"] == "")
          nvc["avgprice"] = "0";
        if (nvc["quotedprice"] == "")
          nvc["quotedprice"] = "0";
        if (m_allowedit == 1)
        {
          Division d = new Division();
          if (!d.Editable("'" + nvc["Division"].ToString() + "'"))
          {
            m_allowedit = 0;
          }
        }
      }
      else
      {
        objInventory = new ModuleoObject(Session["Login"].ToString(), "v_InventoryStoreroom", "Counter");
        
        nvc = objInventory.ModuleData;
        nvc["itemnum"] = m_itemnum;
        nvc["storeroom"] = m_storeroom;
        nvc["qtyonhand"] = "0";
        nvc["lastprice"] = "0";
        nvc["avgprice"] = "0";
        nvc["quotedprice"] = "0";
        Items objitem = new Items(Session["Login"].ToString(), "Items", "ItemNum", m_itemnum);
        nvc["IssueMethod"] = objitem.ModuleData["IssueMethod"];
        //nvc["FixPrice"] = objitem.ModuleData["FixPrice"]; 
      }
      screen = new AzzierScreen("inventory/storemain.aspx", "MainForm", MainControlsPanel.Controls,m_mode);

      InitGrid();

      screen.LoadScreen();
      screen.SetValidationControls();
    }

    private void InitGrid()
    {
      grdinvlot = new RadGrid();
      grdinvlot.ID = "grdinvlot";
      grdinvlot.DataSourceID = "InvLotSQLDataSource";
      grdinvlot.PageSize = 100;
      grdinvlot.AllowPaging = true;
      grdinvlot.AllowSorting = true;
      grdinvlot.MasterTableView.AllowMultiColumnSorting = true;
      grdinvlot.MasterTableView.AutoGenerateColumns = false;
      InvLotSqlDataSource.SelectCommand = "Select * From v_InventoryLot Where itemnum='" + objInventory.ModuleData["ItemNum"] + "' And Storeroom='" + objInventory.ModuleData["Storeroom"] + "' And Inactive=0" ;
      InvLotSqlDataSource.ConnectionString = Application["ConnString"].ToString();

      if (m_allowedit == 1)
      {
        //GridHyperLinkColumn IssueColumn = new GridHyperLinkColumn();
        //IssueColumn.UniqueName = "IssueCommand";
        //IssueColumn.HeaderText = "Issue";
        //IssueColumn.HeaderStyle.Width = 30;
        //grdinvlot.MasterTableView.Columns.Add(IssueColumn);

        GridHyperLinkColumn TransferColumn = new GridHyperLinkColumn();
        TransferColumn.UniqueName = "TransferCommand";
        TransferColumn.HeaderText = "Transfer";
        TransferColumn.ImageUrl = "../images/inventory/transfer_24.png";
        TransferColumn.HeaderStyle.Width = 30;
        grdinvlot.MasterTableView.Columns.Add(TransferColumn);

        GridEditCommandColumn EditColumn = new GridEditCommandColumn();
        EditColumn.HeaderText = "Edit";
        EditColumn.UniqueName = "EditCommand";
        EditColumn.ButtonType = GridButtonColumnType.ImageButton;
        EditColumn.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
        EditColumn.HeaderStyle.Width = 30;
        grdinvlot.MasterTableView.Columns.Add(EditColumn);
      }
      grdinvlot.MasterTableView.DataKeyNames = new string[] { "Counter" };
      screen.SetGridColumns("invlot", grdinvlot);

      //GridBoundColumn c = grdinvlot.MasterTableView.GetColumn("Position") as GridBoundColumn;
      //if (c != null)
      //  grdinvlot.MasterTableView.GroupByExpressions.Add("Group By Position");
      //c = grdinvlot.MasterTableView.GetColumn("StockLevel") as GridBoundColumn;
      //if (c != null)
      //  c.Aggregate = GridAggregateFunction.Sum;

      if (nvc["issuemethod"] != "MIXED")
      {
        GridGroupByExpression exp = new GridGroupByExpression();
        GridGroupByField groupfield = new GridGroupByField();
        groupfield.FieldName = "Position";
        exp.SelectFields.Add(groupfield);
        exp.GroupByFields.Add(groupfield);

        GridGroupByField groupfield1 = new GridGroupByField();
        groupfield1.FieldName = "StockLevel";
        groupfield1.Aggregate = GridAggregateFunction.Sum;
        exp.SelectFields.Add(groupfield1);

        grdinvlot.MasterTableView.GroupByExpressions.Add(exp);
        grdinvlot.MasterTableView.GroupLoadMode = GridGroupLoadMode.Client;
      }
      else
      {
        for (int i = 0; i < grdinvlot.Columns.Count; i++)
        {
          if (grdinvlot.Columns[i].UniqueName.ToLower() == "cost")
            grdinvlot.Columns[i].Visible = false;
        }
      }

      //grdinvmain.MasterTableView.CommandItemTemplate = new InsertFormItemTemplate("Inventory", null, "return editstoremain('');", m_allowedit);
      if (m_counter != "")
        grdinvlot.MasterTableView.CommandItemTemplate = new MultiFunctionItemTemplate(1, true, "InvLot", null, "return editlot('');", m_allowedit, "", false);
      else
        grdinvlot.MasterTableView.CommandItemTemplate = new MultiFunctionItemTemplate("InvLot");
      grdinvlot.MasterTableView.CommandItemDisplay = GridCommandItemDisplay.Top;

      grdinvlot.ItemDataBound += new GridItemEventHandler(grdinvlot_ItemDataBound);

      MainControlsPanel.Controls.Add(grdinvlot);

    }

    protected void grdinvlot_ItemDataBound(object sender, GridItemEventArgs e)
    {
      if (e.Item is GridDataItem && !e.Item.IsInEditMode && m_allowedit == 1)
      {
        GridDataItem item = (GridDataItem)e.Item;

        ImageButton btn = (ImageButton)item["EditCommand"].Controls[0];
        btn.ImageUrl = "~/Images/Edit.gif";
        btn.OnClientClick = "return editlot('" + item.OwnerTableView.DataKeyValues[item.ItemIndex]["Counter"].ToString() + "','" + m_counter + "')";

        //HyperLink issue = (HyperLink)item["IssueCommand"].Controls[0];
        //issue.NavigateUrl = "javascript:issue('" + item.OwnerTableView.DataKeyValues[item.ItemIndex]["Counter"].ToString() + "');";
        //issue.ImageUrl = "../images/inventory/issue_24.png";

        HyperLink transfer = (HyperLink)item["TransferCommand"].Controls[0];
        //transfer.Text = "Transfer";
        transfer.NavigateUrl = "javascript:transfer('" + item.OwnerTableView.DataKeyValues[item.ItemIndex]["Counter"].ToString() + "');";
      }
    }

    protected void Page_Load(object sender, EventArgs e)
    {
      TextBox t;
      if (!Page.IsPostBack)
      {
        t = MainControlsPanel.FindControl("txtitemnum") as TextBox;
        //if (t != null)
        //t.Attributes.Add("readonly", "readonly");
        screen.PopulateScreen("v_InventoryStoreroom", nvc);
        if (m_counter != "")
        {
          t = MainControlsPanel.FindControl("txtlastprice") as TextBox;
          if (t != null)
            t.Attributes.Add("readonly", "readonly");

          t = MainControlsPanel.FindControl("txtavgprice") as TextBox;
          if (t != null)
            t.Attributes.Add("readonly", "readonly");

          t = MainControlsPanel.FindControl("txtqtyonhand") as TextBox;
          if (t != null)
            t.Attributes.Add("readonly", "readonly");

          screen.SetTextControlReadonly("itemnum", MainControlsPanel);
          screen.SetTextControlReadonly("storeroom", MainControlsPanel);
        }
        else
        {
          t.Text = m_itemnum;
          t = MainControlsPanel.FindControl("txtqtyonhand") as TextBox;
          if (t != null && serialized)
            t.Attributes.Add("readonly", "readonly");
          if (m_itemnum != "")
          {
            Items objitem = new Items(Session["Login"].ToString(), "Items", "ItemNum", m_itemnum);
            t = MainControlsPanel.FindControl("txtitemdesc") as TextBox;
            if (t != null)
              t.Text = objitem.ModuleData["ItemDesc"];

            t = MainControlsPanel.FindControl("txtdefvendor") as TextBox;
            if (t != null)
              t.Text = objitem.ModuleData["Vendor"];

            t = MainControlsPanel.FindControl("txtdefmanufacturer") as TextBox;
            if (t != null)
              t.Text = objitem.ModuleData["Manufacturer"];
            t = MainControlsPanel.FindControl("txtvendorpart") as TextBox;
            if (t != null)
              t.Text = objitem.ModuleData["vendorpart"];

            screen.SetTextControlReadonly("itemnum", MainControlsPanel);
          }
          if (m_storeroom != "")
          {
            screen.SetTextControlReadonly("storeroom", MainControlsPanel);
          }
        }
        if (m_allowedit == 0)
        {
          btndelete.Visible = false;
          btnsave.Visible = false;
          btnupdateavgprice.Visible = false;
          btnupdatelastprice.Visible = false;
        }
        if (m_counter == "")
        {
          btndelete.Visible = false;
        }

        RadioButtonList r  = MainControlsPanel.FindControl("rblreordermethod") as RadioButtonList;
        if (r != null)
        {
          r.RepeatDirection = RepeatDirection.Horizontal;
          r.Items.Add("MINMAX");
          r.Items.Add("EOQ");
          r.SelectedIndex = 0;
          if (m_counter != "")
            if (nvc["reordermethod"].ToString().ToUpper() == "EOQ")
              r.SelectedIndex = 1;
        }
      }
      else
      {
        if (m_counter != "")
        {
          t = MainControlsPanel.FindControl("txtqtyonhand") as TextBox;
          if (t != null)
            t.Text = nvc["QtyOnHand"].ToString();

          t = MainControlsPanel.FindControl("txtqtyonorder") as TextBox;
          if (t != null)
            t.Text = nvc["QtyOnOrder"];
          t = MainControlsPanel.FindControl("txtqtyreserved") as TextBox;
          if (t != null)
            t.Text = nvc["QtyReserved"];

          t = MainControlsPanel.FindControl("txtlastprice") as TextBox;
          if (t != null)
            t.Text = nvc["LastPrice"];
          t = MainControlsPanel.FindControl("txtavgprice") as TextBox;
          if (t != null)
            t.Text = nvc["AvgPrice"];
          t = MainControlsPanel.FindControl("txtquotedprice") as TextBox;
          if (t != null)
            t.Text = nvc["QuotedPrice"];

        }
      }

      hidMode.Value = m_mode;
    }

    private void RetrieveMessage()
    {
     // SystemMessage msg = new SystemMessage("workorder/batchclose.aspx");
        SystemMessage msg = new SystemMessage("pm/pmseason.aspx");
      m_msg = msg.GetSystemMessage();
      msg.SetJsMessage(litMessage);

    }

    private int GetColumnIndexByName(RadGrid grid, string name)
    {
      foreach (GridColumn col in grid.Columns)
      {
        if (col.UniqueName.ToLower().Trim() == name.ToLower().Trim())
        {
          return grid.Columns.IndexOf(col);
        }
      }

      return -1;
    }

}