using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

[System.Serializable]
public class ZilaGUI : MonoBehaviour {
	public List<Project> projects = new List<Project>();
	public Project trenutni;

	public string projectPath = "Assets/Saved";

	List<float> omjeri = new List<float>();
	public Vector2 velicinaOkvira = new Vector2(500f, 250f);

	public Texture2D bgTex;
	public Texture2D workingPicture;
	public Texture2D dot;

	public bool crtajTrakt1 = true;
	public float eraseSize = 10f;
	

	public int markerToEdit = -1;
	public Oznaka activeMarker;

	void Update(){
		if(trenutni ==null){
			if(projects.Count == 0) 
				projects.Add(new Project());
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

		GUI.Box(okvir, "");
		{
			GUILayout.BeginArea(lijevi);
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
	}

	#region funkcije
	 
	#region non-GUI

	public void Save() {
		projects.Add(trenutni);
		BinaryFormatter bf = new BinaryFormatter();
		FileStream fs = File.Create (Application.persistentDataPath + "/SavedProjects.zile");
		bf.Serialize(fs, projects);
		fs.Close();
	}   
	
	public void Load() {
		if(File.Exists(Application.persistentDataPath + "/SavedProjects.zile")) {
			BinaryFormatter bf = new BinaryFormatter();
			FileStream fs = File.Open(Application.persistentDataPath + "/SavedProjects.zile", FileMode.Open);
			projects = (List<Project>)bf.Deserialize(fs);
			fs.Close();
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
		oznaka.boja.r = GUILayout.HorizontalSlider(oznaka.boja.r, 0f, 1f);
		oznaka.boja.g = GUILayout.HorizontalSlider(oznaka.boja.g, 0f, 1f);
		oznaka.boja.b = GUILayout.HorizontalSlider(oznaka.boja.b, 0f, 1f);
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
	public Color boja = Color.green;
	public bool notHidden = true;
	public float scale = 10f;		//pixels
	public List<Vector2> pojave = new List<Vector2>();

	public float texToImageRatio = 0.01f;
	public Texture2D tex = null;

	public Oznaka(){}
}

[System.Serializable]
public class Project{
	public GiTrakt trakt1 = new GiTrakt();
	public GiTrakt trakt2 = new GiTrakt();

	public Project(){}
}


