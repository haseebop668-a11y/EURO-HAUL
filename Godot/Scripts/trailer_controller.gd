extends AnimatableBody3D
class_name EuroHaulTrailer

@export var articulation_limit_degrees := 55.0
@export var forward_articulation_gain := 0.35
@export var reverse_articulation_gain := 1.25
@export var articulation_response := 2.4

var truck: EuroHaulTruck
var articulation_degrees := 0.0

func _ready() -> void:
	_build_trailer_visuals()

func _physics_process(delta: float) -> void:
	if not is_instance_valid(truck):
		return
	var speed_factor := clampf(absf(truck.speed_kmh) / 55.0, 0.0, 1.0)
	var gain := reverse_articulation_gain if truck.gear == -1 else forward_articulation_gain
	var direction := -1.0 if truck.gear == -1 else 1.0
	var target_angle := -truck.steer_input * speed_factor * gain * 35.0 * direction
	articulation_degrees = move_toward(articulation_degrees, clampf(target_angle, -articulation_limit_degrees, articulation_limit_degrees), articulation_response * 45.0 * delta)
	global_position = truck.global_position + truck.global_transform.basis.z * 3.3
	global_rotation = Vector3(global_rotation.x, truck.global_rotation.y + deg_to_rad(articulation_degrees), global_rotation.z)

func _build_trailer_visuals() -> void:
	var cargo_collision := CollisionShape3D.new()
	var cargo_shape := BoxShape3D.new()
	cargo_shape.size = Vector3(2.45, 2.65, 8.8)
	cargo_collision.shape = cargo_shape
	cargo_collision.position = Vector3(0.0, 2.05, 4.9)
	add_child(cargo_collision)

	var cargo := MeshInstance3D.new()
	var cargo_mesh := BoxMesh.new()
	cargo_mesh.size = Vector3(2.45, 2.65, 8.8)
	cargo.mesh = cargo_mesh
	cargo.position = Vector3(0.0, 2.05, 4.9)
	var cargo_material := StandardMaterial3D.new()
	cargo_material.albedo_color = Color("#d8dce0")
	cargo_material.metallic = 0.28
	cargo_material.roughness = 0.62
	cargo.material_override = cargo_material
	add_child(cargo)

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

	var rear_door := MeshInstance3D.new()
	var door_mesh := BoxMesh.new()
	door_mesh.size = Vector3(2.3, 2.45, 0.08)
	rear_door.mesh = door_mesh
	rear_door.position = Vector3(0.0, 2.05, 9.32)
	var door_material := StandardMaterial3D.new()
	door_material.albedo_color = Color("#b6c0c8")
	rear_door.material_override = door_material
	add_child(rear_door)
