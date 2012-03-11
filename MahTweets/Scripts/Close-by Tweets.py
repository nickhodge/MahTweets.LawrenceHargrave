import clr
clr.AddReference('MahTweets.Core') # the enumeration for FilterBehaviour is in this DLL
from MahTweets.Core.Filters import FilterBehaviour

def script_filter (update, context):
	"""Show tweets within 25kms"""
	radiusdistance = 25.0
	try:
		if update.InterDistanceInKms < radiusdistance and update.InterDistanceInKms != 0.0 :
			return FilterBehaviour.Include
		else:
			return FilterBehaviour.Exclude
	except Exception:
		return FilterBehaviour.Exclude
	return FilterBehaviour.Exclude