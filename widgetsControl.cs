using Godot;
using System;

public partial class widgetsControl : Control
{
	main main;
	accountList accountList;
	PwService service;
	public override void _Ready()
	{
		main = GetNode<main>("/root/main");
		accountList = GetNode<accountList>("../accountList");
		service = null;

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}



	public void RefreshData(string selId) {
		if (selId != null){
			service = main.user.findServiceById(selId);
		} else {
			service = null;
		}
		LineEdit serviceName = GetNode<LineEdit>("serviceName");
		LineEdit serviceLogin = GetNode<LineEdit>("serviceLogin");
		LineEdit password = GetNode<LineEdit>("password");
		LineEdit include = GetNode<LineEdit>("include");
		LineEdit exclude = GetNode<LineEdit>("exclude");
		SpinBox iteration = GetNode<SpinBox>("iteration");
		SpinBox maxLen = GetNode<SpinBox>("maxLen");
		CheckBox fixedPw = GetNode<CheckBox>("password/fixedPassword");
		
		if (main.paranoiaLevel == 3) {
			service = new PwService {
				name = serviceName.Text,
				iteration = (int)iteration.Value,
				login = serviceLogin.Text,
				maxLen = (int)maxLen.Value,
				include = include.Text,
				exclude = exclude.Text,
			};
		}


		if (service != null) {
			int cc = serviceName.CaretColumn;
			serviceName.Text = service.name;
			serviceName.CaretColumn = cc;
			cc = serviceLogin.CaretColumn;
			serviceLogin.Text = service.login;
			serviceLogin.CaretColumn = cc;

			cc = password.CaretColumn;
			password.Text = service.GeneratePassword(main.user, main.salt);
			fixedPw.SetPressedNoSignal(false);
			if (service.fixedPassword != "") {
				fixedPw.SetPressedNoSignal(true);
				password.Text = service.fixedPassword;
				password.CaretColumn = cc;
			}

			cc = include.CaretColumn;
			include.Text = service.include;
			include.CaretColumn = cc;
			cc = exclude.CaretColumn;
			exclude.Text = service.exclude;
			exclude.CaretColumn = cc;

			iteration.Value = service.iteration;
			maxLen.Value = service.maxLen;
		} else {
				serviceName.Text = "";
				serviceLogin.Text = "";
				password.Text = "";
				include.Text = "";
				exclude.Text = "";
				iteration.Value = 1;
				maxLen.Value = 40;
			}

	}

	public void OnServiceNameTimerTimeout()
	{
		// GD.Print("OnServiceNameChange");
		// GD.Print(newText);
		if (service != null) {
			LineEdit serviceName = GetNode<LineEdit>("serviceName");
			service.name = serviceName.Text;
			// service.FindShameData();
			if (!main.remoteData || main.remoteClient.BaseAddress == null) {
				service.FindShameData();
			} else {
				main.GetRemoteShame(service);
			}
			accountList.RefreshData();
			RefreshData(service.id.ToString());
			main.SaveEncryptedData();
		}

	}

	public void OnServiceNameTextChanged(string newText) 
	{
		Timer t = GetNode<Timer>("serviceName/serviceNameTimer");
		t.Start();
	}			

	
	public void OnServiceLoginTextChanged(string newText) {
		Timer t = GetNode<Timer>("serviceLogin/serviceLoginTimer");
		t.Start();
	}

	public void OnServiceLoginTimerTimeout()
	{
		if (service!= null) {
			LineEdit serviceLogin = GetNode<LineEdit>("serviceLogin");
			service.login = serviceLogin.Text;
			accountList.RefreshData();
			main.SaveEncryptedData();

		}
	}			


	public void OnIterationValueChanged(int value) {
		if (service!= null) {
			service.iteration = value;
			RefreshData(service.id.ToString());
			main.SaveEncryptedData();

		}
	}
	
	public void OnMaxLenValueChanged(int value) {
		if (service!= null) {
			service.maxLen = value;
			RefreshData(service.id.ToString());
			main.SaveEncryptedData();

		}
	}		
	

	public void OnPWTextChanged(string newText) {
		GD.Print("pwtext");
		if (service!= null) {
			CheckBox fixedPw = GetNode<CheckBox>("password/fixedPassword");
			if (fixedPw.ButtonPressed) {
				service.fixedPassword = newText;
				RefreshData(service.id.ToString());
				main.SaveEncryptedData();
			}
		}
	}
	
	public void OnFixedPasswordToggled(bool pressed) {
		GD.Print("pw clicked");

		if (service!= null) {
			LineEdit password = GetNode<LineEdit>("password");
			if (pressed) {
				password.Editable = true;
				OnPWTextChanged("New Password");
			} else {
				password.Editable = false;
				service.fixedPassword = "";
			}
			RefreshData(service.id.ToString());

		}

	}

	
	public void OnIncludeTextChanged(string newText) 
	{
		Timer t = GetNode<Timer>("include/includeTimer");
		t.Start();
	}
	
	public void OnIncludeTimerTimeout() 
	{
		if (service!= null) {
			LineEdit include = GetNode<LineEdit>("include");
			service.include = include.Text;
			RefreshData(service.id.ToString());
			main.SaveEncryptedData();
			CheckIncludeBoxes(include.Text);

		}
	}

	public void OnExcludeTextChanged(string newText) 
	{
		Timer t = GetNode<Timer>("exclude/excludeTimer");
		t.Start();
	}
	
	public void OnExcludeTimerTimeout() 
	{
		DoExcludes();
	}


	public void OnPWVisibilityPressed() 
	{
		LineEdit password = GetNode<LineEdit>("password");
		password.Secret =! password.Secret;

	}

	public void DoExcludes() 
	{
		LineEdit excludes = GetNode<LineEdit>("exclude");
		if (service!= null) {
			service.exclude = excludes.Text;
			RefreshData(service.id.ToString());
			main.SaveEncryptedData();
		}


	}

	public void DoIncludes() 
	{
		LineEdit include = GetNode<LineEdit>("include");
		CheckBox incNums = GetNode<CheckBox>("include/incNums");
		CheckBox incCaps = GetNode<CheckBox>("include/incCaps");
		CheckBox incLower = GetNode<CheckBox>("include/incLower");
		CheckBox incSymbols = GetNode<CheckBox>("include/incSymbols");

		if (incNums.ButtonPressed) {
			if (! include.Text.Contains("[0-9]")) {
				include.Text += " [0-9]";
			}
		} else {
			include.Text = include.Text.Replace("[0-9]","");
		}

		if (incCaps.ButtonPressed) {
			if (! include.Text.Contains("[A-Z]")) {
				include.Text += " [A-Z]";
			} 
		} else {
			include.Text = include.Text.Replace("[A-Z]", "");
		}

		if (incLower.ButtonPressed) {
			if (! include.Text.Contains("[a-z]")) {
				include.Text += " [a-z]";
			} 
		} else {
			include.Text = include.Text.Replace("[a-z]", "");
		}

		if (incSymbols.ButtonPressed) {
			if (! include.Text.Contains("[!@#$%^&*()<>?/\\\\_+]")) {
				include.Text += " [!@#$%^&*()<>?/\\\\_+]";
			} 
		} else {
			include.Text = include.Text.Replace("[!@#$%^&*()<>?/\\\\_+]","");
		}

	   	include.Text = include.Text.Replace("  "," ");

		if (service!= null) {
			service.include = include.Text;
			RefreshData(service.id.ToString());
			main.SaveEncryptedData();
			CheckIncludeBoxes(include.Text);

		}
	

	}

	public void OnCopyToClipboardPressed() 
	{
		LineEdit password = GetNode<LineEdit>("password");
		bool isSecret = password.Secret;
		password.Secret = false;
		password.SelectAll();
		password.MenuOption(1);//copy
		password.Secret = isSecret;
		// password.Select(-1);
		password.Deselect();


	}
	public void CheckIncludeBoxes(string newText) {
		CheckBox incNums = GetNode<CheckBox>("include/incNums");
		CheckBox incCaps = GetNode<CheckBox>("include/incCaps");
		CheckBox incLower = GetNode<CheckBox>("include/incLower");
		CheckBox incSymbols = GetNode<CheckBox>("include/incSymbols");
		GD.Print(newText.Contains("[0-9]"));
		incNums.ButtonPressed = newText.Contains("[0-9]");
		incCaps.ButtonPressed = newText.Contains("[A-Z]");
		incLower.ButtonPressed = newText.Contains("[a-z]");
		incSymbols.ButtonPressed = newText.Contains("[!@#$%^&*()<>?/\\\\_+]");
		
	}

}