extends AnimatableBody3D
class_name EuroHaulTrafficVehicle

@export var cruise_speed_kmh := 48.0
@export var lane_x := -2.0
@export var route_length := 850.0
@export var player_target: Node3D
@export var detection_distance_m := 38.0
@export var lane_change_speed := 2.8

var speed_kmh := 0.0
var _phase := 0.0
var _base_lane_x := 0.0
var _avoidance_active := false
var _clear_timer := 0.0

func _ready() -> void:
	speed_kmh = cruise_speed_kmh
	_base_lane_x = lane_x
	_phase = absf(global_position.z) * 3.6 / maxf(1.0, cruise_speed_kmh)

func _process(delta: float) -> void:
	_phase += delta
	var target_z := -fmod(_phase * cruise_speed_kmh / 3.6, route_length)
	global_position.z = target_z
	_update_player_avoidance(delta)
	global_position.x = move_toward(global_position.x, lane_x, lane_change_speed * delta)

func _update_player_avoidance(delta: float) -> void:
	if not is_instance_valid(player_target):
		lane_x = _base_lane_x
		_avoidance_active = false
		return

	var gap_ahead := player_target.global_position.z - global_position.z
	var player_speed := 0.0
	if player_target is EuroHaulTruck:
		player_speed = player_target.speed_kmh
	var player_is_closing := gap_ahead > 0.0 and gap_ahead < detection_distance_m and player_speed > speed_kmh + 4.0
	var traffic_is_closing_from_behind := gap_ahead < 0.0 and absf(gap_ahead) < 18.0 and speed_kmh > player_speed + 8.0

	if player_is_closing or traffic_is_closing_from_behind:
		_avoidance_active = true
		_clear_timer = 0.0
		lane_x = 2.0 if _base_lane_x < 0.0 else -2.0
	elif _avoidance_active:
		_clear_timer += delta
		if _clear_timer >= 2.0:
			_avoidance_active = false
			lane_x = _base_lane_x

func safety_warning_for_lane(requested_lane_x: float) -> String:
	var lateral_gap := absf(global_position.x - requested_lane_x)
	if lateral_gap > 1.5:
		return ""
	if not is_instance_valid(player_target):
		return ""
	var longitudinal_gap := absf(global_position.z - player_target.global_position.z)
	if longitudinal_gap < 24.0:
		return "TRAFFIC IN ADJACENT LANE"
	return ""
