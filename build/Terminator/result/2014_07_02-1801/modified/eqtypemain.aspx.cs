using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;

using Telerik.Web.UI;

public partial class EQTypeMain : System.Web.UI.Page
{
    protected AzzierScreen screen;
    protected int screenwidth;
    protected string eqlevel;
    protected string counters;
    protected Boolean candelete;
    protected Boolean cansave;
    protected string mode;

    protected NameValueCollection m_msg = new NameValueCollection();

    protected void Page_Init(object sender, EventArgs e)
    {
        UserRights.CheckAccess('');

        UserRights right = new UserRights(Session["Login"].ToString(), "UserRights", "Counter");
        NameValueCollection drRights = right.GetRights(Session["Login"].ToString(), "Codes");

        if (Request.QueryString["eqlevel"] != null)
        {
            eqlevel = Request.QueryString["eqlevel"].ToString();
            hidParentLevel.Value = (Convert.ToInt32(eqlevel) - 1).ToString();
        }
        else
            eqlevel = "";

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
        //HidFilename.Value = "codes/eqtypemain.aspx";
        screen = new AzzierScreen("codes/eqtypemain.aspx", "MainForm", MainControlsPanel.Controls, mode, 1);
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
                EQType obj = new EQType(Session["Login"].ToString(), "eqtype", "counter", counters);
                nvc = obj.ModuleData;
            }

            if (eqlevel == "0")
            {
                TextBox parentcode;
                parentcode = (TextBox)MainControlsPanel.FindControl("txtparentcode");
                parentcode.Visible = false;

                //  Label lblparentcode;
                HyperLink lblparentcode;
                lblparentcode = (HyperLink)MainControlsPanel.FindControl("lblparentcode");
                lblparentcode.Visible = false;

                //  lookup lkuparentcode
                HyperLink lkuparentcode;
                lkuparentcode = (HyperLink)MainControlsPanel.FindControl("lkuparentcode");
                lkuparentcode.Visible = false;
            }

            TextBox level;
            level = (TextBox)MainControlsPanel.FindControl("txteqlevel");
            level.Text = eqlevel;

            screen.PopulateScreen("eqtype", nvc);
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
            nvc = screen.CollectFormValues("eqtype", false);
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

        EQType objEQType;

        bool success = false;
        if (counters == "")
        {
            objEQType = new EQType(Session["Login"].ToString(), "eqtype", "counter");
            success = objEQType.CreateEQType(nvc);
        }
        else
        {
            objEQType = new EQType(Session["Login"].ToString(), "eqtype", "counter", counters);
            success = objEQType.UpdateEQType(nvc);
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
        EQType objEQType = new EQType(Session["Login"].ToString(), "eqtype", "counter", counters);

        success = objEQType.DeleteEQType();
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
        SystemMessage msg = new SystemMessage("codes/eqtypemain.aspx");
        m_msg = msg.GetSystemMessage();
        msg.SetJsMessage(litMessage);
    }
}