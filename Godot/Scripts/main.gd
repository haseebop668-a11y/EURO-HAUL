extends Node3D

const SKY_COLOR := Color("#07111f")
const GOLD := Color("#d5ad62")
const BLUE := Color("#16334e")
const ROAD_LENGTH := 900.0

var truck: EuroHaulTruck
var trailer: EuroHaulTrailer
var career: EuroHaulCareer
var company: EuroHaulCompanyManager
var world_cycle: EuroHaulWorldEnvironmentController
var hud: Label
var status_label: Label
var speed_label: Label
var left_indicator_light: OmniLight3D
var right_indicator_light: OmniLight3D
var _indicator_blink_timer := 0.0
var freight_action_button: Button
var accept_freight_button: Button
var found_company_button: Button
var buy_truck_button: Button
var hire_driver_button: Button
var pause_button: Button
var near_pickup_depot := false
var near_delivery_depot := false
var near_service_station := false
var service_button: Button
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
	_update_indicator_lights(delta)
	var current_position := truck.global_position
	var distance := current_position.distance_to(_last_position)
	if current_position.z < -ROAD_LENGTH + 20.0:
		current_position.z += ROAD_LENGTH
		truck.global_position = current_position
	elif current_position.z > 20.0:
		current_position.z -= ROAD_LENGTH
		truck.global_position = current_position
	_last_position = current_position
	var expected_step := (absf(truck.speed_kmh) / 3.6) * delta * 3.0 + 1.0
	if distance > 0.0 and distance <= maxf(10.0, expected_step) and career != null:
		career.record_distance(distance / 1000.0)
		truck.consume_fuel(distance / 1000.0)
		_last_distance_save += distance
	if _last_distance_save >= 100.0:
		if career != null:
			career.save()
		_last_distance_save = 0.0
	_update_hud()
	_update_traffic_warnings()
	_update_freight_action()

func _notification(what: int) -> void:
	if what == NOTIFICATION_WM_GO_BACK_REQUEST:
		get_tree().paused = not get_tree().paused
		if pause_button != null:
			pause_button.text = "RESUME" if get_tree().paused else "PAUSE"
		if status_label != null:
			status_label.text = "PAUSED" if get_tree().paused else "RESUMED"
		return
	if what == NOTIFICATION_APPLICATION_FOCUS_OUT or what == NOTIFICATION_APPLICATION_PAUSED or what == NOTIFICATION_WM_WINDOW_FOCUS_OUT:
		for key in touch_state.keys():
			touch_state[key] = false
		if truck != null:
			truck.set_controls(0.0, 0.0, 0.0)
		if career != null:
			career.save()
		if company != null:
			company.save_company()

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
	world_cycle = EuroHaulWorldEnvironmentController.new()
	world_cycle.name = "WorldCycle"
	world_cycle.environment = environment
	world_cycle.sun_light = get_node_or_null("DirectionalLight3D") as DirectionalLight3D
	add_child(world_cycle)

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
	_create_depot_area("PickupDepot", -50.0, true)
	_create_depot_area("DeliveryDepot", -820.0, false)
	_create_service_area(-600.0)

func _create_depot_area(depot_name: String, road_z: float, is_pickup: bool) -> void:
	var depot := Area3D.new()
	depot.name = depot_name
	depot.position = Vector3(0.0, 1.0, road_z)
	depot.collision_layer = 0
	depot.collision_mask = 1
	var trigger := CollisionShape3D.new()
	var trigger_shape := SphereShape3D.new()
	trigger_shape.radius = 10.0
	trigger.shape = trigger_shape
	depot.add_child(trigger)
	depot.body_entered.connect(func(body: Node3D):
		if body != truck:
			return
		if is_pickup:
			near_pickup_depot = true
		else:
			near_delivery_depot = true
		_update_freight_action()
	)
	depot.body_exited.connect(func(body: Node3D):
		if body != truck:
			return
		if is_pickup:
			near_pickup_depot = false
		else:
			near_delivery_depot = false
		_update_freight_action()
	)
	add_child(depot)
	var sign_color := GOLD if is_pickup else Color("#78a9bd")
	_add_box("%s Sign" % depot_name, Vector3(-9.0, 2.0, road_z), Vector3(0.25, 3.0, 4.0), sign_color, false)
	var sign_label := Label3D.new()
	sign_label.text = "FREIGHT PICKUP" if is_pickup else "FREIGHT DELIVERY"
	sign_label.position = Vector3(-9.0, 4.0, road_z)
	sign_label.font_size = 36
	sign_label.modulate = Color("#f4f5f6")
	add_child(sign_label)

func _create_service_area(road_z: float) -> void:
	var service_area := Area3D.new()
	service_area.name = "ServiceStation"
	service_area.position = Vector3(0.0, 1.0, road_z)
	service_area.collision_layer = 0
	service_area.collision_mask = 1
	var trigger := CollisionShape3D.new()
	var shape := SphereShape3D.new()
	shape.radius = 15.0
	trigger.shape = shape
	service_area.add_child(trigger)
	service_area.body_entered.connect(func(body: Node3D):
		if body == truck:
			near_service_station = true
			_update_service_button()
	)
	service_area.body_exited.connect(func(body: Node3D):
		if body == truck:
			near_service_station = false
			_update_service_button()
	)
	add_child(service_area)
	_add_box("Service Station Sign", Vector3(-9.0, 2.0, road_z), Vector3(0.25, 3.0, 4.0), Color("#5da68a"), false)
	var sign_label := Label3D.new()
	sign_label.text = "FUEL • SERVICE"
	sign_label.position = Vector3(-9.0, 4.0, road_z)
	sign_label.font_size = 36
	sign_label.modulate = Color("#f4f5f6")
	add_child(sign_label)

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
	var horn_audio := AudioStreamPlayer3D.new()
	horn_audio.name = "HornAudio"
	horn_audio.stream = _create_horn_stream()
	horn_audio.volume_db = -4.0
	truck.add_child(horn_audio)
	truck.horn_pressed.connect(func(): horn_audio.play())
	left_indicator_light = OmniLight3D.new()
	left_indicator_light.light_color = Color("#ffad45")
	left_indicator_light.light_energy = 1.4
	left_indicator_light.omni_range = 4.0
	left_indicator_light.position = Vector3(-1.15, 1.85, 3.0)
	left_indicator_light.visible = false
	truck.add_child(left_indicator_light)
	right_indicator_light = OmniLight3D.new()
	right_indicator_light.light_color = Color("#ffad45")
	right_indicator_light.light_energy = 1.4
	right_indicator_light.omni_range = 4.0
	right_indicator_light.position = Vector3(1.15, 1.85, 3.0)
	right_indicator_light.visible = false
	truck.add_child(right_indicator_light)
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
	for traffic in traffic_vehicles:
		var neighbors: Array[EuroHaulTrafficVehicle] = []
		for other in traffic_vehicles:
			if other != traffic:
				neighbors.append(other)
		traffic.traffic_neighbors = neighbors

func _update_indicator_lights(delta: float) -> void:
	if truck == null or left_indicator_light == null or right_indicator_light == null:
		return
	_indicator_blink_timer += delta
	var blink_on := fmod(_indicator_blink_timer, 1.0) < 0.5
	left_indicator_light.visible = blink_on and (truck.indicators == -1 or truck.indicators == 2)
	right_indicator_light.visible = blink_on and (truck.indicators == 1 or truck.indicators == 2)

func _create_horn_stream() -> AudioStreamWAV:
	var sample_rate := 22050
	var sample_count := int(sample_rate * 0.38)
	var samples := PackedByteArray()
	samples.resize(sample_count * 2)
	for i in range(sample_count):
		var time := float(i) / float(sample_rate)
		var attack := minf(1.0, float(i) / 350.0)
		var release := maxf(0.0, 1.0 - float(i) / float(sample_count))
		var wave := sin(TAU * 392.0 * time) * 0.62 + sin(TAU * 494.0 * time) * 0.38
		var sample := clampi(int(wave * attack * release * 14000.0), -32768, 32767)
		samples.encode_s16(i * 2, sample)
	var stream := AudioStreamWAV.new()
	stream.format = AudioStreamWAV.FORMAT_16_BITS
	stream.mix_rate = sample_rate
	stream.stereo = false
	stream.data = samples
	return stream

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
	layer.process_mode = Node.PROCESS_MODE_ALWAYS
	add_child(layer)
	var panel := PanelContainer.new()
	panel.position = Vector2(20, 18)
	panel.custom_minimum_size = Vector2(520, 150)
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
	_add_special_gear_button(buttons, "NEUTRAL", false)
	_add_special_gear_button(buttons, "REVERSE", true)
	var signal_row := HBoxContainer.new()
	signal_row.position = Vector2(20, 295)
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
	pause_button = Button.new()
	pause_button.text = "PAUSE"
	pause_button.pressed.connect(func():
		get_tree().paused = not get_tree().paused
		pause_button.text = "RESUME" if get_tree().paused else "PAUSE"
		status_label.text = "PAUSED" if get_tree().paused else "RESUMED"
	)
	signal_row.add_child(pause_button)
	accept_freight_button = Button.new()
	accept_freight_button.text = "ACCEPT FREIGHT"
	accept_freight_button.position = Vector2(20, 175)
	accept_freight_button.custom_minimum_size = Vector2(190, 52)
	accept_freight_button.pressed.connect(func(): career.accept_offer(); _update_hud(); _update_freight_action())
	layer.add_child(accept_freight_button)
	freight_action_button = Button.new()
	freight_action_button.text = "DRIVE TO PICKUP"
	freight_action_button.position = Vector2(500, 175)
	freight_action_button.custom_minimum_size = Vector2(210, 52)
	freight_action_button.pressed.connect(_handle_freight_interaction)
	layer.add_child(freight_action_button)
	found_company_button = Button.new()
	found_company_button.text = "FOUND COMPANY • €50,000"
	found_company_button.position = Vector2(220, 175)
	found_company_button.custom_minimum_size = Vector2(230, 52)
	found_company_button.pressed.connect(_found_company)
	layer.add_child(found_company_button)
	buy_truck_button = Button.new()
	buy_truck_button.text = "BUY FLEET TRUCK • €85,000"
	buy_truck_button.position = Vector2(20, 235)
	buy_truck_button.custom_minimum_size = Vector2(230, 48)
	buy_truck_button.pressed.connect(_buy_company_truck)
	layer.add_child(buy_truck_button)
	hire_driver_button = Button.new()
	hire_driver_button.text = "HIRE DRIVER • €5,000"
	hire_driver_button.position = Vector2(260, 235)
	hire_driver_button.custom_minimum_size = Vector2(210, 48)
	hire_driver_button.pressed.connect(_hire_company_driver)
	layer.add_child(hire_driver_button)
	service_button = Button.new()
	service_button.text = "DRIVE TO SERVICE"
	service_button.position = Vector2(500, 235)
	service_button.custom_minimum_size = Vector2(210, 48)
	service_button.pressed.connect(_handle_service_interaction)
	layer.add_child(service_button)
	_update_hud()
	_update_freight_action()
	_update_service_button()

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

func _add_special_gear_button(parent: Control, caption: String, reverse: bool) -> void:
	var button := Button.new()
	button.text = caption
	button.custom_minimum_size = Vector2(110, 82)
	button.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	button.pressed.connect(func():
		truck.set_clutch(1.0)
		if reverse:
			truck.select_reverse()
		else:
			truck.select_neutral()
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

func _handle_service_interaction() -> void:
	if not near_service_station or truck == null or career == null:
		return
	var fuel_cost := int(ceil((100.0 - truck.fuel_percent) * 2.0))
	var repair_cost := int(ceil(truck.damage * 25.0))
	var total_cost := fuel_cost + repair_cost
	if total_cost <= 0:
		status_label.text = "TRUCK ALREADY SERVICED"
		return
	if career.spend_cash(total_cost):
		truck.refuel()
		truck.repair()
		status_label.text = "SERVICE COMPLETE • €%d" % total_cost
	else:
		status_label.text = "SERVICE COST €%d • INSUFFICIENT FUNDS" % total_cost
	_update_hud()
	_update_service_button()

func _update_service_button() -> void:
	if service_button == null or truck == null or career == null:
		return
	var fuel_cost := int(ceil((100.0 - truck.fuel_percent) * 2.0))
	var repair_cost := int(ceil(truck.damage * 25.0))
	var total_cost := fuel_cost + repair_cost
	if not near_service_station:
		service_button.text = "DRIVE TO SERVICE"
		service_button.disabled = true
	elif total_cost <= 0:
		service_button.text = "TRUCK FULLY SERVICED"
		service_button.disabled = true
	else:
		service_button.text = "REFUEL / REPAIR • €%d" % total_cost
		service_button.disabled = career.game_cash < total_cost

func _handle_freight_interaction() -> void:
	if career == null or career.active_contract.is_empty():
		return
	if not career.cargo_loaded and near_pickup_depot:
		if career.load_cargo():
			status_label.text = "CARGO LOADED • DELIVERY STARTED"
	elif career.cargo_loaded and near_delivery_depot:
		if career.unload_cargo():
			status_label.text = "DELIVERY COMPLETED • REWARD PAID"
		else:
			status_label.text = "DELIVERY DEPOT • ROUTE DISTANCE NOT COMPLETE"
	_update_hud()
	_update_freight_action()

func _update_freight_action() -> void:
	if freight_action_button == null or career == null:
		return
	if accept_freight_button != null:
		accept_freight_button.disabled = not career.active_contract.is_empty() or career.offered_contract.is_empty()
	if career.active_contract.is_empty():
		freight_action_button.text = "ACCEPT FREIGHT FIRST"
		freight_action_button.disabled = true
	elif not career.cargo_loaded:
		freight_action_button.disabled = not near_pickup_depot
		freight_action_button.text = "LOAD CARGO" if near_pickup_depot else "DRIVE TO PICKUP DEPOT"
	elif near_delivery_depot:
		var remaining := maxf(0.0, float(career.active_contract.get("distance_km", 0.0)) - float(career.active_contract.get("progress_km", 0.0)))
		if remaining <= 0.0:
			freight_action_button.disabled = false
			freight_action_button.text = "UNLOAD CARGO"
		else:
			freight_action_button.disabled = true
			freight_action_button.text = "DELIVERY DEPOT • %.2f KM LEFT" % remaining
	else:
		freight_action_button.disabled = true
		var remaining := maxf(0.0, float(career.active_contract.get("distance_km", 0.0)) - float(career.active_contract.get("progress_km", 0.0)))
		freight_action_button.text = "DELIVER • %.2f KM LEFT" % remaining

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
		contract_text = "%s → %s | %s | %s | %.2f/%.2f km" % [c.origin, c.destination, c.cargo, "LOADED" if career.cargo_loaded else "PICKUP REQUIRED", c.progress_km, c.distance_km]
	elif not career.offered_contract.is_empty():
		var o: Dictionary = career.offered_contract
		contract_text = "Offer: %s → %s | %s | %.2f km | €%d" % [o.origin, o.destination, o.cargo, o.distance_km, int(o.reward)]
	var company_text := "COMPANY • NOT FOUNDED"
	if company != null and not company.company_name.is_empty():
		company_text = "%s • €%d • FLEET %d • DRIVERS %d" % [company.company_name, company.company_balance, company.owned_trucks, company.hired_drivers]
	if found_company_button != null:
		found_company_button.text = "FOUND COMPANY • €50,000"
		found_company_button.disabled = false
		if company != null and not company.company_name.is_empty():
			found_company_button.disabled = true
			found_company_button.text = "COMPANY ACTIVE"
		elif career.game_cash < EuroHaulCompanyManager.FOUNDING_COST:
			found_company_button.disabled = true
			found_company_button.text = "NEED €50,000 TO FOUND"
	if buy_truck_button != null:
		buy_truck_button.disabled = company == null or company.company_name.is_empty() or company.company_balance < EuroHaulCompanyManager.TRUCK_PURCHASE_COST
	if hire_driver_button != null:
		hire_driver_button.disabled = company == null or company.company_name.is_empty() or company.company_balance < EuroHaulCompanyManager.DRIVER_HIRING_COST or company.hired_drivers >= company.owned_trucks
	var world_text := world_cycle.display_status() if world_cycle != null else "WORLD • DAY"
	hud.text = "DRIVER  %s  •  LVL %d  •  €%d\n%s\n%s\n%s" % [career.driver_name, career.level, career.game_cash, contract_text, company_text, world_text]
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
