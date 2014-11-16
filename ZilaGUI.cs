using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class ZilaGUI : MonoBehaviour {
	GiTrakt trakt1 = new GiTrakt();
	GiTrakt trakt2 = new GiTrakt();
	List<float> omjeri = new List<float>();
	public Vector2 velicinaOkvira = new Vector2(500f, 250f);
	public Texture2D bgTex;
	public Texture2D workingPicture;
	
	void Update(){
		if(trakt1.zile.Count == 0){
			trakt1.zile.Add(new Oznaka());
			trakt2.zile.Add(new Oznaka());
		}

		omjeri = Omjeri(trakt1, trakt2);
	}

	void OnGUI(){
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
		Rect oznake = srednji;	oznake.x = okvir.x -oznake.width -1;

		GUI.Box(okvir, "");
		{
			GUILayout.BeginArea(lijevi);
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.Label("BPC157");
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			ZileGUI(trakt1);
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
			ZileGUI(trakt2);
			GUILayout.EndArea();
		}
		GUI.Box(slike, "");
		{
			GUILayout.BeginArea(slike);
			if (GUILayout.Button("otvori sliku")){

			}
			GUILayout.EndArea();
		}
		GUI.Box(oznake, "");
		{
			GUILayout.BeginArea(oznake);
			OznakeGUI(trakt1, trakt2);
			GUILayout.EndArea();
		}
	}

	#region funkcije
	 
	#region non-GUI
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
	#endregion

	#region GUI
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
			if(GUILayout.Button("" + (trakt1.zile[i].kraticaListen ? "press" : trakt1.zile[i].kratica.ToString()))){
				trakt1.zile[i].kraticaListen = true;
			}
			if(trakt1.zile[i].kraticaListen){
				if(Event.current.type == EventType.keyDown){
					trakt1.zile[i].kraticaListen = false;
					if(Event.current.keyCode != KeyCode.Escape){
						trakt1.zile[i].kratica = Event.current.keyCode;
						trakt2.zile[i].kratica = Event.current.keyCode;
					}
				}
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

public class Oznaka{
	public string ime = "neimenovana oznaka";
	public bool kraticaListen = false;	//TODO make global temp <string,bool> dictionary instead
	public KeyCode kratica = KeyCode.None;
	public Color boja = Color.green;
	public List<Vector2> pojave = new List<Vector2>();

	public float texToImageRatio = 0.01f;
	public Texture2D tex = null;

	public Oznaka(){}
}
