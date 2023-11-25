using Godot;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Collections.Generic;
using Newtonsoft.Json;

using System.Text.RegularExpressions;
using Zanaptak.BinaryToTextEncoding;


public class PwService 
	{
		public string name {get;set;}
		public string login {get;set;}
		public int maxLen {get;set;}
		public int iteration {get;set;}
		public string include {get;set;}
		public string fixedPassword {get;set;}
		public string exclude {get;set;}
		public string algorithm {get;set;}
		public Guid id {get;} 
		public PwService(string _name = "new serivce")
		{
			name = _name;
			iteration = 1;
			algorithm = "sha256";
			login = "";
			maxLen = 42;
			include = "";
			exclude = "";
			fixedPassword = "";
			id = Guid.NewGuid();
		}

		public string GeneratePassword(userData user, string salt) 
		{
			string pw = CalcPWHash(user.password + name + login + iteration.ToString(), false, algorithm);

			foreach (char c in exclude) {
				pw = pw.Replace(c.ToString(),"");
			}

			string characters = "";
			for (int i = 33; i < 127; i++) {
				characters += (char)i;
			}

			if (maxLen > 0  && maxLen < pw.Length) {pw = pw.Substring(0,maxLen);}


			string[] inc = include.Split(" ");
			foreach (string s in inc) {
				try {
				Regex rx = new Regex(s);
				if (!rx.IsMatch(pw)) {
						MatchCollection matches = Regex.Matches(characters, s);
						if (matches.Count > 0) {pw = matches[0] + pw;}
				}
				} catch {
					return "";
					//notify user that include is bad somehow
				}

			}
			if (maxLen > 0 && maxLen < pw.Length) {pw = pw.Substring(0,maxLen);}
			
			if (fixedPassword != "") {
				pw = fixedPassword;
			}
			return pw;
		}
		
		public string CalcPWHash(string password, bool fsSafe = false, string hashType = "sha256")
		{
			// var hash = "";
			byte[] hash = new byte [0];
			// if (hashType == "md5") {hash = ComputeMd5Hash(password);}
			if (hashType == "sha256") {hash = ComputeSha256Hash(password);}

			if (fsSafe) {

				string filename = CalcPWHash(Base91.Default.Encode(hash));
				return Base46.Default.Encode(Base91.Default.Decode(filename));

				
			} else {
				return Base91.Default.Encode(hash);
			}

		}

		// public byte[] ComputeMd5Hash (string data) 
		// {
		//     MD5 md5Hash = new MD5CryptoServiceProvider();
		//     byte[] ba = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(data));
		//     return ba;
		// }

		public byte[] ComputeSha256Hash(string data)  
		{  
			// Create a SHA256   
			using (SHA256 sha256Hash = SHA256.Create())  
			{  
				byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(data));  
				return bytes;
			}  
		}  

		public void SetShameData (ShameObj shame) 
		{	
			if (shame.MaxLen != null) {maxLen = (int)shame.MaxLen;} else {maxLen = 42;}
			include = shame.Include;
			exclude = shame.Exclude;

		}

		public void FindShameData () 
		{
			if (FileAccess.FileExists("res://shamelist/"+name)) {
				GD.Print("Found Shame Data");
				using (var sahmeFile = FileAccess.Open("res://shamelist/"+name, FileAccess.ModeFlags.Read)) {
					try {
						shameData shame = JsonConvert.DeserializeObject<shameData>(sahmeFile.GetAsText());
						maxLen = shame.maxLen;
						include = shame.include;
						exclude = shame.exclude;
					} catch {
						GD.Print("Corruped shame data for " + name);
					}
				}
			}
		}

		private struct shameData {
			public int maxLen;
			public string include;
			public string exclude;
		}

	}
