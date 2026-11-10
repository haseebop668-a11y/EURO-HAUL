extends CharacterBody3D
class_name EuroHaulTruck

signal telemetry_changed(speed_kmh: float, rpm: int, gear: int)
signal horn_pressed
signal indicators_changed(value: int)

@export var max_forward_kmh := 90.0
@export var max_reverse_kmh := 15.0
@export var max_steer_degrees := 27.0
@export var wheelbase_m := 4.2
@export var max_lateral_accel := 2.2
@export var engine_max_torque_nm := 1100.0
@export var idle_rpm := 650.0
@export var redline_rpm := 2400.0
@export var final_drive := 3.7
@export var gear_ratios: Array[float] = [7.2, 4.8, 3.2, 2.2, 1.55, 1.0, 0.78]

var throttle_input := 0.0
var steer_input := 0.0
var brake_input := 0.0
var clutch_input := 0.0
var gear := 1
var engine_rpm := 650.0
var speed_kmh := 0.0
var damage := 0.0
var fuel_percent := 100.0
var _yaw_rate := 0.0
var _horn_cooldown := 0.0
var _impact_cooldown := 0.0
var indicators := 0

func set_controls(throttle: float, steering: float, braking: float) -> void:
	throttle_input = clampf(throttle, -1.0, 1.0)
	steer_input = clampf(steering, -1.0, 1.0)
	brake_input = clampf(braking, 0.0, 1.0)

func set_clutch(value: float) -> void:
	clutch_input = clampf(value, 0.0, 1.0)

func shift_up() -> bool:
	if clutch_input < 0.6 or gear >= gear_ratios.size():
		return false
	gear += 1
	return true

func shift_down() -> bool:
	if clutch_input < 0.6 or gear <= -1:
		return false
	if gear == 0 and absf(speed_kmh) > 3.0:
		return false
	gear -= 1
	return true

func select_neutral() -> void:
	if clutch_input >= 0.6:
		gear = 0

func select_reverse() -> bool:
	if clutch_input < 0.6 or absf(speed_kmh) > 3.0:
		return false
	gear = -1
	return true

func set_indicator(side: int) -> void:
	indicators = clampi(side, -1, 1)
	indicators_changed.emit(indicators)

func toggle_hazards() -> void:
	indicators = 2 if indicators != 2 else 0
	indicators_changed.emit(indicators)

func sound_horn() -> void:
	horn_pressed.emit()

func consume_fuel(distance_km: float) -> void:
	if distance_km > 0.0:
		var consumption := distance_km * 0.45 * (1.0 + absf(throttle_input) * 0.8)
		fuel_percent = maxf(0.0, fuel_percent - consumption)

func refuel() -> void:
	fuel_percent = 100.0

func repair() -> void:
	damage = 0.0

func apply_damage(amount: float) -> void:
	damage = clampf(damage + maxf(0.0, amount), 0.0, 100.0)
	if damage >= 80.0:
		set_controls(0.0, 0.0, 1.0)

func _physics_process(delta: float) -> void:
	var forward := -global_basis.z
	var signed_speed := velocity.dot(forward)
	speed_kmh = signed_speed * 3.6
	var ratio := _current_ratio()
	var engine_load := clampf(absf(throttle_input), 0.0, 1.0)
	var free_rpm := lerpf(idle_rpm, redline_rpm, engine_load)
	var coupled_rpm := absf(speed_kmh) / 3.6 / (2.0 * PI * 0.52) * 60.0 * absf(ratio * final_drive)
	var target_rpm := free_rpm if gear == 0 or clutch_input > 0.85 else clampf(maxf(idle_rpm, coupled_rpm), idle_rpm, redline_rpm)
	engine_rpm = move_toward(engine_rpm, target_rpm, 4200.0 * delta)
	var torque_factor := 1.0 - 0.22 * pow(clampf((engine_rpm - idle_rpm) / maxf(1.0, redline_rpm - idle_rpm), 0.0, 1.0), 2.0)
	var drive_accel := 0.0
	if gear != 0 and clutch_input < 0.95 and damage < 80.0 and fuel_percent > 0.0:
		drive_accel = throttle_input * engine_load * torque_factor * (engine_max_torque_nm / 1100.0) * absf(ratio) * final_drive * 0.045 * (1.0 - clutch_input)
		if gear < 0:
			drive_accel = -absf(drive_accel)
		elif throttle_input < 0.0:
			drive_accel = 0.0
	var longitudinal := signed_speed
	longitudinal += drive_accel * delta
	if brake_input > 0.0:
		longitudinal = move_toward(longitudinal, 0.0, 14.0 * brake_input * delta)
	else:
		longitudinal = move_toward(longitudinal, 0.0, (0.18 + absf(longitudinal) * 0.025) * delta)
	longitudinal = clampf(longitudinal, -max_reverse_kmh / 3.6, max_forward_kmh / 3.6)
	var safe_angle := rad_to_deg(atan((max_lateral_accel * wheelbase_m) / maxf(0.1, longitudinal * longitudinal)))
	var steer_angle := deg_to_rad(clampf(steer_input * max_steer_degrees, -minf(max_steer_degrees, safe_angle), minf(max_steer_degrees, safe_angle)))
	var yaw_rate := 0.0 if absf(longitudinal) < 0.15 else longitudinal / wheelbase_m * tan(steer_angle)
	rotation.y += yaw_rate * delta
	var target_velocity := -global_basis.z * longitudinal
	velocity.x = target_velocity.x
	velocity.z = target_velocity.z
	velocity.y -= 9.8 * delta
	move_and_slide()
	_impact_cooldown = maxf(0.0, _impact_cooldown - delta)
	var side_impact := false
	for collision_index in range(get_slide_collision_count()):
		var hit := get_slide_collision(collision_index)
		if absf(hit.get_normal().y) < 0.65:
			side_impact = true
			break
	if side_impact and absf(speed_kmh) > 12.0 and _impact_cooldown <= 0.0:
		apply_damage(minf(8.0, absf(speed_kmh) * 0.035))
		_impact_cooldown = 0.65
	telemetry_changed.emit(absf(speed_kmh), int(engine_rpm), gear)

func _current_ratio() -> float:
	if gear == -1:
		return -6.5
	if gear == 0:
		return 0.0
	return gear_ratios[clampi(gear - 1, 0, gear_ratios.size() - 1)]
