extends SpringArm3D

@export var target_path: NodePath
@export var follow_height := 3.2
@export var follow_distance := 8.5
@export var smoothing := 5.0
var target: Node3D

func _ready() -> void:
	 spring_length = follow_distance
	 if not target_path.is_empty():
		 target = get_node_or_null(target_path) as Node3D

func _process(delta: float) -> void:
	if target == null:
		return
	var desired := target.global_position + Vector3.UP * follow_height
	global_position = global_position.lerp(desired, minf(1.0, smoothing * delta))
	global_rotation.y = lerp_angle(global_rotation.y, target.global_rotation.y, minf(1.0, smoothing * delta))
