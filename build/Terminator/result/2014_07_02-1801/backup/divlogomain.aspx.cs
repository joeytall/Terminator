using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;

using Telerik.Web.UI;

public partial class DivLogoMain : System.Web.UI.Page
{
    protected AzzierScreen screen;
    protected string counters;
    protected Boolean candelete;
    protected Boolean cansave;
    protected string mode;
    protected string tablename = "divdefaults";
    protected string referer = "";
    protected string system = "";
    protected RadComboBox wostatuscode;
    

    protected NameValueCollection m_msg = new NameValueCollection();

    protected void Page_Init(object sender, EventArgs e)
    {
        RetrieveMessage();
        if (Session["Login"] == null)
        {
            Response.Write("<html><script type=\"text/javascript\">alert('" + m_msg["T1"] + "');top.document.location.href='../login.aspx';</script></html>");
            Response.End();
        }

        if (Request.QueryString["referer"] != null)
            referer = Request.QueryString["referer"].ToString();

        if (Request.QueryString["counter"] != null)
            counters = Request.QueryString["counter"];
        else
            counters = "";

        if (Request.QueryString["system"] != null)
            system = Request.QueryString["system"].ToString();
        else
            system = "";

       
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
        ////if (system == "1")
        ////{
        //    btnDelete.Visible = false;
        ////}

        //btnSave.Visible = true;
        screen = new AzzierScreen("codes/divlogomain.aspx", "MainForm", MainControlsPanel.Controls, mode, 1);

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
                ModuleoObject obj = new ModuleoObject(Session["Login"].ToString(), "divdefaults", "counter", counters);
                nvc = obj.ModuleData;
            }

            AzzierData objWostatus = new AzzierData();
            NameValueCollection NvcWostatus = objWostatus.GetWOStatusCode("divdefault");

            RadComboBoxItem comboBoxItem;
            wostatuscode = (RadComboBox)MainControlsPanel.FindControl("cbbcategory");
            //wostatuscode.UniqueID = "cbbtcode1";
            wostatuscode.OnClientSelectedIndexChanged = "categoryselected";
            for (int i = 0; i < NvcWostatus.Count; i++)
            {
                comboBoxItem = new RadComboBoxItem();
                comboBoxItem.Text = NvcWostatus.Get(i);
                comboBoxItem.Value = NvcWostatus.GetKey(i);
                if (referer == NvcWostatus.GetKey(i))
                {
                    comboBoxItem.Selected = true;
                }

                wostatuscode.Items.Add(comboBoxItem);
            }
            if (mode == "edit")
                wostatuscode.Enabled = false;

            screen.PopulateScreen("divdefaults", nvc);
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
            nvc = screen.CollectFormValues("divdefaults", false);
                
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


        //List<string> values = new List<string>();
        //values.AddRange(nvc.AllKeys.SelectMany(nvc.GetValues).Where(getValues => getValues != null));
        //string Values = string.Join(",", values.ToArray());
        //List<string> items = new List<string>();

        //foreach (String name in nvc)
        //    items.Add(String.Concat(name, "=", System.Web.HttpUtility.UrlEncode(nvc[name])));

        //string Values = String.Join("&", items.ToArray());

        //ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert(' " + Values + " Hello')", true);


        wostatuscode = (RadComboBox)MainControlsPanel.FindControl("cbbcategory");
        //string category = wostatuscode.SelectedValue;
        string category = wostatuscode.SelectedItem.Text;
        if (!string.IsNullOrEmpty(category))
        {
            nvc.Remove("category");
            nvc.Add("category", category);
        }


        ModuleoObject obj;

        bool success = false;
        if (counters == "")
        {
            obj = new ModuleoObject(Session["Login"].ToString(), "divdefaults", "counter");
            success = obj.Create(nvc);
        }
        else
        {
            obj = new ModuleoObject(Session["Login"].ToString(), "divdefaults", "counter", counters);
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
        ModuleoObject obj = new ModuleoObject(Session["Login"].ToString(), "divdefaults", "counter", counters);

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