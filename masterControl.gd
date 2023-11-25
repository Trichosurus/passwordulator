extends Control

class_name masterControl
# Called when the node enters the scene tree for the first time.
func _ready():
	#var main = get_node("/root/main")
	#self.text = main.controlPass
	pass


# Called every frame. 'delta' is the elapsed time since the previous frame.
# func _process(delta):
# 	pass


func adjust_for_paranoia():
	if get_node("/root/main").paranoiaLevel == 0: #access pass is saved in plain text and encryption pass is stored in user settings
		get_node("../widgetsControl/paranoiaLabel").text = "Paranoia Level: I save my passwords in a text file."
	if get_node("/root/main").paranoiaLevel == 1: #encryption pass is saved in user settings but access pass is not saved
		get_node("../widgetsControl/paranoiaLabel").text = "Paranoia Level: Normal Human."
	if get_node("/root/main").paranoiaLevel == 2: #encryptopn pass is not stored in user settings but login data is
		get_node("../widgetsControl/paranoiaLabel").text = "Paranoia Level: They are out to get me."
	if get_node("/root/main").paranoiaLevel == 3: # no user data is stored at all
		get_node("../widgetsControl/paranoiaLabel").text = "Paranoia Level: I am being targeted by a nation state."

	if get_node("/root/main").paranoiaLevel < 3:
		get_node("settingsPW").editable = true
		get_node("username").editable = true
		# get_node("settingsPW").text = ""
		# get_node("/root/main/accountList").clear();
		get_node("/root/main/accountList").set_self_modulate(Color(1,1,1,1))
		get_node("/root/main/accountList/addService").set_disabled(false)
		get_node("/root/main/accountList/deleteService").set_disabled(false)
		# $controlPW.text = ""
	else:
		get_node("settingsPW").editable = false
		get_node("settingsPW").text = ""
		get_node("username").editable = false
		get_node("username").text = ""

		get_node("/root/main/accountList").clear();
		get_node("/root/main/accountList").set_self_modulate(Color(1,1,1,0.5))
		get_node("/root/main/accountList/addService").set_disabled(true)
		get_node("/root/main/accountList/deleteService").set_disabled(true)
		_on_settings_pw_text_changed("")
	

func _on_settings_pw_text_changed(new_text):
	$username/usernameTimer.start()
	#var main = get_node("/root/main")
	#var username = $username.text
	#if !$editSettingsPW.button_pressed:
		#main.NewSettingsPassword(username,  new_text)

func _on_username_text_changed(new_text):
	$username/usernameTimer.start()
	
func _on_username_timer_timeout():
	var main = get_node("/root/main")
	var settings_pw = $settingsPW.text
	var username = $username.text
	if !$editSettingsPW.button_pressed:
		main.NewSettingsPassword(username, settings_pw)
		
		


func _on_control_pw_text_changed(new_text):
	$controlPW/controlPWTimer.start()
		
func _on_control_pw_timer_timeout():
	var main = get_node("/root/main")
	main.SetUserPassword($controlPW.text)

func _on_main_refresh_data():
	
	var main = get_node("/root/main")
	var controlPW = $controlPW
	var cp = controlPW.caret_column
	
	controlPW.text = main.controlPass
	controlPW.caret_column = cp
	
	#var settings_user_pass = main.settingsPass.split("::")
	#print(settings_user_pass)
	#if len(settings_user_pass) > 1:
	cp = $settingsPW.caret_column
	$settingsPW.text = main.settingsPass
	$settingsPW.caret_column = cp
	
	cp = $username.caret_column
	$username.text = main.username
	$username.caret_column = cp
	
	if main.controlPass != '':
		$editSettingsPW.disabled = false
	else:
		$editSettingsPW.disabled = true
		$editSettingsPW.button_pressed = false


func _on_settings_pw_focus_exited():
	$editSettingsPW.button_pressed = false
	var main = get_node("/root/main")
	var username = $username.text	
	var password = $settingsPW.text
	if !$editSettingsPW.button_pressed:
		print("newpass?")
		main.SetSettingsPassword(username, password)
	else:
		main.NewSettingsPassword(username,  password)


func _on_PW_button_toggled(toggled_on):
	$controlPW.secret = toggled_on
	$settingsPW.secret = toggled_on
		


