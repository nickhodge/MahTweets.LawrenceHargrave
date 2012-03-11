import clr
from System.Diagnostics import *
from System import Uri

def text_select_context_menu_click (selectedtext) :
    """Define a word in the dictionary"""
    url = Uri("http://dictionary.reference.com/browse/" + selectedtext)
    Process.Start(url.ToString())
    
def compose_text_context_menu_click (selectedtext) :
    """Define a word in the dictionary"""
    url = Uri("http://dictionary.reference.com/browse/" + selectedtext)
    Process.Start(url.ToString())