using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;

using Telerik.Web.UI;

public partial class DistrictMain : System.Web.UI.Page
{
    protected AzzierScreen screen;
    protected int screenwidth;
    protected string districtcode;
    protected string counters;
    protected Boolean candelete;
    protected Boolean cansave;
    protected string mode;

    protected NameValueCollection m_msg = new NameValueCollection();

    protected void Page_Init(object sender, EventArgs e)
    {
        RetrieveMessage();
        UserRights.CheckAccess('');

        UserRights right = new UserRights(Session["Login"].ToString(), "UserRights", "Counter");
        NameValueCollection drRights = right.GetRights(Session["Login"].ToString(), "Codes");

        if (Request.QueryString["districtcode"] != null)
        {
            districtcode = Request.QueryString["districtcode"].ToString();
            hidParentLevel.Value = (Convert.ToInt32(districtcode) - 1).ToString();
        }
        else
            districtcode = "";

        if (Request.QueryString["district"] != null)
            counters = Request.QueryString["district"];
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

        Session.LCID = Convert.ToInt32(Session["LCID"]);
        screen = new AzzierScreen("codes/districtmain.aspx", "MainForm", MainControlsPanel.Controls, mode, 1);
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
                ModuleoObject obj = new ModuleoObject(Session["Login"].ToString(), "districts", "district", counters);
                nvc = obj.ModuleData;
            }

            if (districtcode == "1")
            {
                TextBox parentcode;
                parentcode = (TextBox)MainControlsPanel.FindControl("txtparentdistrict");
                parentcode.Visible = false;

                //  Label lblparentcode;
                HyperLink lblparentcode;
                lblparentcode = (HyperLink)MainControlsPanel.FindControl("lblparentdistrict");
                lblparentcode.Visible = false;

                //  lookup lkuparentcode
                HyperLink lkuparentcode;
                lkuparentcode = (HyperLink)MainControlsPanel.FindControl("lkuparentdistrict");
                lkuparentcode.Visible = false;
            }

            screen.PopulateScreen("districts", nvc);

            TextBox level;
            level = (TextBox)MainControlsPanel.FindControl("txtdistrictcode");
            level.Text = districtcode;
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
            nvc = screen.CollectFormValues("districts", false);
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
            obj = new ModuleoObject(Session["Login"].ToString(), "districts", "district");
            success =obj.Create(nvc);
        }
        else
        {
            obj = new ModuleoObject(Session["Login"].ToString(), "districts", "district",counters);
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
        ModuleoObject obj = new ModuleoObject(Session["Login"].ToString(), "districts", "district", counters);

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
        SystemMessage msg = new SystemMessage("codes/eqtypemain.aspx");
        m_msg = msg.GetSystemMessage();
        msg.SetJsMessage(litMessage);
    }
}