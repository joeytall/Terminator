using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Web.UI;
using System.Collections.Specialized;
using System.Data;
using System.Data.OleDb;
using System.Linq;

public partial class Codes_Eqplist : System.Web.UI.Page
{
    protected RadGrid grdeqplist;
    protected AzzierScreen screen;
    protected string mode = "";
    protected string runtimefilter = "";
    protected string designtimefilter = "";
    protected string fieldlist = "";
    protected string referer = "";
    protected bool found = false;
    protected string wherestr = "";
    protected string controlid = "";
    protected string fieldid = "";
    protected string tablename = "Equipment";
    protected string mobileequipment = "";
    protected string tmp_location = "0";
    protected string radgridTitle = "";
    protected string filename = "";
    protected string totalCount = "";
    protected string wherestrlinq = "";
    protected string TotalCount = "";
    protected NameValueCollection m_msg = new NameValueCollection();

    protected void RetrieveMessage()
    {
        SystemMessage msg = new SystemMessage("codes/eqplist.aspx");
        m_msg = msg.GetSystemMessage();
        msg.SetJsMessage(litMessage);
    }

    protected void Page_Init(object sender, EventArgs e)
    {
        RetrieveMessage();
        if (Session["Login"] == null)
        {
            Response.Write("<html><script type=\"text/javascript\">alert('" + m_msg["T1"] + ".');top.document.location.href='../login.aspx';</script></html>");
            Response.End();
        }

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

        wherestrlinq = v.AddLinqConditions(filterstrlinq, filename, controlid, tablename,null,null,mode);

        screen = new AzzierScreen("codes/eqplist.aspx", "MainForm", MainControlsPanel.Controls);

        grdeqplist = new RadGrid();
        grdeqplist.ID = "grdeqplist";
        grdeqplist.ClientSettings.Scrolling.AllowScroll = true;
        grdeqplist.ClientSettings.Scrolling.SaveScrollPosition = true;
        grdeqplist.ClientSettings.Scrolling.UseStaticHeaders = true;
        grdeqplist.ClientSettings.EnableRowHoverStyle = true;
        grdeqplist.MasterTableView.TableLayout = GridTableLayout.Fixed;
        grdeqplist.PagerStyle.Visible = true;
        grdeqplist.PagerStyle.AlwaysVisible = true;
        grdeqplist.Skin = "Outlook";

        grdeqplist.Attributes.Add("rules", "all");

        grdeqplist.AutoGenerateColumns = false;
        grdeqplist.AllowPaging = true;
        grdeqplist.PageSize = 100;
        grdeqplist.AllowSorting = true;
        grdeqplist.MasterTableView.AllowMultiColumnSorting = true;
        grdeqplist.AllowFilteringByColumn = true;
        grdeqplist.MasterTableView.CommandItemDisplay = GridCommandItemDisplay.Top;
        grdeqplist.MasterTableView.DataKeyNames = new string[] { "Equipment" };
        //grdeqplist.MasterTableView.ClientDataKeyNames = new string[] { "Equipment" };
        grdeqplist.MasterTableView.CommandItemSettings.ShowAddNewRecordButton = false;

        grdeqplist.ClientSettings.Selecting.AllowRowSelect = true;
        grdeqplist.ClientSettings.ClientEvents.OnRowSelected = "getGridSelectedItems";
        grdeqplist.ItemCreated += new GridItemEventHandler(grdeqplist_ItemCreated);

        if (!string.IsNullOrEmpty(runtimefilter))
        {
            radgridTitle = runtimefilter.Replace("^", ": ");
            grdeqplist.MasterTableView.CommandItemTemplate = new MultiFunctionItemTemplate("Equipment (" + runtimefilter.Replace("^", ": ") + ")");
            if (runtimefilter.Contains("location"))
            {
                tmp_location = "1";
            }
        }
        else
            grdeqplist.MasterTableView.CommandItemTemplate = new MultiFunctionItemTemplate("Equipment");

        grdeqplist.ClientSettings.DataBinding.SortParameterType = GridClientDataBindingParameterType.Linq;
        grdeqplist.ClientSettings.DataBinding.FilterParameterType = GridClientDataBindingParameterType.Linq;

        screen.SetGridColumns("eqplist", grdeqplist);
        MainControlsPanel.Controls.Add(grdeqplist);
        screen.LoadScreen();
    }

    protected void grdeqplist_ItemCreated(object sender, GridItemEventArgs e)
    {
      screen.GridItemCreated(e, "codes/eqplist.aspx", "MainForm", "results", grdeqplist);
    }

    protected void Page_Load(object sender, EventArgs e)
    { 
        hidFieldId.Value = fieldid;
        hidControlId.Value = AzzierData.ActualFieldName("Equipment",controlid);

        mobileequipment = hidMobileEquipment.Value;

        //ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert(' " + mobileequipment + " Hello')", true);

        grdeqplist.ClientSettings.DataBinding.SelectMethod = "LookupDataAndCount_Mobile?wherestr=" + wherestrlinq + "&mode=" + mode + "&mobileequipment=" + mobileequipment + "&location=" + tmp_location;
        
        grdeqplist.ClientSettings.DataBinding.Location = "../InternalServices/ServiceEqpt.svc";

        //grdeqplist.ClientSettings.DataBinding.SelectCountMethod = "";
    }


    private Boolean isReadonly(int value, string mode)
    {
        Boolean result = false;
        if (value == 0)
            return result;
        else
        {
            if (mode == "edit")
                result = true;
            if ((mode == "new" || mode == "duplicate") && value == 2)
                result = true;
        }
        return result;
    }
}