import clr
clr.AddReference('Newtonsoft.Json')
from System import Uri
from System.IO import *
from System.Net import *
from Newtonsoft.Json import *

# http://msdn.microsoft.com/en-us/library/dd576287.aspx

def response_to_translated_text(resp):
	tt = JsonConvert.DeserializeObject(resp)
	rt = ""
	try:
		rt = str(tt["responseData"]["translatedText"])
	except:
		pass
	if rt != None :
		rt = rt.strip('"')
	else:
		rt = ""
	return rt

def detected_source_language(resp):
	tt = JsonConvert.DeserializeObject(resp)
	rt = ""
	try:
		rt = str(tt["responseData"]["language"])
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

def send_http_post_with_header(uri) :
	wr = WebRequest.Create(uri)
	

def format_microsofttranslator_translate_query_to_english(sourcetext):
	clientappid = '/l/UurMzZ35hMwFtvBmjgtLHcZZgaUsC3iQbWRzCCjo='
	clientsecret = '/l/UurMzZ35hMwFtvBmjgtLHcZZgaUsC3iQbWRzCCjo='
	qs = "http://api.microsofttranslator.com/V2/Http.svc/Detect?appId=" + clientappid
	qs = qs + "&text=" + sourcetext
	qs = qs + "&from=" + detected_source_language(send_http_get_response(format_google_detect_query(sourcetext)))
	qs = qs + "&to=" + "en"
	u = Uri(qs)
	return u

def format_microsofttranslator_detect_query(sourcetext):
	clientappid = '/l/UurMzZ35hMwFtvBmjgtLHcZZgaUsC3iQbWRzCCjo='
	clientsecret = '/l/UurMzZ35hMwFtvBmjgtLHcZZgaUsC3iQbWRzCCjo='
	qs = "http://api.microsofttranslator.com/V2/Http.svc/Detect?appId=" + clientappid
	qs = qs + "&text=" + sourcetext
	u = Uri(qs)
	return u

def format_get_microsofttranslator_access_token_query():
	# http://msdn.microsoft.com/en-us/library/hh454950.aspx
	clientappid = 'MahTweetsLH'
	clientsecret = 'qfxZC6dN+OzdUOiKEquQDXyuoSFYF+309AeGbMVS/+Q='
	qs = "https://datamarket.accesscontrol.windows.net/v2/OAuth2-13?grant_type=client_credentials&client_id=" + clientappid + "&client_secret=" + Uri.EscapeUriString(clientsecret) + "&scope=http://api.microsofttranslator.com"
	u = Uri(qs)
	return u

def context_menu_click(selectedtext):
	"""Translate selected tweet to English using Microsoft Translator"""
	return response_to_translated_text(send_http_get_response(format_google_translate_query(selectedtext)))

def text_select_context_menu_click (selectedtext) :
	"""Translate selected text to English using Microsoft Translator"""
	return response_to_translated_text(send_http_get_response(format_google_translate_query(selectedtext)))

def compose_text_context_menu_click (selectedtext) :
	"""Translate selected text to English using Microsoft Translator"""
	return response_to_translated_text(send_http_get_response(format_google_translate_query(selectedtext)))

print format_get_microsofttranslator_access_token_query()