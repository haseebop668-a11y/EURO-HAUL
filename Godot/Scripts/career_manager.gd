extends Node
class_name EuroHaulCareer

signal changed

const SAVE_PATH := "user://euro_haul_career.json"
const CITY_NAMES := ["Berlin", "Hamburg", "Munich", "Prague", "Vienna", "Amsterdam", "Brussels"]
const CARGO_NAMES := ["Packaged food", "Building materials", "Auto parts", "Medical supplies", "Furniture"]

var driver_name := "New Driver"
var level := 1
var xp := 0
var game_cash := 25000
var deliveries := 0
var total_distance_km := 0.0
var active_contract: Dictionary = {}
var offered_contract: Dictionary = {}
var cargo_loaded := false

func _ready() -> void:
	load_save()
	if offered_contract.is_empty() and active_contract.is_empty():
		generate_offer()
	changed.emit()

func xp_for_next_level() -> int:
	return 100 + (level - 1) * 75

func spend_cash(amount: int) -> bool:
	if amount < 0 or game_cash < amount:
		return false
	game_cash -= amount
	save()
	changed.emit()
	return true

func add_xp(amount: int) -> void:
	xp += maxi(0, amount)
	while xp >= xp_for_next_level():
		xp -= xp_for_next_level()
		level += 1

func generate_offer() -> void:
	var from_index := randi_range(0, CITY_NAMES.size() - 1)
	var to_index := (from_index + randi_range(1, CITY_NAMES.size() - 1)) % CITY_NAMES.size()
	var distance := randf_range(2.0, 5.0)
	cargo_loaded = false
	offered_contract = {
		"id": "EH-%06d" % randi_range(100000, 999999),
		"origin": CITY_NAMES[from_index],
		"destination": CITY_NAMES[to_index],
		"cargo": CARGO_NAMES.pick_random(),
		"distance_km": distance,
		"progress_km": 0.0,
		"reward": int(distance * 1000.0 * randf_range(1.2, 1.8)),
		"xp": maxi(100, int(distance * 100.0))
	}
	changed.emit()

func accept_offer() -> bool:
	if not active_contract.is_empty() or offered_contract.is_empty():
		return false
	active_contract = offered_contract.duplicate(true)
	offered_contract.clear()
	cargo_loaded = false
	save()
	changed.emit()
	return true

func load_cargo() -> bool:
	if active_contract.is_empty() or cargo_loaded:
		return false
	cargo_loaded = true
	save()
	changed.emit()
	return true

func unload_cargo() -> bool:
	if active_contract.is_empty() or not cargo_loaded:
		return false
	if float(active_contract.get("progress_km", 0.0)) < float(active_contract.get("distance_km", INF)):
		return false
	game_cash += int(active_contract.get("reward", 0))
	add_xp(int(active_contract.get("xp", 0)))
	deliveries += 1
	active_contract.clear()
	cargo_loaded = false
	generate_offer()
	save()
	changed.emit()
	return true

func record_distance(distance_km: float) -> void:
	if distance_km <= 0.0:
		return
	total_distance_km += distance_km
	if active_contract.is_empty() or not cargo_loaded:
		return
	active_contract.progress_km = minf(active_contract.distance_km, active_contract.progress_km + distance_km)

func _sanitize_contract(value: Variant) -> Dictionary:
	if typeof(value) != TYPE_DICTIONARY:
		return {}
	var distance_value: Variant = value.get("distance_km", null)
	var progress_value: Variant = value.get("progress_km", 0.0)
	var reward_value: Variant = value.get("reward", null)
	var xp_value: Variant = value.get("xp", null)
	if (typeof(distance_value) != TYPE_FLOAT and typeof(distance_value) != TYPE_INT) or (typeof(progress_value) != TYPE_FLOAT and typeof(progress_value) != TYPE_INT):
		return {}
	if (typeof(reward_value) != TYPE_INT and typeof(reward_value) != TYPE_FLOAT) or (typeof(xp_value) != TYPE_INT and typeof(xp_value) != TYPE_FLOAT):
		return {}
	var distance := float(distance_value)
	var progress := float(progress_value)
	if not is_finite(distance) or not is_finite(progress) or distance <= 0.0:
		return {}
	var contract_id := str(value.get("id", ""))
	var origin := str(value.get("origin", ""))
	var destination := str(value.get("destination", ""))
	var cargo := str(value.get("cargo", ""))
	if contract_id.is_empty() or origin.is_empty() or destination.is_empty() or cargo.is_empty():
		return {}
	return {
		"id": contract_id,
		"origin": origin,
		"destination": destination,
		"cargo": cargo,
		"distance_km": distance,
		"progress_km": clampf(progress, 0.0, distance),
		"reward": maxi(0, int(reward_value)),
		"xp": maxi(0, int(xp_value))
	}

func save() -> void:
	var data := {
		"schema": 1, "driver_name": driver_name, "level": level, "xp": xp,
		"game_cash": game_cash, "deliveries": deliveries, "total_distance_km": total_distance_km,
		"active_contract": active_contract, "offered_contract": offered_contract, "cargo_loaded": cargo_loaded
	}
	var temp_path := SAVE_PATH + ".tmp"
	var backup_path := SAVE_PATH + ".bak"
	var file := FileAccess.open(temp_path, FileAccess.WRITE)
	if file == null:
		push_warning("Could not open career save temp file: %s" % FileAccess.get_open_error())
		return
	file.store_string(JSON.stringify(data))
	file.flush()
	file.close()

	var save_absolute := ProjectSettings.globalize_path(SAVE_PATH)
	var temp_absolute := ProjectSettings.globalize_path(temp_path)
	var backup_absolute := ProjectSettings.globalize_path(backup_path)
	if FileAccess.file_exists(SAVE_PATH):
		if FileAccess.file_exists(backup_path):
			DirAccess.remove_absolute(backup_absolute)
		var backup_error := DirAccess.rename_absolute(save_absolute, backup_absolute)
		if backup_error != OK:
			DirAccess.remove_absolute(temp_absolute)
			push_warning("Could not preserve previous career save: %s" % error_string(backup_error))
			return
	var write_error := DirAccess.rename_absolute(temp_absolute, save_absolute)
	if write_error != OK:
		if FileAccess.file_exists(backup_path):
			DirAccess.rename_absolute(backup_absolute, save_absolute)
		push_warning("Could not replace career save: %s" % error_string(write_error))
		return
	if FileAccess.file_exists(backup_path):
		DirAccess.remove_absolute(backup_absolute)

func load_save() -> void:
	for read_path in [SAVE_PATH, SAVE_PATH + ".bak"]:
		if not FileAccess.file_exists(read_path):
			continue
		var file := FileAccess.open(read_path, FileAccess.READ)
		if file == null:
			push_warning("Could not open career save candidate: %s" % read_path)
			continue
		var parsed: Variant = JSON.parse_string(file.get_as_text())
		if typeof(parsed) != TYPE_DICTIONARY or int(parsed.get("schema", 0)) != 1:
			push_warning("Career save candidate is invalid or uses an unsupported schema: %s" % read_path)
			continue
		driver_name = str(parsed.get("driver_name", driver_name))
		level = maxi(1, int(parsed.get("level", 1)))
		xp = maxi(0, int(parsed.get("xp", 0)))
		game_cash = maxi(0, int(parsed.get("game_cash", 25000)))
		deliveries = maxi(0, int(parsed.get("deliveries", 0)))
		total_distance_km = maxf(0.0, float(parsed.get("total_distance_km", 0.0)))
		var active_value: Variant = parsed.get("active_contract", {})
		var offered_value: Variant = parsed.get("offered_contract", {})
		active_contract = _sanitize_contract(active_value)
		offered_contract = _sanitize_contract(offered_value)
		cargo_loaded = bool(parsed.get("cargo_loaded", false)) and not active_contract.is_empty()
		if read_path != SAVE_PATH:
			DirAccess.remove_absolute(ProjectSettings.globalize_path(SAVE_PATH))
			save()
		return
