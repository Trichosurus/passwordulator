using Godot;
using System;
using System.Collections.Generic;

public partial class accountList : ItemList
{
	main main;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		main = GetNode<main>("/root/main");

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void OnItemSelected(int index) 
	{
		string selId = GetItemMetadata(index).ToString();
		GetNode<widgetsControl>("/root/main/widgetsControl").RefreshData(selId);
	}

	public void RefreshData()
	{
		if (main.remoteData && main.remoteClient.BaseAddress != null && main.serverConnection == false) {
			GD.Print("No connection?");
			main.GetSalt(true);
		}
		
		var saltlable = GetNode<Label>("/root/main/widgetsControl/saltLabel");
		if (main.remoteData &&!main.serverConnection) {
			saltlable.Text = "CANNOT CONNECT TO SERVER";
			GetNode<widgetsControl>("/root/main/widgetsControl").RefreshData(null);
			Clear();
			return;
		}
		
		saltlable.Text = "Salt: " + main.salt;

		GD.Print("refreshData");
		if (main.user is null) {
			main.user = new();
		}
		GD.Print(main.user.password);

		int[] sel = GetSelectedItems();
		string selId = "";
		if(sel.Length > 0) {
			selId = GetItemMetadata(sel[0]).ToString();
		}
		GD.Print(selId);

		if (selId == "") {GetNode<widgetsControl>("/root/main/widgetsControl").RefreshData(null);}

		Clear();

		foreach (var service in main.user.services) {
			GD.Print("name");
					GD.Print(service.name);

			AddItem(service.name + " - " + service.login);
			SetItemMetadata(ItemCount-1, service.id.ToString());
			if (service.id.ToString() == selId) {
				Select(ItemCount-1);
				GetNode<widgetsControl>("/root/main/widgetsControl").RefreshData(selId);
			}
		}
		SortItemsByText();

	}


	public void SelectByID(string id) 
	{
		for (int i = 0; i < ItemCount; i++) {
			if (GetItemMetadata(i).ToString() == id) {
				Select(i);
				RefreshData();
				break;
			}
		}
	}
		 

	public void OnAddButtonPressed() 
	{
		PwService pws = new PwService();
		GD.Print(pws.name);
		GD.Print(pws.id.ToString());
		main.user.services.Add(pws);
		if (!main.remoteData || main.remoteClient.BaseAddress == null) {
			pws.FindShameData();
		} else {
			main.GetRemoteShame(pws);
		}
		this.RefreshData();
		SelectByID(pws.id.ToString());
		
	}

	public void OnDeleteServicePressed()
	{
		int[] sel = GetSelectedItems();
		string selId = "";
		if(sel.Length > 0) {
			selId = GetItemMetadata(sel[0]).ToString();
		}
		GD.Print(selId);
		var item = main.user.findServiceById(selId);

		if (selId != "") {
			main.user.services.Remove(item);
			RefreshData();
		}

	}


}
