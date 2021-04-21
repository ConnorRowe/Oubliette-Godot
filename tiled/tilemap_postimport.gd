extends Reference

var children = []

var Door = load("res://scenes/Door.tscn")
var Torch = load("res://scenes/Torch.tscn")

func post_import(scene):
	print("Starting tilemap post_import script.")
	
	var new_scene = YSort.new()
	new_scene.name = scene.name
	
	for layer in scene.get_children():
		print("for " + layer.name + " in scene.get_children()")

		var new_layer = layer.duplicate()
		
		print("new_layer.get_children():")
		print(new_layer.get_children())
		#
		new_scene.add_child(new_layer)
		new_layer.set_owner(new_scene)
		
	print("getallnodes(new_scene):")
	getallnodes(new_scene)

	var Objects = new_scene.find_node("Objects", true, false)

	var navigation = Navigation2D.new()
	new_scene.add_child(navigation)
	navigation.set_owner(new_scene)

	print("Node specific logic loop:")
	for node in children:
		print("- " + node.name)
		node.set_owner(new_scene)
		if node is CollisionShape2D:
			node.z_index = 1
		if node is StaticBody2D:
			node.collision_layer = 0b1101
		if node.has_meta("tilemap_type"):
			if node.get_meta("tilemap_type") == "ground":
				node.cell_y_sort = false
				
			if node.get_meta("tilemap_type") == "walls":
				node.cell_tile_origin = TileMap.TILE_ORIGIN_TOP_LEFT
				node.collision_layer = 0b1101
		if node.name == "Objects" || node.name == "NavSubtraction":
			node.position.y -= 16
		if node.name == "door":
			var newDoor = Door.instance()
			newDoor.position = node.position
			newDoor.position.y -= 16
			Objects.get_parent().add_child(newDoor)
			newDoor.set_owner(new_scene)
			node.get_parent().remove_child(node)
		if node.name == "navigation" || node.name == "navigation2":
			var poly: CollisionPolygon2D = node.get_child(0)
			var navPoly = NavigationPolygon.new()
			var navPolyInst = NavigationPolygonInstance.new()

			var vertices: PoolVector2Array = poly.polygon

			navPoly.add_outline(vertices)
			navPoly.make_polygons_from_outlines()
			navPolyInst.navpoly = navPoly
			navPolyInst.transform = Transform2D(0, node.position - Vector2(0, 16))
			
			navigation.add_child(navPolyInst, true)

			navPolyInst.set_owner(new_scene)

			node.remove_child(poly)
			node.get_parent().remove_child(node)
		if(node.has_meta("type") && node.get_meta("type") == "torch"):
			var newTorch = Torch.instance()
			newTorch.position = node.position
			newTorch.position.y -= 16
			newTorch.name = node.name
			Objects.get_parent().add_child(newTorch)
			newTorch.set_owner(new_scene)
			node.get_parent().remove_child(node)
			

	new_scene.move_child(new_scene.find_node("Objects"), new_scene.get_child_count() - 1)


	print("Starting navigation subtraction")
	
	var nav_poly_inst: NavigationPolygonInstance = navigation.get_child(0)
	nav_poly_inst.enabled = false
	nav_poly_inst.enabled = true
	var nav_poly = nav_poly_inst.get_navigation_polygon()


	var NavSubraction = new_scene.find_node("NavSubtraction", true, false)
	children.clear()
	getallnodes(NavSubraction)

	for node in children:
		if node is CollisionPolygon2D:
			print("Subtracting " + node.name)
			var new_poly = PoolVector2Array()
			var poly: PoolVector2Array = node.polygon
			var poly_transform: Transform2D = Transform2D(0.0, node.get_parent().position - nav_poly_inst.position - Vector2(0, 16))

			for vtx in poly:
				new_poly.append(poly_transform.xform(vtx))
			
			nav_poly.add_outline(new_poly)
			
	nav_poly.make_polygons_from_outlines()
	nav_poly_inst.set_navigation_polygon(nav_poly)
	nav_poly_inst.z_index = 2;

	print("Removing leftover nav subtraction nodes")
	new_scene.remove_child(NavSubraction)
	NavSubraction.queue_free()

	print("Tilemap post_import finished")
		
	return new_scene

func getallnodes(node):
	for N in node.get_children():
		if N.get_child_count() > 0:
			print("["+N.get_name()+"]")
			
			if !children.has(N):
				children.push_back(N)
			
			getallnodes(N)
		else:
			if !children.has(N):
				children.push_back(N)
				
			print("- "+N.get_name())
