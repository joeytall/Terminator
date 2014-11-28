# vim: tabstop=2 expandtab shiftwidth=2 softtabstop=2
import os, shutil, errno, datetime, random, pdb

path, folderName, replaceString = "", "", ""
imgPath = "/Volumes/Webwork/Production/Azzierdev/Images2"
IMAGEPath = "/Volumes/Webwork/Production/Azzierdev/IMAGES"

def Main(pathFromUser, replaceStringFromUser):
  global path, replaceString, problemFiles, replacedFiles
  path = pathFromUser
  replaceString = replaceStringFromUser
  problemFiles = []
  replacedFiles = []
  count = 0
  mkdir()

  for file in os.listdir(path):
    if file.endswith(("list.aspx","list.aspx.cs")):
      shutil.copyfile(path + "/" + file, folderName + "/backup/" + file)
      if replace(file, replaceString) == False:
        problemFiles.append(file)
      else:
        replacedFiles.append(file)
      count+=1
    #if count == 1: break
  print ("\n")
  print (folderName)
  print (str(count) + " files scanned!")
  print (str(len(problemFiles)) + " problem files found.")
  print (str(len(replacedFiles)) + " files replaced.")
  if len(problemFiles) != 0:
    print ("\nProblem Files: " + ', '.join(problemFiles))
  if len(replacedFiles) != 0:
    print ("\nReplaced Files: " + ', '.join(replacedFiles))
  print ("******************************************************************************************************************")
  return

def check():
  if len(replacedFiles) == 0:
    return
  file = random.choice(os.listdir(folderName + "/modified"))
  # while file in problemFiles:
  #   file = random.choice(os.listdir(folderName + "/modified"))
  while file not in replacedFiles:
    file = random.choice(os.listdir(folderName + "/modified"))
  print (file + "\n")
  with open(folderName + "/modified/" + file) as f:
    for line in f:
      if 'Customer.css' in line:
        print (line)
    return

def applyChanges():
  for file in os.listdir(folderName + "/modified"):
    shutil.copyfile(folderName + "/modified/" + file, path + "/" + file)
  print ("Changes have been successfully Applied!")

def undo():
  for file in os.listdir(folderName + "/backup"):
    shutil.copyfile(folderName + "/backup/" + file, path + "/" + file)
  print ("Files have been restored!")

def replace(file, replaceString):
  print (file)
  w = open(folderName + "/modified/" + file, "w")
  with open(folderName + "/backup/"+file) as f:
    if file.endswith("aspx"):
      return replaceASPX(f,w)
    elif file.endswith("cs"):
      return replaceCS(f,w)

def replaceASPX(f,w):
  match = False
  for line in f:
    if '<body' in line:
      print (line)
      while 'RadWindow.js' not in line:
        if '/body' in line:
          break
        w.write(line)
        line=next(f)
        print (line)
      else:
        match = True
        w.write(line)
        line = getIndent(line) + "<script type=\"text/javascript\" src=\"../Jscript/RadControls.js\"></script>" + "\r\n"
        w.write(line)
        print(line)
        line = next(f)
        print(line)
    if '<form' in line:
      print (line)
      while 'asp:Panel' not in line:
        if '/form' in line:
          match = False
          break
        w.write(line)
        line=next(f)
        print (line)
      else:
        match = True
        w.write(line)
        indent = getIndent(line)
        addLine(w, indent, "<asp:LinkButton ID=\"btnapply\" runat=\"server\" CausesValidation=\"False\" CssClass=\"postback\" href=\"\" OnClientClick=\"btnApply(false); return false;\">\r\n")
        addLine(w, indent, "  <asp:PlaceHolder ID=\"PlaceHolder3\" runat=\"server\">\r\n")
        addLine(w, indent, "    <p align=\"center\" >\r\n")
        addLine(w, indent, "      <asp:Image ID=\"Image3\" runat=\"server\" ImageUrl=\"../images/workorder/update_procedure_32.png\" Height=\"34px\" Width=\"34px\" /><br />\r\n")
        addLine(w, indent, "         <label>Apply</label>\r\n")
        addLine(w, indent, "    </p>\r\n")
        addLine(w, indent, "  </asp:PlaceHolder>\r\n")
        addLine(w, indent, "</asp:LinkButton>\r\n")
        line = next(f)
        print(line)
    w.write(line)
  w.close()
  return match

def replaceCS(f,w):
  match = False
  for line in f:
    if 'RadGrid()' in line:
      print(line)
      grid = line.rsplit('=',1)[0].strip()
      print (grid)
      while 'OnRowSelected' not in line:
        if 'SetGridColumns' in line:
          break
        w.write(line)
        line=next(f)
        print(line)
      else:
        match = True
        indent = getIndent(line)
        line = line.replace("OnRowSelected","OnRowClick")
        w.write(line)
        print(line)
        addLine(w,indent,"GridSelectColumn.addCheckBoxColumn(mode, " + grid  + ", btnapply);\r\n");
        line=next(f)
        print(line)
    w.write(line)
  w.close()
  return match

def addLine(w,indent,newline):
  line = indent + newline
  w.write(line)
  print(line)
  return

def find_between( s, first, last ):
  start = s.index( first ) + len( first )
  for endword in last:
    try:
      end = s.index( endword, start )
      imageFound = s[start:end]
    except ValueError:
      imageFound = ""
    if imageFound != "":
      return imageFound + endword

def getIndent(line, indent = ""):
  for char in line:
    if char == " ":
      indent += char
    else: return indent

def mkdir():
    now = datetime.datetime.now()
    global folderName
    (filepath,folder) = os.path.split(path)
    folderName = "result/" + folder + now.strftime("-%H%M")
    print (folderName)
    try:
        os.makedirs(folderName)
        os.makedirs(folderName + "/modified")
        os.makedirs(folderName + "/backup")
    except OSError as exc: # Python >2.5
        if exc.errno == errno.EEXIST and os.path.isdir(folderName):
          pass
        else: raise

def copyImages():
  for imageFile in tempImages:
    if imageFile not in os.listdir(IMAGEPath):
      shutil.copyfile(imgPath + "/" + imageFile, IMAGEPath + "/" + imageFile)


def generateList():
  if len(replacedFiles) == 0:
    print("no files replaced")
    return
  listFile = open(folderName + "/replacedFiles", "w")
  for replacedFile in replacedFiles:
    print>>listFile, replacedFile
  listFile.close();
  print ("\n Filepath: " + path )
  print ("\n" + str(len(replacedFiles)) + " files replaced.")
  print ('\n '.join(replacedFiles))
  print ("******************************************************************************************************************")
