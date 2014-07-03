using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;

using Telerik.Web.UI;

public partial class FailurecodeMain : System.Web.UI.Page
{
    protected AzzierScreen screen;
    protected int screenwidth;
    protected string failurelevel;
    protected string counters;
    protected Boolean candelete;
    protected Boolean cansave;
    protected string mode;

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

        if (Request.QueryString["failurelevel"] != null)
        {
           failurelevel = Request.QueryString["failurelevel"].ToString();
            hidParentLevel.Value = (Convert.ToInt32(failurelevel) - 1).ToString();
        }
        else
           failurelevel = "";

        if (Request.QueryString["counter"] != null)
            counters = Request.QueryString["counter"];
        else
            counters = "";

        if (counters == "")
        {
            btndelete.Visible = false;
            mode = "new";
        }
        else
        {
            if (drRights["urDelete"] == "1")
            {
                btndelete.Visible = true;
            }
            else
            {
                btndelete.Visible = false;
            }
            mode = "edit";
        }

        if ((drRights["urAddNew"] == "1" && mode == "new") || (drRights["urEdit"] == "1" && mode == "edit"))
        {
            btnsave.Visible = true;
        }
        else
        {
            btnsave.Visible = false;
        }
        screen = new AzzierScreen("codes/failurecodemain.aspx", "MainForm", MainControlsPanel.Controls, mode, 1);

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
              Failurecode  obj = new Failurecode(Session["Login"].ToString(), "failurecode", "counter", counters);
                nvc = obj.ModuleData;
            }

            if (failurelevel == "1")
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
            level = (TextBox)MainControlsPanel.FindControl("txtfailurelevel");
            level.Text = failurelevel;

            screen.PopulateScreen("failurecode", nvc);
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
            nvc = screen.CollectFormValues("failurecode", false);
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

        Failurecode objFailurecode;

        bool success = false;
        if (counters == "")
        {
           objFailurecode = new Failurecode(Session["Login"].ToString(), "failurecode", "counter");
            success = objFailurecode.CreateFailurecode(nvc);
        }
        else
        {
          objFailurecode = new Failurecode(Session["Login"].ToString(), "failurecode", "counter", counters);
            success = objFailurecode.UpdateFailurecode(nvc);
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
        Failurecode objFailurecode = new Failurecode(Session["Login"].ToString(), "failurecode", "counter", counters);

        success = objFailurecode.DeleteFailurecode();
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
        SystemMessage msg = new SystemMessage("codes/failurecodemain.aspx");
        m_msg = msg.GetSystemMessage();
        msg.SetJsMessage(litMessage);
    }
}