using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class ZilaGUI : MonoBehaviour {
	List<Project> projects = new List<Project>();
	Project trenutni;
	
	List<float> omjeri = new List<float>();
	Vector2 velicinaOkvira = new Vector2(500f, 250f);

	Texture2D bgTex;
	Texture2D workingPicture;
	Texture2D dot;

	bool crtajTrakt1 = true;
	float eraseSize = 10f;

	int markerToEdit = -1;
	Oznaka activeMarker;

	void Awake(){
		Load();
	}
	void Update(){
		if(trenutni ==null){
			if(projects.Count == 0) {
				projects.Add(new Project());
			}
			trenutni = projects[0];
		}
		else{
			if(trenutni.trakt1.zile.Count == 0){
				trenutni.trakt1.zile.Add(new Oznaka());
				trenutni.trakt2.zile.Add(new Oznaka());
			}
			omjeri = Omjeri(trenutni.trakt1, trenutni.trakt2);
		}
	}

	void OnGUI(){
		if(trenutni ==null) return;

		if(Event.current.type == EventType.mouseUp)
			Save();

		UpdateActiveMarker();
		BackgroundGUI();

		Rect okvir = new Rect();
		okvir.size = velicinaOkvira;
		Vector2 _scr = new Vector2(Screen.width, Screen.height);
		okvir.position = (_scr -velicinaOkvira)/2f;
		okvir.y -= okvir.height/2f;

		Rect lijevi = okvir; 	lijevi.width *= 0.45f;
		Rect srednji = lijevi;	srednji.width = okvir.width*0.1f;	srednji.x += lijevi.width;
		Rect desni = lijevi;	desni.position = new Vector2 (desni.position.x +desni.width + okvir.width*0.1f, desni.position.y);
		Rect slike = okvir;		slike.y += slike.height;
		Rect oznake = srednji;	oznake.width *= 1.5f; oznake.x = okvir.x -oznake.width -1;
		Rect load = okvir; 		load.x += load.width;	load.width = srednji.width*3f;

		GUI.Box(okvir, "");
		{
			GUILayout.BeginArea(okvir);
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			if(trenutni.uPreimenovanju){
				if (Event.current.type == EventType.keyDown && Event.current.keyCode == KeyCode.Return){
					trenutni.uPreimenovanju = false;
				}
				trenutni.name = GUILayout.TextField(trenutni.name, GUI.skin.box, GUILayout.MinWidth(150f));
			}
			else if(GUILayout.Button(trenutni.name, GUILayout.MinWidth(150f))){
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
		GUI.Box(slike, "");
		{
			GUILayout.BeginArea(slike);
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("otvori sliku -BPC157")){}
			if (GUILayout.Button("otvori sliku -Kontrola")){}
			if(Event.current.type == EventType.mouseDown && Event.current.button == 0){
				if(activeMarker !=null){
					if(activeMarker.notHidden ==false) activeMarker.notHidden = true;
					activeMarker.pojave.Add(Event.current.mousePosition);
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
			foreach (Project proj in projects.ToArray()){
				if(GUILayout.Button(""+proj.name)){
					trenutni = proj;
				}
				if(proj.name == "delete"){
					if(GUILayout.Button(""+proj.name)){
						projects.Remove(proj);
					}
				}
			}
			if(GUILayout.Button("novi")){
				projects.Add(new Project());
			}
			GUILayout.EndArea();
		}
	}

	#region funkcije
	 
	#region non-GUI

	void Save(){
		foreach(Project proj in projects){
			proj.UpdateAllPojaveXY();
		}

		BinaryFormatter bf = new BinaryFormatter();
		FileStream fs = File.Create (Application.persistentDataPath + "/SavedProjects.zile");
		bf.Serialize(fs, projects);
		fs.Close();
	}   
	
	void Load() {
		if(File.Exists(Application.persistentDataPath + "/SavedProjects.zile")) {
			BinaryFormatter bf = new BinaryFormatter();
			FileStream fs = File.Open(Application.persistentDataPath + "/SavedProjects.zile", FileMode.Open);
			projects = (List<Project>)bf.Deserialize(fs);
			fs.Close();
		}
		else {
			//TODO dangerous loop
			Save();
			Load();
		}
		foreach(Project proj in projects){
			proj.UpdateAllPojave();
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
				Rect rect = new Rect(pojava.x -oznaka.scale/2f, pojava.y -oznaka.scale/2f, oznaka.scale, oznaka.scale);
				GUI.DrawTexture(rect, dot);
			}
			GUI.color = temp;
		}
	}

	void UpdateActiveMarker(){
		if(trenutni.trakt1.zile.Contains(activeMarker) ==false)
			activeMarker = null;
		foreach(Oznaka oznaka in trenutni.trakt1.zile){
			if(Event.current.keyCode != KeyCode.None && oznaka.kratica == Event.current.keyCode && Event.current.type == EventType.keyDown){
				//TODO if not listening for rename
				activeMarker = oznaka;
				break;
			}
		}
	}
	
	#endregion

	#region GUI
	void MarkerEditGUI(int markerIndex){
		Oznaka oznaka = trenutni.trakt1.zile[markerIndex];

		Color temp = GUI.color;
		GUI.color = oznaka.boja;

		GUILayout.BeginVertical();
		GUILayout.Label("");	//tip
		GUILayout.Label("");	//ime
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
		GUILayout.Label("");	//tip
		GUILayout.Label("");	//ime
		GUILayout.Label("");	//zbroj
		GUILayout.Label("");	//prosjek
		for(int i = 0; i < trakt1.zile.Count; ++i){
			GUILayout.BeginHorizontal();
			Color temp = GUI.color;
			GUI.color = trakt1.zile[i].boja;
			
			if(GUILayout.Button("" +(activeMarker == trakt1.zile[i] ? "[*] ": "") + trakt1.zile[i].kratica.ToString())){
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
		if(bgTex)
		{
			bgTex.wrapMode = TextureWrapMode.Repeat;
			Rect screenBounds = new Rect(0,0, Screen.width, Screen.height);
			GUI.DrawTextureWithTexCoords(screenBounds, bgTex, new Rect(0, 0, screenBounds.width / bgTex.width, screenBounds.height / bgTex.height));
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
				GUILayout.Label("prosjek: " +suma + "/" +trakt.sa +" = " +trakt.KvocijentZbrojaZilaSa());
				
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
	public string ime = "neimenovani trakt";
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
	public string ime = "neimenovana oznaka";
	public bool kraticaListen = false;	//TODO make global temp <string,bool> dictionary instead
	public KeyCode kratica = KeyCode.None;
	public float[] _boja = {1f,1f,0.5f,1f};
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
	
	public float texToImageRatio = 0.01f;
	[System.NonSerialized]public Texture2D tex = null;

	public Oznaka(){
		boja = Color.green;
	}
}

[System.Serializable]
public class Project{
	public string name = "neimenovani projekt";
	public bool uPreimenovanju = false;	//TODO make temp global dict
	public GiTrakt trakt1 = new GiTrakt();
	public GiTrakt trakt2 = new GiTrakt();

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
