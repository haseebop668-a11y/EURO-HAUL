extends Node3D

const SKY_COLOR := Color("#07111f")
const GROUND_COLOR := Color("#101d2c")

func _ready() -> void:
	_setup_environment()
	print("EURO HAUL Godot runtime initialized")

func _setup_environment() -> void:
	var environment := Environment.new()
	environment.background_mode = Environment.BG_COLOR
	environment.background_color = SKY_COLOR
	environment.ambient_light_source = Environment.AMBIENT_SOURCE_COLOR
	environment.ambient_light_color = Color("#9db4ca")
	environment.ambient_light_energy = 0.55
	var world := get_node_or_null("WorldEnvironment") as WorldEnvironment
	if world:
		world.environment = environment
