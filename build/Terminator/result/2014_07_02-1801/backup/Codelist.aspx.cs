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
    protected RadGrid grdcodelist;

    protected string mode = "";
    protected string runtimefilter = "";
    protected string designtimefilter = "";
    protected string tablename = "Codes";
    protected string fieldlist = "";
    protected string referer = "";
    protected bool found = false;
    protected bool allowedit = true;
    protected string wherestr = "";
    protected string controlid = "";
    protected string fieldid = "";
    private string tfield = "";
    private string tcode1 = "";
    private string tcode2 = "";
    protected string totalCount = "";
    private NameValueCollection m_msg = new NameValueCollection();

    protected void Page_Init(object sender, EventArgs e)
    {
        RetrieveMessage();
        if (Session["Login"] == null)
        {
            Response.Write("<html><script type=\"text/javascript\">alert('" + m_msg["T1"] + "');top.document.location.href='../login.aspx';</script></html>");
            //Response.Write("<html><script type=\"text/javascript\">alert('" + m_msg["T1"] + "');radwindowClose();</script></html>");
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
        string filterstr = "", filename = "";
        filterstr = runtimefilter + "";
        if (designtimefilter + "" != "")
            if (filterstr == "")
                filterstr = designtimefilter + "";
            else
                filterstr = filterstr + "," + designtimefilter + "";
        string wherestr = v.AddConditions(filterstr, filename, controlid, tablename,null,null,mode);
        tfield = v.GetFilterValue(designtimefilter, "tfield");
        tcode1 = v.GetFilterValue(designtimefilter, "tcode1");
        tcode2 = v.GetFilterValue(designtimefilter, "tcode2");
        screen = new AzzierScreen("codes/codelist.aspx", "MainForm", MainControlsPanel.Controls);

       if( tfield.ToLower()=="wostatus")
       {
           allowedit=false;
       }

        string connstring = Application["ConnString"].ToString();
        CodeListSqlDataSource.ConnectionString = connstring;
        //EmpListSqlDataSource.SelectCommand = "SELECT * FROM v_Employee ORDER BY empid";
        if (wherestr == "")
            CodeListSqlDataSource.SelectCommand = "Select * From " + tablename + " Order By tCode";
        else
            CodeListSqlDataSource.SelectCommand = "Select * From " + tablename + " " + wherestr + " Order By tCode";
        grdcodelist = new RadGrid();
        grdcodelist.ID = "grdcodelist";
        grdcodelist.ClientSettings.Scrolling.AllowScroll = true;
        //grdcodelist.ClientSettings.Scrolling.ScrollHeight = 300;
        grdcodelist.ClientSettings.Scrolling.SaveScrollPosition = true;
        grdcodelist.ClientSettings.Scrolling.UseStaticHeaders = true;
        grdcodelist.ClientSettings.EnableRowHoverStyle = true;
        grdcodelist.MasterTableView.TableLayout = GridTableLayout.Fixed;
        grdcodelist.PagerStyle.Visible = true;// false;
        grdcodelist.PagerStyle.AlwaysVisible = true;
        grdcodelist.Skin = "Outlook";

        grdcodelist.Attributes.Add("rules", "all");
        grdcodelist.DataSourceID = "CodeListSqlDataSource";
        grdcodelist.AutoGenerateColumns = false;
        grdcodelist.AllowPaging = true;
        grdcodelist.PageSize = 100;
        grdcodelist.AllowSorting = true;
        grdcodelist.MasterTableView.AllowMultiColumnSorting = true;
        grdcodelist.AllowFilteringByColumn = true;
        grdcodelist.MasterTableView.CommandItemDisplay = GridCommandItemDisplay.Top;
        grdcodelist.MasterTableView.DataKeyNames = new string[] { "counter" };

        grdcodelist.ClientSettings.Selecting.AllowRowSelect = true;
        grdcodelist.ClientSettings.ClientEvents.OnRowSelected = "getGridSelectedItems";

        if (allowedit && drRights["urEdit"] == "1")
        {
            GridEditCommandColumn EditColumn = new GridEditCommandColumn();
            EditColumn.HeaderText = "Edit";
            EditColumn.UniqueName = "EditCommand";
            EditColumn.ButtonType = GridButtonColumnType.ImageButton;

            EditColumn.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
            EditColumn.HeaderStyle.Width = 30;
            grdcodelist.MasterTableView.Columns.Add(EditColumn);
            grdcodelist.MasterTableView.EditMode = GridEditMode.InPlace;
        }

        screen.SetGridColumns("codelist", grdcodelist);

        if (checkUserRight("Delete",drRights) && allowedit)
        {
            GridButtonColumn DeleteColumn = new GridButtonColumn();
            DeleteColumn.HeaderText = "Delete";
            DeleteColumn.UniqueName = "DeleteButton";
            DeleteColumn.CommandName = "Delete";
            DeleteColumn.ButtonType = GridButtonColumnType.ImageButton;
            DeleteColumn.ImageUrl = "~/Images2/Delete.gif";
            DeleteColumn.Text = "Delete";
            DeleteColumn.ConfirmText = m_msg["T2"];
            DeleteColumn.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
            DeleteColumn.HeaderStyle.Width = 30;
            grdcodelist.MasterTableView.Columns.Add(DeleteColumn);
        }


        if (checkUserRight("AddNew", drRights) && allowedit)
        {
           grdcodelist.MasterTableView.CommandItemTemplate = new InsertFormItemTemplate("Codes ", null, "return EditCodes('','"+tfield+"')", 1, Session["UserGroup"].ToString());
        }
        else
        {
           grdcodelist.MasterTableView.CommandItemTemplate = new CodesCommandItem("Codes ", 0);
        }
        grdcodelist.ItemCreated += new GridItemEventHandler(grdcodelist_ItemCreated);
        grdcodelist.ItemDataBound += new GridItemEventHandler(grdcodelist_ItemDataBound);
        

        MainControlsPanel.Controls.Add(grdcodelist);

        screen.LoadScreen();
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

    protected void Page_Load(object sender, EventArgs e)
    {
        hidFieldId.Value = fieldid;
        hidControlId.Value = AzzierData.ActualFieldName("codes",controlid);
        //CalRadwinSize();
    }

    protected void grdcodelist_ItemCreated(object sender, GridItemEventArgs e)
    {
      screen.GridItemCreated(e, "codes/codelist.aspx", "MainForm", "results", grdcodelist);
    }

    protected void grdcodelist_DeleteCommand(object source, Telerik.Web.UI.GridCommandEventArgs e)
    {
        GridDataItem item = (GridDataItem)e.Item;
        string counter = item.OwnerTableView.DataKeyValues[item.ItemIndex]["counter"].ToString();

        ModuleoObject obj = new ModuleoObject(Session["Login"].ToString(), tablename, "Counter", counter);
        bool success = obj.Delete();
        if (!success)
        {
            grdcodelist.Controls.Add(new LiteralControl(m_msg["T3"] + obj.ErrorMessage));
            e.Canceled = true;
        }
    }

    protected void grdcodelist_ItemDataBound(object sender, GridItemEventArgs e)
    {

        if (e.Item is GridDataItem && !e.Item.IsInEditMode)
        {
            if (allowedit)
            {
                GridDataItem item = (GridDataItem)e.Item;
                ImageButton btn = (ImageButton)item["EditCommand"].Controls[0];
                btn.ImageUrl = "~/Images2/Edit.gif";
                btn.OnClientClick = "return EditCodes('" + item.OwnerTableView.DataKeyValues[item.ItemIndex]["counter"].ToString() + "','"+tfield+"')";

                if (Convert.ToDecimal(item["System"].Text) * 1 == 1) // system code
                {
                  ImageButton btn2 = (ImageButton)item["DeleteButton"].Controls[0];
                  //btn2.Enabled = false;
                  btn2.Visible = false;
                }
            }
        }

        screen.GridItemDataBound(e, "codes/codelist.aspx", "MainForm", "codelist");
    }


    protected void RadAjaxManager1_AjaxRequest(object sender, AjaxRequestEventArgs e)
    {
    }

    private void RetrieveMessage()
    {
        SystemMessage msg = new SystemMessage("codes/codelist.aspx");
        m_msg = msg.GetSystemMessage();
        msg.SetJsMessage(litMessage);
    }

}