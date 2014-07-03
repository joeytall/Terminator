using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections.Specialized;
using Telerik.Web.UI;

public partial class inventory_alternatepartmain : System.Web.UI.Page
{
  protected NameValueCollection m_msg = new NameValueCollection();
  protected string m_itemnum;
  protected string m_alternateitemnum;
  protected string m_counter = "";
  protected string m_mode = "";
  protected string m_main = "";
  protected AzzierScreen screen;
  protected NameValueCollection m_rights;
  protected Int16 m_allowedit;
  protected AlternatePart objalternate;

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
        if (Request.QueryString["alternateitemnum"] != null)
        {
          m_alternateitemnum = Request.QueryString["alternateitemnum"].ToString();
        }
      }
      
      UserRights r = new UserRights(Session["Login"].ToString(), "UserRights", "counter");
      m_rights = r.GetRights(Session["Login"].ToString(), "Inventory");
      m_allowedit = Convert.ToInt16(m_rights["urEdit"].ToString());

      screen = new AzzierScreen("inventory/alternatepartmain.aspx", "MainForm", MainControlsPanel.Controls,m_mode);
      if (m_alternateitemnum == "")
        objalternate = new AlternatePart(Session["Login"].ToString(), "AlternatePart", "Counter");
      else
      {
        objalternate = new AlternatePart(Session["Login"].ToString(), "AlternatePart", "Counter", m_itemnum, m_alternateitemnum);
        m_counter = objalternate.ModuleData["Counter"];
      }

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
          t.Text = m_itemnum;
        }
        else
        {
          screen.PopulateScreen("Alternatepart", objalternate.ModuleData);
        }
        if (m_counter == "")
        {
          btndelete.Visible = false;
        }
        CheckBox chkreverse = MainControlsPanel.FindControl("chksavereverse") as CheckBox;
        chkreverse.Checked = true;
      }
      hidMode.Value = m_mode;
    }

    private void RetrieveMessage()
    {
      SystemMessage msg = new SystemMessage("pm/pmseason.aspx");
      m_msg = msg.GetSystemMessage();
      msg.SetJsMessage(litMessage);
    }

}