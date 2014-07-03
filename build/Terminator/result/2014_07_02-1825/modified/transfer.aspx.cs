using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections.Specialized;
using Telerik.Web.UI;

public partial class inventory_transfer : System.Web.UI.Page
{
  protected NameValueCollection m_msg = new NameValueCollection();
  protected string m_itemnum;
  protected string m_storeroom;
  protected string m_position;
  protected string m_lotnum;
  protected string m_counter = "";
  protected string m_mode = "";
  protected string m_maincounter;
  protected Boolean serialized = false;
  protected AzzierScreen screen;

  protected NameValueCollection m_rights;
  protected Int16 m_allowedit;
  protected ModuleoObject objInvLot;
  protected NameValueCollection nvc;

    protected void Page_Init(object sender, EventArgs e)
    {
      RetrieveMessage();
      UserRights.CheckAccess('');
      Session.LCID = Convert.ToInt32(Session["LCID"]);
      if (Request.QueryString["counter"] != null)
      {
        m_counter = Request.QueryString["counter"].ToString();
      }
      else
      {
        Response.Write("<script>alert('" + m_msg["T2"] + "');top.document.location.href='../Login.aspx';</script>");
        Response.End();
      }

      UserRights r = new UserRights(Session["Login"].ToString(), "UserRights", "counter");
      m_rights = r.GetRights(Session["Login"].ToString(), "Inventory");
      m_allowedit = Convert.ToInt16(m_rights["urEdit"].ToString());
      objInvLot = new ModuleoObject(Session["Login"].ToString(), "InvLot", "Counter", m_counter);
      nvc = objInvLot.ModuleData;

      screen = new AzzierScreen("inventory/transfer.aspx", "MainForm", MainControlsPanel.Controls,m_mode);

      screen.LoadScreen();
      screen.SetValidationControls();
    }

    protected void Page_Load(object sender, EventArgs e)
    {
      if (!Page.IsPostBack)
      {
        screen.PopulateScreen("invlot", nvc);
      }

    }

    private void RetrieveMessage()
    {
     // SystemMessage msg = new SystemMessage("workorder/batchclose.aspx");
        SystemMessage msg = new SystemMessage("pm/pmseason.aspx");
      m_msg = msg.GetSystemMessage();
      msg.SetJsMessage(litMessage);

    }

}