using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Web.UI;
using System.Collections.Specialized;

public partial class Codes_CompRemarkList : System.Web.UI.Page
{
    protected AzzierScreen screen;
    protected RadGrid grdcompremarklist;
    protected string mode = "";
    protected string runtimefilter = "";
    protected string designtimefilter = "";
    protected string fieldlist = "";
    protected string referer = "";
    protected bool found = false;
    protected string wherestr = "";
    protected string controlid = "";
    protected string fieldid = "";
    protected string tablename = "closeremarks";
    protected string filterstr = "", filename = "";
    protected NameValueCollection m_msg = new NameValueCollection();
    protected bool allowedit = false;

    protected void Page_Init(object sender, EventArgs e)
    {
        if (Session["Login"] == null)
        {
            Response.Write("<script>alert('" + m_msg["T1"] + "');top.document.location.href='../Login.aspx';</script>");
            Response.End();
        }

        Session.LCID = Convert.ToInt32(Session["LCID"]);
        UserRights right = new UserRights(Session["Login"].ToString(), "UserRights", "Counter");
        NameValueCollection drRights = right.GetRights(Session["Login"].ToString(), "Codes");
        RetrieveMessage();

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

        filterstr = runtimefilter;
        if (designtimefilter != "")
        {
          if (filterstr == "")
          {
            filterstr = designtimefilter;
          }
          else
          {
            filterstr = filterstr + "," + designtimefilter;
          }
        }

        Validation v = new Validation();

        wherestr = v.AddLinqConditions(filterstr, filename, controlid, tablename,null,null,mode);

        screen = new AzzierScreen("codes/compremarklist.aspx", "MainForm", MainControlsPanel.Controls);

        grdcompremarklist = new RadGrid();
        grdcompremarklist.ID = "grdcompremarklist";
        grdcompremarklist.ClientSettings.Scrolling.AllowScroll = true;
        grdcompremarklist.ClientSettings.Scrolling.SaveScrollPosition = true;
        grdcompremarklist.ClientSettings.Scrolling.UseStaticHeaders = true;
        grdcompremarklist.ClientSettings.EnableRowHoverStyle = true;
        grdcompremarklist.MasterTableView.TableLayout = GridTableLayout.Fixed;
        grdcompremarklist.PagerStyle.Visible = true;
        grdcompremarklist.PagerStyle.AlwaysVisible = true;
        grdcompremarklist.Skin = "Outlook";

        grdcompremarklist.Attributes.Add("rules", "all");
        grdcompremarklist.AutoGenerateColumns = false;
        grdcompremarklist.AllowPaging = true;
        grdcompremarklist.PageSize = 100;
        grdcompremarklist.AllowSorting = true;
        grdcompremarklist.MasterTableView.AllowMultiColumnSorting = true;
        grdcompremarklist.AllowFilteringByColumn = true;
        grdcompremarklist.MasterTableView.CommandItemDisplay = GridCommandItemDisplay.Top;
        grdcompremarklist.MasterTableView.DataKeyNames = new string[] { "CloseRemCode" };
        grdcompremarklist.MasterTableView.ClientDataKeyNames = new string[] { "CloseRemCode" };

        grdcompremarklist.ClientSettings.Selecting.AllowRowSelect = true;
        grdcompremarklist.ClientSettings.ClientEvents.OnRowSelected = "getGridSelectedItems";
        if (drRights["urAddNew"] == "1")
        {
          allowedit = true;
          grdcompremarklist.MasterTableView.CommandItemTemplate = new MultiFunctionItemTemplate(1,true,"Complete Remarks",null,"return edit('');",1,"",false,"","Add New Complete Remark");
        }
        else
          grdcompremarklist.MasterTableView.CommandItemTemplate = new MultiFunctionItemTemplate("Complete Remarks");
        grdcompremarklist.ItemCreated += new GridItemEventHandler(grdcompremarkist_ItemCreated);
        grdcompremarklist.ItemDataBound+=new GridItemEventHandler(grdcompremarklist_ItemDataBound);

        grdcompremarklist.ClientSettings.DataBinding.FilterParameterType = GridClientDataBindingParameterType.Linq;
        grdcompremarklist.ClientSettings.DataBinding.SortParameterType = GridClientDataBindingParameterType.Linq;

        if (allowedit)
        {
          GridEditCommandColumn EditColumn = new GridEditCommandColumn();
          EditColumn.HeaderText = "Edit";
          EditColumn.UniqueName = "EditCommand";
          EditColumn.ButtonType = GridButtonColumnType.ImageButton;

          EditColumn.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
          EditColumn.HeaderStyle.Width = 30;
          grdcompremarklist.MasterTableView.Columns.Add(EditColumn);
        }

        screen.SetGridColumns("compremarklist", grdcompremarklist);
        MainControlsPanel.Controls.Add(grdcompremarklist);

        screen.LoadScreen();
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        hidFieldId.Value = fieldid;
        hidControlId.Value = AzzierData.ActualFieldName("CloseRemarks", controlid);

        grdcompremarklist.ClientSettings.DataBinding.SelectMethod = "GetCompRemarkList?where=" + wherestr;
        grdcompremarklist.ClientSettings.DataBinding.Location = "../InternalServices/ServiceWO.svc";
    }


    protected void grdcompremarkist_ItemCreated(object sender, GridItemEventArgs e)
    {
      screen.GridItemCreated(e, "codes/compremarklist.aspx", "MainForm", "compremarklist", grdcompremarklist);
    }

    protected void grdcompremarklist_ItemDataBound(object sender, GridItemEventArgs e)
    {
      if (e.Item is GridDataItem && !e.Item.IsInEditMode && allowedit)
      {
        GridDataItem item = (GridDataItem)e.Item;
        ImageButton btn = (ImageButton)item["EditCommand"].Controls[0];
        btn.ImageUrl = "~/Images/Edit.gif";
        btn.OnClientClick = "return edit('" + item.ItemIndex.ToString() + "')";
      }

      screen.GridItemDataBound(e, "codes/compremarklist.aspx", "MainForm", "compremarklist");
    }
  
    private void RetrieveMessage()
    {
        SystemMessage msg = new SystemMessage("codes/loclist.aspx");
        m_msg = msg.GetSystemMessage();
        
    }
}