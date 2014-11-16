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

		GUI.Box(okvir, "");
		{
			GUILayout.BeginArea(lijevi);
			GUILayout.Label("BPC157");
			ZileGUI(trakt1);
			GUILayout.EndArea();
			
			GUILayout.BeginArea(srednji);
			OmjerGUI(omjeri);
			GUILayout.EndArea();
			
			GUILayout.BeginArea(desni);
			GUILayout.Label("Kontrola");
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
	}

	#region funkcije
	 
	#region non-GUI
	void DodajPraznuNaKrajAkoNemaPrazne(List<Oznaka> oznake){
		bool dodaj = true;
		foreach (Oznaka oznaka in oznake){
			if(oznaka.pojave.Count == 0) {
				dodaj = false;
				break;
			}
		}
		if(dodaj) oznake.Add(new Oznaka());
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
	#endregion

	#region GUI
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
		DodajPraznuNaKrajAkoNemaPrazne(trakt.zile);
		
		GUILayout.BeginVertical();
		{
			int suma = trakt.Zbroj();
			float prosjek = trakt.KvocijentZbrojaZilaSa();
			
			Event e = Event.current;
			if (e.type == EventType.keyDown && e.keyCode == KeyCode.Return){
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
		
				string brojPojavaString = "";
				if(brojPojava != 0) brojPojavaString = brojPojava.ToString();
				GUILayout.Label("" +brojPojavaString +"/" + trakt.sa +" = " +(float)brojPojava/trakt.sa);
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
	public bool uPreimenovanju = false;
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
	public KeyCode kratica = KeyCode.None;
	public Color boja = Color.green;
	public List<Vector2> pojave = new List<Vector2>();

	public float texToImageRatio = 0.01f;
	public Texture2D tex = null;

	public Oznaka(){}
}
