using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections.Specialized;
using Telerik.Web.UI;

public partial class inventory_itemvendor : System.Web.UI.Page
{
  protected NameValueCollection m_msg = new NameValueCollection();
  protected string m_itemnum;
  protected string m_vendor;
  protected string m_counter = "";
  protected string m_mode = "";
  protected string m_main = "";
  protected AzzierScreen screen;
  protected NameValueCollection m_rights;
  protected Int16 m_allowedit;
  protected ModuleoObject objitemvendor;
  protected string refreshgrid = "";

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
      if (Request.QueryString["main"] != null)
      {
        m_main = Request.QueryString["main"].ToString();
      }
      if (Request.QueryString["itemnum"] != null)
      {
        m_itemnum = Request.QueryString["itemnum"].ToString();
        m_mode = "new";
        if (Request.QueryString["vendor"] != null)
        {
          m_vendor = Request.QueryString["vendor"].ToString();
        }
      }
      else
      {
        if (Request.QueryString["counter"] != null)
        {
          m_counter = Request.QueryString["counter"].ToString();
          m_mode = "edit";
        }
        else
        {
          Response.Write("<script>alert('" + m_msg["T2"] + "');top.document.location.href='../Login.aspx';</script>");
          Response.End();
        }
      }
      if (Request.QueryString["refreshgrid"] != null)
        refreshgrid = Request.QueryString["refreshgrid"].ToString();
      UserRights r = new UserRights(Session["Login"].ToString(), "UserRights", "counter");
      m_rights = r.GetRights(Session["Login"].ToString(), "workorder");
      m_allowedit = Convert.ToInt16(m_rights["urEdit"].ToString());
      if (m_counter != "")
        objitemvendor = new ModuleoObject(Session["Login"].ToString(), "ItemVendor", "Counter", m_counter);
      else
        objitemvendor = new ModuleoObject(Session["Login"].ToString(), "ItemVendor", "Counter");

      screen = new AzzierScreen("inventory/itemvendor.aspx", "MainForm", MainControlsPanel.Controls,m_mode);

      screen.LoadScreen();
      screen.SetValidationControls();
    }

    protected void Page_Load(object sender, EventArgs e)
    {
      if (!Page.IsPostBack)
      {
        if (m_counter == "")
        {
          TextBox t = MainControlsPanel.FindControl("txtitemnum") as TextBox;
          if (t != null)
          {
            if (m_itemnum!="")
              t.Attributes.Add("readonly", "readonly");
          }
          t.Text = m_itemnum;
          t = MainControlsPanel.FindControl("txtdiscount") as TextBox;
          if (t != null)
            t.Text = "0";
          t = MainControlsPanel.FindControl("txtvendor") as TextBox;
          if (t != null)
          {
            if (m_itemnum == "" && m_vendor != "")
            t.Attributes.Add("readonly", "readonly");
          }
          t.Text = m_vendor;
        }
        else
        {
          screen.PopulateScreen("ItemVendor", objitemvendor.ModuleData);
          TextBox t = MainControlsPanel.FindControl("txtvendor") as TextBox;
          if (t != null)
            t.Attributes.Add("readonly", "readonly");

          t = MainControlsPanel.FindControl("txtitemnum") as TextBox;
          if (t != null)
            t.Attributes.Add("readonly", "readonly");


        }
        if (m_counter == "")
        {
          btndelete.Visible = false;
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

}