using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;

using Telerik.Web.UI;

public partial class Codes_CompRemarkMain : System.Web.UI.Page
{
    protected AzzierScreen screen;
    protected string code ="";
    protected Boolean candelete;
    protected Boolean cansave;
    protected string mode;
    protected string tablename = "CloseRemarks";
    protected string referer = "";    

    protected NameValueCollection m_msg = new NameValueCollection();

    protected void Page_Init(object sender, EventArgs e)
    {
        RetrieveMessage();
        UserRights.CheckAccess('');

        UserRights right = new UserRights(Session["Login"].ToString(), "UserRights", "Counter");
        NameValueCollection drRights = right.GetRights(Session["Login"].ToString(), "Codes");

        if (Request.QueryString["referer"] != null)
            referer = Request.QueryString["referer"].ToString();

        if (Request.QueryString["Code"] != null)
            code = Request.QueryString["Code"];
        else
            code = "";        
       
        if (code == "")
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
        screen = new AzzierScreen("codes/compremarkmain.aspx", "MainForm", MainControlsPanel.Controls, mode, 1);

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

            if (code != "")
            {
                ModuleoObject obj = new ModuleoObject(Session["Login"].ToString(), "CloseRemarks", "CloseRemCode", code);
                nvc = obj.ModuleData;
            }

            screen.PopulateScreen("CloseRemarks", nvc);
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
            nvc = screen.CollectFormValues("CloseRemarks", false);
                
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
        if (code == "")
        {
            obj = new ModuleoObject(Session["Login"].ToString(), "CloseRemarks", "CloseRemCode");
            success = obj.Create(nvc);
        }
        else
        {
          obj = new ModuleoObject(Session["Login"].ToString(), "CloseRemarks", "CloseRemCode", code);
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
        ModuleoObject obj = new ModuleoObject(Session["Login"].ToString(), "CloseRemarks", "CloseRemCode", code);
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