import os, shutil, errno, datetime, random, pdb

path, folderName, replaceString = "", "", ""
images = []
tempImages = []
imgPath = "/Volumes/Webwork/Production/Azzierdev/Images2"
IMAGEPath = "/Volumes/Webwork/Production/Azzierdev/IMAGES"

def Main(pathFromUser, replaceStringFromUser):
  global path, replaceString, problemfiles, filesWithImages, tempImages
  path = pathFromUser
  replaceString = replaceStringFromUser
  problemfiles = []
  filesWithImages = []
  tempImages = []
  count = 0
  mkdir()

  for file in os.listdir(path):
    if file.endswith((".aspx",".ascx",".aspx.cs")):
      shutil.copyfile(path + "/" + file, folderName + "/backup/" + file)
      if replace(file, replaceString) == False:
        problemfiles.append(file)
      else:
        filesWithImages.append(file)
      count+=1
    #if count == 1: break
  print ("\n")
  print (folderName)
  print (str(count) + " files scanned!")
  # print (str(len(problemfiles)) + " problem files found.")
  print (str(len(filesWithImages)) + " files with images found.")
  print (str(len(tempImages)) + " images found.")
  # if len(problemfiles) != 0:
  #   print ("\nProblem Files: " + ', '.join(problemfiles))
  if len(filesWithImages) != 0:
    print ("\nFiles With Images: " + ', '.join(filesWithImages))
  if len(tempImages) != 0:
    print ("\n" + ', '.join(tempImages))
  print ("******************************************************************************************************************")
  return

def check():
  if len(filesWithImages) == 0:
    return
  file = random.choice(os.listdir(folderName + "/modified"))
  # while file in problemfiles:
  #   file = random.choice(os.listdir(folderName + "/modified"))
  while file not in filesWithImages:
    file = random.choice(os.listdir(folderName + "/modified"))
  print (file + "\n")
  with open(folderName + "/modified/" + file) as f:
    for line in f:
      if '/IMAGES/' in line:
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
  global images, tempImages
  n = 0
  match = False
  print (file)
  w = open(folderName + "/modified/" + file, "w")
  with open(folderName + "/backup/"+file) as f:
    for line in f:
      #if 'Frame' in line:
      #   return False
      if '/images2/' in line:
        print (line)
        image = find_between(line, "/images2/", ["png","jpg","gif"])
        line = line.replace("/images2/", "/IMAGES/")
        if image not in tempImages:
          tempImages.append(image)
        if image not in images:
          images.append(image)
        match = True
      w.write(line)
    w.close()
    return match

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
  if len(images) == 0:
    print("no image found")
    return
  listFile = open("result/imageList", "w")
  for image in images:
    print>>listFile, image
  listFile.close();
  print ("\n" + str(len(tempImages)) + " images found.")
  print ('\n '.join(images))

  print ("\nImages not in IMAGES folder:")
  for imageFile in tempImages:
    if imageFile not in os.listdir(IMAGEPath):
      print(imageFile)
  print ("******************************************************************************************************************")
