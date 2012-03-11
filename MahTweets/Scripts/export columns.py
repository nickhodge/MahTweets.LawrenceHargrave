import clr
clr.AddReference('MahTweets.Core')
clr.AddReference('MahTweets.UI')
clr.AddReference('System.Windows.Forms')
clr.AddReference('System.Drawing')
clr.AddReference("Newtonsoft.Json")

from System.Windows.Forms import DialogResult, SaveFileDialog
from MahTweets.Core.Filters import *
from Newtonsoft.Json import *
from System.IO import *
	
def do_persist(filters, filename):
	json = JsonSerializer()
	sw = StreamWriter(filename)
	writer = JsonTextWriter(sw)
	json.Serialize(writer, filters)
	writer.Close()
	sw.Close()

def do_save_dialogs():
	dlg = SaveFileDialog()
	dlg.Title = "Save Column Settings"
	dlg.FileName = "Column Export"
	dlg.Filter = 'MahTweets Settings (*.mahtweets) | *.mahtweets'
	dlg.DefaultExt = ".mahtweets"
	if dlg.ShowDialog() == DialogResult.OK:
		return dlg.FileName
	else:
		return None

def mainwindow_context_menu_click (window) :
	"""Export of all Column Definitions"""
	filename = do_save_dialogs()
	if filename != None:
		do_persist(window.FilterGroups, filename)
	else:
		pass