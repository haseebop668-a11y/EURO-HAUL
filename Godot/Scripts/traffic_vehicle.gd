extends Node3D
class_name EuroHaulTrafficVehicle

@export var cruise_speed_kmh := 48.0
@export var lane_x := -2.0
@export var route_length := 850.0
var speed_kmh := 0.0
var _phase := 0.0

func _ready() -> void:
	speed_kmh = cruise_speed_kmh
	_phase = absf(global_position.z) * 3.6 / maxf(1.0, cruise_speed_kmh)

func _process(delta: float) -> void:
	_phase += delta
	var target_z := -fmod(_phase * cruise_speed_kmh / 3.6, route_length)
	global_position.x = lerpf(global_position.x, lane_x, minf(1.0, delta * 2.0))
	global_position.z = target_z
