using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using System.IO;

//class that will keep will map the name  of a gun to a prefab and price
public static class GunShopDatabase {

    public struct GunData {

        public string name;
        public GameObject gunPrefab;
        public int price;
    }

    //relative path to file where gun data is stored
    static private string pathToGunData = @"Data/gunShopData.json";

    public static Dictionary<string, GunData> guns = new Dictionary<string, GunData>();

    static GunShopDatabase() {

        loadGunData();
        //load all gun data
        //JObject
    }

    static void loadGunData() {

        File.WriteAllText("test.txt", "Hello");
        string gunDataJSON = File.ReadAllText(pathToGunData);
        JObject loadedData = JObject.Parse(gunDataJSON);

        foreach(var data in loadedData) {

            GunData gun = new GunData();

            string gunPath = (string)data.Value["pathToPrefab"];

            gun.name = data.Key;
            gun.price = (int)data.Value["price"];
            gun.gunPrefab = Resources.Load<GameObject>(gunPath);

            guns.Add(gun.name, gun);
        }
    }
}
