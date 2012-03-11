import clr
clr.AddReference('MahTweets.Core') # the enumeration for FilterBehaviour is in this DLL
from MahTweets.Core.Filters import FilterBehaviour

def script_filter (update, context):
	"""Show updates that have geocoding"""
	try:
		x = update.Location.Latitude
		return FilterBehaviour.Include
	except:
		return FilterBehaviour.Exclude