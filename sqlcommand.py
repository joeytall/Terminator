import pyodbc
# import iopro.pyodbc as pyodbc


dsn = 'sqlserverdatasource'
user = '<username>'
password = '<password>'
database = '<dbname>'

con_string = 'DSN=%s;UID=%s;PWD=%s;DATABASE=%s;' % (dsn, user, password, database)
cnxn = pyodbc.connect(con_string)

# import sqlite3
# conn = sqlite3.connect('AZDEV1')
#
# print(conn)
# c = conn.cursor()
# print(c)

# for row in c.execute("select * from sysobjects WHERE xtype='U'"):
#   print(row)
#
# conn.commit()
# conn.close()
