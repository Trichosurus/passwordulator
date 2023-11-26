using Godot;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Collections.Generic;
using Newtonsoft.Json;
using Zanaptak.BinaryToTextEncoding;
using System.Net.Http.Headers;
using System.Net.Http;
// using System.IO;

public partial class main : Control
{
	public System.Net.Http.HttpClient remoteClient = new(); 
	public bool serverConnection = false;

	public userData user {get;set;}
	public int paranoiaLevel {get;set;}
	public string salt {get;set;}

	public string controlPass {get;set;}

	public string settingsPass {get;set;}
	public string username {get;set;}

	public bool remoteData {get; set;} = true;
	public string remoteServer {get; set;} = "passwords.acinonyx.me";

	[Signal]
	public delegate void refreshDataEventHandler();

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{

		LoadSettings();
		var saltlable = GetNode<Label>("widgetsControl/saltLabel");
		saltlable.Text = "Salt: " + salt;

		DisplayServer.WindowSetMinSize(new Vector2I(800,600));
		if (DisplayServer.ScreenGetDpi(DisplayServer.WindowGetCurrentScreen()) > 150) {
			var window = GetNode<Window>("/root");
			window.ContentScaleFactor = 2;
			DisplayServer.WindowSetMinSize(new Vector2I(1600,1200));
			DisplayServer.WindowSetSize(DisplayServer.WindowGetSize()*2);
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	// public override void _Process(double delta)
	// {
	// }


	public void LoadSettings()
	{

		int defParanoia = 1;
		string defSettingsPass = "";
		string defUsername = "";
		bool foundParanoia = false;
		bool foundPass = false; 
		bool foundUser = false; 
		// remoteData = true;

		// bool loadRemote = remoteData;

		string settings = "";
		if (FileAccess.FileExists("user://settings")) {
			using var settingsFile = FileAccess.Open("user://settings", FileAccess.ModeFlags.Read);
			settings = settingsFile.GetAsText();
		}


		// string controlPass = null;
		string[] sl = settings.Split('\n');
		remoteData = false;
		foreach (string s in sl) {
			if (s.StartsWith("settingsPass=")) {
				foundPass = true;
				settingsPass = s.Replace("settingsPass=","");
				GetNode<LineEdit>("masterControl/settingsPW").Text = settingsPass;

			}
			if (s.StartsWith("paranoiaLevel=")) {
				string pn = s.Replace("paranoiaLevel=","");
				int pl;
				foundParanoia = int.TryParse(pn, out pl);
				if (foundParanoia) {setParanoiaLevel(pl);}
			}
			if (s.StartsWith("remoteData=True")) {
				remoteData = true;
			}
			if (s.StartsWith("remoteServer=")) {
				remoteServer = s.Replace("remoteServer=","");
			}

			if (s.StartsWith("username=")) {
				LineEdit usernameEdit = GetNode<LineEdit>("masterControl/username");
				username = s.Replace("username=","");
				usernameEdit.Text = username;
				foundUser = true;
			}



		}

		if (remoteData == true && remoteServer != "") {
			// LoadSettings();
			remoteClient.BaseAddress = new Uri("https://" + remoteServer);

			GetSalt(true);
		} else {	
			GetSalt(false);
		}


		if (!foundParanoia) {
			setParanoiaLevel(defParanoia);
			if (!foundPass) {
				settingsPass = defSettingsPass;
			}
			if (!foundUser) {
				username = defUsername;
			}
		}

		if (paranoiaLevel == 0) {
			LoadEncryptedData(username + "::" + settingsPass);

		}

	}
	public void SaveSettings()
	{
		using var settingsFile = FileAccess.Open("user://settings", FileAccess.ModeFlags.WriteRead);

		// GD.Print(paranoiaLevel);
		// GD.Print("rd");
		// GD.Print(remoteData);
		// GD.Print("rd");

		settingsFile.StoreLine("paranoiaLevel="+paranoiaLevel.ToString());
		if (remoteData) {
			settingsFile.StoreLine("remoteData=True");
		} else {
			settingsFile.StoreLine("remoteData=False");
		}
		settingsFile.StoreLine("remoteServer="+remoteServer);
		if (paranoiaLevel == 0) {
			settingsPass = GetNode<LineEdit>("masterControl/settingsPW").Text;
			settingsFile.StoreLine("settingsPass="+settingsPass);
			
		}
		if (paranoiaLevel < 2 ) {
			var username = GetNode<LineEdit>("masterControl/username").Text;
			settingsFile.StoreLine("username="+username);
		}
		
	}


	public async void LoadEncryptedData(string password)
	{
		GD.Print("LoadEncryptedData");
		GD.Print(password);
		GD.Print("salt");
		GD.Print(salt);
		
		password = password + salt;
		GD.Print(password);
		PwService ps = new PwService();
		// string pw = Base91.Default.Encode(ps.ComputeSha256Hash(password));
		string pw = ps.CalcPWHash(password, true);
		GD.Print(pw);

		string encData = "";

		user = new userData();
		user.id = pw;

		if (!remoteData || remoteClient.BaseAddress == null) {
			if (!FileAccess.FileExists("user://"+pw) ) {
				GD.Print("no File " + pw);
				EmitSignal(SignalName.refreshData);
				return;
			}
			using  var encFile = FileAccess.Open("user://"+pw, FileAccess.ModeFlags.Read);
			encData = encFile.GetAsText();
			DecryptData(encData, password, pw);
		} else if (remoteData && serverConnection) {
			GD.Print("LoadREMOTEEncryptedData");
			string json = "";
			var response = await remoteClient.GetAsync("/user/" +pw );
			if (response.IsSuccessStatusCode) {
				json = await response.Content.ReadAsStringAsync();
				GD.Print("success");
				GD.Print(json);

				EncryptdObj userObj = JsonConvert.DeserializeObject<EncryptdObj>(json);
				encData = userObj.Data;
				
				EmitSignal(SignalName.refreshData);
				DecryptData(encData, password, pw);
			} else {
				GD.Print("fail");
				GD.Print(response);
				EmitSignal(SignalName.refreshData);
				return;
			}


		}



		// return true;
	}

	public void DecryptData(string encData, string password, string pw) {
		GD.Print("decrypting with salt",salt);
		try {
			if (encData != "") {
				encData = Decrypt(encData, password);
			}		
		// user = new userData();
			user = JsonConvert.DeserializeObject<userData>(encData);
			if (user == null) {
				user = new userData();
				user.id = pw;
			}
		} catch {
			GD.Print("decode fail. probably wrong hash");
			user = new userData();
			user.id = pw;
		}

		controlPass = user.password;
		EmitSignal(SignalName.refreshData);

	}
	public async void SaveEncryptedData()
	{
		if (user == null) {return;}

		GD.Print("saving" + "user://" + user.id );
		

		string userPass = user.password;
		GD.Print(userPass);

		if (paranoiaLevel > 1) {
			user.password = "";
		}
		string data = JsonConvert.SerializeObject(user);
		GD.Print("username");
		GD.Print(username);
		GD.Print("settingsPass");
		GD.Print(settingsPass);
		data = Encrypt(data, username + "::" + settingsPass + salt);
		// GD.Print(data);

		if (remoteData && remoteClient.BaseAddress != null && serverConnection) {
				EncryptdObj ud = new();
				ud.Name = user.id;
				ud.Data = data;
				var usercontent = JsonConvert.SerializeObject(ud);
				var buffer = Encoding.UTF8.GetBytes(usercontent);
				var byteContent = new System.Net.Http.ByteArrayContent(buffer);
				byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

				var post = await remoteClient.PostAsync(
        			 "user/",
					 byteContent
					 );

				GD.Print(post);

				// SaltObj s = JsonConvert.DeserializeObject<SaltObj>(json);
				// salt = s.Salt;
				// GD.Print(salt);
				// EmitSignal(SignalName.refreshData);
		} else {
			using var encFile = FileAccess.Open("user://" + user.id, FileAccess.ModeFlags.WriteRead);
			encFile.StoreString(data);
		}
		user.password = userPass;
		GD.Print("saved" + "user://" + user.id );

		SaveSettings();

	}




	public async void GetSalt(bool remote = false)
	{
		string saltdata = "";
		
		if (remote) {
				GD.Print("remotesalt");
				remoteClient = new();
				remoteClient.BaseAddress = new Uri("https://" + remoteServer);

				HttpResponseMessage response;
				GD.Print("getsalt?");
				GD.Print(remoteClient.BaseAddress);
				try {
					response = await remoteClient.GetAsync("/salt" );
					GD.Print(response);
				} catch {
					GD.Print("CONNFAIL?");
					serverConnection = false;
					EmitSignal(SignalName.refreshData);
					return;
				}
				// GD.Print("responded?");
				if (response.IsSuccessStatusCode) {
					serverConnection = true;
					var json = await response.Content.ReadAsStringAsync();

					SaltObj s = JsonConvert.DeserializeObject<SaltObj>(json);
					salt = s.Salt;
					GD.Print("GOTSALT",salt);
					LoadEncryptedData(username + "::" + settingsPass);
				}
				EmitSignal(SignalName.refreshData);

		} else {
			GD.Print("localsalt");

			if (FileAccess.FileExists("res://salt")) {
				using var saltFile = FileAccess.Open("res://salt", FileAccess.ModeFlags.ReadWrite);
				saltdata = saltFile.GetAsText();
			}
		

			if (saltdata == "") {
				GD.Print("No salt. Generating");
				saveSalt(genSalt());
			} else {
				salt = saltdata;
			}
			EmitSignal(SignalName.refreshData);
		}

	}

	public string genSalt() {
		StringBuilder builder = new StringBuilder();
		Random random = new Random();
		char ch;
		for (int i = 0; i < 32; i++)
		{
			ch = Convert.ToChar(Convert.ToInt32(Math.Floor(94 * random.NextDouble() + 32)));                 
			builder.Append(ch);
		}

		return builder.ToString();
	}

	public void saveSalt(string saltdata)
	{
		if (saltdata == "") {GetSalt();}

		salt = saltdata;
		
		using (var saltFile = FileAccess.Open("res://salt", FileAccess.ModeFlags.WriteRead)) {
			saltFile.StoreString(salt);
		}
	}

	public void setParanoiaLevel(int paranoia)
	{
		paranoiaLevel = paranoia;
		
		var mc = GetNode<Control>("/root/main/masterControl");    
		mc.Call("adjust_for_paranoia");
	}

	public string Encrypt(string data, string password)  
	{  
		GD.Print("encrypt password");
		GD.Print(password);

		byte[] encrypted;

		using (var crypt = Aes.Create()) {

			PwService pw = new();
			byte[] key = pw.ComputeSha256Hash(password);
			GD.Print(key.ToString());
			GD.Print(key.Length);
			GD.Print(crypt.KeySize);
			crypt.KeySize = 256;
			crypt.BlockSize = 128;

			
			crypt.Key = key;
			byte[] iv  = new byte[16];

			for (int i = 0; i < 16; i++)
			{
				iv[i] = key[key.Length - i - 1];
			}
			crypt.IV = iv;
			crypt.Padding = PaddingMode.PKCS7;
			ICryptoTransform encriptor = crypt.CreateEncryptor(crypt.Key, crypt.IV);


			using (System.IO.MemoryStream ms = new System.IO.MemoryStream()) {
				using (CryptoStream cs = new CryptoStream(ms, encriptor, CryptoStreamMode.Write)) {
					using (System.IO.StreamWriter sw = new System.IO.StreamWriter(cs)) {
						sw.Write(data);
					}
					encrypted = ms.ToArray();
				}
			
			}
		}  
		return Base91.Default.Encode(encrypted);

	}  


	public string Decrypt(string data, string password)  
	{  
		string decrypted = "";

		GD.Print(data);
		GD.Print("decrypt salt");
		GD.Print(salt);
		GD.Print("decrypt password");
		GD.Print(password);
		using (var crypt = Aes.Create()) {
			PwService pw = new PwService();
			byte[] bytes = Base91.Default.Decode(data);
			crypt.KeySize = 256;
			crypt.BlockSize = 128;
			crypt.Key = pw.ComputeSha256Hash(password);
			byte[] iv  = new byte[16];

			for (int i = 0; i < 16; i++)
			{
				iv[i] = crypt.Key[crypt.Key.Length - i - 1];
			}
			crypt.IV = iv;

			crypt.Padding = PaddingMode.PKCS7;
			ICryptoTransform decriptor = crypt.CreateDecryptor(crypt.Key, crypt.IV);


			using (System.IO.MemoryStream ms = new System.IO.MemoryStream(bytes)) {
				using (CryptoStream cs = new CryptoStream(ms, decriptor, CryptoStreamMode.Read)) {
					using (System.IO.StreamReader sr = new System.IO.StreamReader(cs)) {
						decrypted = sr.ReadToEnd();
					}
				}
			
			}
		}  
		return decrypted;
	}  

	public void SetUserPassword (string newpass) 
	{
		if (user is null) {
			user = new userData();
		}
		user.password = newpass;
		controlPass = user.password;
		GD.Print(user.password);
		EmitSignal(SignalName.refreshData);
		if (paranoiaLevel < 3){
			SaveEncryptedData();
		}
	}

	public void SetSettingsPassword (string newusername, string newpass) 
	{
		GD.Print("setSettingsPassword");
		GD.Print(newpass);

		string pw = new PwService().CalcPWHash(newusername + "::" + newpass + salt, true);
		settingsPass = newpass;
		username = newusername;
		// using var dir = DirAccess.Open("user://");
		// var del = dir.Remove("user://"+user.id);
		DeleteUserData();
		user.id = pw;
		GD.Print(user.id);
		if (paranoiaLevel < 3){
			SaveEncryptedData();
		}

	}

	public async void DeleteUserData() 
	{
		if (remoteData && remoteClient.BaseAddress != null && serverConnection) {
			EncryptdObj ud = new();
			ud.Name = user.id;
			ud.Data = "";
			var usercontent = JsonConvert.SerializeObject(ud);
			var buffer = Encoding.UTF8.GetBytes(usercontent);
			var byteContent = new System.Net.Http.ByteArrayContent(buffer);
			byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

			var post = await remoteClient.PostAsync(
					"user/",
					byteContent
					);

			GD.Print("DELETE?");
			GD.Print(post);
		} else {
			if (FileAccess.FileExists("user://" + user.id)) {
				using var dir = DirAccess.Open("user://");
				var del = dir.Remove("user://"+user.id);
			}
		}
	}

	public async void GetRemoteShame(PwService pws) {
		GD.Print("remoteshame?",pws.name);

		if (remoteData && remoteClient.BaseAddress != null && serverConnection) {
			var response = await remoteClient.GetAsync("/ShameList/" + pws.name );
			if (response.IsSuccessStatusCode) {
				var json = await response.Content.ReadAsStringAsync();

				ShameObj s = JsonConvert.DeserializeObject<ShameObj>(json);

				pws.SetShameData(s);
				GD.Print("GOTSHAME",pws.name);
			}
			EmitSignal(SignalName.refreshData);

		}
	}

	public void NewSettingsPassword (string newusername, string newpass) 
	{
		GD.Print("newSettingsPassword");
		GD.Print(newpass);
		settingsPass = newpass;
		username = newusername;
		controlPass = "";
		LoadEncryptedData(newusername + "::" + newpass);
		if (paranoiaLevel < 2) {controlPass = user.password;}
		
		EmitSignal(SignalName.refreshData); 

	}


}
struct SaltObj {
	public string Salt;
}

struct EncryptdObj {
	public string Name;
	public string Data;
}

#nullable enable
public struct ShameObj {
    public int? MaxLen {get;set;}
    public string? Include {get;set;}
    public string? Exclude {get;set;}
}