using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.OleDb;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Web.UI;

public partial class Codes_TaskLibrary : System.Web.UI.Page
{
  protected AzzierScreen screen;
  protected RadGrid grdtasklist;

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
  protected string tablename = "WOTasks";
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

    screen = new AzzierScreen("codes/tasklibrary.aspx", "MainForm", MainControlsPanel.Controls);

    grdtasklist = new RadGrid();
    grdtasklist.ID = "grdtasklist";
    grdtasklist.ClientSettings.Scrolling.AllowScroll = true;
    grdtasklist.ClientSettings.Scrolling.SaveScrollPosition = true;
    grdtasklist.ClientSettings.Scrolling.UseStaticHeaders = true;
    grdtasklist.MasterTableView.TableLayout = GridTableLayout.Fixed;
    grdtasklist.PagerStyle.Visible = true;
    grdtasklist.PagerStyle.AlwaysVisible = true;
    grdtasklist.Skin = "Outlook";

    grdtasklist.Attributes.Add("rules", "all");

    grdtasklist.AutoGenerateColumns = false;
    grdtasklist.AllowPaging = true;
    grdtasklist.PageSize = 100;
    grdtasklist.AllowSorting = true;
    grdtasklist.MasterTableView.AllowMultiColumnSorting = true;
    grdtasklist.AllowFilteringByColumn = true;
    grdtasklist.MasterTableView.CommandItemDisplay = GridCommandItemDisplay.Top;
    grdtasklist.MasterTableView.DataKeyNames = new string[] { "Counter" };
    grdtasklist.MasterTableView.ClientDataKeyNames = new string[] { "Counter" };
    grdtasklist.ClientSettings.Selecting.AllowRowSelect = true;
    if (referer == "tasklibrary")
    {
    }
    else
    {
      grdtasklist.ClientSettings.ClientEvents.OnRowSelected = "getGridSelectedItems";
    }
    
    grdtasklist.ClientSettings.EnableRowHoverStyle = true;

    if (referer == "tasklibrary")
    {
      grdtasklist.MasterTableView.CommandItemTemplate = new InsertFormItemTemplate("Task Library", null, "return EditAccount('')", 1, "Admin");
    }
    else
    {
      //grdacctlist.MasterTableView.CommandItemTemplate = new InsertFormItemTemplate("Account Detail", null, "return EditAccount('')", 1, "");
      grdtasklist.MasterTableView.CommandItemTemplate = new InsertFormItemTemplate("Taslk Library");
    }

    grdtasklist.ClientSettings.DataBinding.SelectMethod = "GetTaskLibrary?wherestr=" + wherestr;
    grdtasklist.ClientSettings.DataBinding.Location = "../InternalServices/ServiceProc.svc";
    grdtasklist.ClientSettings.DataBinding.SortParameterType = GridClientDataBindingParameterType.Linq;
    grdtasklist.ClientSettings.DataBinding.FilterParameterType = GridClientDataBindingParameterType.Linq;
    //grdacctlist.MasterTableView.VirtualItemCount = 10;

    if (referer == "tasklibrary")
    {
      GridButtonColumn gridbutcol = new GridButtonColumn();
      gridbutcol.UniqueName = "Edit";
      gridbutcol.HeaderText = "Edit";
      gridbutcol.ImageUrl = "~/Images2/Edit.gif";
      gridbutcol.HeaderStyle.Width = 20;
      gridbutcol.ButtonType = GridButtonColumnType.ImageButton;
      grdtasklist.MasterTableView.Columns.Add(gridbutcol);
    }

    screen.SetGridColumns("tasklist", grdtasklist);

    grdtasklist.ItemCreated += new GridItemEventHandler(grdtasklist_ItemCreated);
    /*
    grdacctlist.DeleteCommand += new GridCommandEventHandler(grdacctlist_DeleteCommand);
    grdacctlist.InsertCommand += new GridCommandEventHandler(grdacctlist_InsertCommand);
    grdacctlist.UpdateCommand += new GridCommandEventHandler(grdacctlist_UpdateCommand);
     * */
    grdtasklist.ItemDataBound += new GridItemEventHandler(grdtasklist_ItemDataBound);

    MainControlsPanel.Controls.Add(grdtasklist);

    screen.LoadScreen();
  }

  protected void Page_Load(object sender, EventArgs e)
  {
    hidFieldId.Value = fieldid;
    hidControlId.Value = AzzierData.ActualFieldName("WOTasks",controlid);
  }

  protected void RadAjaxManager1_AjaxRequest(object sender, AjaxRequestEventArgs e)
  {
    grdtasklist.PageSize = 1 + grdtasklist.PageSize;
    grdtasklist.Rebind();
  }

  protected void grdtasklist_ItemCreated(object sender, GridItemEventArgs e)
  {
    if (e.Item is GridCommandItem)
    {
      if (referer == "TaskLibrary")
        if (e.Item.FindControl("InitInsertButton") != null)
        {
          e.Item.FindControl("InitInsertButton").Visible = false;
        }
    }
    GridDataItem dataItem = e.Item as GridDataItem;
    if (dataItem != null && referer == "TaskLibrary")
    {
      ImageButton button = dataItem["Edit"].Controls[0] as ImageButton;
      //button.OnClientClick = "EditAccount(" + dataItem.OwnerTableView.DataKeyValues[dataItem.ItemIndex]["Account"].ToString() + "); return false;"; 
      button.OnClientClick = "EditAccount(" + dataItem.ItemIndex + "); return false;";
      //int i = e.Item.
    }

    screen.GridItemCreated(e, "codes/tasklibrary.aspx", "MainForm", "results", grdtasklist);
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
  protected void grdtasklist_ItemDataBound(object sender, GridItemEventArgs e)
  {
    screen.GridItemDataBound(e, "codes/tasklibrary.aspx", "MainForm", "tasklist");

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
  }

  private void RetrieveMessage()
  {
    SystemMessage msg = new SystemMessage("codes/acctlist.aspx");
    //m_msg = msg.GetSystemMessage();
    //SystemMessage msg = new SystemMessage();
    msg.SetJsMessage(litMessage);
  }
}