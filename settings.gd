extends Control

var salt = null
var waslocal = null

# Called when the node enters the scene tree for the first time.
func _ready():
	print("ready")
	showData()



# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	pass


func showData():
	var main  = get_node("/root/main")
	var paranoiaSlider  = get_node("paranoiaSlider")
	var saltText = get_node("saltText")
	var remoteServer = get_node("remoteServer/remoteServerCheck")
	var remoteAddress = get_node("remoteServer")
	
	if salt == null:
		salt = main.salt
	if waslocal == null:
		waslocal = main.remoteData
	print(main.paranoiaLevel)
	paranoiaSlider.value = main.paranoiaLevel
	saltText.text = salt


	remoteServer.set_pressed(main.remoteData)
	remoteAddress.text = main.remoteServer
	$saltText.editable = !main.remoteData

func _on_salt_gen_pressed():
	var main  = get_node("/root/main")
	salt = main.genSalt()
	showData()


func _on_ok_pressed():
	var main  = get_node("/root/main")
	var paranoiaSlider  = get_node("paranoiaSlider")
	var saltText = get_node("saltText")


	

	if (paranoiaSlider.value > main.paranoiaLevel or saltText.text != main.salt) and waslocal == main.remoteData:
		var reallyok  = get_node("ok/reallyok")
		var warninglabel = get_node("ok/reallyok/warningLabel")
		reallyok.visible = true
		get_node("ok").set_flat(true)
		if paranoiaSlider.value == 1:
			warninglabel.text = "Make sure you know what your password is because it wont be saved any more."
		if paranoiaSlider.value == 2:
			warninglabel.text = "Make sure you know both your settings and control passwords."
		if paranoiaSlider.value == 3:
			warninglabel.text = "Warning. This will remove any saved data you have about your logins."
		if saltText.text != main.salt:
			warninglabel.text = "Warning. You have changed the salt. This will basically invalidate any saved information."
	else:
		setData()
		
		var settings = get_node("/root/settings")
		get_node("/root").remove_child(settings)

func setData():
	var main  = get_node("/root/main")
	var paranoiaSlider  = get_node("paranoiaSlider")
	var saltText = get_node("saltText")
	var remoteServer = get_node("remoteServer/remoteServerCheck")
	var remoteAddress = get_node("remoteServer")
	main.setParanoiaLevel(paranoiaSlider.value)
	main.salt = saltText.text
	main.remoteData = remoteServer.is_pressed()
	main.remoteServer = remoteAddress.text
	
	
	print(remoteServer.is_pressed())
	print(main.remoteData)
	main.GetSalt(remoteServer.is_pressed())
	print("SaveEncryptedData")
	main.SaveEncryptedData()

func _on_reallyok_pressed():
	setData()
	var settings = get_node("/root/settings")
	get_node("/root").remove_child(settings)


func _on_remote_server_check_toggled(toggled_on):
	var main  = get_node("/root/main")
	main.remoteData = toggled_on
	main.GetSalt(toggled_on)
	$saltText.editable = toggled_on
	salt = main.salt
	showData()

