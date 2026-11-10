extends SceneTree

const TruckScript = preload("res://Scripts/vehicle_controller.gd")
const CareerScript = preload("res://Scripts/career_manager.gd")
const CompanyScript = preload("res://Scripts/company_manager.gd")

func _initialize() -> void:
	call_deferred("_run_tests")

func _expect(condition: bool, message: String) -> void:
	if not condition:
		push_error("LOGIC TEST FAILED: " + message)
		quit(1)

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

	var career = CareerScript.new()
	root.add_child(career)
	_expect(not career.offered_contract.is_empty(), "new career must generate a freight offer")
	_expect(not career.accept_offer(), "accept must not work twice without a new offer")
	career.active_contract = {
		"distance_km": 0.2,
		"progress_km": 0.0,
		"reward": 100,
		"xp": 100
	}
	career.offered_contract.clear()
	career.record_distance(0.2)
	_expect(career.deliveries == 1, "completed contract must count once")
	_expect(career.game_cash >= 25100, "completed contract must pay its reward")
	_expect(career.active_contract.is_empty(), "completed contract must clear")
	career.add_xp(300)
	_expect(career.level >= 3, "large XP award must process multiple level-ups")
	career.save()

	var company = CompanyScript.new()
	root.add_child(company)
	company.company_name = "QA Logistics"
	company.company_balance = 10000
	company.owned_trucks = 1
	company.hired_drivers = 1
	company._process(299.0)
	_expect(company.company_balance == 10000, "operating costs must not be charged before a simulated day")
	company._process(1.0)
	_expect(company.company_balance == 9470, "one simulated day must charge exactly one daily cost")
	company.complete_company_delivery(500)
	_expect(company.company_balance == 9970, "company delivery must credit revenue")
	company.save_company()

	print("EURO HAUL logic smoke tests passed")
	quit(0)
