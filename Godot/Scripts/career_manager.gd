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

func _ready() -> void:
	load_save()
	if offered_contract.is_empty() and active_contract.is_empty():
		generate_offer()
	changed.emit()

func xp_for_next_level() -> int:
	return 100 + (level - 1) * 75

func add_xp(amount: int) -> void:
	xp += maxi(0, amount)
	while xp >= xp_for_next_level():
		xp -= xp_for_next_level()
		level += 1

func generate_offer() -> void:
	var from_index := randi_range(0, CITY_NAMES.size() - 1)
	var to_index := (from_index + randi_range(1, CITY_NAMES.size() - 1)) % CITY_NAMES.size()
	var distance := randi_range(180, 250)
	offered_contract = {
		"id": "EH-%06d" % randi_range(100000, 999999),
		"origin": CITY_NAMES[from_index],
		"destination": CITY_NAMES[to_index],
		"cargo": CARGO_NAMES.pick_random(),
		"distance_km": distance,
		"progress_km": 0.0,
		"reward": int(distance * randf_range(1.2, 1.8)),
		"xp": int(distance * 0.12)
	}
	changed.emit()

func accept_offer() -> bool:
	if not active_contract.is_empty() or offered_contract.is_empty():
		return false
	active_contract = offered_contract.duplicate(true)
	offered_contract.clear()
	save()
	changed.emit()
	return true

func record_distance(distance_km: float) -> void:
	if distance_km <= 0.0:
		return
	total_distance_km += distance_km
	if active_contract.is_empty():
		return
	active_contract.progress_km = minf(active_contract.distance_km, active_contract.progress_km + distance_km)
	if active_contract.progress_km >= active_contract.distance_km:
		game_cash += int(active_contract.reward)
		add_xp(int(active_contract.xp))
		deliveries += 1
		active_contract.clear()
		generate_offer()
		save()
	changed.emit()

func save() -> void:
	var data := {
		"schema": 1, "driver_name": driver_name, "level": level, "xp": xp,
		"game_cash": game_cash, "deliveries": deliveries, "total_distance_km": total_distance_km,
		"active_contract": active_contract, "offered_contract": offered_contract
	}
	var file := FileAccess.open(SAVE_PATH + ".tmp", FileAccess.WRITE)
	if file == null:
		return
	file.store_string(JSON.stringify(data))
	file.close()
	if FileAccess.file_exists(SAVE_PATH):
		DirAccess.remove_absolute(ProjectSettings.globalize_path(SAVE_PATH))
	DirAccess.rename_absolute(ProjectSettings.globalize_path(SAVE_PATH + ".tmp"), ProjectSettings.globalize_path(SAVE_PATH))

func load_save() -> void:
	if not FileAccess.file_exists(SAVE_PATH):
		return
	var file := FileAccess.open(SAVE_PATH, FileAccess.READ)
	if file == null:
		return
	var parsed: Variant = JSON.parse_string(file.get_as_text())
	if typeof(parsed) != TYPE_DICTIONARY or int(parsed.get("schema", 0)) != 1:
		return
	driver_name = str(parsed.get("driver_name", driver_name))
	level = maxi(1, int(parsed.get("level", 1)))
	xp = maxi(0, int(parsed.get("xp", 0)))
	game_cash = maxi(0, int(parsed.get("game_cash", 25000)))
	deliveries = maxi(0, int(parsed.get("deliveries", 0)))
	total_distance_km = maxf(0.0, float(parsed.get("total_distance_km", 0.0)))
	active_contract = parsed.get("active_contract", {})
	offered_contract = parsed.get("offered_contract", {})
