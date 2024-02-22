// Napravi da imas dva atributa: yPrvogObj, yPoslObjOvogCiklusa.
// Pre pocetka ciklusa generisanja pozicija (vec odredjenog brObjekata) radis yOvogObj = yPrvogObj. 
//  A na kraju generisanja jednog objekta ciklusa radis yOvogObj -= Random(0.__,..), kako bi objekat koji ce se naredni iscrtati (i time biti preko postojecih) po ekranu bio nize
//  od vec iscrtanih. Koristis Random da ne bi svi objekti ciklusa bili na istoj visini sto ne bi bilo zanimljivo. Pri zadavnju pozicije objektu za y-koord stavljas yOvogObj.

using UnityEngine;
using System.Collections.Generic;

public class Generator : MonoBehaviour {
    public Objekat[] objekti;  // Niz prefaba prepreki i paketica odakle uzimamo ono sto generisemo.
                                  // 0 - Paketic. 1 - Drvo. 2 - Snesko.

    public float periodGenerisanja = 1.0f;
    public float odstojanjeOdPostojeceg = 0.225f; // Da se objekti ne generisu jedni preko drugih.

    private Vector2 graniceEkrana;              
    public float odstojanjeOdIvice = 0.8f;      // Da se objekti ne generisu po crveno-belim stapovima uz ivice ekrana.
    public float yDonjaGranica = 0.2f;
    public float yGornjaGranica = 0.6f;
    public float yMaxGornjaGranica = 1.0f;
    public float pocetniRng = 0.25f;

    private Objekat poslStvoren = null, pretposlStvoren = null;

    public static int idCiklusaStvaranja = 0;   // Za debugovanje.


    private void Start() {
        idCiklusaStvaranja = 0;
        graniceEkrana = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, Camera.main.transform.position.z));
        InvokeRepeating("GenerisiObjekat", periodGenerisanja, periodGenerisanja);
    }

    // Treba ispitivati vrednost boolean atributa koji govori da li je kraj pre celog tela ove f-je.
    private void GenerisiObjekat() {
        if (!GameManager.instance.krajIgre) {
            if (yGornjaGranica < yMaxGornjaGranica) {
                yDonjaGranica += 0.002f;    
                yGornjaGranica += 0.009f;   
            }

            if (Objekat.brzina > 2.0f) pocetniRng = 0.1f;
            else if (Objekat.brzina > 1.6f) pocetniRng = 0.15f;
            else if (Objekat.brzina > 1.4f) pocetniRng = 0.25f;
            else if (Objekat.brzina > 1.2) pocetniRng = 0.35f;

            /// RNG za broj objekata koji cemo stvoriti u ovom ciklusu -- 0, 1 ili 2:
            int brObjekata = 0;
            float rng = Random.value;
            if (rng < pocetniRng) brObjekata = 0;
            else if (rng < 0.85) brObjekata = 1;
            else if (rng < 0.99) brObjekata = 2;
            else brObjekata = 3;

            float[] xPozicijeObjekata = { 0, 0, 0 };
            float yOvogObj = graniceEkrana.y * -2.5f;
            /// Stvaranje objekata:
            for (int i = 0; i < brObjekata; i++) {
                /// RNG za odabir prefaba od svih postojecih prefaba;
                int idObjekta;
                if (i == 2) idObjekta = 0;  // Previse bi bilo da se mogu tri prepreke stvoriti, pa cemo definitivno jos jedan paketic stvarati u slucaju tri objekta.
                else {
                    rng = Random.value;
                    if (rng < 0.4) idObjekta = 0;           // Paketic.
                    else if (rng < 0.8) idObjekta = 1;      // Drvo.
                    else idObjekta = 2;                     // Snesko.
                }

                /// Stvaranje objekta na poziciju u blizini koje nije vec stvoren objekat:
                Objekat objekat = Instantiate(objekti[idObjekta]) as Objekat;

                float xOvogObj;
                while (true) {
                    bool podudaranje = false;
                    xOvogObj = Random.Range(-graniceEkrana.x + odstojanjeOdIvice, graniceEkrana.x - odstojanjeOdIvice);
                    for (int j = 0; j < i; j++) {
                        if (Mathf.Abs(xOvogObj - xPozicijeObjekata[j]) < odstojanjeOdPostojeceg) {
                            podudaranje = true;
                            break;
                        }
                    }
                    if (podudaranje) {
                        Debug.Log("Podudaranje.");
                        continue;
                    }
                    break;
                }
                xPozicijeObjekata[i] = xOvogObj;


                // Ako se objekat kog stvaramo preklapa sa vec stvorenim objektom, onda uvecavamo sortingOrder tekucem u odnosu na taj sa kojim se preklapa ako su iste vrste.
                //  (ne proveravamo paketic-prepreka jer to je vec layerima sortirano uvek dobro).
                if (poslStvoren && poslStvoren.gameObject.layer == objekat.gameObject.layer) {
                    if (Mathf.Sqrt(Mathf.Pow(xOvogObj - poslStvoren.transform.position.x, 2) + Mathf.Pow(yOvogObj - poslStvoren.transform.position.y, 2)) < 0.75f) {
                        objekat.UvecajSortingOrder(poslStvoren.DohvatiSortingOrder() + 1);
                    }
                }
                if (pretposlStvoren && pretposlStvoren.gameObject.layer == objekat.gameObject.layer) {
                    if (Mathf.Sqrt(Mathf.Pow(xOvogObj - pretposlStvoren.transform.position.x, 2) + Mathf.Pow(yOvogObj - pretposlStvoren.transform.position.y, 2)) < 0.75f) {
                        objekat.UvecajSortingOrder(pretposlStvoren.DohvatiSortingOrder() + 1);
                    }
                }


                objekat.transform.position = new Vector2(xOvogObj, yOvogObj);
                GameManager.instance.generisaniObjekti.Enqueue(objekat);
                //Debug.Log("Ubacio u red objekat: " + objekat.nazivObjekta);

                yOvogObj -= Random.Range(yDonjaGranica, yGornjaGranica);

                pretposlStvoren = poslStvoren;
                poslStvoren = objekat;

                objekat.idCiklusa = idCiklusaStvaranja;
            }
            idCiklusaStvaranja++;
        }
    }
}
