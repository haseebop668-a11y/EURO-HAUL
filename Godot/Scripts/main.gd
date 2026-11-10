extends Node3D

const SKY_COLOR := Color("#07111f")
const GOLD := Color("#d5ad62")
const BLUE := Color("#16334e")
const ROAD_LENGTH := 900.0

var truck: EuroHaulTruck
var trailer: EuroHaulTrailer
var career: EuroHaulCareer
var company: EuroHaulCompanyManager
var hud: Label
var status_label: Label
var speed_label: Label
var touch_state := {"throttle": false, "brake": false, "left": false, "right": false}
var traffic_vehicles: Array[EuroHaulTrafficVehicle] = []
var _last_position := Vector3.ZERO
var _last_distance_save := 0.0

func _ready() -> void:
	_setup_environment()
	_build_world()
	_build_truck()
	_build_traffic()
	_build_career()
	_build_hud()
	print("EURO HAUL Godot vertical-slice booted")

func _process(delta: float) -> void:
	if truck == null:
		return
	var throttle := 1.0 if touch_state.throttle else 0.0
	var brake := 1.0 if touch_state.brake else 0.0
	var steer := float(touch_state.right) - float(touch_state.left)
	if not OS.has_feature("mobile"):
		throttle = Input.get_action_strength("accelerate") - Input.get_action_strength("reverse")
		brake = Input.get_action_strength("brake")
		steer = Input.get_action_strength("steer_right") - Input.get_action_strength("steer_left")
	truck.set_controls(throttle, steer, brake)
	var current_position := truck.global_position
	var distance := current_position.distance_to(_last_position)
	_last_position = current_position
	var expected_step := (absf(truck.speed_kmh) / 3.6) * delta * 3.0 + 1.0
	if distance > 0.0 and distance <= maxf(10.0, expected_step) and career != null:
		career.record_distance(distance / 1000.0)
		_last_distance_save += distance
	if _last_distance_save >= 100.0:
		if career != null:
			career.save()
		_last_distance_save = 0.0
	_update_hud()
	_update_traffic_warnings()

func _unhandled_key_input(event: InputEvent) -> void:
	if event is InputEventKey and event.pressed and not event.echo and truck != null:
		match event.keycode:
			KEY_E:
				truck.set_clutch(1.0)
				truck.shift_up()
				truck.set_clutch(0.0)
			KEY_Q:
				truck.set_clutch(1.0)
				truck.shift_down()
				truck.set_clutch(0.0)
			KEY_N:
				truck.set_clutch(1.0)
				truck.select_neutral()
				truck.set_clutch(0.0)
			KEY_R:
				truck.set_clutch(1.0)
				truck.select_reverse()
				truck.set_clutch(0.0)
			KEY_H:
				truck.sound_horn()
				status_label.text = "HORN"
			KEY_Z:
				truck.set_indicator(-1 if truck.indicators != -1 else 0)
			KEY_X:
				truck.set_indicator(1 if truck.indicators != 1 else 0)
			KEY_ESCAPE:
				get_tree().paused = not get_tree().paused

func _setup_environment() -> void:
	var environment := Environment.new()
	environment.background_mode = Environment.BG_COLOR
	environment.background_color = SKY_COLOR
	environment.ambient_light_source = Environment.AMBIENT_SOURCE_COLOR
	environment.ambient_light_color = Color("#9db4ca")
	environment.ambient_light_energy = 0.7
	environment.tonemap_mode = Environment.TONE_MAPPER_FILMIC
	var world := get_node_or_null("WorldEnvironment") as WorldEnvironment
	if world:
		world.environment = environment

func _build_world() -> void:
	_add_box("Ground", Vector3(0, -0.65, -ROAD_LENGTH * 0.5), Vector3(100, 1, ROAD_LENGTH), Color("#172b2d"), true)
	_add_box("European motorway", Vector3(0, -0.05, -ROAD_LENGTH * 0.5), Vector3(12, 0.2, ROAD_LENGTH), Color("#303a43"), true)
	for x in [-3.0, 3.0]:
		_add_box("Road edge", Vector3(x, 0.07, -ROAD_LENGTH * 0.5), Vector3(0.12, 0.025, ROAD_LENGTH), GOLD, false)
	for z in range(0, -int(ROAD_LENGTH), -12):
		_add_box("Lane dash", Vector3(0, 0.075, float(z)), Vector3(0.14, 0.025, 5.5), Color("#e7e5d7"), false)
	for side in [-1.0, 1.0]:
		for i in range(45):
			var z := -float(i) * 19.0 - 12.0
			_add_box("Roadside barrier", Vector3(side * 7.0, 0.45, z), Vector3(0.18, 0.8, 9.0), Color("#8b969e"), true)
			if i % 3 == 0:
				_add_box("Roadside marker", Vector3(side * 8.0, 0.9, z), Vector3(0.16, 1.8, 0.16), Color("#d8dce0"), false)
	for i in range(18):
		var z := -float(i) * 45.0 - 30.0
		_add_box("Distant warehouse", Vector3(-18.0, 3.0, z), Vector3(10, 6, 18), Color("#34495b"), true)
		_add_box("Warehouse roof", Vector3(-18.0, 6.2, z), Vector3(10.5, 0.4, 18.5), BLUE, false)

func _build_truck() -> void:
	truck = EuroHaulTruck.new()
	truck.name = "PlayerTruck"
	truck.position = Vector3(0.0, 0.15, -8.0)
	var collision := CollisionShape3D.new()
	var shape := BoxShape3D.new()
	shape.size = Vector3(2.45, 2.7, 6.2)
	collision.shape = shape
	collision.position = Vector3(0, 1.35, 0)
	truck.add_child(collision)
	var chassis := _make_box_mesh(Vector3(2.5, 1.05, 6.0), Color("#d5ad62"))
	chassis.position = Vector3(0, 1.35, 0)
	truck.add_child(chassis)
	var cab := _make_box_mesh(Vector3(2.35, 2.3, 2.2), Color("#173d5c"))
	cab.position = Vector3(0, 2.8, -1.45)
	truck.add_child(cab)
	var windshield := _make_box_mesh(Vector3(2.0, 0.85, 0.08), Color("#7fa6ba"))
	windshield.position = Vector3(0, 3.2, -2.58)
	truck.add_child(windshield)
	for x in [-1.3, 1.3]:
		for z in [-2.0, 2.0]:
			var wheel := _make_cylinder_mesh(0.62, 0.42, Color("#101419"))
			wheel.position = Vector3(x, 0.65, z)
			wheel.rotation.z = PI / 2.0
			truck.add_child(wheel)
	var nameplate := Label3D.new()
	nameplate.text = "EURO HAUL"
	nameplate.position = Vector3(0, 4.2, -1.6)
	nameplate.font_size = 48
	nameplate.modulate = GOLD
	truck.add_child(nameplate)
	add_child(truck)
	trailer = EuroHaulTrailer.new()
	trailer.name = "ArticulatedTrailer"
	trailer.truck = truck
	trailer.position = Vector3(0.0, 0.0, 3.3)
	truck.add_child(trailer)
	var camera_rig := SpringArm3D.new()
	camera_rig.spring_length = 9.0
	camera_rig.position = Vector3(0, 4.3, 1.0)
	camera_rig.rotation.x = deg_to_rad(-12.0)
	truck.add_child(camera_rig)
	var camera := Camera3D.new()
	camera.position = Vector3.ZERO
	camera.current = true
	camera_rig.add_child(camera)
	_last_position = truck.global_position

func _build_traffic() -> void:
	for i in range(6):
		var traffic := EuroHaulTrafficVehicle.new()
		traffic.position = Vector3(-2.0, 0.0, -55.0 - float(i) * 85.0)
		traffic.player_target = truck
		traffic.lane_x = -2.0 if i % 2 == 0 else 2.0
		traffic.cruise_speed_kmh = 42.0 + float(i % 3) * 8.0
		var body := _make_box_mesh(Vector3(1.9, 1.5, 4.8), Color("#7892a2") if i % 2 == 0 else Color("#9a624e"))
		body.position.y = 0.9
		traffic.add_child(body)
		var traffic_collision := CollisionShape3D.new()
		var traffic_shape := BoxShape3D.new()
		traffic_shape.size = Vector3(1.9, 1.5, 4.8)
		traffic_collision.shape = traffic_shape
		traffic_collision.position.y = 0.9
		traffic.add_child(traffic_collision)
		add_child(traffic)
		traffic_vehicles.append(traffic)

func _build_career() -> void:
	truck.horn_pressed.connect(func(): status_label.text = "HORN • SIGNAL")
	truck.indicators_changed.connect(func(value: int): status_label.text = "HAZARD LIGHTS" if value == 2 else ("LEFT INDICATOR" if value == -1 else ("RIGHT INDICATOR" if value == 1 else "SIGNALS OFF")))
	career = EuroHaulCareer.new()
	career.name = "CareerManager"
	add_child(career)
	career.changed.connect(_update_hud)
	company = EuroHaulCompanyManager.new()
	company.name = "CompanyManager"
	add_child(company)
	company.company_changed.connect(_update_hud)

func _build_hud() -> void:
	var layer := CanvasLayer.new()
	layer.name = "HUD"
	add_child(layer)
	var panel := PanelContainer.new()
	panel.position = Vector2(20, 18)
	panel.custom_minimum_size = Vector2(360, 112)
	var style := StyleBoxFlat.new()
	style.bg_color = Color(0.025, 0.055, 0.09, 0.92)
	style.border_color = GOLD
	style.set_border_width_all(2)
	style.set_corner_radius_all(12)
	panel.add_theme_stylebox_override("panel", style)
	layer.add_child(panel)
	var column := VBoxContainer.new()
	panel.add_child(column)
	hud = Label.new()
	hud.add_theme_color_override("font_color", Color("#f2f4f6"))
	column.add_child(hud)
	status_label = Label.new()
	status_label.text = "FREIGHT • BERLIN REGION"
	status_label.add_theme_color_override("font_color", GOLD)
	column.add_child(status_label)
	speed_label = Label.new()
	speed_label.add_theme_color_override("font_color", Color("#dce7f0"))
	column.add_child(speed_label)
	var buttons := HBoxContainer.new()
	buttons.anchor_left = 0.0
	buttons.anchor_top = 1.0
	buttons.anchor_right = 1.0
	buttons.anchor_bottom = 1.0
	buttons.offset_left = 18
	buttons.offset_top = -118
	buttons.offset_right = -18
	buttons.offset_bottom = -18
	layer.add_child(buttons)
	_add_touch_button(buttons, "◀", "left")
	_add_touch_button(buttons, "BRAKE", "brake")
	_add_touch_button(buttons, "GAS", "throttle")
	_add_touch_button(buttons, "▶", "right")
	_add_shift_button(buttons, "GEAR −", false)
	_add_shift_button(buttons, "GEAR +", true)
	var signal_row := HBoxContainer.new()
	signal_row.position = Vector2(20, 265)
	signal_row.custom_minimum_size = Vector2(500, 44)
	layer.add_child(signal_row)
	_add_signal_button(signal_row, "LEFT SIGNAL", -1)
	_add_signal_button(signal_row, "RIGHT SIGNAL", 1)
	var hazard := Button.new()
	hazard.text = "HAZARDS"
	hazard.pressed.connect(func(): truck.toggle_hazards())
	signal_row.add_child(hazard)
	var horn := Button.new()
	horn.text = "HORN"
	horn.pressed.connect(func(): truck.sound_horn())
	signal_row.add_child(horn)
	var accept := Button.new()
	accept.text = "ACCEPT FREIGHT"
	accept.position = Vector2(20, 145)
	accept.custom_minimum_size = Vector2(190, 52)
	accept.pressed.connect(func(): career.accept_offer(); _update_hud())
	layer.add_child(accept)
	var company_button := Button.new()
	company_button.text = "FOUND COMPANY • €50,000"
	company_button.position = Vector2(220, 145)
	company_button.custom_minimum_size = Vector2(230, 52)
	company_button.pressed.connect(_found_company)
	layer.add_child(company_button)
	var buy_truck_button := Button.new()
	buy_truck_button.text = "BUY FLEET TRUCK • €85,000"
	buy_truck_button.position = Vector2(20, 205)
	buy_truck_button.custom_minimum_size = Vector2(230, 48)
	buy_truck_button.pressed.connect(_buy_company_truck)
	layer.add_child(buy_truck_button)
	var hire_driver_button := Button.new()
	hire_driver_button.text = "HIRE DRIVER • €5,000"
	hire_driver_button.position = Vector2(260, 205)
	hire_driver_button.custom_minimum_size = Vector2(210, 48)
	hire_driver_button.pressed.connect(_hire_company_driver)
	layer.add_child(hire_driver_button)
	_update_hud()

func _add_touch_button(parent: Control, caption: String, key: String) -> void:
	var button := Button.new()
	button.text = caption
	button.custom_minimum_size = Vector2(120, 82)
	button.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	button.button_down.connect(func(): touch_state[key] = true)
	button.button_up.connect(func(): touch_state[key] = false)
	parent.add_child(button)

func _add_signal_button(parent: Control, caption: String, side: int) -> void:
	var button := Button.new()
	button.text = caption
	button.pressed.connect(func(): truck.set_indicator(side if truck.indicators != side else 0))
	parent.add_child(button)

func _add_shift_button(parent: Control, caption: String, up: bool) -> void:
	var button := Button.new()
	button.text = caption
	button.custom_minimum_size = Vector2(100, 82)
	button.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	button.pressed.connect(func():
		truck.set_clutch(1.0)
		if up:
			truck.shift_up()
		else:
			truck.shift_down()
		truck.set_clutch(0.0)
	)
	parent.add_child(button)

func _buy_company_truck() -> void:
	if company != null and company.purchase_truck():
		_update_hud()

func _hire_company_driver() -> void:
	if company != null and company.hire_driver():
		_update_hud()

func _found_company() -> void:
	if company == null or career == null or not company.company_name.is_empty():
		return
	if career.spend_cash(EuroHaulCompanyManager.FOUNDING_COST):
		company.found_company("EURO HAUL LOGISTICS", EuroHaulCompanyManager.FOUNDING_COST)
		_update_hud()

func _update_traffic_warnings() -> void:
	if status_label == null or truck == null:
		return
	if truck.indicators == 0:
		status_label.text = "FREIGHT • BERLIN REGION"
		var nearest_zone := ""
		var nearest_distance := INF
		for traffic in traffic_vehicles:
			if is_instance_valid(traffic):
				var report := traffic.get_proximity_report()
				if not report.is_empty() and float(report.distance_m) < nearest_distance:
					nearest_distance = float(report.distance_m)
					nearest_zone = str(report.zone)
		if not nearest_zone.is_empty():
			status_label.text = "%s TRAFFIC • %.0f m" % [nearest_zone, nearest_distance]
		return
	var requested_lane := -2.0 if truck.indicators == -1 else (2.0 if truck.indicators == 1 else 0.0)
	if requested_lane == 0.0:
		return
	for traffic in traffic_vehicles:
		if is_instance_valid(traffic):
			var warning := traffic.safety_warning_for_lane(requested_lane)
			if not warning.is_empty():
				status_label.text = warning
				return
	status_label.text = "LEFT LANE CLEAR" if truck.indicators == -1 else "RIGHT LANE CLEAR"

func _update_hud() -> void:
	if hud == null or career == null:
		return
	var contract_text := "No active contract"
	if not career.active_contract.is_empty():
		var c: Dictionary = career.active_contract
		contract_text = "%s → %s | %s | %.2f/%.2f km" % [c.origin, c.destination, c.cargo, c.progress_km, c.distance_km]
	elif not career.offered_contract.is_empty():
		var o: Dictionary = career.offered_contract
		contract_text = "Offer: %s → %s | %s | %.2f km | €%d" % [o.origin, o.destination, o.cargo, o.distance_km, int(o.reward)]
	hud.text = "DRIVER  %s  •  LVL %d  •  €%d\n%s" % [career.driver_name, career.level, career.game_cash, contract_text]
	if truck != null and speed_label != null:
		speed_label.text = "%03d km/h     RPM %04d     GEAR %s     DAMAGE %d%%" % [int(absf(truck.speed_kmh)), int(truck.engine_rpm), "R" if truck.gear == -1 else ("N" if truck.gear == 0 else str(truck.gear)), int(truck.damage)]

func _add_box(label: String, position_value: Vector3, size_value: Vector3, color: Color, solid: bool) -> void:
	var node := StaticBody3D.new() if solid else Node3D.new()
	node.name = label
	node.position = position_value
	var mesh := _make_box_mesh(size_value, color)
	node.add_child(mesh)
	if solid:
		var collision := CollisionShape3D.new()
		var shape := BoxShape3D.new()
		shape.size = size_value
		collision.shape = shape
		node.add_child(collision)
	add_child(node)

func _make_box_mesh(size_value: Vector3, color: Color) -> MeshInstance3D:
	var mesh := MeshInstance3D.new()
	var box := BoxMesh.new()
	box.size = size_value
	mesh.mesh = box
	var material := StandardMaterial3D.new()
	material.albedo_color = color
	material.roughness = 0.82
	mesh.material_override = material
	return mesh

func _make_cylinder_mesh(height: float, radius: float, color: Color) -> MeshInstance3D:
	var mesh := MeshInstance3D.new()
	var cylinder := CylinderMesh.new()
	cylinder.height = height
	cylinder.top_radius = radius
	cylinder.bottom_radius = radius
	mesh.mesh = cylinder
	var material := StandardMaterial3D.new()
	material.albedo_color = color
	mesh.material_override = material
	return mesh
