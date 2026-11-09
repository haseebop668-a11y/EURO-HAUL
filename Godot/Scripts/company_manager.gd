extends Node
class_name EuroHaulCompanyManager

signal company_changed

const SAVE_PATH := "user://euro_haul_company.json"
const FOUNDING_COST := 50000
const TRUCK_PURCHASE_COST := 85000
const DRIVER_HIRING_COST := 5000

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
	company_balance = 10000
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
	if company_name.is_empty() or company_balance < DRIVER_HIRING_COST:
		return false
	company_balance -= DRIVER_HIRING_COST
	hired_drivers += 1
	company_changed.emit()
	save_company()
	return true

func complete_company_delivery(revenue: int) -> void:
	if company_name.is_empty():
		return
	var safe_revenue := maxi(0, revenue)
	company_balance += safe_revenue
	completed_company_jobs += 1
	reputation = mini(1000, reputation + 5)
	company_changed.emit()
	save_company()

func _process(delta: float) -> void:
	if company_name.is_empty():
		return
	_elapsed_day_seconds += delta
	if _elapsed_day_seconds >= 300.0:
		_elapsed_day_seconds = 0.0
	var daily_cost := operating_cost_per_day * owned_trucks + hired_drivers * 180
	company_balance = maxi(0, company_balance - daily_cost)
	company_changed.emit()
	save_company()

func save_company() -> void:
	var payload := {
		"schema": 1, "company_name": company_name, "company_balance": company_balance,
		"owned_trucks": owned_trucks, "hired_drivers": hired_drivers,
		"reputation": reputation, "completed_company_jobs": completed_company_jobs
	}
	var file := FileAccess.open(SAVE_PATH, FileAccess.WRITE)
	if file == null:
		return
	file.store_string(JSON.stringify(payload))
	file.close()

func load_company() -> void:
	if not FileAccess.file_exists(SAVE_PATH):
		return
	var file := FileAccess.open(SAVE_PATH, FileAccess.READ)
	if file == null:
		return
	var payload: Variant = JSON.parse_string(file.get_as_text())
	if typeof(payload) != TYPE_DICTIONARY or int(payload.get("schema", 0)) != 1:
		return
	company_name = str(payload.get("company_name", ""))
	company_balance = maxi(0, int(payload.get("company_balance", 0)))
	owned_trucks = maxi(0, int(payload.get("owned_trucks", 0)))
	hired_drivers = maxi(0, int(payload.get("hired_drivers", 0)))
	reputation = clampi(int(payload.get("reputation", 0)), 0, 1000)
	completed_company_jobs = maxi(0, int(payload.get("completed_company_jobs", 0)))
