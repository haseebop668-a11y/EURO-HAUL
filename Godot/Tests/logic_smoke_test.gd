extends SceneTree

const TruckScript = preload("res://Scripts/vehicle_controller.gd")
const CareerScript = preload("res://Scripts/career_manager.gd")
const CompanyScript = preload("res://Scripts/company_manager.gd")
const TrafficScript = preload("res://Scripts/traffic_vehicle.gd")
const TrailerScript = preload("res://Scripts/trailer_controller.gd")

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
	truck.set_controls(2.0, -2.0, 2.0)
	_expect(is_equal_approx(truck.throttle_input, 1.0), "throttle must clamp to 1")
	_expect(is_equal_approx(truck.steer_input, -1.0), "steering must clamp to -1")
	_expect(is_equal_approx(truck.brake_input, 1.0), "brake must clamp to 1")

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
	traffic.position.z = -10.0
	traffic.speed_kmh = 40.0
	traffic._update_player_avoidance(0.1)
	_expect(is_equal_approx(traffic.lane_x, 2.0), "slower traffic ahead must move away from a closing player")
	traffic.position.z = -100.0
	traffic._update_player_avoidance(2.1)
	_expect(is_equal_approx(traffic.lane_x, -2.0), "traffic must return to its lane after the gap clears")

	var trailer = TrailerScript.new()
	root.add_child(trailer)
	trailer.truck = truck
	truck.gear = 1
	truck.speed_kmh = 50.0
	truck.steer_input = 1.0
	trailer._process(0.1)
	var forward_articulation := trailer.articulation_degrees
	truck.gear = -1
	trailer._process(0.1)
	_expect(trailer.articulation_degrees > forward_articulation, "reverse steering must articulate the trailer in the opposite direction")
	_expect(absf(trailer.articulation_degrees) <= trailer.articulation_limit_degrees, "trailer articulation must remain within the jackknife safety limit")

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
	_expect(career.load_cargo(), "active contract must allow cargo pickup")
	_expect(not career.load_cargo(), "cargo must not be loaded twice")
	_expect(not career.unload_cargo(), "cargo must not unload before required route distance")
	career.record_distance(0.2)
	_expect(career.deliveries == 0, "contract must not auto-deliver before depot unloading")
	_expect(career.unload_cargo(), "loaded contract at full route distance must unload successfully")
	_expect(not career.unload_cargo(), "delivery reward must not be claimed twice")
	_expect(career.deliveries == 1, "completed contract must count once")
	_expect(career.game_cash >= 25100, "completed contract must pay its reward")
	_expect(career.active_contract.is_empty(), "completed contract must clear")
	career.add_xp(300)
	_expect(career.level >= 3, "large XP award must process multiple level-ups")
	career.save()

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

	if _failed:
		quit(1)
		return
	print("EURO HAUL logic smoke tests passed")
	quit(0)
