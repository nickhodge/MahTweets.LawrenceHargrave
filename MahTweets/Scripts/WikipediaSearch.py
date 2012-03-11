import clr
from System.Diagnostics import *
from System import Uri

def text_select_context_menu_click (selectedtext) :
	"""Search selected text within Wikipedia"""
	url = Uri("http://en.wikipedia.org/w/index.php?go=Go&search=" + selectedtext)
	Process.Start(str(url))

def compose_text_context_menu_click (selectedtext) :
	"""Search selected text within Wikipedia"""
	url = Uri("http://en.wikipedia.org/w/index.php?go=Go&search=" + selectedtext)
	Process.Start(str(url))