using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;

using Telerik.Web.UI;

public partial class Codes_WOTypeMain : System.Web.UI.Page
{
    protected AzzierScreen screen;
    protected string wotype;
    protected Boolean candelete;
    protected Boolean cansave;
    protected string mode;
    protected string referer = "";
    protected string system = "";

    protected NameValueCollection m_msg = new NameValueCollection();

    protected void Page_Init(object sender, EventArgs e)
    {
        RetrieveMessage();
        if (Session["Login"] == null)
        {
            Response.Write("<html><script type=\"text/javascript\">alert('" + m_msg["T1"] + "');top.document.location.href='../login.aspx';</script></html>");
            Response.End();
        }

        if (Request.QueryString["WOType"] != null)
            wotype = Request.QueryString["WOType"];
        else
            wotype = "";

        if (wotype == "")
        {
            btnDelete.Visible = false;
            mode = "new";
        }
        else
        {
            mode = "edit";
        }

        screen = new AzzierScreen("codes/wotypemain.aspx", "MainForm", MainControlsPanel.Controls, mode, 1);

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

            if (wotype != "")
            {
                ModuleoObject obj = new ModuleoObject(Session["Login"].ToString(), "WorkType", "WOType", wotype);
                nvc = obj.ModuleData;
            }
            if (nvc["system"] != null)
                system = nvc["system"].ToString();
            else
                system = "";

            if (system == "1")
            {
                btnDelete.Visible = false;
            }
            
            screen.PopulateScreen("WorkType", nvc);
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
            nvc = screen.CollectFormValues("worktype", false);
                
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

        ModuleoObject obj;

        bool success = false;
        if (wotype == "")
        {
            obj = new ModuleoObject(Session["Login"].ToString(), "WorkType", "WOType");
            success = obj.Create(nvc);
        }
        else
        {
            obj = new ModuleoObject(Session["Login"].ToString(), "WorkType", "WOType", wotype);
            success = obj.Update(nvc);
        }

        if (success)
        {
            litScript1.Text = "setTimeout(\"CloseAndRebind()\",100)";
        }
        else
        {
            litScript1.Text = "alert(\"" + obj.ErrorMessage + "\")";
        }
    }

    protected void Delete(object sender, EventArgs e)
    {
        bool success = false;
        ModuleoObject obj = new ModuleoObject(Session["Login"].ToString(), "WorkType", "WOType", wotype);

        success = obj.Delete();
        if (success)
        {
            litScript1.Text = "setTimeout(\"CloseAndRebind()\",100)";
        }
        else
        {
            litScript1.Text = "alert(\"" + m_msg["T3"] + "\")";
        }
    }

    private void RetrieveMessage()
    {
        SystemMessage msg = new SystemMessage("codes/chgstatusmain.aspx");
        m_msg = msg.GetSystemMessage();
        msg.SetJsMessage(litMessage);
    }
}