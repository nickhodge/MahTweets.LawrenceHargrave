import clr
from System.Diagnostics import *
from System import Uri

def text_select_context_menu_click (selectedtext) :
    """Search selected text with Google"""
    url = Uri("http://www.google.com/search?q=" + selectedtext)
    Process.Start(str(url))
    
def compose_text_context_menu_click (selectedtext) :
    """Search selected text with Google"""
    url = Uri("http://www.google.com/search?q=" + selectedtext)
    Process.Start(str(url))