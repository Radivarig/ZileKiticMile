﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class ZilaGUI : MonoBehaviour {
	GiTrakt trakt1 = new GiTrakt();
	GiTrakt trakt2 = new GiTrakt();
	List<float> omjeri = new List<float>();
	public Vector2 velicinaOkvira = new Vector2(500f, 300f);
	public Texture2D bgTex;


	void Update(){
		omjeri = Omjeri(trakt1, trakt2);
	}

	void OnGUI(){
		Rect okvir = new Rect();
		okvir.size = velicinaOkvira;
		Vector2 _scr = new Vector2(Screen.width, Screen.height);
		okvir.position = (_scr -velicinaOkvira)/2f;

		if(bgTex)
		{
			bgTex.wrapMode = TextureWrapMode.Repeat;
			Rect screenBounds = new Rect(0,0, _scr.x, _scr.y);
			GUI.DrawTextureWithTexCoords(screenBounds, bgTex, new Rect(0, 0, screenBounds.width / bgTex.width, screenBounds.height / bgTex.height));
		}
		GUI.Box(okvir, "");

		Rect lijevi = okvir;
		lijevi.width *= 0.45f;

		Rect srednji = lijevi;
		srednji.width = okvir.width*0.1f;
		srednji.x += lijevi.width;

		Rect desni = lijevi;
		desni.position = new Vector2 (desni.position.x +desni.width + okvir.width*0.1f, desni.position.y);

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

	#region funkcije
	void DodajPraznuNaKrajAkoNemaPrazne(List<int> lista){
		bool dodaj = true;
		foreach (int clan in lista){
			if(clan == 0) {
				dodaj = false;
				break;
			}
		}
		if(dodaj) lista.Add(0);
	}

	float RoundToDecimal(float value, int dec){
		float pow = Mathf.Pow(10f, dec);
		value *= pow;
		value = Mathf.Round(value);
		return value/pow;
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
				GUILayout.BeginHorizontal();
				string zilaToString = "";
				if(trakt.zile[i] != 0) zilaToString = trakt.zile[i].ToString();
				zilaToString = GUILayout.TextField(zilaToString);

				zilaToString = Regex.Replace(zilaToString, @"[^0-9]", "");

				int zilaInt = 0;
				int.TryParse(zilaToString, out zilaInt);
				trakt.zile[i] = zilaInt;

				GUILayout.Label(""+trakt.zile[i] +"/" + trakt.sa +" = " +(float)trakt.zile[i]/trakt.sa);
				GUILayout.EndHorizontal();
			}
		}
		GUILayout.EndVertical();
	}
	#endregion
}
public class GiTrakt{
	public string ime = "neimenovani trakt";
	public bool uPreimenovanju = false;
	public List<int> zile = new List<int>();
	public int sa = 1;

	public GiTrakt(){}

	public 	int Zbroj(){
		int rez = 0;
		foreach (int zila in zile){
			rez += zila;
		}
		return rez;
	}

	public float KvocijentPojedineSa(int i){
		return (float)zile[i]/sa;
	}
	public float KvocijentZbrojaZilaSa (){
		float rez = 0;
		foreach (int zila in zile){
			rez += zila;
		}
		return (float)rez/sa;
	}
}
