import os, shutil, errno, datetime, random

path, folderName, replaceString = "", "", ""

def Main(pathFromUser, replaceStringFromUser):
  global path, replaceString
  path = pathFromUser
  replaceString = replaceStringFromUser
  count = 0
  problemfiles = []
  mkdir()

  for file in os.listdir(path):
    if file.endswith(".aspx.cs"):
      shutil.copyfile(path + "/" + file, folderName + "/backup/" + file)
      if replace(file, replaceString) == False:
        problemfiles.append(file)
      count+=1
    #if count == 1: break
  print (folderName)
  print (str(count) + " files scanned!")
  print (str(len(problemfiles)) + " problem files found.")
  if len(problemfiles) != 0:
    print ("Problem Files: " + ', '.join(problemfiles))
  return

def check():
  file = random.choice(os.listdir(folderName + "/modified"))
  print (file)
  with open(folderName + "/modified/" + file) as f:
    for line in f:
      if 'Page_Init' in line:
        while replaceString not in line:
          print(line)
          line = next(f)
        for n in range(3):
          print(line)
          line = next(f)
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
  match = False
  print (file)
  w = open(folderName + "/modified/" + file, "w")
  with open(folderName + "/backup/"+file) as f:
    for line in f:
      if 'framepanelcontrol' in line:
        return False
      if 'Page_Init' in line:
        w.write(line)
        match = True
        line = next(f)
        w.write(line)
        line = next(f)
        if 'RetrieveMessage' in line:
          w.write(line)
        while 'if' not in line:
          line = next(f)
        while '}' not in line:
          print(line)
          line = next(f)
        print(line)
        if '}' in line:
          line = line.replace('}', replaceString)
      w.write(line)
  if match == False:
    w.close()
    return False

def mkdir():
    now = datetime.datetime.now()
    global folderName
    folderName = "result/" + now.strftime("%Y_%m_%d-%H%M")
    print (folderName)
    try:
        os.makedirs(folderName)
        os.makedirs(folderName + "/modified")
        os.makedirs(folderName + "/backup")
    except OSError as exc: # Python >2.5
        if exc.errno == errno.EEXIST and os.path.isdir(folderName):
          pass
        else: raise
