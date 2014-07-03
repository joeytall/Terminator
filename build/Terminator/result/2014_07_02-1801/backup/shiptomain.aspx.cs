using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Data.OleDb;

using Telerik.Web.UI;

public partial class ShiptoMain : System.Web.UI.Page
{
  protected AzzierScreen screen;
  protected string shipname;
  protected Boolean candelete;
  protected Boolean cansave;
  protected string mode;
  protected string tablename = "shipto";
  protected string referer = "";

  protected NameValueCollection m_msg = new NameValueCollection();

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

    if (Request.QueryString["referer"] != null)
      referer = Request.QueryString["referer"].ToString();

    if (Request.QueryString["shipname"] != null)
      shipname = Request.QueryString["shipname"];
    else
      shipname = "";

    if (shipname == "")
    {
      btnDelete.Visible = false;
      mode = "new";
    }
    else
    {
      if (drRights["urDelete"] == "1")
      {
        btnDelete.Visible = true;
      }
      else
      {
        btnDelete.Visible = false;
      }
      mode = "edit";
    }

    if ((drRights["urAddNew"] == "1" && mode == "new") || (drRights["urEdit"] == "1" && mode == "edit"))
    {
      btnSave.Visible = true;
    }
    else
    {
      btnSave.Visible = false;
    }
    screen = new AzzierScreen("codes/shiptomain.aspx", "MainForm", MainControlsPanel.Controls, mode, 1);

    Session.LCID = Convert.ToInt32(Session["LCID"]);
    screen.LCID = Session.LCID;
    screen.LoadScreen();
    screen.SetValidationControls();
    NameValueCollection nvc = new NameValueCollection();
  }

  protected void Page_Load(object sender, EventArgs e)
  {
    if (!Page.IsPostBack)
    {
      NameValueCollection nvc = new NameValueCollection();

      HyperLink lbl = (HyperLink)MainControlsPanel.FindControl("lblshiptype");
      CheckBox chk = (CheckBox)MainControlsPanel.FindControl("chkshiptype");

      if (shipname != "")
      {
        ModuleoObject obj = new ModuleoObject(Session["Login"].ToString(), "shipto", "shipname", shipname);
        nvc = obj.ModuleData;
      }

      screen.PopulateScreen("shipto", nvc);

      if (lbl != null)
      {
        lbl.Text = "Billing Address";
        if (referer == "billto")
        {
          lbl.Text = "Shipping Address";
        }
      }

      if (nvc["shiptype"] == "Both")
      {
        chk.Checked = true;
      }
      else
      {
        chk.Checked = false;
      }
    }
  }

  protected void Save(object sender, EventArgs e)
  {
    NameValueCollection nvc;
    Panel CntlPanel = Page.FindControl("MainControlsPanel") as Panel;
    TextBox tbx = null;
    string dirtylog = "0";
    if (CntlPanel != null)
    {
      nvc = screen.CollectFormValues("shipto", false);

      tbx = CntlPanel.FindControl("txtdirtylog") as TextBox;
      if (nvc["dirtylog"] == null)
      {
        if (tbx != null)
        {
          dirtylog = tbx.Text;
          nvc.Add("dirtylog", dirtylog);
        }
      }
      else
        dirtylog = nvc["dirtylog"];
    }
    else
      nvc = null;

    if (nvc["shiptype"] == "1")
    {
      nvc["shiptype"] = "Both";
    }
    else
    {
      nvc["shiptype"] = "Ship";
      if (referer == "billto")
      {
        nvc["shiptype"] = "Bill";
      }
    }
    //wostatuscode = (RadComboBox)MainControlsPanel.FindControl("cbbtcode1");
    //string tcode1 = wostatuscode.SelectedValue;
    //if (!string.IsNullOrEmpty(tcode1))
    //{
    //    nvc.Remove("tcode1");
    //    nvc.Add("tcode1", tcode1);
    //}
    //ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert(' " + wostatuscode.SelectedValue + " Hello')", true);

    ModuleoObject obj;

    bool success = false;
    if (shipname == "")
    {
      obj = new ModuleoObject(Session["Login"].ToString(), "shipto", "shipname");
      success = obj.Create(nvc);
    }
    else
    {
      obj = new ModuleoObject(Session["Login"].ToString(), "shipto", "shipname", shipname);
      success = obj.Update(nvc);
    }

    if (nvc["defaultaddress"] == "1") {
      string connstring = Application["ConnString"].ToString();
      OleDbConnection conn = new OleDbConnection(connstring);

      string cmdstr = "";
      if (nvc["shiptype"] == "Both")
      {
        cmdstr = "UPDATE ShipTo SET DefaultAddress = 0 WHERE ShipName <> '" + nvc["shipname"].ToString() + "'";
      }
      else
      {
        if (referer == "shipto")
        {
          cmdstr = "UPDATE ShipTo SET DefaultAddress = 0 WHERE ShipName <> '" + nvc["shipname"].ToString() + "' AND (ShipType = 'Ship' OR ShipType = 'Both')";
        }
        else
        {
          cmdstr = "UPDATE ShipTo SET DefaultAddress = 0 WHERE ShipName <> '" + nvc["shipname"].ToString() + "' AND (ShipType = 'Bill' OR ShipType = 'Both')";
        }
      }

      OleDbCommand cmd = new OleDbCommand(cmdstr, conn);

      // Try to open database and read information.
      try
      {
        conn.Open();
        cmd.ExecuteNonQuery();

      }
      catch (Exception err)
      {
        Response.Write(err.Message);
      }
      finally
      {
        conn.Close();
      }
    }

    if (success)
    {
      litScript1.Text = "setTimeout(\"CloseAndRebind()\",100)";
    }
    else
    {
      litScript1.Text = "alert('" + m_msg["T2"] + "')";
    }
  }

  protected void Delete(object sender, EventArgs e)
  {
    bool success = false;
    ModuleoObject obj = new ModuleoObject(Session["Login"].ToString(), "shipto", "shipname", shipname);

    success = obj.Delete();
    if (success)
    {
      litScript1.Text = "setTimeout(\"CloseAndRebind()\",100)";
    }
    else
    {
      litScript1.Text = "alert('" + m_msg["T3"] + "')";
    }
  }

  private void RetrieveMessage()
  {
    SystemMessage msg = new SystemMessage("codes/chgstatusmain.aspx");
    m_msg = msg.GetSystemMessage();
    msg.SetJsMessage(litMessage);
  }
}