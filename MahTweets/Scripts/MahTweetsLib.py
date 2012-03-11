def Walk(tree):
	yield tree
	if hasattr(tree, 'Children'):
		for child in tree.Children:
			for x in Walk(child):
				yield x
	elif hasattr(tree, 'Child'):
		for x in Walk(tree.Child):
			yield x
	elif hasattr(tree, 'Content'):
		for x in Walk(tree.Content):
			yield x