import wx,os,sys
import Replace

class Terminator(wx.Frame):

    def __init__(self, parent, title):
        super(Terminator, self).__init__(parent, title=title,
                size=(520, 400))


        self.currentDirectory = os.getcwd()
        self.InitUI()
        self.Centre()
        self.Show()

    def InitUI(self):
        panel = wx.Panel(self)

        sizer = wx.GridBagSizer(5, 5)

      ###################################################################

        text1 = wx.StaticText(panel, label="Folder")
        sizer.Add(text1, pos=(1,0), flag=wx.LEFT, border=10)

        tc1 = wx.TextCtrl(panel, 1)
        sizer.Add(tc1, pos=(1,1), span=(1,2), flag=wx.EXPAND, border=5)

        dirBtn = wx.Button(panel, label="Browse")
        dirBtn.Bind(wx.EVT_BUTTON, self.onDir)
        sizer.Add(dirBtn, pos=(1, 3), flag=wx.RIGHT, border=10)

      ###################################################################

        text2 = wx.StaticText(panel, label="Replace with")
        sizer.Add(text2, pos=(3,0), flag=wx.LEFT, border=10)

        tc2 = wx.TextCtrl(panel, -1,"",size=(200, 100), style=wx.TE_MULTILINE)
        tc2.SetInsertionPoint(0)
        sizer.Add(tc2, pos=(3,1), span=(2,3), flag=wx.EXPAND, border=10)

       # text1 = wx.StaticText(panel, label="An Advanced Search and Replace Tool made by Joey!")
       # sizer.Add(text1, pos=(0,0), span=(1,4), flag=wx.ALIGN_CENTER_HORIZONTAL, border=10)

       # icon = wx.StaticBitmap(panel, bitmap=wx.Bitmap('UI/terminator.gif'))
       # sizer.Add(icon, pos=(0, 4), flag=wx.TOP|wx.RIGHT|wx.ALIGN_RIGHT, border=5)

       # taskBarIcon = wx.Icon('UI/terminator.ico', wx.BITMAP_TYPE_ICO)
       # tbicon = wx.TaskBarIcon()
       # tbicon.SetIcon(taskBarIcon , "I am an Icon")

       # wx.EVT_TASKBAR_RIGHT_UP(tbicon, OnTaskBarRight)

       # line = wx.StaticLine(panel)
       # sizer.Add(line, pos=(1, 0), span=(1, 5), flag=wx.EXPAND|wx.BOTTOM, border=10)


        console = wx.TextCtrl(panel, -1,"",size=(200, 100), style=wx.TE_MULTILINE|wx.TE_READONLY)
        console.SetInsertionPoint(0)
        sizer.Add(console, pos=(6, 0), span=(1, 5), flag=wx.EXPAND|wx.TOP|wx.LEFT|wx.RIGHT|wx.BOTTOM|wx.TE_MULTILINE|wx.TE_READONLY, border=10)

        # redirect text here
        redir=RedirectText(console)
        sys.stdout=redir

        button4 = wx.Button(panel, label="Replace")
        button4.Bind(wx.EVT_BUTTON, self.replace)
        sizer.Add(button4, pos=(5, 1))

        button5 = wx.Button(panel, label="Undo")
        sizer.Add(button5, pos=(5, 2))

        sizer.AddGrowableCol(2)
        sizer.AddGrowableRow(6)

        panel.SetSizer(sizer)

    def onDir(self, event):
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
        Replace.Main(path)

class RedirectText(object):
    def __init__(self,aWxTextCtrl):
        self.out=aWxTextCtrl

    def write(self,string):
        self.out.WriteText(string)

def OnTaskBarRight(event):
    app.ExitMainLoop()

if __name__ == '__main__':

    app = wx.App()
    Terminator(None, title="Terminator")
    app.MainLoop()
