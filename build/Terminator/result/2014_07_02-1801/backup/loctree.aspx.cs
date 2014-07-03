using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.OleDb;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Web.UI;
using System.Web.Services;
using System.Data.SqlClient;
using System.Text;

public partial class codes_loctree : System.Web.UI.Page
{
    protected RadTreeView trvlocation;
    protected AzzierScreen screen;
    protected NameValueCollection m_rights;
    protected int m_allowedit = 0;
    private string m_top = "";
    protected string template = "";
    protected string mode = "";
    protected string runtimefilter = "";
    protected string designtimefilter = "";
    protected string fieldlist = "";
    protected string referer = "";
    protected string tablename = "Location";
    protected string extrafilter = "";
    protected string controlid = "";
    protected string fieldid = "";

    protected NameValueCollection m_msg = new NameValueCollection();

    protected void Page_Init(object sender, EventArgs e)
    {
        RetrieveMessage();
        Page.EnableViewState = false;
        if (Session["Login"] == null)
        {
            //Response.Write("<script>alert('Your session has expired. Please login again.');top.document.location.href='../Login.aspx';</script>");
            Response.Write("<script>alert('" + m_msg["T1"] + "');top.document.location.href='../Login.aspx';</script>");
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
        if (Request.QueryString["toplocfilter"] != null)
          tablename = Request.QueryString["toplocfilter"].ToString();
        
        
        string connstring = Application["ConnString"].ToString();
        
        trvlocation = new RadTreeView();
        trvlocation.EnableViewState = false;
        trvlocation.Skin = "Outlook";
        trvlocation.ID = "trvlocation";
        trvlocation.EnableDragAndDrop = true;
        //trvlocation.EnableDragAndDropBetweenNodes = true;
        StringBuilder clientTemplate = new StringBuilder();
        clientTemplate.Append("<div>");
        clientTemplate.Append("<image  border=\"none\" src=\"../images/location/locations_24.png\"></image>");
        //clientTemplate.Append("<a href=\"equipmentmain.aspx?mode=edit&equipment=#= Text #\">#= Text #</a>");
        clientTemplate.Append("<a href=\"javascript:selectnode('#= Text #')\">#= Text #</a>");
        clientTemplate.Append("</div>");
        //hidLocationNodeTemplate.Value = clientTemplate.ToString();
        template = clientTemplate.ToString();
        trvlocation.ClientNodeTemplate = clientTemplate.ToString();
        
        RadTreeNode n = new RadTreeNode();
        //trvlocation.OnClientNodeDragging = "onNodeDragging";
        //trvlocation.OnClientNodeDropping = "onNodeDropping";
        trvlocation.OnClientMouseOver = "OnClientMouseOver";
        trvlocation.WebServiceSettings.Path = "~/internalservices/servicetreeview.asmx";
        trvlocation.WebServiceSettings.Method = "GetLocationByParent";
        trvlocation.BackColor = System.Drawing.Color.White;
        screen = new AzzierScreen("codes/loctree.aspx", "MainForm", MainControlsPanel.Controls);
        screen.LCID = Session.LCID;
        //if (!Page.IsPostBack)
        //InitTree();
        MainControlsPanel.Controls.Add(trvlocation);
        screen.LoadScreen();
    }

    protected void InitTree(object sender, EventArgs e)
    {
      trvlocation.NodeTemplate = new LocationNodeTemplate();


      Validation v = new Validation();
      string filterstr = "parentid^is null", filename = "";
      filterstr = filterstr + runtimefilter + "";
      if (designtimefilter + "" != "")
        filterstr = filterstr + "," + designtimefilter + "";
      if (extrafilter != "")
        filterstr = filterstr + "," + extrafilter;
      TextBox t = MainControlsPanel.FindControl("txttoplocation") as TextBox;
      if (t != null)
        if (t.Text != null)
          filterstr = filterstr + "," + "location^" + t.Text;
      
      string wherestr = v.AddConditions(filterstr, filename, controlid, tablename);
      string connstring = Application["ConnString"].ToString();

      NameValueCollection condition = new NameValueCollection();
      string[] filters = filterstr.Split(',');
      for (int i = 0; i < filters.Length; i++)
      {
        string[] list = filters[i].Split('^');
        if (list.Length == 2)
        {
          condition.Add(list[0], list[1]);
        }
      }

      Location l = new Location(Session["Login"].ToString(), "Location", "Location");
      string[] loclist = l.Query(condition);
      for (int i = 0; i < loclist.Length; i++)
      {
        RadTreeNode node = new RadTreeNode();
        node.Value = loclist[i];
        node.Text = loclist[i];
        node.Attributes["id"] = Guid.NewGuid().ToString();
        node.ExpandMode = TreeNodeExpandMode.WebService;
        RadToolTipManager1.TargetControls.Add(node.Attributes["id"], node.Value, true);
        trvlocation.Nodes.Add(node);
      }
      trvlocation.DataBind();
    }
  
    private void AddControlsToNode(RadTreeNode node, string text, string url)
    {
      HyperLink h = new HyperLink();
      h.Text = text;
      h.NavigateUrl = url;
      node.Controls.Add(h);
    }

    protected void Page_Load(object sender, EventArgs e)
    {
      litFrameScript.Text = "";

      if (!Page.IsPostBack)
      {
        RadToolTipManager1.TargetControls.Clear();
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
        hidFieldId.Value = fieldid;
        hidControlId.Value = controlid;

      }
      else
      {
        /*
        if (Request.Form["__EVENTTARGET"] == "loadtree")
        {
          trvlocation.Nodes.Clear();
          InitTree();
        }
        */
      }
      
    }

    private void RetrieveMessage()
    {
        SystemMessage msg = new SystemMessage("codes/loctree.aspx");//"location/locationspecs.aspx");
        m_msg = msg.GetSystemMessage();
        msg.SetJsMessage(litMessage);
    }
  
  
    protected void OnAjaxUpdate(object sender, ToolTipUpdateEventArgs args)
    {
      this.UpdateToolTip(args.Value, args.UpdatePanel);
    }

    private void UpdateToolTip(string elementID, UpdatePanel panel)
    {
      Control ctrl = Page.LoadControl("../Tooltips/locationtooltip.ascx");
      panel.ContentTemplateContainer.Controls.Add(ctrl);
      LocationToolTip tooltip = (LocationToolTip)ctrl;
      tooltip.Location = elementID;
    }

    class LocationNodeTemplate : ITemplate
    {
      public void InstantiateIn(Control container)
      {
        Image image = new Image();
        image.ImageUrl = "../images/location/locations_24.png";
        image.DataBinding += new EventHandler(image_DataBinding);
        container.Controls.Add(image);

        HyperLink link = new HyperLink();
        link.DataBinding += new EventHandler(link_DataBinding);
        container.Controls.Add(link);
       
      }

      private void link_DataBinding(object sender, EventArgs e)
      {
        HyperLink target = (HyperLink)sender;
        RadTreeNode node = (RadTreeNode)target.BindingContainer;
        string nodeText = (string)DataBinder.Eval(node, "Text");
        target.Text = nodeText;
        target.NavigateUrl = "javascript:selectnode('" + nodeText + "')";
      }

      private void image_DataBinding(object sender, EventArgs e)
      {
        Image target = (Image)sender;
        RadTreeNode node = (RadTreeNode)target.BindingContainer;
      }

    }
}