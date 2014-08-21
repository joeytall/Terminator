import wx,os,sys
import Replace

class Terminator(wx.Frame):

    def __init__(self, parent, title, redirect=False, filename=None, useBestVisual=False, clearSigInt=True):
        super(Terminator, self).__init__(parent, title=title,
                size=(600, 800))

        self.currentDirectory = os.getcwd()
        self.InitUI()
        self.Centre()
        self.Show()

    def InitUI(self):
        panel = wx.Panel(self)

        sizer = wx.GridBagSizer(5, 5)

      ###################################################################

        folder = wx.StaticText(panel, label="Folder")
        sizer.Add(folder, pos=(1,0), flag=wx.LEFT, border=10)

        foldertc = wx.TextCtrl(panel, 1)
        sizer.Add(foldertc, pos=(1,1), span=(1,2), flag=wx.EXPAND, border=5)

        dirBtn = wx.Button(panel, label="Browse")
        dirBtn.Bind(wx.EVT_BUTTON, self.onDir)
        sizer.Add(dirBtn, pos=(1, 3), flag=wx.RIGHT, border=10)

       ###################################################################

        extension = wx.StaticText(panel, label="File Extension")
        sizer.Add(extension, pos=(2,0), flag=wx.LEFT, border=10)

        extensiontc = wx.TextCtrl(panel, 3)
        sizer.Add(extensiontc, pos=(2,1), span=(1,1), flag=wx.EXPAND, border=5)

        exclude = wx.StaticText(panel, label="Exclude File ends with...")
        sizer.Add(exclude, pos=(2,2), flag=wx.RIGHT, border=10)

        excludetc = wx.TextCtrl(panel, 3)
        sizer.Add(excludetc, pos=(2,3), flag=wx.LEFT, border=5)

      ###################################################################

        replace = wx.StaticText(panel, label="Replace")
        sizer.Add(replace, pos=(3,0), flag=wx.LEFT, border=10)

        replacetc = wx.TextCtrl(panel, 2,"<link type=\"text/css\" href=\"~/Styles/Customer.css\" rel=\"stylesheet\" />", size=(200, 100), style=wx.TE_MULTILINE)
        replacetc.SetInsertionPoint(13)
        sizer.Add(replacetc, pos=(3,1), span=(2,3), flag=wx.EXPAND, border=10)

       # text1 = wx.StaticText(panel, label="An Advanced Search and Replace Tool made by Joey!")
       # sizer.Add(text1, pos=(0,0), span=(1,4), flag=wx.ALIGN_CENTER_HORIZONTAL, border=10)

        icon = wx.StaticBitmap(panel, bitmap=wx.Bitmap('img/terminator.gif'))
        sizer.Add(icon, pos=(4, 0), flag=wx.TOP|wx.RIGHT|wx.ALIGN_RIGHT, border=5)

        #taskBarIcon = wx.Icon('terminator.ico', wx.BITMAP_TYPE_ICO)
        #tbicon = wx.TaskBarIcon()
        #tbicon.SetIcon(taskBarIcon , "I am an Icon")

       # wx.EVT_TASKBAR_RIGHT_UP(tbicon, OnTaskBarRight)

       # line = wx.StaticLine(panel)
       # sizer.Add(line, pos=(1, 0), span=(1, 5), flag=wx.EXPAND|wx.BOTTOM, border=10)


        console = wx.TextCtrl(panel, 3, "",size=(200, 100), style=wx.TE_MULTILINE|wx.TE_READONLY|wx.TE_RICH2)
        console.SetInsertionPoint(0)
        console.Disable()
        console.SetDefaultStyle(wx.TextAttr(wx.BLACK))
        sizer.Add(console, pos=(6, 0), span=(1, 5), flag=wx.EXPAND|wx.TOP|wx.LEFT|wx.RIGHT|wx.BOTTOM|wx.TE_MULTILINE|wx.TE_READONLY, border=10)

        # redirect text here
        redir=RedirectText(console)
        sys.stdout=redir

        replaceBtn = wx.Button(panel, label="Replace")
        replaceBtn.Bind(wx.EVT_BUTTON, self.replace)
        sizer.Add(replaceBtn, pos=(5, 0))

        checkBtn = wx.Button(panel, label="Check Random File")
        checkBtn.Bind(wx.EVT_BUTTON, self.check)
        sizer.Add(checkBtn, pos=(5, 1))

        applyBtn = wx.Button(panel, label="Apply")
        applyBtn.Bind(wx.EVT_BUTTON, self.applyChanges)
        sizer.Add(applyBtn, pos=(5, 2))

        undoBtn = wx.Button(panel, label="Undo")
        undoBtn.Bind(wx.EVT_BUTTON, self.undo)
        sizer.Add(undoBtn, pos=(5, 3))

        sizer.AddGrowableCol(2)
        sizer.AddGrowableRow(6)

        panel.SetSizer(sizer)

    def check(self, event):
      Replace.check()

    def applyChanges(self, event):
      Replace.applyChanges()

    def undo(self, event):
      Replace.undo()

    def onDir(self, event):
        last1 = self.FindWindowById(1).GetLastPosition()
        last3 = self.FindWindowById(3).GetLastPosition()
        self.FindWindowById(1).Remove(0, last1)
        self.FindWindowById(3).Remove(0, last3)
        dlg = wx.DirDialog(self, "Choose a directory:",
                style=wx.DD_DEFAULT_STYLE
                #| wx.DD_DIR_MUST_EXIST
                #| wx.DD_CHANGE_DIR
                )
        if dlg.ShowModal() == wx.ID_OK:
            self.FindWindowById(1).WriteText(dlg.GetPath())
            print "You chose %s" % dlg.GetPath()
        dlg.Destroy()

    def replace(self,event):
        path = self.FindWindowById(1).GetValue()
        replaceString = self.FindWindowById(2).GetValue()
        Replace.Main(path,replaceString)

class RedirectText(object):
    def __init__(self,aWxTextCtrl):
        self.out=aWxTextCtrl

    def write(self,string):
        self.out.WriteText(string)

def OnTaskBarRight(event):
    app.ExitMainLoop()

if __name__ == '__main__':
    app = wx.App(0)
    Terminator(None, title="Terminator")
    app.MainLoop()
