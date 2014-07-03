using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.OleDb;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Web.UI;

public partial class Codes_Acctlist : System.Web.UI.Page
{
  protected AzzierScreen screen;
  protected RadGrid grdacctlist;

  protected string mode = "";
  protected string runtimefilter = "";
  protected string designtimefilter = "";
  protected string fieldlist = "";
  protected string referer = "";
  protected bool found = false;
  protected string wherestr = "";
  protected string controlid = "";
  protected string fieldid = "";
  protected string totalCount = "";
  protected string tablename = "Accounts";
  protected string filename = "";
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

    wherestr = v.AddLinqConditions(filterstrlinq, filename, controlid, tablename,null,null,mode);

    screen = new AzzierScreen("codes/acctlist.aspx", "MainForm", MainControlsPanel.Controls);

    grdacctlist = new RadGrid();
    grdacctlist.ID = "grdacctlist";
    grdacctlist.ClientSettings.Scrolling.AllowScroll = true;
    grdacctlist.ClientSettings.Scrolling.SaveScrollPosition = true;
    grdacctlist.ClientSettings.Scrolling.UseStaticHeaders = true;
    grdacctlist.MasterTableView.TableLayout = GridTableLayout.Fixed;
    grdacctlist.PagerStyle.Visible = true;
    grdacctlist.PagerStyle.AlwaysVisible = true;
    grdacctlist.Skin = "Outlook";

    grdacctlist.Attributes.Add("rules", "all");

    grdacctlist.AutoGenerateColumns = false;
    grdacctlist.AllowPaging = true;
    grdacctlist.PageSize = 100;
    grdacctlist.AllowSorting = true;
    grdacctlist.MasterTableView.AllowMultiColumnSorting = true;
    grdacctlist.AllowFilteringByColumn = true;
    grdacctlist.MasterTableView.CommandItemDisplay = GridCommandItemDisplay.Top;
    grdacctlist.MasterTableView.DataKeyNames = new string[] { "Account", "BudgetType" };
    grdacctlist.MasterTableView.ClientDataKeyNames = new string[] { "Account" };
    grdacctlist.ClientSettings.Selecting.AllowRowSelect = true;
    grdacctlist.ClientSettings.ClientEvents.OnRowSelected = "getGridSelectedItems";
    grdacctlist.ClientSettings.EnableRowHoverStyle = true;

    if (referer == "Admin")
    {
      grdacctlist.MasterTableView.CommandItemTemplate = new InsertFormItemTemplate("Account Detail", null, "return EditAccount('')", 1, "Admin");
    }
    else
    {
      //grdacctlist.MasterTableView.CommandItemTemplate = new InsertFormItemTemplate("Account Detail", null, "return EditAccount('')", 1, "");
      grdacctlist.MasterTableView.CommandItemTemplate = new InsertFormItemTemplate("Account Detail");
    }

    grdacctlist.ClientSettings.DataBinding.SelectMethod = "LookupDataAndCount?wherestr=" + wherestr + "&mode=" + mode;
    grdacctlist.ClientSettings.DataBinding.Location = "../InternalServices/ServiceAcct.svc";
    grdacctlist.ClientSettings.DataBinding.SortParameterType = GridClientDataBindingParameterType.Linq;
    grdacctlist.ClientSettings.DataBinding.FilterParameterType = GridClientDataBindingParameterType.Linq;
    //grdacctlist.MasterTableView.VirtualItemCount = 10;

    if (referer == "Admin")
    {
      GridButtonColumn gridbutcol = new GridButtonColumn();
      gridbutcol.UniqueName = "EditAccount";
      gridbutcol.HeaderText = "Edit";
      gridbutcol.ImageUrl = "~/Images2/Edit.gif";
      gridbutcol.HeaderStyle.Width = 20;
      gridbutcol.ButtonType = GridButtonColumnType.ImageButton;
      grdacctlist.MasterTableView.Columns.Add(gridbutcol);
    }

    screen.SetGridColumns("acctlist", grdacctlist);

    grdacctlist.ItemCreated += new GridItemEventHandler(grdacctlist_ItemCreated);
    /*
    grdacctlist.DeleteCommand += new GridCommandEventHandler(grdacctlist_DeleteCommand);
    grdacctlist.InsertCommand += new GridCommandEventHandler(grdacctlist_InsertCommand);
    grdacctlist.UpdateCommand += new GridCommandEventHandler(grdacctlist_UpdateCommand);
     * */
    grdacctlist.ItemDataBound += new GridItemEventHandler(grdacctlist_ItemDataBound);

    MainControlsPanel.Controls.Add(grdacctlist);

    screen.LoadScreen();
  }

  protected void Page_Load(object sender, EventArgs e)
  {
    hidFieldId.Value = fieldid;
    hidControlId.Value = AzzierData.ActualFieldName("Accounts",controlid);
  }

  protected void RadAjaxManager1_AjaxRequest(object sender, AjaxRequestEventArgs e)
  {
    grdacctlist.PageSize = 1 + grdacctlist.PageSize;
    grdacctlist.Rebind();
  }

  protected void grdacctlist_ItemCreated(object sender, GridItemEventArgs e)
  {
    if (e.Item is GridCommandItem)
    {
      if (referer == "Admin")
        if (e.Item.FindControl("InitInsertButton") != null)
        {
          e.Item.FindControl("InitInsertButton").Visible = false;
        }
    }
    GridDataItem dataItem = e.Item as GridDataItem;
    if (dataItem != null && referer == "Admin")
    {
      ImageButton button = dataItem["EditAccount"].Controls[0] as ImageButton;
      //button.OnClientClick = "EditAccount(" + dataItem.OwnerTableView.DataKeyValues[dataItem.ItemIndex]["Account"].ToString() + "); return false;"; 
      button.OnClientClick = "EditAccount(" + dataItem.ItemIndex + "); return false;";
      //int i = e.Item.
    }

    screen.GridItemCreated(e, "codes/acctlist.aspx", "MainForm", "results", grdacctlist);
  }
  /*
  protected void grdacctlist_DeleteCommand(object source, Telerik.Web.UI.GridCommandEventArgs e)
  {
    GridDataItem item = (GridDataItem)e.Item;
    string counter = item.OwnerTableView.DataKeyValues[item.ItemIndex]["Account"].ToString();

    ModuleoObject obj = new ModuleoObject(Session["Login"].ToString(), tablename, "Account", counter);
    bool success = obj.Delete();
    if (!success)
    {
      grdacctlist.Controls.Add(new LiteralControl(m_msg["T3"] + obj.ErrorMessage));
      e.Canceled = true;
    }
  }

  protected void grdacctlist_InsertCommand(object source, Telerik.Web.UI.GridCommandEventArgs e)
  {
    NameValueCollection nvcFT = screen.GetGridFieldTypes("acctlist", tablename);
    string[] fields = nvcFT.AllKeys;
    NameValueCollection nvc = new NameValueCollection();
    DateFormat objDateFormat = new DateFormat(Session.LCID);
    GridEditableItem editedItem = e.Item as GridEditableItem;
    if (e.Item.OwnerTableView.EditMode == GridEditMode.InPlace)
    {
      //Get the primary key value using the DataKeyValue.
      foreach (string field in fields)
      {
        if (field != "Account")
        {
          string dbtype = nvcFT[field].ToString();
          if (dbtype == "system.string" || dbtype == "system.decimal")
          {
            nvc.Add(field, (editedItem[field].Controls[0] as TextBox).Text);
          }
          else if (dbtype == "system.datetime")
          {
            string sdate = (editedItem[field].Controls[0] as RadDatePicker).SelectedDate.ToString();
            string result = "";
            bool a = objDateFormat.ValidateInputDate(sdate, out result);    // convert date from screen format to db date format
            sdate = result;
            nvc.Add(field, sdate);
          }
        }
      }
    }

    ModuleoObject obj = new Resources(Session["Login"].ToString(), tablename, "Account");
    bool success = obj.Create(nvc);
    if (!success)
    {
      grdacctlist.Controls.Add(new LiteralControl(m_msg["T4"] + obj.ErrorMessage));
      e.Canceled = true;
    }
  }

  protected void grdacctlist_UpdateCommand(object source, Telerik.Web.UI.GridCommandEventArgs e)
  {
    NameValueCollection nvcFT = screen.GetGridFieldTypes("acctlist", tablename);
    string[] fields = nvcFT.AllKeys;
    NameValueCollection nvc = new NameValueCollection();
    DateFormat objDateFormat = new DateFormat(Session.LCID);
    string counter = "";
    GridEditableItem editedItem = e.Item as GridEditableItem;
    counter = editedItem.OwnerTableView.DataKeyValues[editedItem.ItemIndex]["Account"].ToString();
    if (e.Item.OwnerTableView.EditMode == GridEditMode.InPlace)
    {
      //Get the primary key value using the DataKeyValue.
      foreach (string field in fields)
      {
        if (field != "Account")
        {
          string dbtype = nvcFT[field].ToString();
          if (dbtype == "system.string" || dbtype == "system.decimal")
          {
            nvc.Add(field, (editedItem[field].Controls[0] as TextBox).Text);
          }
          else if (dbtype == "system.datetime")
          {
            string sdate = (editedItem[field].Controls[0] as RadDatePicker).SelectedDate.ToString();
            string result = "";
            bool a = objDateFormat.ValidateInputDate(sdate, out result);    // convert date from screen format to db date format
            sdate = result;
            nvc.Add(field, sdate);
          }
        }
      }
    }

    ModuleoObject obj = new Resources(Session["Login"].ToString(), tablename, "Account", counter);
    bool success = obj.Update(nvc);
    if (!success)
    {
      grdacctlist.Controls.Add(new LiteralControl(m_msg["T5"] + obj.ErrorMessage));
      e.Canceled = true;
    }
  }
  */
  protected void grdacctlist_ItemDataBound(object sender, GridItemEventArgs e)
  {
    screen.GridItemDataBound(e, "codes/acctlist.aspx", "MainForm", "acctlist");

    if (e.Item is GridCommandItem)
    {
      Button addButton = e.Item.FindControl("addFormButton") as Button;
      if (addButton != null)
      {
        addButton.Visible = false;
      }
      LinkButton lnkButton = (LinkButton)e.Item.FindControl("InitInsertButton");
      if (lnkButton != null)
      {
        lnkButton.Visible = false;
      }
    }

    /*if (e.Item is GridDataItem)
    {
      GridDataItem item = (GridDataItem)e.Item;

      if (item.OwnerTableView.DataKeyValues[item.ItemIndex]["BudgetType"].ToString() == "1")
      {
        TextBox txt = e.Item.FindControl("txtbudgettype") as TextBox;
        if (txt != null)
        {
          txt.Text = "Fiscal";
        }
      }
    }*/
  }

  private void RetrieveMessage()
  {
    SystemMessage msg = new SystemMessage("codes/acctlist.aspx");
    //m_msg = msg.GetSystemMessage();
    //SystemMessage msg = new SystemMessage();
    msg.SetJsMessage(litMessage);
  }
}