import clr
clr.AddReference('Newtonsoft.Json')
from System import Uri
from System.Diagnostics import *
from System.IO import *
from System.Net import *
from Newtonsoft.Json import *


# documentation Geocoding API http://code.google.com/apis/maps/documentation/geocoding/


def detected_lat_long(resp):
	tt = JsonConvert.DeserializeObject(resp)
	rt = ""
	try:
		rt = str(tt["results"][0]["geometry"]["location"]["lat"]) + "," + str(tt["results"][0]["geometry"]["location"]["lng"])
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

def format_google_locate_query(sourcetext):
	qs = "http://maps.google.com/maps/api/geocode/json?"
	qs = qs + "address=" + sourcetext
	qs = qs + "&sensor=false"
	u = Uri(qs)
	return u

def text_select_context_menu_click (selectedtext) :
	"""Locate an address with Bing Maps"""
	latlng = detected_lat_long(send_http_get_response(format_google_locate_query(selectedtext)))
	if latlng != None :
		url = Uri("http://www.bing.com/maps/?q=" + latlng)
		Process.Start(str(url))

def compose_text_context_menu_click (selectedtext) :	
	"""Locate an address with Bing Maps and create link"""
	latlng = detected_lat_long(send_http_get_response(format_google_locate_query(selectedtext)))
	if latlng != None :
		url = Uri("http://www.bing.com/maps/?q=" + latlng)
		return(str(url))
	else :
		return None