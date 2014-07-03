using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.OleDb;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Web.UI;
using System.Text;

public partial class Codes_Codelist : System.Web.UI.Page
{
    protected AzzierScreen screen;
    protected RadGrid grdadmindivlist;

    protected string mode = "";
    protected string runtimefilter = "";
    protected string designtimefilter = "";
    protected string tablename = "tbldivision";
    protected string fieldlist = "";
    protected string referer = "";
    protected bool found = true;
    protected string wherestr = "";
    protected string controlid = "";
    protected string fieldid = "";
    protected string filename = "";
    //private string tfield = "";
    //private string tcode1 = "";
    //private string tcode2 = "";
    protected string totalCount = "";
    private NameValueCollection m_msg = new NameValueCollection();

    protected void Page_Init(object sender, EventArgs e)
    {
        RetrieveMessage();
        if (Session["Login"] == null)
        {
            Response.Write("<html><script type=\"text/javascript\">alert('" + m_msg["T1"] + "');top.document.location.href='../login.aspx';</script></html>");
            Response.End();
        }

        UserRights right = new UserRights(Session["Login"].ToString(), "UserRights", "Counter");
        NameValueCollection drRights = right.GetRights(Session["Login"].ToString(), "Codes");

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

        //ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert(' " + designtimefilter + " Hello')", true);
        Validation v = new Validation();
        string filterstr = "";//, filename = "";
        filterstr = runtimefilter + "";
        if (designtimefilter + "" != "")
            if (filterstr == "")
                filterstr = designtimefilter + "";
            else
                filterstr = filterstr + "," + designtimefilter + "";
        //ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert(' " + filterstr + " Hello')", true);
        //wherestr = v.AddLinqConditions(filterstr, filename, controlid, tablename,null,null,mode);
        //tfield = v.GetFilterValue(designtimefilter, "tfield");
        //tcode1 = v.GetFilterValue(designtimefilter, "tcode1");
        //tcode2 = v.GetFilterValue(designtimefilter, "tcode2");

        if (filename == "codes/admindivmain.aspx")//;//filename=codes/admindivmain.aspx
        {
            //wherestr = " where flddivisionparent='0' or flddivisionparent is null ";
            found = false;
        }
        //ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert(' " + wherestr + " Hello')", true);

        screen = new AzzierScreen("codes/admindivlist.aspx", "MainForm", MainControlsPanel.Controls);

        string connstring = Application["ConnString"].ToString();
        AdminDivListSqlDataSource.ConnectionString = connstring;

        if (wherestr == "")
            AdminDivListSqlDataSource.SelectCommand = "Select * From " + tablename + " Order By flddivision";
        else
            AdminDivListSqlDataSource.SelectCommand = "Select * From " + tablename + " " + wherestr + " Order By flddivision";

        grdadmindivlist = new RadGrid();
        grdadmindivlist.ID = "grdadmindivlist";
        grdadmindivlist.ClientSettings.Scrolling.AllowScroll = true;
        //grdadmindivlist.ClientSettings.Scrolling.ScrollHeight = 300;
        grdadmindivlist.ClientSettings.Scrolling.SaveScrollPosition = true;
        grdadmindivlist.ClientSettings.Scrolling.UseStaticHeaders = true;
        //grdadmindivlist.ClientSettings.ClientEvents.OnScroll = "HandleScrolling";
        grdadmindivlist.ClientSettings.EnableRowHoverStyle = true;
        grdadmindivlist.MasterTableView.TableLayout = GridTableLayout.Fixed;
        grdadmindivlist.PagerStyle.Visible = true;// false;
        grdadmindivlist.PagerStyle.AlwaysVisible = true;
        grdadmindivlist.Skin = "Outlook";

        grdadmindivlist.Attributes.Add("rules", "all");
        //grdadmindivlist.DataSourceID = "AdminDivListSqlDataSource";
        grdadmindivlist.AutoGenerateColumns = false;
        grdadmindivlist.AllowPaging = true;
        grdadmindivlist.PageSize = 100;
        grdadmindivlist.AllowSorting = true;
        grdadmindivlist.MasterTableView.AllowMultiColumnSorting = true;
        grdadmindivlist.AllowFilteringByColumn = true;
        grdadmindivlist.MasterTableView.CommandItemDisplay = GridCommandItemDisplay.Top;
        grdadmindivlist.MasterTableView.DataKeyNames = new string[] { "flddivision" };
        grdadmindivlist.MasterTableView.ClientDataKeyNames = new string[] { "flddivision" };

        grdadmindivlist.ClientSettings.Selecting.AllowRowSelect = true;
        grdadmindivlist.ClientSettings.ClientEvents.OnRowSelected = "getGridSelectedItems";

        //if (Session["UserGroup"].ToString() == "Admin" && found)
        //if (Session["UserGroup"].ToString() == "Admin")
        if (checkUserRight("Delete",drRights) && found)
        {
            GridEditCommandColumn EditColumn = new GridEditCommandColumn();
            EditColumn.HeaderText = "Edit";
            EditColumn.UniqueName = "EditCommand";
            EditColumn.ButtonType = GridButtonColumnType.ImageButton;

            EditColumn.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
            EditColumn.HeaderStyle.Width = 30;
            grdadmindivlist.MasterTableView.Columns.Add(EditColumn);
            //grdadmindivlist.MasterTableView.EditMode = GridEditMode.InPlace;
        }

        screen.SetGridColumns("admindivlist", grdadmindivlist);

        //if (Session["UserGroup"].ToString() == "Admin")
        //{
        //    GridButtonColumn DeleteColumn = new GridButtonColumn();
        //    DeleteColumn.HeaderText = "Delete";
        //    DeleteColumn.UniqueName = "DeleteButton";
        //    DeleteColumn.CommandName = "Delete";
        //    DeleteColumn.ButtonType = GridButtonColumnType.ImageButton;
        //    DeleteColumn.ImageUrl = "~/Images2/Delete.gif";
        //    DeleteColumn.Text = "Delete";
        //    DeleteColumn.ConfirmText = m_msg["T2"];
        //    DeleteColumn.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
        //    DeleteColumn.HeaderStyle.Width = 30;
        //    grdadmindivlist.MasterTableView.Columns.Add(DeleteColumn);
        //}

        //if (Session["UserGroup"].ToString() == "Admin")
        //  grdadmindivlist.MasterTableView.CommandItemTemplate = new CodesCommandItem(tfield.ToUpper(), 1);
        //else
        //  grdadmindivlist.MasterTableView.CommandItemTemplate = new CodesCommandItem(tfield.ToUpper(), 0);
        
        //if (Session["UserGroup"].ToString() == "Admin" && found)
        //if (Session["UserGroup"].ToString() == "Admin")
        if (checkUserRight("AddNew",drRights) && found)
        {
            grdadmindivlist.MasterTableView.CommandItemTemplate = new InsertFormItemTemplate("Data Division", null, "return editadmindivision('','')", 1, Session["UserGroup"].ToString());
        }
        else
        {
           grdadmindivlist.MasterTableView.CommandItemTemplate = new CodesCommandItem("Data Division", 0);
        }

        grdadmindivlist.ItemCreated += new GridItemEventHandler(grdadmindivlist_ItemCreated);
        //grdadmindivlist.DeleteCommand += new GridCommandEventHandler(grdadmindivlist_DeleteCommand);
        //grdadmindivlist.InsertCommand += new GridCommandEventHandler(grdadmindivlist_InsertCommand);
        //grdadmindivlist.UpdateCommand += new GridCommandEventHandler(grdadmindivlist_UpdateCommand);
        grdadmindivlist.ItemDataBound += new GridItemEventHandler(grdadmindivlist_ItemDataBound);
        grdadmindivlist.PreRender += new EventHandler(grdadmindivlist_PreRender);
        grdadmindivlist.ItemEvent += new GridItemEventHandler(grdadmindivlist_ItemEvent);
        grdadmindivlist.ClientSettings.DataBinding.FilterParameterType = GridClientDataBindingParameterType.Linq;
        grdadmindivlist.ClientSettings.DataBinding.SortParameterType = GridClientDataBindingParameterType.Linq;         
        MainControlsPanel.Controls.Add(grdadmindivlist);

        screen.LoadScreen();
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        hidFieldId.Value = fieldid;
        hidControlId.Value = AzzierData.ActualFieldName("tbldivision","flddivision");
        grdadmindivlist.ClientSettings.DataBinding.SelectMethod = "GetAdminDivList?where=" + wherestr;
        grdadmindivlist.ClientSettings.DataBinding.Location = "../InternalServices/ServiceGeneral.svc";
    }

    protected void grdadmindivlist_ItemEvent(object sender, GridItemEventArgs e)
    {
        //if (e.EventInfo is GridInitializePagerItem)
        //{
        //    int rowCount = (e.EventInfo as GridInitializePagerItem).PagingManager.DataSourceCount;
        //    string countstr = grdadmindivlist.MasterTableView.Items.Count.ToString();
        //    totalCount =  countstr + " / " + rowCount.ToString();
        //}
    }

    protected void grdadmindivlist_ItemCreated(object sender, GridItemEventArgs e)
    {
      screen.GridItemCreated(e, "codes/admindivlist.aspx", "MainForm", "results", grdadmindivlist);
      /*
        if (e.Item is GridCommandItem)
        {
            if (Session["UserGroup"].ToString() != "Admin")
                e.Item.FindControl("InitInsertButton").Visible = false;
        }
        */
    }

    //protected void grdadmindivlist_DeleteCommand(object source, Telerik.Web.UI.GridCommandEventArgs e)
    //{
    //    GridDataItem item = (GridDataItem)e.Item;
    //    string counter = item.OwnerTableView.DataKeyValues[item.ItemIndex]["counter"].ToString();

    //    ModuleoObject obj = new ModuleoObject(Session["Login"].ToString(), tablename, "Counter", counter);
    //    bool success = obj.Delete();
    //    if (!success)
    //    {
    //        grdadmindivlist.Controls.Add(new LiteralControl(m_msg["T3"] + obj.ErrorMessage));
    //        e.Canceled = true;
    //    }
    //}

    //protected void grdadmindivlist_InsertCommand(object source, Telerik.Web.UI.GridCommandEventArgs e)
    //{
    //    NameValueCollection nvcFT = screen.GetGridFieldTypes("codelist", tablename);
    //    string[] fields = nvcFT.AllKeys;
    //    NameValueCollection nvc = new NameValueCollection();
    //    DateFormat objDateFormat = new DateFormat(Session.LCID);
    //    GridEditableItem editedItem = e.Item as GridEditableItem;
    //    //counter = editedItem.OwnerTableView.DataKeyValues[editedItem.ItemIndex]["counter"].ToString();
    //    if (e.Item.OwnerTableView.EditMode == GridEditMode.InPlace)
    //    {
    //        //Get the primary key value using the DataKeyValue.
    //        foreach (string field in fields)
    //        {
    //            if (field != "counter" && field != "tfield")
    //            {
    //                string dbtype = nvcFT[field].ToString();
    //                if (dbtype == "system.string" || dbtype == "system.decimal")
    //                {
    //                    nvc.Add(field, (editedItem[field].Controls[0] as TextBox).Text);
    //                }
    //                else if (dbtype == "system.datetime")
    //                {
    //                    string sdate = (editedItem[field].Controls[0] as RadDatePicker).SelectedDate.ToString();
    //                    string result = "";
    //                    bool a = objDateFormat.ValidateInputDate(sdate, out result);    // convert date from screen format to db date format
    //                    sdate = result;
    //                    nvc.Add(field, sdate);
    //                }
    //            }
    //        }
    //    }
    //    //nvc.Add("tfield", tfield);
    //    //if (nvc["tcode1"] == null)
    //    //    nvc.Add("tcode1", tcode1);
    //    //if (nvc["tcode2"] == null)
    //    //    nvc.Add("tcode2", tcode2);

    //    ModuleoObject obj = new Resources(Session["Login"].ToString(), tablename, "Counter");
    //    bool success = obj.Create(nvc);
    //    if (!success)
    //    {
    //        grdadmindivlist.Controls.Add(new LiteralControl(m_msg["T4"] + obj.ErrorMessage));
    //        e.Canceled = true;
    //    }
    //}

    //protected void grdadmindivlist_UpdateCommand(object source, Telerik.Web.UI.GridCommandEventArgs e)
    //{
    //    NameValueCollection nvcFT = screen.GetGridFieldTypes("codelist", tablename);
    //    string[] fields = nvcFT.AllKeys;
    //    NameValueCollection nvc = new NameValueCollection();
    //    DateFormat objDateFormat = new DateFormat(Session.LCID);
    //    string counter = "";
    //    GridEditableItem editedItem = e.Item as GridEditableItem;
    //    counter = editedItem.OwnerTableView.DataKeyValues[editedItem.ItemIndex]["counter"].ToString();
    //    if (e.Item.OwnerTableView.EditMode == GridEditMode.InPlace)
    //    {
    //        //Get the primary key value using the DataKeyValue.
    //        foreach (string field in fields)
    //        {
    //            if (field != "counter" && field != "tcode")
    //            {
    //                string dbtype = nvcFT[field].ToString();
    //                if (dbtype == "system.string" || dbtype == "system.decimal")
    //                {
    //                    nvc.Add(field, (editedItem[field].Controls[0] as TextBox).Text);
    //                }
    //                else if (dbtype == "system.datetime")
    //                {
    //                    string sdate = (editedItem[field].Controls[0] as RadDatePicker).SelectedDate.ToString();
    //                    string result = "";
    //                    bool a = objDateFormat.ValidateInputDate(sdate, out result);    // convert date from screen format to db date format
    //                    sdate = result;
    //                    nvc.Add(field, sdate);
    //                }
    //            }
    //        }
    //    }

    //    ModuleoObject obj = new Resources(Session["Login"].ToString(), tablename, "Counter", counter);
    //    bool success = obj.Update(nvc);
    //    if (!success)
    //    {
    //        grdadmindivlist.Controls.Add(new LiteralControl(m_msg["T5"] + obj.ErrorMessage));
    //        e.Canceled = true;
    //    }
    //}

    protected void grdadmindivlist_ItemDataBound(object sender, GridItemEventArgs e)
    {
        if ((e.Item is GridEditableItem) && !(e.Item.IsInEditMode))
        {
            GridEditableItem item = (GridEditableItem)e.Item;

            //if (Session["UserGroup"].ToString() == "Admin" && found)
            if (found)
            {
                ImageButton btn = (ImageButton)item["EditCommand"].Controls[0];
                btn.ImageUrl = "~/Images/Edit.gif";
                btn.OnClientClick = "return editadmindivision('" + item.ItemIndex.ToString() + "','')";
            }
        }
        screen.GridItemDataBound(e, "codes/admindivlist.aspx", "MainForm", "tbldivision");
    }

    protected void grdadmindivlist_PreRender(object sender, EventArgs e)
    {
        //GridItem commandItem = grdadmindivlist.MasterTableView.GetItems(GridItemType.CommandItem)[0];
        //Label lbl = (Label)commandItem.FindControl("lbltitle");
        //lbl.Text = totalCount;
    }

    protected void RadAjaxManager1_AjaxRequest(object sender, AjaxRequestEventArgs e)
    {
        //grdadmindivlist.PageSize = 100 + grdadmindivlist.PageSize;
        //grdadmindivlist.Rebind();
    }

    private void RetrieveMessage()
    {
        SystemMessage msg = new SystemMessage("codes/codelist.aspx");
        m_msg = msg.GetSystemMessage();
        msg.SetJsMessage(litMessage);
    }

    private bool checkUserRight(string command, NameValueCollection dr)
    {
        bool right = false;
        switch (command)
        {
            case "AddNew":
                if (dr["urAddNew"] == "1")
                {
                    right = true;
                }
                break;
            case "Edit":
                if (dr["urEdit"] == "1")
                {
                    right = true;
                }
                break;
            case "Delete":
                if (dr["urDelete"] == "1")
                {
                    right = true;
                }
                break;
            default:
                right = false;
                break;
        }
        return right;
    }

}
