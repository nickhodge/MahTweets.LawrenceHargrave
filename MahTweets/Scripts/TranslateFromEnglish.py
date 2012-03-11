import clr
clr.AddReferenceByPartialName("PresentationCore")
clr.AddReferenceByPartialName("PresentationFramework")
clr.AddReferenceByPartialName("WindowsBase")
clr.AddReference('Newtonsoft.Json')
from System import Uri
from System.IO import *
from System.Windows import *
from System.Windows.Media import *
from System.Windows.Media.Animation import *
from System.Windows.Controls import *
from System.Windows.Shapes import *
from System.Net import *
from Newtonsoft.Json import *

# language source/destination types http://msdn.microsoft.com/en-us/library/dd877907(v=MSDN.10).aspx
binglanguages = []
binglanguages.append({'name': 'Arabic', 'shortname' : 'ar'})
binglanguages.append({'name': 'Dutch', 'shortname' : 'nl'})
binglanguages.append({'name': 'French', 'shortname' : 'fr'})
binglanguages.append({'name': 'German', 'shortname' : 'de'})
binglanguages.append({'name': 'Italian', 'shortname' : 'it'})
binglanguages.append({'name': 'Japanese', 'shortname' : 'ja'})
binglanguages.append({'name': 'Korean', 'shortname' : 'ko'})
binglanguages.append({'name': 'Polish', 'shortname' : 'pl'})
binglanguages.append({'name': 'Portuguese', 'shortname' : 'pt'})
binglanguages.append({'name': 'Russian', 'shortname' : 'ru'})
binglanguages.append({'name': 'Spanish', 'shortname' : 'es'})
binglanguages.append({'name': 'Simplified Chinese', 'shortname' : 'zh-CHS'})
binglanguages.append({'name': 'Traditional Chinese', 'shortname' : 'zh-CHT'})

def response_to_translated_text(resp):
	tt = JsonConvert.DeserializeObject(resp)
	rt = ""
	try:
		rt = str(tt["SearchResponse"]["Translation"]["Results"][0]["TranslatedTerm"])
	except:
		pass
	if rt != None :
		rt = rt.strip('"')
	else:
		rt = ""
	return rt

def send_http_get_response(uri) :
	wr = WebRequest.Create(uri)
	tr = wr.GetResponse()
	ds = tr.GetResponseStream()
	rd = StreamReader(ds)
	rs = rd.ReadToEnd()
	return rs

def format_bing_translate_query(sourcetext, targetlang):
	bingappid = '3BA09947B8CC5875868BED9CBCDF041AC94FD86D'
	qs = "http://api.bing.net/json.aspx?"
	qs = qs + "AppID=" + bingappid
	qs = qs + "&Query=" + sourcetext
	qs = qs + "&Sources=Translation"
	qs = qs + "&Version=2.2"
	qs = qs + "&Translation.SourceLanguage=en"
	qs = qs + "&Translation.TargetLanguage=" + targetlang
	u = Uri(qs)
	return u

def btn_click(s, e):
	global w, lst, englishtext
	ql = lst.SelectedItem.Tag.ToString()
	translateresponse = response_to_translated_text(send_http_get_response(format_bing_translate_query(englishtext, ql)))
	w.Close()
	ScriptingHelper.ChangeSelectedText(translateresponse)
	
def translate_to_selectedlanguage (selectedtext) :
	"Translate to a different language"
	global w, lst, englishtext, binglanguages
	englishtext = selectedtext
	w = ScriptingHelper.LoadWindow(ScriptingHelper.ScriptsPath() + 'SimpleListChoice.xaml')
	btn = w.FindName('button1') # find by name in the window
	lst = w.FindName('listBox1')
	lbl = w.FindName('label1')
	w.Title = "Bing Translate from English"

	lbl.Content = "Choose Language:"
	btn.Content = "Translate"
	btn.Click += btn_click # this adds the event to return to once clicked

	for lg in binglanguages:
		lstItem = ListBoxItem()
		lstItem.Content = lg['name']
		lstItem.Tag = lg['shortname']
		lst.Items.Add(lstItem) #add two target languages

	w.Show()

def text_select_context_menu_click (selectedtext) :
	"Translate to a different language"
	translate_to_selectedlanguage(selectedtext)

def compose_text_context_menu_click (selectedtext) :
	"Translate to a different language"
	return translate_to_selectedlanguage(selectedtext)