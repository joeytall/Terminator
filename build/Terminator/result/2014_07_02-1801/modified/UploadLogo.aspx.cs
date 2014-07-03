using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.IO;

using Telerik.Web.UI;

public partial class UploadLogo : System.Web.UI.Page
{
    protected AzzierScreen screen;
    protected NameValueCollection m_msg = new NameValueCollection();
    protected Panel uploadpanel;
    protected string logofilename;
    protected RadAsyncUpload uploadlogo;

    protected void Page_Load(object sender, EventArgs e)
    {
        RetrieveMessage();
        if (Session["Login"] == null)
        {
            Response.Write("<html><script type=\"text/javascript\">alert('" + m_msg["T1"] + "');top.document.location.href='../login.aspx';</script></html>");
            Response.End();
        }

        if (Request.QueryString["logofilename"] != null)
        {
            logofilename = Request.QueryString["logofilename"].ToString();
        }
        else logofilename = "";

        screen = new AzzierScreen("codes/uploadlogo.aspx", "MainForm", MainControlsPanel.Controls, "new", 1);

        Session.LCID = Convert.ToInt32(Session["LCID"]);
        screen.LCID = Session.LCID;
        
        uploadpanel = new Panel();
        uploadpanel.ID = "trcradupload";
        MainControlsPanel.Controls.Add(uploadpanel);
        screen.LoadScreen();
        AddradUpload();
        screen.SetValidationControls();
    }

    protected void AddradUpload()
    {
        RadProgressArea radprogressarea = new RadProgressArea();

        radprogressarea.ID = "RadProgressArea1";
        uploadlogo = new RadAsyncUpload();
        uploadlogo.ID = "RadUpload1";
        //uploadlogo
        uploadlogo.FileUploaded += RadAsyncUpload1_FileUploaded;
        //uploadlogo.MaxFileInputsCount=1;//.ControlObjectsVisibility = "None";// ControlObjectsVisibility.None;// "None";
        uploadlogo.MaxFileInputsCount = 1;
        uploadlogo.Width = 350;// "300px";
        uploadlogo.Skin = "Outlook";
        uploadlogo.OnClientValidationFailed = "OnClientValidationFailed";
        //string[] imgFile = new string[] { "one", "two", "three" };
        //uploadlogo.AllowedFileExtensions = imgFile;
        uploadlogo.AllowedFileExtensions = new string[] { "jpg","jpeg", "png", "gif"};

        uploadlogo.Controls.Add(radprogressarea);

        //RadUpload uploadfile = new RadUpload();
        //uploadfile.ID = "RadUpload1";
        //uploadfile.TargetFolder = "~/IMAGES";
        ////uploadfile.FileUploaded += RadAsyncUpload1_FileUploaded;

        //uploadfile.ControlObjectsVisibility = ControlObjectsVisibility.None;// "None";
        //uploadfile.MaxFileInputsCount = 1;
        //uploadfile.Width = 350;// "300px";
        //uploadfile.Skin = "Outlook";// "Office2007";
        //uploadfile.Controls.Add(radprogressarea);
        uploadpanel.Controls.Add(uploadlogo);
    }


    protected void RadAsyncUpload1_FileUploaded(object sender, Telerik.Web.UI.FileUploadedEventArgs e)
    {
        RadAsyncUpload upload;
        upload = (RadAsyncUpload)MainControlsPanel.FindControl("RadUpload1");

        foreach (UploadedFile file in upload.UploadedFiles)
        {
            string path = MapPath("~/stephen/UploadIMG");
            file.SaveAs(Path.Combine(path, logofilename),true);
        }

        litScript1.Text = "setTimeout(\"Close()\",100)";
    }


    protected void Button1_Click(object sender, EventArgs e)
    {
        //RadAsyncUpload upload;
        //upload = (RadAsyncUpload)MainControlsPanel.FindControl("RadUpload1");
        
        //foreach (UploadedFile file in upload.UploadedFiles)
        //{
        //    //string path = Server.MapPath("Files/");
        //    string path = MapPath("~/stephen/UploadIMG");
        //    //MapPath
        //   // string path = upload.TargetFolder;
        //    //litScript1.Text = "alert('" + logofilename + "')";
        //    //file.SaveAs(path + file.FileName);   //???
        //    //litScript1.Text = "alert('" + path.Replace('/', '-') + " -- " + file.FileName.Replace('/', '-') + "')";
        //    //litScript1.Text = "alert('" + path.Replace('/', '-') + " -- " + file.GetName().ToString() + "')";
        //    //litScript1.Text = "alert('" + path.Replace('/', '-') + " -- " + logofilename + "')";
        //    //file.SaveAs(path+file.GetName().ToString());
        //    //file.SaveAs(path + logofilename);
        //    //litScript1.Text = "alert('" + Server.MapPath(Path.Combine(path, "abc.jpg")).ToString().Replace('/',' ') + "')";
        //    //litScript1.Text = "alert('" + Path.Combine(path, "abc.jpg").ToString().Replace('/',' ') + "')";
        //    //file.SaveAs(Path.Combine(path, "abc.jpg"));
        //    file.SaveAs(Path.Combine(path,logofilename));
        //    //file.SaveAs
        //}
    }

    private void RetrieveMessage()
    {
        SystemMessage msg = new SystemMessage("codes/chgstatusmain.aspx");
        m_msg = msg.GetSystemMessage();
        //msg.SetJsMessage(litMessage);
    }
}
