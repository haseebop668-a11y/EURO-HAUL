extends SceneTree

const TruckScript = preload("res://Scripts/vehicle_controller.gd")
const CareerScript = preload("res://Scripts/career_manager.gd")
const CompanyScript = preload("res://Scripts/company_manager.gd")
const TrafficScript = preload("res://Scripts/traffic_vehicle.gd")
const TrailerScript = preload("res://Scripts/trailer_controller.gd")
const WorldCycleScript = preload("res://Scripts/world_environment_controller.gd")
const MainScript = preload("res://Scripts/main.gd")

var _failed := false

func _initialize() -> void:
	call_deferred("_run_tests")

func _expect(condition: bool, message: String) -> void:
	if not condition:
		push_error("LOGIC TEST FAILED: " + message)
		_failed = true

func _run_tests() -> void:
	var truck = TruckScript.new()
	root.add_child(truck)
	_expect(not truck.shift_up(), "gear shift must fail without clutch")
	truck.set_clutch(1.0)
	_expect(truck.shift_up(), "gear shift must succeed with clutch")
	_expect(truck.gear == 2, "upshift must select second gear")
	_expect(truck.select_reverse(), "reverse must engage with clutch at standstill")
	_expect(truck.gear == -1, "reverse gear state must be -1")
	_expect(truck._current_ratio() < 0.0, "reverse gear must use a negative transmission ratio")
	truck.set_retarder(2.0)
	_expect(is_equal_approx(truck.retarder_input, 1.0), "retarder input must clamp to 1")
	truck.set_retarder(0.0)
	truck.set_controls(2.0, -2.0, 2.0)
	_expect(is_equal_approx(truck.throttle_input, 1.0), "throttle must clamp to 1")
	_expect(is_equal_approx(truck.steer_input, -1.0), "steering must clamp to -1")
	_expect(is_equal_approx(truck.brake_input, 1.0), "brake must clamp to 1")
	truck.set_indicator(-1)
	_expect(truck.indicators == -1, "left indicator must set the correct state")
	truck.toggle_hazards()
	_expect(truck.indicators == 2, "hazards must override the single-side indicator")
	truck.toggle_hazards()
	_expect(truck.indicators == 0, "hazards must toggle off cleanly")
	truck.fuel_percent = 1.0
	truck.consume_fuel(2.0)
	_expect(is_equal_approx(truck.fuel_percent, 0.0), "fuel consumption must clamp at empty")
	truck.refuel()
	_expect(is_equal_approx(truck.fuel_percent, 100.0), "service refuelling must restore a full tank")
	truck.set_cargo_mass(35.0)
	_expect(is_equal_approx(truck.cargo_mass_tons, 30.0), "truck cargo mass must clamp to safe limits")
	truck.set_cargo_mass(0.0)
	truck.damage = 25.0
	truck.repair()
	_expect(is_equal_approx(truck.damage, 0.0), "service repair must clear vehicle damage")

	var traffic = TrafficScript.new()
	root.add_child(traffic)
	traffic.player_target = truck
	traffic.position = Vector3(-2.0, 0.0, -10.0)
	truck.position = Vector3.ZERO
	truck.speed_kmh = 70.0
	traffic.speed_kmh = 40.0
	_expect(traffic.safety_warning_for_lane(-2.0) == "TRAFFIC IN ADJACENT LANE", "lane-change sensor must warn about nearby traffic")
	var front_report := traffic.get_proximity_report()
	_expect(front_report.get("zone", "") == "FRONT", "forward sensor must identify slower traffic ahead")
	traffic.position.z = 10.0
	traffic.speed_kmh = 90.0
	var rear_report := traffic.get_proximity_report()
	_expect(rear_report.get("zone", "") == "REAR", "rear sensor must identify a faster approaching vehicle")
	traffic.route_length = 900.0
	truck.position.z = -880.0
	traffic.position.z = -20.0
	traffic.speed_kmh = 40.0
	var loop_report := traffic.get_proximity_report()
	_expect(loop_report.get("zone", "") == "FRONT", "traffic sensing must calculate gaps across the road loop seam")
	truck.position.z = 0.0
	traffic.position.z = -10.0
	traffic.speed_kmh = 40.0
	var lane_blocker = TrafficScript.new()
	root.add_child(lane_blocker)
	lane_blocker.position = Vector3(2.0, 0.0, -10.0)
	traffic.traffic_neighbors.append(lane_blocker)
	traffic._update_player_avoidance(0.1)
	_expect(is_equal_approx(traffic.lane_x, -2.0), "traffic must not change into an occupied lane")
	_expect(traffic.speed_kmh > 40.0, "traffic must adjust speed when the safe lane is blocked")
	traffic.traffic_neighbors.clear()
	traffic._update_player_avoidance(0.1)
	_expect(is_equal_approx(traffic.lane_x, 2.0), "slower traffic ahead must move away from a closing player when the lane clears")
	traffic.position.z = -100.0
	traffic._update_player_avoidance(2.1)
	_expect(is_equal_approx(traffic.lane_x, -2.0), "traffic must return to its lane after the gap clears")

	var trailer = TrailerScript.new()
	root.add_child(trailer)
	trailer.truck = truck
	trailer.set_cargo_mass(20.0)
	_expect(is_equal_approx(trailer.cargo_mass_tons, 20.0), "trailer must retain its cargo mass")
	_expect(trailer is AnimatableBody3D, "trailer must be an independent physics body")
	_expect(trailer.get_node_or_null("TrailerImpactSensor") != null, "trailer must expose an impact sensor")
	truck.gear = 1
	truck.speed_kmh = 50.0
	truck.steer_input = 1.0
	trailer._physics_process(0.1)
	var forward_articulation := trailer.articulation_degrees
	truck.gear = -1
	trailer._process(0.1)
	_expect(trailer.articulation_degrees > forward_articulation, "reverse steering must articulate the trailer in the opposite direction")
	_expect(absf(trailer.articulation_degrees) <= trailer.articulation_limit_degrees, "trailer articulation must remain within the jackknife safety limit")

	var world_cycle = WorldCycleScript.new()
	root.add_child(world_cycle)
	world_cycle.environment = Environment.new()
	world_cycle.sun_light = DirectionalLight3D.new()
	root.add_child(world_cycle.sun_light)
	world_cycle.time_of_day_hours = 6.0
	world_cycle._apply_environment()
	var night_ambient := world_cycle.environment.ambient_light_energy
	world_cycle.time_of_day_hours = 12.0
	world_cycle._apply_environment()
	_expect(world_cycle.environment.ambient_light_energy > night_ambient, "daylight cycle must increase ambient lighting at noon")
	world_cycle.weather_state = "FOG"
	world_cycle._apply_environment()
	_expect(world_cycle.environment.fog_enabled, "fog weather must enable environment fog")
	_expect(world_cycle.display_status().contains("FOG"), "world status must report current weather")
	var main_scene_logic = MainScript.new()
	var horn_stream: AudioStreamWAV = main_scene_logic._create_horn_stream()
	_expect(horn_stream.data.size() > 0, "generated horn audio must contain PCM samples")

	var career = CareerScript.new()
	root.add_child(career)
	career.driver_name = "QA Driver"
	career.level = 1
	career.xp = 0
	career.game_cash = 25000
	career.deliveries = 0
	career.total_distance_km = 0.0
	career.active_contract.clear()
	career.offered_contract.clear()
	career.generate_offer()
	_expect(career._sanitize_contract({"distance_km": "invalid"}).is_empty(), "malformed freight saves must be rejected")
	var sanitized_contract := career._sanitize_contract({"id": "QA-1", "origin": "A", "destination": "B", "cargo": "Test", "mass_tons": 35.0, "fragility": "HIGH VALUE", "damage_percent": 150.0, "distance_km": 2.0, "progress_km": 9.0, "reward": 100, "xp": 20})
	_expect(is_equal_approx(float(sanitized_contract.get("progress_km", 0.0)), 2.0), "saved freight progress must be clamped to route length")
	_expect(is_equal_approx(float(sanitized_contract.get("mass_tons", 0.0)), 30.0), "saved cargo mass must be clamped to safe bounds")
	_expect(is_equal_approx(float(sanitized_contract.get("damage_percent", 0.0)), 100.0), "saved cargo damage must be clamped to 100 percent")
	_expect(not career.offered_contract.is_empty(), "new career must generate a freight offer")
	_expect(career.accept_offer(), "new freight offer must be accepted")
	_expect(not career.accept_offer(), "second acceptance must fail while a contract is active")
	career.active_contract = {
		"distance_km": 0.2,
		"progress_km": 0.0,
		"reward": 100,
		"xp": 100
	}
	career.offered_contract.clear()
	career.record_distance(0.5)
	_expect(is_equal_approx(float(career.active_contract.progress_km), 0.0), "contract distance must not advance before pickup")
	_expect(career.load_cargo(), "active contract must allow cargo pickup")
	career.apply_cargo_damage(10.0)
	_expect(is_equal_approx(float(career.active_contract.damage_percent), 15.0), "fragile cargo must take amplified damage")
	_expect(not career.load_cargo(), "cargo must not be loaded twice")
	_expect(not career.unload_cargo(), "cargo must not unload before required route distance")
	career.record_distance(0.2, false)
	_expect(is_equal_approx(float(career.active_contract.progress_km), 0.0), "reversing must not advance freight route progress")
	career.record_distance(0.2)
	_expect(career.deliveries == 0, "contract must not auto-deliver before depot unloading")
	_expect(career.unload_cargo(), "loaded contract at full route distance must unload successfully")
	_expect(not career.unload_cargo(), "delivery reward must not be claimed twice")
	_expect(career.deliveries == 1, "completed contract must count once")
	_expect(career.game_cash == 25090, "cargo damage must reduce the delivery reward")
	_expect(career.active_contract.is_empty(), "completed contract must clear")
	career.add_xp(300)
	_expect(career.level >= 3, "large XP award must process multiple level-ups")
	career.game_cash = 43210
	career.update_vehicle_state(42.0, 12.0)
	career.save()
	var career_save_absolute := ProjectSettings.globalize_path("user://euro_haul_career.json")
	var career_backup_absolute := ProjectSettings.globalize_path("user://euro_haul_career.json.bak")
	DirAccess.copy_absolute(career_save_absolute, career_backup_absolute)
	var corrupt_career_file := FileAccess.open("user://euro_haul_career.json", FileAccess.WRITE)
	if corrupt_career_file != null:
		corrupt_career_file.store_string("{corrupt")
		corrupt_career_file.close()
	career.game_cash = 1
	career.load_save()
	_expect(career.game_cash == 43210, "career loader must recover from a corrupt primary save using its backup")
	_expect(is_equal_approx(career.saved_fuel_percent, 42.0), "career save must preserve truck fuel")
	_expect(is_equal_approx(career.saved_vehicle_damage, 12.0), "career save must preserve truck damage")

	var company = CompanyScript.new()
	root.add_child(company)
	company.company_name = ""
	company.company_balance = 0
	company.owned_trucks = 0
	company.hired_drivers = 0
	company.found_company("QA Logistics", 50000)
	_expect(company.company_balance == 100000, "new company must receive starting operating capital")
	_expect(company.purchase_truck(), "company must be able to buy a fleet truck")
	_expect(company.company_balance == 15000, "truck purchase must deduct the correct price")
	_expect(company.hire_driver(), "company must hire a driver for an owned truck")
	_expect(not company.hire_driver(), "company must not hire more drivers than owned trucks")
	company._process(299.0)
	_expect(company.company_balance == 10000, "operating costs and driver revenue must wait until a full simulated day")
	company._process(1.0)
	_expect(company.company_balance == 11270, "one simulated day must apply driver revenue and operating costs")
	_expect(company.completed_company_jobs == 1, "active hired driver must complete one simulated job per day")
	company.complete_company_delivery(500)
	_expect(company.company_balance == 11770, "company delivery must credit revenue")
	company.save_company()
	var company_save_absolute := ProjectSettings.globalize_path("user://euro_haul_company.json")
	var company_backup_absolute := ProjectSettings.globalize_path("user://euro_haul_company.json.bak")
	DirAccess.copy_absolute(company_save_absolute, company_backup_absolute)
	var corrupt_company_file := FileAccess.open("user://euro_haul_company.json", FileAccess.WRITE)
	if corrupt_company_file != null:
		corrupt_company_file.store_string("{corrupt")
		corrupt_company_file.close()
	company.company_balance = 0
	company.load_company()
	_expect(company.company_balance == 11770, "company loader must recover from a corrupt primary save using its backup")

	if _failed:
		quit(1)
		return
	print("EURO HAUL logic smoke tests passed")
	quit(0)
