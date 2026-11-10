extends AnimatableBody3D
class_name EuroHaulTrailer

signal cargo_damaged(amount: float)

@export var articulation_limit_degrees := 55.0
@export var forward_articulation_gain := 0.35
@export var reverse_articulation_gain := 1.25
@export var articulation_response := 2.4

var truck: EuroHaulTruck
var articulation_degrees := 0.0
var cargo_mass_tons := 0.0
var cargo_loaded := false
var _cargo_animation_state := 0
var _door_open_progress := 0.0
var _cargo_slide_progress := 0.0
var _left_door_pivot: Node3D
var _right_door_pivot: Node3D
var _cargo_visual: MeshInstance3D

func set_cargo_mass(mass_tons: float) -> void:
	cargo_mass_tons = clampf(mass_tons, 0.0, 30.0)

func set_cargo_loaded(value: bool, animate: bool = true) -> void:
	cargo_loaded = value
	if _cargo_visual == null:
		return
	if not animate:
		_cargo_animation_state = 0
		_door_open_progress = 0.0
		_cargo_slide_progress = 1.0 if value else 0.0
		_cargo_visual.visible = value
		_update_cargo_visuals()
		return
	if value:
		_cargo_visual.visible = true
		_cargo_slide_progress = 0.0
		_cargo_animation_state = 1
	else:
		_cargo_visual.visible = true
		_cargo_slide_progress = 1.0
		_cargo_animation_state = 4

func _ready() -> void:
	collision_layer = 4
	collision_mask = 1
	_build_trailer_visuals()

func _physics_process(delta: float) -> void:
	_update_cargo_animation(delta)
	if not is_instance_valid(truck):
		return
	global_position = truck.global_position + truck.global_transform.basis.z * 3.3
	if absf(truck.speed_kmh) < 0.5:
		global_rotation = Vector3(global_rotation.x, truck.global_rotation.y + deg_to_rad(articulation_degrees), global_rotation.z)
		return
	var speed_factor := clampf(absf(truck.speed_kmh) / 55.0, 0.0, 1.0)
	var gain := reverse_articulation_gain if truck.gear == -1 else forward_articulation_gain
	var direction := -1.0 if truck.gear == -1 else 1.0
	var load_factor := 1.0 + cargo_mass_tons / 50.0
	var target_angle := -truck.steer_input * speed_factor * gain * 35.0 * direction * load_factor
	articulation_degrees = move_toward(articulation_degrees, clampf(target_angle, -articulation_limit_degrees, articulation_limit_degrees), articulation_response * 45.0 * delta)
	global_rotation = Vector3(global_rotation.x, truck.global_rotation.y + deg_to_rad(articulation_degrees), global_rotation.z)

func _build_trailer_visuals() -> void:
	var cargo_collision := CollisionShape3D.new()
	var cargo_shape := BoxShape3D.new()
	cargo_shape.size = Vector3(2.45, 2.65, 8.8)
	cargo_collision.shape = cargo_shape
	cargo_collision.position = Vector3(0.0, 2.05, 4.9)
	add_child(cargo_collision)
	var impact_area := Area3D.new()
	impact_area.name = "TrailerImpactSensor"
	impact_area.collision_layer = 0
	impact_area.collision_mask = 1
	var impact_shape := CollisionShape3D.new()
	var impact_box := BoxShape3D.new()
	impact_box.size = Vector3(2.55, 2.75, 8.9)
	impact_shape.shape = impact_box
	impact_shape.position = Vector3(0.0, 2.05, 4.9)
	impact_area.add_child(impact_shape)
	impact_area.body_entered.connect(_on_trailer_body_entered)
	add_child(impact_area)

	_add_trailer_panel("Left Side Panel", Vector3(0.08, 2.65, 8.8), Vector3(-1.22, 2.05, 4.9), Color("#d8dce0"))
	_add_trailer_panel("Right Side Panel", Vector3(0.08, 2.65, 8.8), Vector3(1.22, 2.05, 4.9), Color("#d8dce0"))
	_add_trailer_panel("Roof Panel", Vector3(2.45, 0.08, 8.8), Vector3(0.0, 3.37, 4.9), Color("#c5cdd3"))
	_add_trailer_panel("Front Bulkhead", Vector3(2.45, 2.65, 0.08), Vector3(0.0, 2.05, 0.5), Color("#c5cdd3"))
	_add_trailer_panel("Cargo Floor", Vector3(2.35, 0.08, 8.7), Vector3(0.0, 1.0, 4.9), Color("#59636c"))

	_left_door_pivot = Node3D.new()
	_left_door_pivot.name = "LeftRearDoorPivot"
	_left_door_pivot.position = Vector3(-1.18, 2.05, 9.32)
	add_child(_left_door_pivot)
	var left_door := MeshInstance3D.new()
	var left_door_mesh := BoxMesh.new()
	left_door_mesh.size = Vector3(1.2, 2.5, 0.08)
	left_door.mesh = left_door_mesh
	left_door.position = Vector3(0.6, 0.0, 0.0)
	left_door.material_override = _create_door_material()
	_left_door_pivot.add_child(left_door)

	_right_door_pivot = Node3D.new()
	_right_door_pivot.name = "RightRearDoorPivot"
	_right_door_pivot.position = Vector3(1.18, 2.05, 9.32)
	add_child(_right_door_pivot)
	var right_door := MeshInstance3D.new()
	var right_door_mesh := BoxMesh.new()
	right_door_mesh.size = Vector3(1.2, 2.5, 0.08)
	right_door.mesh = right_door_mesh
	right_door.position = Vector3(-0.6, 0.0, 0.0)
	right_door.material_override = _create_door_material()
	_right_door_pivot.add_child(right_door)

	_cargo_visual = MeshInstance3D.new()
	_cargo_visual.name = "CargoVisual"
	var cargo_mesh := BoxMesh.new()
	cargo_mesh.size = Vector3(1.8, 1.25, 1.8)
	_cargo_visual.mesh = cargo_mesh
	_cargo_visual.position = Vector3(0.0, 1.72, 9.0)
	var cargo_material := StandardMaterial3D.new()
	cargo_material.albedo_color = Color("#b88a52")
	cargo_material.roughness = 0.86
	_cargo_visual.material_override = cargo_material
	_cargo_visual.visible = false
	add_child(_cargo_visual)

	var frame := MeshInstance3D.new()
	var frame_mesh := BoxMesh.new()
	frame_mesh.size = Vector3(2.5, 0.42, 9.2)
	frame.mesh = frame_mesh
	frame.position = Vector3(0.0, 0.72, 4.9)
	var frame_material := StandardMaterial3D.new()
	frame_material.albedo_color = Color("#29333d")
	frame.material_override = frame_material
	add_child(frame)

	for axle_z in [2.0, 3.0, 6.8, 7.8]:
		for wheel_x in [-1.3, 1.3]:
			var wheel := MeshInstance3D.new()
			var wheel_mesh := CylinderMesh.new()
			wheel_mesh.top_radius = 0.48
			wheel_mesh.bottom_radius = 0.48
			wheel_mesh.height = 0.34
			wheel.mesh = wheel_mesh
			wheel.position = Vector3(wheel_x, 0.5, axle_z)
			wheel.rotation.z = PI / 2.0
			var wheel_material := StandardMaterial3D.new()
			wheel_material.albedo_color = Color("#11161b")
			wheel.material_override = wheel_material
			add_child(wheel)

	_update_cargo_visuals()

func _on_trailer_body_entered(body: Node3D) -> void:
	if body == truck or not is_instance_valid(truck):
		return
	if absf(truck.speed_kmh) > 8.0:
		var impact_damage := clampf(absf(truck.speed_kmh) / 35.0 * 6.0, 1.0, 18.0)
		truck.apply_damage(impact_damage)
		cargo_damaged.emit(impact_damage * 0.75)

func _add_trailer_panel(panel_name: String, panel_size: Vector3, panel_position: Vector3, panel_color: Color) -> void:
	var panel := MeshInstance3D.new()
	panel.name = panel_name
	var panel_mesh := BoxMesh.new()
	panel_mesh.size = panel_size
	panel.mesh = panel_mesh
	panel.position = panel_position
	var panel_material := StandardMaterial3D.new()
	panel_material.albedo_color = panel_color
	panel_material.metallic = 0.22
	panel_material.roughness = 0.68
	panel.material_override = panel_material
	add_child(panel)

func _create_door_material() -> StandardMaterial3D:
	var material := StandardMaterial3D.new()
	material.albedo_color = Color("#b6c0c8")
	material.metallic = 0.2
	material.roughness = 0.7
	return material

func _update_cargo_animation(delta: float) -> void:
	if _cargo_visual == null:
		return
	match _cargo_animation_state:
		1:
			_door_open_progress = move_toward(_door_open_progress, 1.0, delta * 1.5)
			if _door_open_progress >= 0.999:
				_cargo_animation_state = 2
		2:
			_cargo_slide_progress = move_toward(_cargo_slide_progress, 1.0, delta * 0.8)
			if _cargo_slide_progress >= 0.999:
				_cargo_animation_state = 3
		3:
			_door_open_progress = move_toward(_door_open_progress, 0.0, delta * 1.5)
			if _door_open_progress <= 0.001:
				_cargo_animation_state = 0
		4:
			_door_open_progress = move_toward(_door_open_progress, 1.0, delta * 1.5)
			if _door_open_progress >= 0.999:
				_cargo_animation_state = 5
		5:
			_cargo_slide_progress = move_toward(_cargo_slide_progress, 0.0, delta * 0.8)
			if _cargo_slide_progress <= 0.001:
				_cargo_visual.visible = false
				_cargo_animation_state = 6
		6:
			_door_open_progress = move_toward(_door_open_progress, 0.0, delta * 1.5)
			if _door_open_progress <= 0.001:
				_cargo_animation_state = 0
	_update_cargo_visuals()

func _update_cargo_visuals() -> void:
	if _cargo_visual != null:
		_cargo_visual.position.z = lerpf(9.0, 7.2, _cargo_slide_progress)
	if _left_door_pivot != null:
		_left_door_pivot.rotation.y = _door_open_progress * PI / 2.0
	if _right_door_pivot != null:
		_right_door_pivot.rotation.y = -_door_open_progress * PI / 2.0
