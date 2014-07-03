using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections.Specialized;
using Telerik.Web.UI;
using System.DirectoryServices.ActiveDirectory;
using System.Data;

public partial class inventory_issuereserve : System.Web.UI.Page
{
  protected NameValueCollection m_msg = new NameValueCollection();
  protected AzzierScreen screen;
  protected RadGrid grdissuelist;
  protected string m_storeroom = "";
  protected string returnfrom = "";
  protected string returnfromcode = "";
  protected string fromdate = "";
  protected string todate = "";

    protected void Page_Init(object sender, EventArgs e)
    {
      RetrieveMessage();
      UserRights.CheckAccess('');
      Session.LCID = Convert.ToInt32(Session["LCID"]);

      screen = new AzzierScreen("inventory/return.aspx", "MainForm", MainControlsPanel.Controls);
      InitGrid();
      screen.LoadScreen();
      screen.SetValidationControls();
    }

    protected void Filter(object sender, EventArgs e)
    {
      TextBox t = MainControlsPanel.FindControl("txtfromdate") as TextBox;
      fromdate = t.Text;
      t = MainControlsPanel.FindControl("txttodate") as TextBox;
      todate = t.Text;
      RadComboBox cbb = MainControlsPanel.FindControl("cbbreturnfrom") as RadComboBox;
      returnfrom = cbb.SelectedValue;
      t = MainControlsPanel.FindControl("txtreturnfromcode") as TextBox;
      returnfromcode = t.Text;
      string sql = "Select * from v_Return";

      Validation v = new Validation();
      string filterstr = "TransType^ISSUE,Storeroom^<>null,Quantity^>0";
      if (fromdate != "")
      {
        filterstr = filterstr + ",TransDate^>=" + fromdate;
      }
      if (todate != "")
      {
        filterstr = filterstr + ",TransDate^<=" + todate;
      }
      if (returnfrom != "")
      {
        filterstr = filterstr + ",IssueType^" + returnfrom;
      }
      if (returnfromcode != "")
      {
        filterstr = filterstr + ",Number^" + returnfromcode;
      }
      string wherestr = v.AddConditions(filterstr, "inventory/return.aspx", "", "v_Return");

      IssueListSqlDataSource.SelectCommand = "Select * from v_Return " + wherestr;
    }

    protected void Page_Load(object sender, EventArgs e)
    {
      if (!Page.IsPostBack)
      {
        RadComboBox cbb = MainControlsPanel.FindControl("cbbreturnfrom") as RadComboBox;
        AddComboBoxReturnFromItem("", cbb);
        AddComboBoxReturnFromItem("WORKORDER", cbb);
        AddComboBoxReturnFromItem("EQUIPMENT", cbb);
        AddComboBoxReturnFromItem("LOCATION", cbb);
        AddComboBoxReturnFromItem("EMPLOYEE", cbb);
        AddComboBoxReturnFromItem("ACCOUNT", cbb);
        AddComboBoxReturnFromItem("REQUISITION", cbb);
        cbb.OnClientSelectedIndexChanged = "updatereturnfrom";

        TextBox t = MainControlsPanel.FindControl("txtreturndate") as TextBox;
        if (t!=null)
          t.Text = DateTime.Today.ToShortDateString();

        t = MainControlsPanel.FindControl("txttodate") as TextBox;
        if (t != null)
          t.Text = DateTime.Today.ToShortDateString();

        t = MainControlsPanel.FindControl("txtfromdate") as TextBox;
        if (t != null)
          t.Text = DateTime.Today.AddMonths(-1).ToShortDateString();

        Filter(null,null);
      }
    }

    private void AddComboBoxReturnFromItem(string value, RadComboBox cbb)
    {
      if (cbb != null)
      {
        RadComboBoxItem item = new RadComboBoxItem(value, value);
        if (returnfrom == value)
          item.Selected = true;
        cbb.Items.Add(item);
      }
    }

    private void InitGrid()
    {
      grdissuelist = new RadGrid();
      grdissuelist.ID = "grdissuelist";
      grdissuelist.DataSourceID = "IssueListSQLDataSource";
      grdissuelist.PageSize = 100;
      grdissuelist.AllowPaging = true;
      grdissuelist.AllowFilteringByColumn = true;
      grdissuelist.AllowSorting = true;
      grdissuelist.MasterTableView.AllowMultiColumnSorting = true;
      grdissuelist.MasterTableView.AutoGenerateColumns = false;
      grdissuelist.ItemDataBound += new GridItemEventHandler(grdissuelist_ItemDataBound);
      grdissuelist.PreRender += new EventHandler(grdissuelist_PreRender);
      grdissuelist.ItemCreated +=new GridItemEventHandler(grdissuelist_ItemCreated);
      grdissuelist.MasterTableView.EditMode = GridEditMode.InPlace;
      grdissuelist.ShowFooter = true;
      grdissuelist.AllowMultiRowEdit = true;
      grdissuelist.ClientSettings.Scrolling.AllowScroll = true;
      grdissuelist.ClientSettings.Scrolling.UseStaticHeaders = true;
      grdissuelist.FooterStyle.HorizontalAlign = HorizontalAlign.Right;
      grdissuelist.MasterTableView.ClientDataKeyNames = new string[] {"BatchNum","Quantity","IssueMethod","Serialized"};

      IssueListSqlDataSource.ConnectionString = Application["ConnString"].ToString();
      Validation v = new Validation();
      string filterstr = "TransType^ISSUE,Storeroom^<>null,Quantity^>0";
      string wherestr = v.AddConditions(filterstr, "inventory/return.aspx", "itemlist", "v_Return");
      IssueListSqlDataSource.SelectCommand = "Select * from v_Return" + wherestr ;



      screen.SetGridColumns("issuelist", grdissuelist);
      for (int i = 0; i < grdissuelist.Columns.Count; i++)
      {
        GridBoundColumn col = grdissuelist.Columns[i] as GridBoundColumn;
        if (col != null)
        {
          if (col.UniqueName.ToLower() != "returnqty")
            col.ReadOnly = true;
        }
      }

      GridHyperLinkColumn issuedetialcol = new GridHyperLinkColumn();
      issuedetialcol.HeaderStyle.Width = 60;
      issuedetialcol.ImageUrl = "../images/edit.gif";
      issuedetialcol.HeaderText = "Detail";
      issuedetialcol.UniqueName = "Detail";
      issuedetialcol.AllowFiltering = false;
      issuedetialcol.AllowSorting = false;
      grdissuelist.Columns.Add(issuedetialcol);

      MainControlsPanel.Controls.Add(grdissuelist);

    }

    protected void grdissuelist_ItemCreated(object sender, GridItemEventArgs e)
    {
      screen.GridItemCreated(e, "inventory/return.aspx", "MainForm", "issuelist", grdissuelist);
    }

    private void grdissuelist_PreRender(object sender, System.EventArgs e)
    {
      foreach (GridItem item in grdissuelist.MasterTableView.Items)
      {
        if (item is GridEditableItem)
        {
          GridEditableItem editableItem = item as GridDataItem;
          editableItem.Edit = true;
        }
      }
      grdissuelist.Rebind();
    }

    protected void grdissuelist_ItemDataBound(object sender, GridItemEventArgs e)
    {
      if (e.Item is GridEditableItem && e.Item.IsInEditMode)
      {
        GridEditableItem editedItem = (GridEditableItem)e.Item;
        DataRowView item = (DataRowView)editedItem.DataItem;

        //(editedItem["Detail"].Controls[0] as HyperLink).NavigateUrl = "javascript:detail('" + item["Counter"].ToString() + "','" + item["ReqLineCounter"].ToString() + "')";
        (editedItem["Detail"].Controls[0] as HyperLink).NavigateUrl = "javascript:detail('" + editedItem.ItemIndex.ToString() + "')";
        
        if (editedItem["ReturnQty"] != null)
        {
          TextBox t = editedItem["ReturnQty"].Controls[0] as TextBox;
          t.Text = "";
          /*
          if (Convert.ToDecimal(item["Serialized"]) == 1)

            t.ReadOnly = true;
          else
           * */
            t.Attributes.Add("onchange", "checkqty('" + editedItem.ItemIndex.ToString() + "')");
        }
      }
      screen.GridItemDataBound(e, "inventory/return.aspx", "MainForm","issuelist");
    }

    private void RetrieveMessage()
    {
     // SystemMessage msg = new SystemMessage("workorder/batchclose.aspx");
        SystemMessage msg = new SystemMessage("pm/pmseason.aspx");
      m_msg = msg.GetSystemMessage();
      msg.SetJsMessage(litMessage);

    }

}