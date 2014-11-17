using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class ZilaGUI : MonoBehaviour {
	Project trenutni = null;
	
	List<float> omjeri = new List<float>();
	public Vector2 velicinaOkvira = new Vector2(0.6f, 0.45f);

	public Texture2D bgTex;
	public Texture2D bgOverlay;
	public Texture2D dot;

	public Texture2D pictureLeft;
	public Texture2D pictureRight;

	bool crtajTrakt1 = true;
	float eraseSize = 10f;

	public float zoom1 = 1f;
	public float zoom2 = 1f;
	Vector2 gridOffset1 = new Vector2(0f, 0f);
	Vector2 gridOffset2 = new Vector2(0f, 0f);

	FileInfo[] fileInfo = new FileInfo[0]; 

	int markerToEdit = -1;
	Oznaka activeMarker;

	void Awake(){
		Load();
		trenutni = null;
	}
	void Update(){
		HandleTrenutni();
	}

	void OnGUI(){
		if(trenutni ==null) return;

		if(Event.current.type == EventType.mouseUp)
			Save(trenutni.name);

		if(Event.current.type == EventType.mouseDrag && Event.current.button == 2){
			if(crtajTrakt1) gridOffset1 += Event.current.delta;
			else gridOffset2 += Event.current.delta;
		}

		UpdateActiveMarker();
		BackgroundGUI();

		Rect okvir = new Rect();
		Vector2 velOkvira = new Vector2(velicinaOkvira.x* Screen.width, velicinaOkvira.y *Screen.height);
		okvir.size = velOkvira;
		Vector2 _scr = new Vector2(Screen.width, Screen.height);
		okvir.position = (_scr -velOkvira)/2f;
		okvir.y -= okvir.height/2f;

		Rect lijevi = okvir; 	lijevi.width *= 0.45f;
		Rect srednji = lijevi;	srednji.width = okvir.width*0.1f;	srednji.x += lijevi.width;
		Rect desni = lijevi;	desni.position = new Vector2 (desni.position.x +desni.width + okvir.width*0.1f, desni.position.y);
		Rect oznake = srednji;	oznake.width *= 1.5f; oznake.x = okvir.x -oznake.width -1;
		Rect load = okvir; 		load.x += load.width;	load.width = srednji.width*3f;
		Rect slike = okvir;		slike.y += slike.height;
		Rect imgMenu = oznake;	imgMenu.y += imgMenu.height+1;
		Rect inicijali = imgMenu;	inicijali.y += inicijali.height; inicijali.height = 40f; inicijali.width = 1000f;

		//if(Event.current.type == EventType.mouseDown && Event.current.button == 2) {
		if(pictureLeft && crtajTrakt1){
			float diffW = slike.width -pictureLeft.width;
			float diffH = slike.height -pictureLeft.height;
			if(diffW > 0) diffW = 0f;
			if(diffH > 0) diffH = 0f;

			gridOffset1.x = Mathf.Clamp(gridOffset1.x, diffW, 0f);
			gridOffset1.y = Mathf.Clamp(gridOffset1.y, diffH, 0f);
		}
		if(pictureRight && crtajTrakt1 ==false){
			float diffW = slike.width -pictureRight.width;
			float diffH = slike.height -pictureRight.height;
			if(diffW > 0) diffW = 0f;
			if(diffH > 0) diffH = 0f;
		
			gridOffset2.x = Mathf.Clamp(gridOffset2.x, diffW, 0f);
			gridOffset2.y = Mathf.Clamp(gridOffset2.y, diffH, 0f);
		} 

		//}
	
		GUI.Box(okvir, "");
		{
			GUILayout.BeginArea(okvir);
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			/*if(trenutni.uPreimenovanju){
				if (Event.current.type == EventType.keyDown && Event.current.keyCode == KeyCode.Return){
					trenutni.uPreimenovanju = false;
				}
				trenutni.name = GUILayout.TextField(trenutni.name, GUI.skin.box, GUILayout.MinWidth(150f));
			}
			else 
			*/if(GUILayout.Button(trenutni.name, GUILayout.MinWidth(150f))){
				trenutni.uPreimenovanju = true;
			}
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			GUILayout.EndArea();
			GUILayout.BeginArea(lijevi);
			GUILayout.Label("");
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.Label("BPC157");
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			ZileGUI(trenutni.trakt1);
			GUILayout.EndArea();
			
			GUILayout.BeginArea(srednji);
			OmjerGUI(omjeri);
			GUILayout.EndArea();
			
			GUILayout.BeginArea(desni);
			GUILayout.Label("");
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.Label("Kontrola");
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			ZileGUI(trenutni.trakt2);
			GUILayout.EndArea();
		}

		GUI.Box(imgMenu, "");
		{
			GUILayout.BeginArea(imgMenu);
			GUILayout.BeginVertical();
			{
				if(crtajTrakt1){
					GUILayout.Box("BPC157");
					if(GUILayout.Button("Kontrola"))
					   crtajTrakt1 = false;

				}
				else{
					if (GUILayout.Button("BPC157"))
						crtajTrakt1 = true;
					GUILayout.Box("Kontrola");
				}
			}
			GUILayout.Label("");
			GUILayout.Label("");
			GUILayout.Label("");
			if(GUILayout.Button("Discard")){
				if(crtajTrakt1)
					pictureLeft = null;
				else pictureRight = null;
			}
			GUILayout.EndVertical();
			GUILayout.EndArea();
		}
		
		GUI.Box(slike, "");
		{
			GUILayout.BeginArea(slike);
			GUILayout.BeginHorizontal();

			if(pictureLeft ==null && crtajTrakt1){
				trenutni.pictureLeftPath = GUILayout.TextField(trenutni.pictureLeftPath, GUILayout.Width(300));
				if (Event.current.type == EventType.keyDown && Event.current.keyCode == KeyCode.Return ||
				    GUILayout.Button("DRIVE:/path/to/{ BPC157 }.jpg")){
					pictureLeft = LoadImage(trenutni.pictureLeftPath);
				}

			}
			else if(pictureRight ==null && crtajTrakt1 ==false){
				trenutni.pictureRightPath = GUILayout.TextField(trenutni.pictureRightPath, GUILayout.Width(300));
				if (Event.current.type == EventType.keyDown && Event.current.keyCode == KeyCode.Return ||
				    GUILayout.Button("DRIVE:/path/to/{ Kontrola }.jpg")){
					pictureRight = LoadImage(trenutni.pictureRightPath);
				}
			}
			if(pictureLeft && crtajTrakt1){
				slike.width = pictureLeft.width;
				slike.height = pictureLeft.height;
				GUI.DrawTexture(new Rect(0f +gridOffset1.x, 0f +gridOffset1.y, pictureLeft.width, pictureLeft.height), pictureLeft);
			}
			else{
				if(pictureRight && crtajTrakt1 ==false){
					slike.width = pictureRight.width;
					slike.height = pictureRight.height;
					GUI.DrawTexture(new Rect(0f +gridOffset2.x, 0f +gridOffset2.y, pictureRight.width, pictureRight.height), pictureRight);
				}
			}

			if(Event.current.type == EventType.mouseDown && Event.current.button == 0){
				if(activeMarker !=null){
					if(activeMarker.notHidden ==false) activeMarker.notHidden = true;
					Vector2 offset = gridOffset2;
					if(crtajTrakt1) offset = gridOffset1;
					activeMarker.pojave.Add(Event.current.mousePosition - offset);
				}
			}

			if(Event.current.isMouse && Event.current.button == 1){
				Rect aroundMouse = new Rect(Event.current.mousePosition.x, Event.current.mousePosition.y, eraseSize, eraseSize);
				GiTrakt trakt = trenutni.trakt2;
				if(crtajTrakt1) trakt = trenutni.trakt1;
				foreach(Oznaka oznaka in trakt.zile){
					for(int i = 0; i < oznaka.pojave.Count; ++i){
						Vector2 pojava = oznaka.pojave[i];
						if(aroundMouse.Contains(pojava) && oznaka.notHidden ==true){
							oznaka.pojave.RemoveAt(i--);
						}
					}
				}
			}

			if(crtajTrakt1) NacrtajPojave(trenutni.trakt1.zile);
			else NacrtajPojave(trenutni.trakt2.zile);
			GUILayout.EndHorizontal();
			GUILayout.EndArea();
		}

		GUI.Box(oznake, "");
		{
			GUILayout.BeginArea(oznake);
			if(markerToEdit > -1) MarkerEditGUI(markerToEdit);
			else OznakeGUI(trenutni.trakt1, trenutni.trakt2);
			GUILayout.EndArea();
		}

		GUI.Box(load, "");
		{
			GUILayout.BeginArea(load);

			RefreshStorageFileInfo();
			bool existsInFolder = false;
			foreach (FileInfo f in fileInfo){
				GUIStyle style = GUI.skin.button;
				if(f.Name == trenutni.name){
					style = GUI.skin.box;
					existsInFolder = true;
				} 

				string name = f.Name.Substring(0, f.Name.Length -5);		//-6 is '.assets'
				if(GUILayout.Button(name, style)){
					if(trenutni.name == "delete"){
						trenutni = null;
						File.Delete(f.FullName);
					}
					else {
						Load(f.Name);
						pictureLeft = LoadImage(trenutni.pictureLeftPath);
						pictureRight = LoadImage(trenutni.pictureRightPath);
					}
				}
			}
			if(existsInFolder ==false){
				trenutni = null;
			}
		
			GUILayout.Label("");
			if(GUILayout.Button("novi")){
				Save();
			}
			GUILayout.EndArea();
		}
		//GUI.Box(inicijali, "");
		{
			GUILayout.BeginArea(inicijali);
			GUILayout.Label("Koordinatorica: Dora Žaler (dora.zaler@gmail.com)" +"\t\t" +"tehnička pitanja: reslav.hollos@gmail.com" +"\t\t" +"licenca: MIT");
			GUILayout.EndArea();
		}
	}

	#region funkcije
	void HandleTrenutni(){
		if(trenutni ==null){
			RefreshStorageFileInfo();
			foreach (FileInfo f in fileInfo){
				Load (f.Name);
				break;
			}
			if(trenutni ==null) Save();
			pictureLeft = LoadImage(trenutni.pictureLeftPath);
			pictureRight = LoadImage(trenutni.pictureRightPath);

		}
		else{
			if(trenutni.trakt1.zile.Count == 0){
				trenutni.trakt1.zile.Add(new Oznaka());
				trenutni.trakt2.zile.Add(new Oznaka());
			}
			//TODO solve this with one unified marker
			for(int i = 0; i < trenutni.trakt1.zile.Count; ++i){
				trenutni.trakt2.zile[i].CopyFrom(trenutni.trakt1.zile[i]);
			}
			omjeri = Omjeri(trenutni.trakt1, trenutni.trakt2);
		}
	}
	
	void RefreshStorageFileInfo(){
		string myPath = Application.streamingAssetsPath;
		if(Directory.Exists(myPath) ==false) Directory.CreateDirectory(myPath);
		DirectoryInfo dir = new DirectoryInfo(myPath);
		fileInfo = dir.GetFiles("*.zile");
	}
	
	#region non-GUI
	Texture2D LoadImage(string filePath){
		
		Texture2D tex = null;
		byte[] fileData;
		
		if (File.Exists(filePath)){
			fileData = File.ReadAllBytes(filePath);
			tex = new Texture2D(200, 200);
			tex.LoadImage(fileData); //..this will auto-resize the texture dimensions.
		}
		return tex;
	}	

	void Save(string fname = ""){
		if(trenutni == null)
			trenutni = new Project();

		trenutni.UpdateAllPojaveXY();
		string basePath =  Application.streamingAssetsPath;
		if (fname == "") fname = System.DateTime.Now.Day +"-"+System.DateTime.Now.Month +"-"+System.DateTime.Now.Year +"-" +System.DateTime.Now.Hour +"-" +System.DateTime.Now.Second +"-" +System.DateTime.Now.Millisecond +".zile";

		string fullPath = basePath +"/" +fname;
		BinaryFormatter bf = new BinaryFormatter();
		FileStream fs = File.Create (fullPath);
		bf.Serialize(fs, trenutni);
		fs.Close();
	}   
	
	void Load(string fname = "") {
		string basePath =  Application.streamingAssetsPath;
		string fullPath = basePath +"/" +fname;
		if(File.Exists(fullPath)) {
			BinaryFormatter bf = new BinaryFormatter();
			FileStream fs = File.Open(fullPath, FileMode.Open);
			trenutni = (Project)bf.Deserialize(fs);
			fs.Close();
		}

		if (trenutni !=null){
			trenutni.UpdateAllPojave();
			trenutni.name = fname;
		} 
	}

	float RoundToDecimal(float value, int dec){
		float pow = Mathf.Pow(10f, dec);
		value *= pow;
		value = Mathf.Round(value);
		return value/pow;
	}

	List<float> Omjeri(GiTrakt trakt1, GiTrakt trakt2){
		List<float> omjeri = new List<float>();
		int minCount = 0;
		if(trakt2.zile.Count > trakt1.zile.Count) minCount = trakt1.zile.Count;
		else minCount = trakt2.zile.Count;
		
		for(int i = 0; i < minCount; ++i){
			float t1 = trakt1.KvocijentPojedineSa(i);
			float t2 = trakt2.KvocijentPojedineSa(i);
			
			if(t1 != 0 && t2 != 0) omjeri.Add(t1/t2);
			else omjeri.Add(0);
		}
		return omjeri;
	}

	void NacrtajPojave(List<Oznaka> oznake){
		foreach(Oznaka oznaka in oznake){
			if(oznaka.notHidden ==false) continue;
			Color temp = GUI.color;
			GUI.color = oznaka.boja;
			foreach(Vector2 pojava in oznaka.pojave){
				Vector2 gridOffset = gridOffset2;
				if(crtajTrakt1) gridOffset = gridOffset1;
				Rect rect = new Rect(pojava.x -oznaka.scale/2f +gridOffset.x, pojava.y -oznaka.scale/2f +gridOffset.y, oznaka.scale, oznaka.scale);
				GUI.DrawTexture(rect, dot);
			}
			GUI.color = temp;
		}
	}

	void UpdateActiveMarker(){
		if(trenutni.trakt1.zile.Contains(activeMarker) ==false && trenutni.trakt2.zile.Contains(activeMarker) ==false)
			activeMarker = null;
		for(int i = 0; i < trenutni.trakt1.zile.Count; ++i){
			if(Event.current.keyCode != KeyCode.None && trenutni.trakt1.zile[i].kratica == Event.current.keyCode && Event.current.type == EventType.keyDown){
				if(crtajTrakt1)
					activeMarker = trenutni.trakt1.zile[i];	//TODO if not listening for rename
				else activeMarker = trenutni.trakt2.zile[i];
				break;
			}
			else{
				if(activeMarker == trenutni.trakt1.zile[i] && crtajTrakt1 ==false)
					activeMarker = trenutni.trakt2.zile[i];
				if(activeMarker == trenutni.trakt2.zile[i] && crtajTrakt1)
					activeMarker = trenutni.trakt1.zile[i];

			}
		}
	}

	void loadTrenutni(){

	}

	#endregion

	#region GUI
	void MarkerEditGUI(int markerIndex){
		Oznaka oznaka = trenutni.trakt1.zile[markerIndex];
		Oznaka oznaka2 = trenutni.trakt2.zile[markerIndex];

		Color temp = GUI.color;
		GUI.color = oznaka.boja;

		GUILayout.BeginVertical();
		GUILayout.Label("promjer: ");
		oznaka.scale = GUILayout.HorizontalSlider(oznaka.scale, 2f, 15f);
		GUILayout.Label("boja, r-g-b: ");
		oznaka._boja[0] = GUILayout.HorizontalSlider(oznaka._boja[0], 0f, 1f);
		oznaka._boja[1] = GUILayout.HorizontalSlider(oznaka._boja[1], 0f, 1f);
		oznaka._boja[2] = GUILayout.HorizontalSlider(oznaka._boja[2], 0f, 1f);
		if(GUILayout.Button("" + (oznaka.kraticaListen ? "press key" : "key: "+oznaka.kratica.ToString()))){
			oznaka.kraticaListen = true;
		}
		if(oznaka.kraticaListen){
			if(Event.current.type == EventType.keyDown){
				oznaka.kraticaListen = false;
				if(Event.current.keyCode != KeyCode.Escape){
					oznaka.kratica = Event.current.keyCode;
				}
			}
		}
		GUI.color = temp;
	
		GUILayout.Space(10);
		if(GUILayout.Button("ok")){
			markerToEdit = -1;
			if(crtajTrakt1) activeMarker = oznaka;
			else activeMarker = oznaka2;
		}
		GUILayout.Space(10);
		GUI.color = oznaka.boja;
		GUILayout.Label("to delete,");
		GUILayout.Label("set key to");
		if(GUILayout.Button("delete")){
			if(oznaka.kratica == KeyCode.Delete){
				trenutni.trakt1.zile.RemoveAt(markerIndex);
				trenutni.trakt2.zile.RemoveAt(markerIndex);
				markerToEdit = -1;
			}
		}
		GUI.color = temp;
		GUILayout.EndVertical();
	}

	void OznakeGUI(GiTrakt trakt1, GiTrakt trakt2){
		GUILayout.BeginVertical();
		GUILayout.Label("");	//projekt
		GUILayout.Label("");	//tip
		GUILayout.Label("");	//ime
		GUILayout.Label("");	//zbroj
		GUILayout.Label("");	//prosjek
		for(int i = 0; i < trakt1.zile.Count; ++i){
			GUILayout.BeginHorizontal();
			Color temp = GUI.color;
			GUI.color = trakt1.zile[i].boja;
			
			if(GUILayout.Button("" +((activeMarker == trakt1.zile[i] || activeMarker == trakt2.zile[i]) ? "[*] ": "") + trakt1.zile[i].kratica.ToString())){
				markerToEdit = i;
			}
			GUI.color = temp;
			GUILayout.EndHorizontal();
		}
		if(GUILayout.Button("novi")){
			trakt1.zile.Add(new Oznaka());
			trakt2.zile.Add(new Oznaka());
		}
		GUILayout.EndVertical();
	}
	
	void OmjerGUI(List<float> omjeri){
		GUILayout.BeginVertical();
		GUILayout.Label("");	//tip
		GUILayout.Label("");	//ime
		GUILayout.Label("");	//zbroj
		GUILayout.Label("");	//prosjek
		foreach(float o in omjeri){
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			if(o != 0) GUILayout.Label("" +RoundToDecimal(o, 5));
			else GUILayout.Label("null");
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
		}
		GUILayout.EndVertical();
	}

	void BackgroundGUI(){
		if(bgTex){
			Rect scr = new Rect(0f, 0f, Screen.width, Screen.height);
			GUI.DrawTexture(scr, bgTex);
		}
		if(bgOverlay){
			Color temp = GUI.color;
			GUI.color = new Color(temp.r, temp.g, temp.b, 0.85f);
			bgOverlay.wrapMode = TextureWrapMode.Repeat;
			Rect screenBounds = new Rect(0,0, Screen.width, Screen.height);
			GUI.DrawTextureWithTexCoords(screenBounds, bgOverlay, new Rect(0, 0, screenBounds.width / bgOverlay.width, screenBounds.height / bgOverlay.height));
			GUI.color = temp;
		}
	}

	void ZileGUI(GiTrakt trakt){		
		GUILayout.BeginVertical();
		{
			int suma = trakt.Zbroj();
			//float prosjek = trakt.KvocijentZbrojaZilaSa();
	
			if (Event.current.type == EventType.keyDown && Event.current.keyCode == KeyCode.Return){
				trakt.uPreimenovanju = false;
			}
			if(trakt.uPreimenovanju) trakt.ime = GUILayout.TextField(trakt.ime);
			else if(GUILayout.Button("" +trakt.ime)){
				trakt.uPreimenovanju = true;
			}
			GUILayout.Label("zbroj: "+suma);

			GUILayout.BeginHorizontal();
			{
				GUILayout.Label("prosjek: " +suma + "/" +trakt.sa +" = " +trakt.KvocijentZbrojaZilaSa(), GUILayout.Width(120));
				
				trakt.sa = (int)GUILayout.HorizontalSlider(trakt.sa, 1f, 5f);
				GUILayout.Label("" +trakt.sa);
			}
			GUILayout.EndHorizontal();
			
			for (int i = 0; i < trakt.zile.Count; ++i){
				int brojPojava = trakt.zile[i].pojave.Count;

				GUILayout.BeginHorizontal();
				trakt.zile[i].notHidden = GUILayout.Toggle(trakt.zile[i].notHidden, "", GUILayout.Width(30));
				GUILayout.Box("" +brojPojava);
				GUILayout.Label("/" + trakt.sa +" = " +(float)brojPojava/trakt.sa);
				GUILayout.EndHorizontal();
			}
		}
		GUILayout.EndVertical();
	}
	#endregion
	#endregion
	
}
[System.Serializable]
public class GiTrakt{
	public string ime = "neimenovaniTrakt";
	public bool uPreimenovanju = false;		//TODO make global temp <string,bool> dictionary instead
	public List<Oznaka> zile = new List<Oznaka>();
	public int sa = 1;

	public GiTrakt(){}

	public 	int Zbroj(){
		int rez = 0;
		foreach (Oznaka zila in zile){
			rez += zila.pojave.Count;
		}
		return rez;
	}

	public float KvocijentPojedineSa(int i){
		return (float)zile[i].pojave.Count/sa;
	}
	public float KvocijentZbrojaZilaSa(){
		float rez = 0;
		foreach (Oznaka zila in zile){
			rez += zila.pojave.Count;
		}
		return (float)rez/sa;
	}
}

[System.Serializable]
public class Oznaka{
	public string ime = "neimenovanaOznaka";
	public bool kraticaListen = false;	//TODO make global temp <string,bool> dictionary instead
	public KeyCode kratica = KeyCode.None;
	public float[] _boja = {0f, 1f, 0f, 1f};
	public Color boja{
		get { return new Color(_boja[0], _boja[1], _boja[2], _boja[3]); }
		set { _boja[0] = boja.r; _boja[1] = boja.g; _boja[2] = boja.b; _boja[3] = boja.a; }
	}
	public bool notHidden = true;
	public float scale = 10f;		//pixels

	public float[] pojaveX = {};
	public float[] pojaveY = {};

	[System.NonSerialized]public List<Vector2> pojave = new List<Vector2>();

	public void UpdatePojave(){
		List<Vector2> p = new List<Vector2>();
		for (int i = 0; i < pojaveX.Length; ++i){
			p.Add(new Vector2(pojaveX[i], pojaveY[i]));
		}
		pojave = p;
	}

	public void UpdatePojaveXY(){
		int count = pojave.Count;
			float[] pX = new float[count];
			float[] pY = new float[count];
			for (int i = 0; i < count; ++i){
				pX[i] = pojave[i].x;
				pY[i] = pojave[i].y;
			}
			pojaveX = pX;
			pojaveY = pY;
	}
	
	public Oznaka(){}

	public void CopyFrom(Oznaka oznaka){
		//this.boja   = oznaka.boja;
		this._boja[0] = oznaka._boja[0];
		this._boja[1] = oznaka._boja[1];
		this._boja[2] = oznaka._boja[2];
		this.kratica  = oznaka.kratica;
		this.scale 	  = oznaka.scale;
		this.ime 	  = oznaka.ime;
	}
}

[System.Serializable]
public class Project{
	public string name = "neimenovaniProjekt";
	public bool uPreimenovanju = false;	//TODO make temp global dict
	public GiTrakt trakt1 = new GiTrakt();
	public GiTrakt trakt2 = new GiTrakt();

	public string pictureLeftPath = "";
	public string pictureRightPath = "";

	public void UpdateAllPojave(){
		for(int i = 0; i < trakt1.zile.Count; ++i){
			trakt1.zile[i].UpdatePojave();
			trakt2.zile[i].UpdatePojave();
		}
	}

	public void UpdateAllPojaveXY(){
		for(int i = 0; i < trakt1.zile.Count; ++i){
			trakt1.zile[i].UpdatePojaveXY();
			trakt2.zile[i].UpdatePojaveXY();
		}
	}

	public Project(){}
}
