extends Node
class_name EuroHaulWorldEnvironmentController

var environment: Environment
var sun_light: DirectionalLight3D
var time_of_day_hours := 9.0
var weather_state := "CLEAR"
var _weather_elapsed := 0.0

const WEATHER_STATES := ["CLEAR", "CLOUDY", "FOG"]

func _process(delta: float) -> void:
	time_of_day_hours = fmod(time_of_day_hours + maxf(0.0, delta) / 120.0, 24.0)
	_weather_elapsed += maxf(0.0, delta)
	if _weather_elapsed >= 300.0:
		var periods := int(floor(_weather_elapsed / 300.0))
		_weather_elapsed = fmod(_weather_elapsed, 300.0)
		var current_index := WEATHER_STATES.find(weather_state)
		weather_state = WEATHER_STATES[posmod(current_index + periods, WEATHER_STATES.size())]
	_apply_environment()

func _apply_environment() -> void:
	if environment == null:
		return
	var daylight := clampf(sin((time_of_day_hours - 6.0) / 12.0 * PI), 0.0, 1.0)
	var day_color := Color("#8fb6d5")
	var night_color := Color("#07111f")
	environment.background_color = night_color.lerp(day_color, daylight)
	environment.ambient_light_energy = lerpf(0.16, 0.82, daylight)
	environment.ambient_light_color = Color("#7188a5").lerp(Color("#b7c9d8"), daylight)
	environment.fog_enabled = weather_state != "CLEAR"
	environment.fog_density = 0.012 if weather_state == "FOG" else (0.004 if weather_state == "CLOUDY" else 0.0)
	if sun_light != null:
		sun_light.rotation_degrees = Vector3(lerpf(-70.0, 35.0, daylight), 30.0, 0.0)
		sun_light.light_energy = lerpf(0.04, 1.15, daylight)
		sun_light.light_color = Color("#8b9fc0").lerp(Color("#fff0d2"), daylight)

func display_status() -> String:
	var hours := int(floor(time_of_day_hours))
	var minutes := int(floor((time_of_day_hours - float(hours)) * 60.0))
	return "%02d:%02d • %s" % [hours, minutes, weather_state]
