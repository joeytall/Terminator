import os, datetime

path = "my/little/pony.asdf"
(filepath, filename) = os.path.split(path)

print (filepath)
print (filename)
millis = datetime.datetime.now().microsecond
print millis
