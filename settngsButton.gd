extends Button


# Called when the node enters the scene tree for the first time.
func _ready():
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	pass


func _on_settngs_button_pressed():
	var settingsScene = preload("res://settings.tscn").instantiate()
	get_tree().get_root().add_child(settingsScene)
