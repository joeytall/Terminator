import os, shutil, errno, datetime

def Main(path, count=0, problemfiles = []):
  mkdir_p(path + "/backup")
  for file in os.listdir(path):
    if file.endswith(".aspx.cs"):
      shutil.copyfile(path + "/" + file, path + "/backup" + file)
      if deleteSelected(file) == False:
        problemfiles.append(file)
      count+=1
    #if count == 1: break
  print (str(count) + " files scanned!")
  print (str(len(problemfiles)) + " problem files found.")
  if len(problemfiles) != 0:
    print ("Problem Files: " + ', '.join(problemfiles))
  return

def printSelected(file, match = False):
  print (file)
  with open("Codes/"+file) as f:
    for line in f:
      if 'Page_Init' in line:
        match = True
        next(f)
        line = next(f)
        while 'if' not in line:
          line = next(f)
        while '}' not in line:
          print(line)
          line = next(f)
        print(line)
        return
  if match == False:
    return False

def deleteSelected(file, match = False):
  print (file)
  w = open("new/" + file, "w")
  with open("Codes/"+file) as f:
    for line in f:
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
          line = line.replace('}',"UserRights.CheckAccess('');")
      w.write(line)
  if match == False:
    w.close()
    return False

def mkdir_p():
    now = datetime.datetime.now()
    folderName = now.strftime("%Y_%m_%d-%H%M")
    try:
        os.makedirs(folderName)
        os.makedirs(folderName + "/new")
        os.makedirs(folderName + "/backup")
    except OSError as exc: # Python >2.5
        if exc.errno == errno.EEXIST and os.path.isdir(path):
            pass
        else: raise
        return folderName
