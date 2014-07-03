using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.OleDb;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Web.UI;

public partial class inventory_invspecs : System.Web.UI.Page
{
    private string m_itemnum = "";
    protected RadGrid grdspecs;
    protected AzzierScreen screen;
    protected NameValueCollection m_rights;
    protected int m_allowedit = 0;

    protected NameValueCollection m_msg = new NameValueCollection();

    protected void Page_Init(object sender, EventArgs e)
    {
        RetrieveMessage();
        UserRights.CheckAccess('');
        Session.LCID = Convert.ToInt32(Session["LCID"]);

        UserRights r = new UserRights(Session["Login"].ToString(), "UserRights", "counter");
        m_rights = r.GetRights(Session["Login"].ToString(), "Inventory");
        m_allowedit = Convert.ToInt16(m_rights["urEdit"].ToString()); ;

        if (Request.QueryString["itemnum"] == null)
        {
            //Response.Write("<script>alert('Illegal page access.'); top.document.location.href='../Login.aspx';</script>");
            Response.Write("<script>alert('" + m_msg["T2"] + "'); top.document.location.href='../Login.aspx';</script>");
            Response.End();
        }
        else
          m_itemnum = Request.QueryString["itemnum"].ToString();

        string connstring = Application["ConnString"].ToString();
        grdspecs = new RadGrid();
        grdspecs.ID = "grdspecs";
        grdspecs.DataSourceID = "SpecsSqlDataSource";
        grdspecs.ShowFooter = true;
        grdspecs.ShowHeader = true;
        grdspecs.MasterTableView.ShowHeadersWhenNoRecords = true;
        grdspecs.AutoGenerateColumns = false;
        //grdspecs.MasterTableView.CommandItemTemplate = new CommandItemTemplate(1, true, "Specifications", "", "return addTemplate('" + m_location + "','location')", m_allowedit,"specification");

        screen = new AzzierScreen("inventory/invspecs.aspx", "MainForm", MainControlsPanel.Controls);
        screen.LCID = Session.LCID;
        //screen.Top = 20;

        SpecsSqlDataSource.ConnectionString = connstring;
        SpecsSqlDataSource.SelectCommand = "SELECT * FROM specification WHERE linktype='inventory' and LinkId='" + m_itemnum + "' And IsTemplate=0";
        grdspecs.MasterTableView.DataKeyNames = new string[] { "Counter" };
        grdspecs.Width = Unit.Percentage(90);
        grdspecs.MasterTableView.EditMode = GridEditMode.InPlace;


        if (m_allowedit == 1)
        {
            GridEditCommandColumn editColumn = new GridEditCommandColumn();
            editColumn.HeaderText = "Edit";
            editColumn.UniqueName = "EditCommand";
            editColumn.ButtonType = GridButtonColumnType.ImageButton;
            editColumn.EditImageUrl = "~/Images2/Edit.gif";
            editColumn.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
            editColumn.HeaderStyle.Width = 30;
            grdspecs.MasterTableView.Columns.Add(editColumn);
            grdspecs.MasterTableView.CommandItemDisplay = GridCommandItemDisplay.Top;
        }
        screen.SetGridColumns("specs", grdspecs);

        if (m_allowedit == 1)
        {
            GridButtonColumn buttonColumn = new GridButtonColumn();
            buttonColumn.HeaderText = "Delete";
            buttonColumn.UniqueName = "DeleteButton";
            buttonColumn.CommandName = "Delete";
            buttonColumn.ButtonType = GridButtonColumnType.ImageButton;
            buttonColumn.ImageUrl = "~/Images2/Delete.gif";
            buttonColumn.Text = "Delete";
            buttonColumn.HeaderStyle.Width = 30;
            grdspecs.MasterTableView.Columns.Add(buttonColumn);
        }

        MainControlsPanel.Controls.Add(grdspecs);

        screen.LoadScreen();

        grdspecs.ItemDataBound += new GridItemEventHandler(grdspecs_ItemDataBound);
        grdspecs.UpdateCommand += new GridCommandEventHandler(grdspecs_UpdateCommand);
        grdspecs.InsertCommand += new GridCommandEventHandler(grdspecs_InsertCommand);
        grdspecs.DeleteCommand += new GridCommandEventHandler(grdspecs_DeleteCommand);
    }

    protected void grdspecs_ItemDataBound(object sender, GridItemEventArgs e)
    {
      if (e.Item is GridCommandItem)
      {

        GridCommandItem commandItem = (GridCommandItem)e.Item;
        ImageButton t_addInPlaceButton = (ImageButton)commandItem.FindControl("addInPlaceButton");
        if (t_addInPlaceButton != null)
          t_addInPlaceButton.ValidationGroup = "specs";
      }

      if (e.Item is GridEditableItem && e.Item.IsInEditMode)
      {
        GridEditableItem editedItem = (GridEditableItem)e.Item;
        ImageButton t_imagebtn;
        if (e.Item.OwnerTableView.IsItemInserted)
        {
          t_imagebtn = ((ImageButton)editedItem.FindControl("PerformInsertButton"));
          if (t_imagebtn != null)
            t_imagebtn.ValidationGroup = "specs";
        }
        else
        {
          t_imagebtn = ((ImageButton)editedItem.FindControl("UpdateButton"));
          if (t_imagebtn != null)
            t_imagebtn.ValidationGroup = "specs";
        }
      }
        screen.GridItemDataBound(e, "inventory/invpecs.aspx", "MainForm", "specs");
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        litFrameScript.Text = "";

        Items objitem = new Items(Session["Login"].ToString(), "items", "itemnum", m_itemnum);
        NameValueCollection nvcitem = objitem.ModuleData;

        ucHeader1.Mode = "edit";
        ucHeader1.TabName = "Specifications";// "Accounts"; //Additional Info
        ucHeader1.OperationLabel = "Specifications";// "Accounts";//Additional Info
        ucHeader1.ModuleData = nvcitem;
        

        if (!Page.IsPostBack)
        {
            screen.PopulateScreen("items", nvcitem);
        }
    }

    protected void grdspecs_UpdateCommand(object source, Telerik.Web.UI.GridCommandEventArgs e)
    {
        GridEditableItem editedItem = e.Item as GridEditableItem;
        //Get the primary key value using the DataKeyValue.
        string counter = editedItem.OwnerTableView.DataKeyValues[editedItem.ItemIndex]["Counter"].ToString();

        DateFormat objDateFormat = new DateFormat(Session.LCID);
        NameValueCollection nvcFT = screen.GetGridFieldTypes("specs", "specification");
        string[] fields = nvcFT.AllKeys;
        NameValueCollection nvc = new NameValueCollection();
        foreach (string field in fields)
        {
            if (field != "counter" && field != "linkid" && field != "linktype")
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
                    bool a = objDateFormat.ValidateInputDate(sdate, out result);
                    sdate = result;
                    nvc.Add(field, sdate);
                }
            }
        }

        ModuleoObject obj = new ModuleoObject(Session["Login"].ToString(), "specification", "Counter", counter);
        bool success = obj.Update(nvc);
        if (!success)
        {
            //grdlabourinfo.Controls.Add(new LiteralControl("Unable to update cost account record. Reason: " + obj.ErrorMessage));
            grdspecs.Controls.Add(new LiteralControl(m_msg["T5"] + obj.ErrorMessage));
            e.Canceled = true;
        }
    }

    protected void grdspecs_DeleteCommand(object source, Telerik.Web.UI.GridCommandEventArgs e)
    {
        GridEditableItem editedItem = e.Item as GridEditableItem;

        //Get the primary key value using the DataKeyValue.
        string counter = editedItem.OwnerTableView.DataKeyValues[editedItem.ItemIndex]["Counter"].ToString();

        ModuleoObject obj = new ModuleoObject(Session["Login"].ToString(), "specification", "Counter", counter);
        bool success = obj.Delete();
        if (!success)
        {
            //grdlabourinfo.Controls.Add(new LiteralControl("Unable to delete cost account record. Reason: " + obj.ErrorMessage));
            grdspecs.Controls.Add(new LiteralControl(m_msg["T3"] + obj.ErrorMessage));
            e.Canceled = true;
        }
    }

    protected void grdspecs_InsertCommand(object source, Telerik.Web.UI.GridCommandEventArgs e)
    {
        GridEditableItem editedItem = e.Item as GridEditableItem;

        //Get the primary key value using the DataKeyValue.
        //string counter = editedItem.OwnerTableView.DataKeyValues[editedItem.ItemIndex]["Counter"].ToString();

        DateFormat objDateFormat = new DateFormat(Session.LCID);
        NameValueCollection nvcFT = screen.GetGridFieldTypes("specs", "specification");
        string[] fields = nvcFT.AllKeys;
        NameValueCollection nvc = new NameValueCollection();
        foreach (string field in fields)
        {
            if (field != "counter" && field != "linkid" && field != "linktype")
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
        nvc.Add("linkid", m_itemnum);
        nvc.Add("linktype", "inventory");

        ModuleoObject obj = new ModuleoObject(Session["Login"].ToString(), "specification", "Counter"); //???
        bool success = obj.Create(nvc);
        if (!success)
        {
            grdspecs.Controls.Add(new LiteralControl(m_msg["T4"] + obj.ErrorMessage));
            e.Canceled = true;
        }
    }

    private void RetrieveMessage()
    {
        SystemMessage msg = new SystemMessage("location/locationspecs.aspx");
        m_msg = msg.GetSystemMessage();
        msg.SetJsMessage(litMessage);
    }
}