using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;

using Telerik.Web.UI;

public partial class ChgStatusMain : System.Web.UI.Page
{
    protected AzzierScreen screen;
    protected string counters;
    protected Boolean candelete;
    protected Boolean cansave;
    protected string mode;
    protected string tablename = "Codes";
    protected string referer = "", statuscode = "";
    protected string system = "";
    protected RadComboBox wostatuscode;
    protected NameValueCollection m_msg = new NameValueCollection();

    protected void Page_Init(object sender, EventArgs e)
    {
        RetrieveMessage();
        UserRights.CheckAccess('');

        if (Request.QueryString["referer"] != null)
        {
            referer = Request.QueryString["referer"].ToString();
            if (referer == "WO") { hidStatusModuleName.Value = "wostatus"; }
            else if (referer == "PO") { hidStatusModuleName.Value = "postatus"; }
            else if (referer == "PROJ") { hidStatusModuleName.Value = "projstatus"; }
        }

        if (Request.QueryString["counter"] != null)
            counters = Request.QueryString["counter"];
        else
            counters = "";

        if (counters == "")
        {
            btnDelete.Visible = false;
            mode = "new";
        }
        else
        {
            btnDelete.Visible = true;
            mode = "edit";
        }

        btnSave.Visible = true;
        screen = new AzzierScreen("codes/chgstatusmain.aspx", "MainForm", MainControlsPanel.Controls, mode, 1);

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

            if (counters != "")
            {
                ModuleoObject obj = new ModuleoObject(Session["Login"].ToString(), "v_StatusCodeWithDesc", "counter", counters);
                nvc = obj.ModuleData;
            }

            if (nvc["tcode1"] != null)
            {
                hidStatusCode.Value = nvc["tcode1"];
                HyperLink statusCtl;
                statusCtl = (HyperLink)MainControlsPanel.FindControl("lblstarttcode2");
                statusCtl.Text = nvc["tcode1"];
                int val = Convert.ToInt32(nvc["tcode1"]);
                if (val == -1 || val == 400)
                {
                    if (val == -1)
                        statusCtl.Text = "<=" + val.ToString();
                    else
                        statusCtl.Text = ">=" + val.ToString();
                }
                else
                {
                    statusCtl.Text = val.ToString() + " -- " + (val + 99).ToString();
                }
            }

            if (nvc["system"] != null)
                system = nvc["system"].ToString();
            else
                system = "";

            if (system == "1")
            {
                btnDelete.Visible = false;
            }

            AzzierData objWostatus = new AzzierData();
            NameValueCollection NvcWostatus = objWostatus.GetWOStatusCode("wostatuscode");

            RadComboBoxItem comboBoxItem;
            wostatuscode = (RadComboBox)MainControlsPanel.FindControl("cbbtcode1");
            wostatuscode.OnClientSelectedIndexChanged = "selectedindexchanged";
            if (string.IsNullOrEmpty(nvc["tcode1"]))
            {
                comboBoxItem = new RadComboBoxItem();
                comboBoxItem.Text = "      ";
                comboBoxItem.Value = "";
                wostatuscode.Items.Add(comboBoxItem);
            }

            for (int i = 0; i < NvcWostatus.Count; i++)
            {
                comboBoxItem = new RadComboBoxItem();
                comboBoxItem.Text = NvcWostatus.Get(i);
                comboBoxItem.Value = NvcWostatus.GetKey(i);
                if (nvc["tcode1"] == NvcWostatus.GetKey(i))
                {
                    comboBoxItem.Selected = true;
                    statuscode = nvc["tcode1"].ToString();
                }
                wostatuscode.Items.Add(comboBoxItem);
            }
            if (mode == "edit")
            {
                wostatuscode.Enabled = false;
            }

            screen.PopulateScreen("v_StatusCodeWithDesc", nvc);
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
          //  nvc = screen.CollectFormValues("codes", false);
            nvc = screen.CollectFormValues("v_StatusCodeWithDesc",false);

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

        if (referer == "WO")
            nvc["tfield"] = "wostatus";
        if (referer == "PO")
            nvc["tfield"] = "postatus";
        if (referer == "PROJ")
            nvc["tfield"] = "projstatus";


        wostatuscode = (RadComboBox)MainControlsPanel.FindControl("cbbtcode1");
        string tcode1 = wostatuscode.SelectedValue;
        if (!string.IsNullOrEmpty(tcode1))
        {
            nvc.Remove("tcode1");
            nvc.Add("tcode1", tcode1);
        }
        //ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert(' " + wostatuscode.SelectedValue + " Hello')", true);

        ModuleoObject obj;

        bool success = false;
        if (counters == "")
        {
            obj = new ModuleoObject(Session["Login"].ToString(), "codes", "counter");
            success = obj.Create(nvc);
        }
        else
        {
            obj = new ModuleoObject(Session["Login"].ToString(), "codes", "counter", counters);
            success = obj.Update(nvc);
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
        ModuleoObject obj = new ModuleoObject(Session["Login"].ToString(), "codes", "counter", counters);

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