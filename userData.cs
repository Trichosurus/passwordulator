using Godot;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class userData
{
	public string id {get;set;}
	public string password {get;set;}
	// public string password {get;set;}
	public List<PwService> services {get; set;}


	public userData()
	{
	   services = new List<PwService>();
	}

	public PwService findService(string name, string login) 
	{
		foreach (PwService service in services) {
			if (service.name == name && service.login == login) {
				return service;
			}
		}

		return null;
	}

	public PwService findServiceById(string id) 
	{	
		if (id == null) {return null;}
		foreach (PwService service in services) {
			if (service.id.ToString() == id) {
				return service;
			}
		}

		return null;
	}


}

