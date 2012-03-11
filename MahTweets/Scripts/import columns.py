import clr
clr.AddReference('MahTweets.Core')
clr.AddReference('MahTweets.UI')
clr.AddReference('System.Windows.Forms')
clr.AddReference('System.Drawing')
clr.AddReference("Newtonsoft.Json")
clr.AddReferenceByPartialName('PresentationCore')
clr.AddReferenceByPartialName('PresentationFramework')

from System.Windows.Forms import DialogResult, OpenFileDialog
from System.Windows import MessageBox, MessageBoxResult, MessageBoxButton
from MahTweets.Core.Filters import *
from Newtonsoft.Json import *
from System.IO import *

def import_stream_into_mahtweets(streams):
	for s in streams:
		if s["SelectAll"].ToString() == "false":
			selectall = False
		else:
			selectall = True
		streamname = s["GroupName"].ToString().Trim('\"')
		if not ScriptingHelper.DoesStreamExist(streamname):	#only import if the streamname does not exist already
			r = import_this_stream_dialog(streamname)
			importtype = "import"
		else:
			r = merge_this_stream_dialog(streamname)
			importtype = "merge"
		if r == MessageBoxResult.Yes:
			if importtype == "import":
				newstream = ScriptingHelper.CreateStream(streamname)
				newstream.SelectAll = selectall
			if importtype == "merge":
				newstream = ScriptingHelper.FindStream(streamname)
			for f in s["Filters"]:
				contactname = ""
				try:
					contactname = f["ContactName"].ToString().Trim('\"')
				except Exception:
					pass
				if contactname == "":
					accountname = f["MicroblogAccountName"].ToString().Trim('\"')
					if accountname != "":
						microblog = f["MicroblogName"].ToString().Trim('\"')
						updatetype = f["UpdateTypeName"].ToString().Trim('\"')
						colour = f["Color"].ToString().Trim('\"')
						behaviour = f["IsIncluded"].ToString().Trim('\"')
						ScriptingHelper.CreateUpdateFilterInStream(accountname, updatetype, microblog, colour, behaviour, newstream)		
				else:
					microblog = f["AccountName"].ToString().Trim('\"')
					colour = f["Color"].ToString().Trim('\"')
					behaviour = f["IsIncluded"].ToString().Trim('\"')
					ScriptingHelper.CreateContactFilterInStream(contactname, microblog, colour, behaviour, newstream)
			for sf in s["ScriptFiltersActivated"]:
				scriptfilter = sf["ScriptKey"].ToString().Trim('\"')
				scriptfiltercolour = sf["ScriptFilterColor"].ToString().Trim('\"')
				ScriptingHelper.CreateUpdateFilterInStream(scriptfilter, scriptfiltercolour, newstream)
			if importtype == "import":
				ScriptingHelper.ShowStream(newstream)

				
def merge_this_stream_dialog(colname):
	messageBoxText = "Import and merge into existing column: " + colname + "?"
	caption = "MahTweets Column Import and Merge"
	button = MessageBoxButton.YesNo
	return	MessageBox.Show(messageBoxText, caption, button)

def import_this_stream_dialog(colname):
	messageBoxText = "Import and create new column: " + colname + "?"
	caption = "MahTweets Column Import"
	button = MessageBoxButton.YesNo
	return	MessageBox.Show(messageBoxText, caption, button)	
		
def do_reinflate(filename):
	json = JsonSerializer()
	sr = StreamReader(filename)
	reader = JsonTextReader(sr)
	filters = json.Deserialize(reader)
	reader.Close()
	sr.Close()
	return filters

def do_open_dialog():
	dlg = OpenFileDialog()
	dlg.Title = "Import Column Settings"
	dlg.FileName = "Column Import"
	dlg.Filter = 'MahTweets Settings (*.mahtweets) | *.mahtweets'
	if dlg.ShowDialog() == DialogResult.OK:
		return dlg.FileName
	else:
		return None

def mainwindow_context_menu_click (column) :
	"""Import or Merge saved Columns"""
	filename = do_open_dialog()
	if filename != None:
		streams = do_reinflate(filename)
		import_stream_into_mahtweets(streams)
	else:
		pass