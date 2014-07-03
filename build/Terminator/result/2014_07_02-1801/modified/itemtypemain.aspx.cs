using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;

using Telerik.Web.UI;

public partial class ItemTypeMain : System.Web.UI.Page
{
    protected AzzierScreen screen;
    protected int screenwidth;
    protected string itemlevel;
    protected string counter;
    protected Boolean candelete;
    protected Boolean cansave;
    protected string mode;

    protected NameValueCollection m_msg = new NameValueCollection();

    protected void Page_Init(object sender, EventArgs e)
    {
        UserRights.CheckAccess('');

        UserRights right = new UserRights(Session["Login"].ToString(), "UserRights", "Counter");
        NameValueCollection drRights = right.GetRights(Session["Login"].ToString(), "Codes");

        if (Request.QueryString["itemlevel"] != null)
        {
            itemlevel = Request.QueryString["itemlevel"].ToString();
            hidParentLevel.Value = (Convert.ToInt32(itemlevel) - 1).ToString();
        }
        else
            itemlevel = "";

        if (Request.QueryString["counter"] != null)
            counter = Request.QueryString["counter"];
        else
            counter = "";

        if (counter == "")
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
        screen = new AzzierScreen("codes/itemtypemain.aspx", "MainForm", MainControlsPanel.Controls, mode, 1);
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

            if (counter != "")
            {
                ItemType obj = new ItemType(Session["Login"].ToString(), "ItemType", "counter", counter);
                nvc = obj.ModuleData;
            }

            if (itemlevel == "0")
            {
                TextBox parentcode;
                parentcode = (TextBox)MainControlsPanel.FindControl("txtparentcode");
                if (parentcode!=null)
                  parentcode.Visible = false;

                //  Label lblparentcode;
                HyperLink lblparentcode;
                lblparentcode = (HyperLink)MainControlsPanel.FindControl("lblparentcode");
                if (lblparentcode!=null)
                  lblparentcode.Visible = false;

                //  lookup lkuparentcode
                HyperLink lkuparentcode;
                lkuparentcode = (HyperLink)MainControlsPanel.FindControl("lkuparentcode");
                if (lkuparentcode!=null)
                  lkuparentcode.Visible = false;
            }

            TextBox level;
            level = (TextBox)MainControlsPanel.FindControl("txtitemlevel");
            level.Text = itemlevel;
            hidMode.Value = mode;
            screen.PopulateScreen("itemtype", nvc);
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
            nvc = screen.CollectFormValues("itemtype", false);
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

        ItemType objItemType;

        bool success = false;
        if (counter == "")
        {
            objItemType = new ItemType(Session["Login"].ToString(), "itemtype", "counter");
            success = objItemType.Create(nvc);
        }
        else
        {
            objItemType = new ItemType(Session["Login"].ToString(), "itemtype", "counter", counter);
            success = objItemType.Update(nvc);
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
        ItemType objItemType = new ItemType(Session["Login"].ToString(), "itemtype", "counter", counter);

        success = objItemType.Delete();
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
        SystemMessage msg = new SystemMessage("codes/itemtypemain.aspx");
        m_msg = msg.GetSystemMessage();
        msg.SetJsMessage(litMessage);
    }
}