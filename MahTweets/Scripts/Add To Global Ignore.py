import clr
clr.AddReference('MahTweets.Core')

from MahTweets.Core import *

def text_select_context_menu_click (selectedtext) :
	"""Add selected text to global ignore list"""
	settings = Composition.CompositionManager.Get[Interfaces.Settings.IApplicationSettingsProvider]()
	settings.GlobalExclude.Add(selectedtext)