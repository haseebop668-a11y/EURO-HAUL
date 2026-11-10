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
var traffic_neighbors: Array[EuroHaulTrafficVehicle] = []

func _ready() -> void:
	speed_kmh = cruise_speed_kmh
	_base_lane_x = lane_x
	_phase = absf(global_position.z) * 3.6 / maxf(1.0, cruise_speed_kmh)

func _physics_process(delta: float) -> void:
	_phase += delta
	var target_z := -fmod(_phase * maxf(20.0, speed_kmh) / 3.6, route_length)
	global_position.z = target_z
	_update_player_avoidance(delta)
	global_position.x = move_toward(global_position.x, lane_x, lane_change_speed * delta)

func _update_player_avoidance(delta: float) -> void:
	if not is_instance_valid(player_target):
		lane_x = _base_lane_x
		_avoidance_active = false
		return

	var gap_ahead := _signed_loop_gap(player_target.global_position.z, global_position.z)
	var player_speed := 0.0
	var player_truck := player_target as EuroHaulTruck
	if player_truck != null:
		player_speed = player_truck.speed_kmh
	var player_is_closing := gap_ahead > 0.0 and gap_ahead < detection_distance_m and player_speed > speed_kmh + 4.0
	var traffic_is_closing_from_behind := gap_ahead < 0.0 and absf(gap_ahead) < 18.0 and speed_kmh > player_speed + 8.0

	if player_is_closing or traffic_is_closing_from_behind:
		_avoidance_active = true
		_clear_timer = 0.0
		var target_lane := 2.0 if _base_lane_x < 0.0 else -2.0
		if _has_safe_lane(target_lane):
			lane_x = target_lane
			speed_kmh = move_toward(speed_kmh, cruise_speed_kmh, 8.0 * delta)
		elif player_is_closing:
			var safe_speed := minf(cruise_speed_kmh + 12.0, maxf(20.0, player_speed - 10.0))
			speed_kmh = move_toward(speed_kmh, safe_speed, 10.0 * delta)
		else:
			speed_kmh = move_toward(speed_kmh, maxf(20.0, player_speed + 5.0), 10.0 * delta)
	elif _avoidance_active:
		_clear_timer += delta
		speed_kmh = move_toward(speed_kmh, cruise_speed_kmh, 8.0 * delta)
		if _clear_timer >= 2.0:
			_avoidance_active = false
			lane_x = _base_lane_x
	else:
		speed_kmh = move_toward(speed_kmh, cruise_speed_kmh, 8.0 * delta)

func _has_safe_lane(target_lane_x: float) -> bool:
	for neighbor in traffic_neighbors:
		if not is_instance_valid(neighbor) or neighbor == self:
			continue
		var neighbor_gap := absf(_signed_loop_gap(neighbor.global_position.z, global_position.z))
		if absf(neighbor.global_position.x - target_lane_x) < 1.5 and neighbor_gap < 18.0:
			return false
	return true

func safety_warning_for_lane(requested_lane_x: float) -> String:
	var lateral_gap := absf(global_position.x - requested_lane_x)
	if lateral_gap > 1.5:
		return ""
	if not is_instance_valid(player_target):
		return ""
	var longitudinal_gap := absf(_signed_loop_gap(player_target.global_position.z, global_position.z))
	if longitudinal_gap < 24.0:
		return "TRAFFIC IN ADJACENT LANE"
	return ""

func get_proximity_report() -> Dictionary:
	if not is_instance_valid(player_target):
		return {}
	var player_truck := player_target as EuroHaulTruck
	if player_truck == null:
		return {}
	var gap := _signed_loop_gap(player_truck.global_position.z, global_position.z)
	var distance := absf(gap)
	var player_speed: float = player_truck.speed_kmh
	if gap > 0.0 and distance <= 45.0 and player_speed > speed_kmh + 2.0:
		return {"zone": "FRONT", "distance_m": distance, "closing_kmh": player_speed - speed_kmh}
	if gap < 0.0 and distance <= 25.0 and speed_kmh > player_speed + 2.0:
		return {"zone": "REAR", "distance_m": distance, "closing_kmh": speed_kmh - player_speed}
	return {}

func _signed_loop_gap(reference_z: float, other_z: float) -> float:
	var raw_gap := reference_z - other_z
	return wrapf(raw_gap + route_length * 0.5, 0.0, route_length) - route_length * 0.5
