import clr
import re
clr.AddReference('MahTweets.Core')
clr.AddReference('MahTweets.UI')
clr.AddReference('MahTweets.Library')

def global_script_filter (update) : # note with update, this is a IStatusUpdate.cs; so you get access to all the elements from here; only update is passed in as a parameter
	"No iPhones, please as a global script filter"
	if update.Text.ToLower().Contains("iphone") : #if any text in the update contains MahTweets, include
		return False #returning False says "this is not to be included
	if update.Text.ToLower().Contains("aeoth") :
		update.Text = re.sub('aeoth','awesome',update.Text) #update all occurances of aeoth with awesome
		return False # dont filter this out, but the text should be changed
	return True