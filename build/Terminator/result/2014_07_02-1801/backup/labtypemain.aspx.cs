using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;

using Telerik.Web.UI;

public partial class CraftMain : System.Web.UI.Page
{
    protected AzzierScreen screen;
    protected string counters;
    protected Boolean candelete;
    protected Boolean cansave;
    protected string mode;
    protected string tablename = "labortype";
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
        screen = new AzzierScreen("codes/labtypemain.aspx", "MainForm", MainControlsPanel.Controls, mode, 1);

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
                ModuleoObject obj = new ModuleoObject(Session["Login"].ToString(), "labortype", "labtype", counters);
                nvc = obj.ModuleData;
            }

            screen.PopulateScreen("labortype", nvc);
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
            nvc = screen.CollectFormValues("labortype", false);
                
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
        if (counters == "")
        {
            obj = new ModuleoObject(Session["Login"].ToString(), "labortype", "labtype");
            success = obj.Create(nvc);
        }
        else
        {
            obj = new ModuleoObject(Session["Login"].ToString(), "labortype", "labtype", counters);
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
        ModuleoObject obj = new ModuleoObject(Session["Login"].ToString(), "labortype", "labtype", counters);
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