def generateSql(files, folderName):
  w = open(folderName + "/script.txt", "w")
  z = open(folderName + "/fileList.txt", "w")
  line = '\r\n '.join(files)
  z.write(line)
  z.close()
  for file in files:
    if file.endswith("aspx"):
      line = " /**************************" + file + "*********************/\r\n"
      w.write(line)

      line = "insert into screenfieldattr (filename, controlid, controltype, showtooltip, systemmandatory, systemLookup, isReadOnly, Mandatory, screenmastercounter)\r\n"
      line += "select distinct 'codes/" + file + "', 'apply','linkbutton','1','0','0','0','0',screenmastercounter from screenfieldattr where filename = 'codes/" + file + "'\r\n"
      w.write(line)

      line = "insert into screenfieldattr (filename, controlid, controltype, showtooltip, systemmandatory, systemLookup, isReadOnly, Mandatory, screenmastercounter)\r\n"
      line += "select distinct 'codes/" + file + "', 'cancel','linkbutton','1','0','0','0','0',screenmastercounter from screenfieldattr where filename = 'codes/" + file + "'\r\n\r\n"
      w.write(line)

      line = "insert into screendetail (mastercount, subtype, text, showorder, colspan,rowspan, columnposition, rowposition, fontsize, dirtylog, display, LeftPos, TopPos, SWidth, SHeight)\r\n"
      line += "select distinct Counter,'linkbutton', 'Apply', 1,1,1,1,1,8,0,1,D.MaxWidth, D.MaxHeight,50,50 From screenfieldattr M inner join \r\n"
      line += "(Select Max(LeftPos+SWidth)/2-60 as MaxWidth,Max(TopPos+SHeight)+30 as MaxHeight,M1.ScreenMasterCounter From ScreenDetail D1 Inner Join ScreenFieldAttr M1 ON M1.Counter=D1.MasterCount Group By M1.ScreenMasterCounter) D \r\n"
      line += "ON M.ScreenMasterCounter=D.ScreenMasterCounter where filename = 'codes/" + file + "' and controlid = 'apply'\r\n"

      w.write(line)

      line = "insert into screendetail (mastercount, subtype, text, showorder, colspan,rowspan, columnposition, rowposition, fontsize, dirtylog, display, LeftPos, TopPos, SWidth, SHeight)\r\n"
      line += "select distinct Counter,'linkbutton', 'Cancel', 1,1,1,1,1,8,0,1,D.MaxWidth, D.MaxHeight,50,50 From screenfieldattr M inner join \r\n"
      line += "(Select Max(LeftPos+SWidth)/2+10 as MaxWidth,Max(TopPos+SHeight)-50 as MaxHeight,M1.ScreenMasterCounter From ScreenDetail D1 Inner Join ScreenFieldAttr M1 ON M1.Counter=D1.MasterCount Group By M1.ScreenMasterCounter) D \r\n"
      line += "ON M.ScreenMasterCounter=D.ScreenMasterCounter where filename = 'codes/" + file + "' and controlid = 'cancel'\r\n\r\n\r\n"

      w.write(line)
  w.close()
  return
