using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ZilaGUI : MonoBehaviour {
	GiTrakt trakt1 = new GiTrakt();
	GiTrakt trakt2 = new GiTrakt();
	List<float> omjeri = new List<float>();
	public Vector2 velicinaOkvira = new Vector2(500f, 300f);


	void Update(){
		omjeri = Omjeri(trakt1, trakt2);
	}

	void OnGUI(){
		Rect okvir = new Rect();
		okvir.size = velicinaOkvira;
		Vector2 _scr = new Vector2(Screen.width, Screen.height);
		okvir.position = (_scr -velicinaOkvira)/2f;

		GUILayout.BeginArea(okvir);
		GUILayout.BeginHorizontal();
		GUILayout.Label("BPC157");
		GUILayout.Label("Kontrola");
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		{
			ZileGUI(trakt1);
			OmjerGUI(omjeri);
			ZileGUI(trakt2);
		}
		GUILayout.EndHorizontal();
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

	void OmjerGUI(List<float> omjeri){
		GUILayout.BeginVertical();
		GUILayout.Label("");	//tip
		GUILayout.Label("");	//ime
		GUILayout.Label("");	//zbroj
		foreach(float o in omjeri){
			if(o != 0) GUILayout.Label("omjer: " +o);
			else GUILayout.Label("null");
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
				int zilaInt = 0;
				if(int.TryParse(zilaToString, out zilaInt))
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
