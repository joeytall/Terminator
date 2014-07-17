import os, shutil, errno, datetime, random

path, folderName, replaceString = "", "", ""

def Main(pathFromUser, replaceStringFromUser):
  global path, replaceString, problemfiles
  path = pathFromUser
  replaceString = replaceStringFromUser
  problemfiles = []
  count = 0
  mkdir()

  for file in os.listdir(path):
    if file.endswith("frame.aspx"):
      problemfiles.append(file)
      count+=1
    elif file.endswith(".aspx"):
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
  while file in problemfiles:
    file = random.choice(os.listdir(folderName + "/modified"))
  print (file + "\n")
  with open(folderName + "/modified/" + file) as f:
    for line in f:
      if 'Azzier.css' in line:
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
  n = 0
  match = False
  print (file)
  w = open(folderName + "/modified/" + file, "w")
  with open(folderName + "/backup/"+file) as f:
    for line in f:
      #if 'Frame' in line:
      #   return False
      if '<head' in line:
        print (line)
        while 'zzier.css' not in line:
          if '/head' in line or n == 10:
            match == False
            break
          w.write(line)
          line = next(f)
          print (line)
          n += 1
        else:
          match = True
          w.write(line)
          line = getIndent(line) + replaceString + "\r\n"
          w.write(line)
          line = next(f)
          print (line)
      w.write(line)
  if match == False:
    w.close()
    return False

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
