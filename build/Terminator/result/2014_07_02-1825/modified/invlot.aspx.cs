using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections.Specialized;
using Telerik.Web.UI;

public partial class inventory_invlot : System.Web.UI.Page
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
        m_mode = "edit";
      }
      else
      {
        if (Request.QueryString["maincounter"] != null)
        {
          m_maincounter = Request.QueryString["maincounter"].ToString();
          m_mode = "new";
        }
        else
        {
          Response.Write("<script>alert('" + m_msg["T2"] + "');top.document.location.href='../Login.aspx';</script>");
          Response.End();
        }
      }

      UserRights r = new UserRights(Session["Login"].ToString(), "UserRights", "counter");
      m_rights = r.GetRights(Session["Login"].ToString(), "workorder");
      m_allowedit = Convert.ToInt16(m_rights["urEdit"].ToString());
      if (m_counter != "")
      {
        objInvLot = new ModuleoObject(Session["Login"].ToString(), "v_inventorylot", "Counter", m_counter);
        nvc = objInvLot.ModuleData;
      }
      else
      {
        ModuleoObject objmain = new ModuleoObject(Session["Login"].ToString(), "v_InventoryStoreroom", "Counter", m_maincounter);
        objInvLot = new ModuleoObject(Session["Login"].ToString(), "v_InventoryLot", "Counter");
        nvc = objInvLot.ModuleData;

        nvc["itemnum"] = objmain.ModuleData["itemnum"];
        nvc["itemdesc"] = objmain.ModuleData["itemdesc"];
        nvc["storeroom"] = objmain.ModuleData["storeroom"];
        nvc["position"] = objmain.ModuleData["defposition"];
        nvc["vendor"] = objmain.ModuleData["defvendor"];
        nvc["stocklevel"] = "0";
        nvc["price"] = "0";
        


      }
      screen = new AzzierScreen("inventory/invlot.aspx", "MainForm", MainControlsPanel.Controls,m_mode);

      screen.LoadScreen();
      screen.SetValidationControls();
    }

    protected void Page_Load(object sender, EventArgs e)
    {
      if (!Page.IsPostBack)
      {
        TextBox t = MainControlsPanel.FindControl("txtitemnum") as TextBox;
        if (t != null)
          t.Attributes.Add("readonly", "readonly");
        screen.PopulateScreen("v_inventorylot", nvc);
        if (m_counter != "")
        {
          
          t = MainControlsPanel.FindControl("txtprice") as TextBox;
          if (t!=null)
            t.Attributes.Add("readonly", "readonly");

          t = MainControlsPanel.FindControl("txtstocklevel") as TextBox;
          if (t != null)
            t.Attributes.Add("readonly", "readonly");

        }
        else
        {
          
          t = MainControlsPanel.FindControl("txtstocklevel") as TextBox;
          if (t != null && serialized)
            t.Attributes.Add("readonly", "readonly");

          t = MainControlsPanel.FindControl("txtinstoredate") as TextBox;
          if (t!=null)
            t.Text = DateTime.Today.ToShortDateString();

          t = MainControlsPanel.FindControl("txtreceivedate") as TextBox;
          if (t != null)
            t.Text = DateTime.Today.ToShortDateString();
          t = MainControlsPanel.FindControl("txtposition") as TextBox;
          if (t != null)
          {
            t.Text = nvc["position"].ToString();
          }

          t = MainControlsPanel.FindControl("txtvendor") as TextBox;
          if (t != null)
          {
            t.Text = nvc["vendor"].ToString();
          }

          btndelete.Visible = false;
        }

        RadioButtonList r = MainControlsPanel.FindControl("rblinactive") as RadioButtonList;

        if (r != null)
        {
          r.Style.Add("valign", "top");
          r.RepeatDirection = RepeatDirection.Horizontal;
          ListItem litm1 = new ListItem("Yes", "1");
          r.Items.Add(litm1);
          ListItem litm2 = new ListItem("No", "0");
          r.Items.Add(litm2);
           if (m_mode == "edit")
          {
            if (nvc["inactive"].ToString() == "1")
              r.SelectedIndex = 0;
            else
              r.SelectedIndex = 1;
          }
          else
          {
            r.SelectedIndex = 1;
          }
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