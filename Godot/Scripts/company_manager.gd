extends Node
class_name EuroHaulCompanyManager

signal company_changed

const SAVE_PATH := "user://euro_haul_company.json"
const FOUNDING_COST := 50000
const TRUCK_PURCHASE_COST := 85000
const DRIVER_HIRING_COST := 5000
const SIMULATED_DAY_SECONDS := 300.0
const DRIVER_REVENUE_PER_DAY := 1800

var company_name := ""
var company_balance := 0
var owned_trucks := 0
var hired_drivers := 0
var reputation := 0
var completed_company_jobs := 0
var operating_cost_per_day := 350
var _elapsed_day_seconds := 0.0

func _ready() -> void:
	load_company()

func found_company(name: String, available_game_cash: int) -> int:
	if not company_name.is_empty() or available_game_cash < FOUNDING_COST:
		return available_game_cash
	company_name = name.strip_edges()
	if company_name.is_empty():
		company_name = "Euro Haul Logistics"
	company_balance = 100000
	company_changed.emit()
	save_company()
	return available_game_cash - FOUNDING_COST

func purchase_truck() -> bool:
	if company_name.is_empty() or company_balance < TRUCK_PURCHASE_COST:
		return false
	company_balance -= TRUCK_PURCHASE_COST
	owned_trucks += 1
	company_changed.emit()
	save_company()
	return true

func hire_driver() -> bool:
	if company_name.is_empty() or company_balance < DRIVER_HIRING_COST or hired_drivers >= owned_trucks:
		return false
	company_balance -= DRIVER_HIRING_COST
	hired_drivers += 1
	company_changed.emit()
	save_company()
	return true

func complete_company_delivery(revenue: int) -> void:
	if company_name.is_empty():
		return
	company_balance += maxi(0, revenue)
	completed_company_jobs += 1
	reputation = mini(1000, reputation + 5)
	company_changed.emit()
	save_company()

func _process(delta: float) -> void:
	if company_name.is_empty():
		return
	_elapsed_day_seconds += maxf(0.0, delta)
	if _elapsed_day_seconds < SIMULATED_DAY_SECONDS:
		return
	var elapsed_days := int(floor(_elapsed_day_seconds / SIMULATED_DAY_SECONDS))
	_elapsed_day_seconds = fmod(_elapsed_day_seconds, SIMULATED_DAY_SECONDS)
	var active_drivers := mini(hired_drivers, owned_trucks)
	var daily_cost := operating_cost_per_day * owned_trucks + hired_drivers * 180
	var delivery_revenue := active_drivers * DRIVER_REVENUE_PER_DAY * elapsed_days
	company_balance = maxi(0, company_balance + delivery_revenue - daily_cost * elapsed_days)
	if active_drivers > 0:
		completed_company_jobs += active_drivers * elapsed_days
		reputation = mini(1000, reputation + active_drivers * 2 * elapsed_days)
	company_changed.emit()
	save_company()

func save_company() -> void:
	var payload := {
		"schema": 1, "company_name": company_name, "company_balance": company_balance,
		"owned_trucks": owned_trucks, "hired_drivers": hired_drivers,
		"reputation": reputation, "completed_company_jobs": completed_company_jobs
	}
	var temp_path := SAVE_PATH + ".tmp"
	var backup_path := SAVE_PATH + ".bak"
	var file := FileAccess.open(temp_path, FileAccess.WRITE)
	if file == null:
		push_warning("Could not open company save temp file: %s" % FileAccess.get_open_error())
		return
	file.store_string(JSON.stringify(payload))
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
			push_warning("Could not preserve previous company save: %s" % error_string(backup_error))
			return
	var write_error := DirAccess.rename_absolute(temp_absolute, save_absolute)
	if write_error != OK:
		if FileAccess.file_exists(backup_path):
			DirAccess.rename_absolute(backup_absolute, save_absolute)
		push_warning("Could not replace company save: %s" % error_string(write_error))
		return
	if FileAccess.file_exists(backup_path):
		DirAccess.remove_absolute(backup_absolute)

func load_company() -> void:
	for read_path in [SAVE_PATH, SAVE_PATH + ".bak"]:
		if not FileAccess.file_exists(read_path):
			continue
		var file := FileAccess.open(read_path, FileAccess.READ)
		if file == null:
			push_warning("Could not open company save candidate: %s" % read_path)
			continue
		var payload: Variant = JSON.parse_string(file.get_as_text())
		if typeof(payload) != TYPE_DICTIONARY or int(payload.get("schema", 0)) != 1:
			push_warning("Company save candidate is invalid or uses an unsupported schema: %s" % read_path)
			continue
		company_name = str(payload.get("company_name", ""))
		company_balance = maxi(0, int(payload.get("company_balance", 0)))
		owned_trucks = maxi(0, int(payload.get("owned_trucks", 0)))
		hired_drivers = maxi(0, mini(owned_trucks, int(payload.get("hired_drivers", 0))))
		reputation = clampi(int(payload.get("reputation", 0)), 0, 1000)
		completed_company_jobs = maxi(0, int(payload.get("completed_company_jobs", 0)))
		return
