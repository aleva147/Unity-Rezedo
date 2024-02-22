//using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class GameManager : MonoBehaviour {
    // Referenca na objekat ove klase, jer je Singleton:
    public static GameManager instance;
    // Reference na objekte u sceni:
    public Sanke sanke;
    public GameObject UIpanel;
    public UnityEngine.UI.Text textBrPoeni, textBrRekord;
    public GameObject meniPolazni;     // Ref na meni kako bismo ga sakrili/prikazali.
    public GameObject rekord;          // Tekst "REKORD" koji se ispisuje ako je rekord oboren u ovoj partiji.
    public GameObject vatromet;
    // Booleani za aktivnost generatora.
    public bool crtajTragove = true;
    public bool krajIgre = true;
    // Za score:
    public int poeni;
    public int najboljiRezultat;
    public int kolicinaPeriodicnihPoena = 10;
    private static int pocetnaKolicnaPeriodicnihPoena = 10;
    public float periodicnoUvecavanjePoena = 0.3f;
    // Za muziku:
    [SerializeField] public AudioSource[] pozadinaMuzika;
    public int idPozadinskeMuzike;
    public static bool pustenaMuz = false;
    public float najbrzaMuzika = 1.3f;
    public bool ubrzavajMuziku = false;
    public float ubrzanjeMuzike = 0.0005f;
    private float pocetnaBrzinaMuzike;
    // Za zvuk:
    [SerializeField] public AudioSource startZvuk, gameOverZvuk, oborenRekordZvuk;

    public Queue<Objekat> generisaniObjekti;    // Kada objekat stigne do vrha ekrana, unistavamo ga, a pre toga izbacujemo najstariji element ovog reda (ref na taj objekat).
                                                // Kad se partija zavrsi, prolazim kroz sve objekte u ovom redu i uvecavam sortingOrder u odnosu na onaj kod prethodnog u redu.
                                                // Ovim bi trebalo da ako je sneg sa drveta pao preko nizeg objekta, da se to popravi i da se nizi objekat iscrta preko viseg drveta.



    private void Awake() {
        // Singleton
        if (instance) {
            Destroy(gameObject);
            Destroy(meniPolazni);
            Destroy(UIpanel);
            Destroy(vatromet);
            return;
        }
        instance = this;

        DontDestroyOnLoad(gameObject);
        DontDestroyOnLoad(meniPolazni);
        DontDestroyOnLoad(UIpanel);
        DontDestroyOnLoad(vatromet);


        generisaniObjekti = new Queue<Objekat>();

        // Prikaz menija:
        meniPolazni.SetActive(true);   // U inspektoru sam stavio da se glavni meni ne prikazuje po ucitavanju scene, nego samo ovde kroz kod kad mu kazemo. A kako je GameManager
                                       // singleton, ovo ce se samo jedanput izvrsiti (po paljenju igrice nadam se, valjda kad se ugasi aplikacija gubi se i ovaj singleton).
        // Muzika:
        idPozadinskeMuzike = 0;
        pocetnaBrzinaMuzike = pozadinaMuzika[idPozadinskeMuzike].pitch;
        pozadinaMuzika[idPozadinskeMuzike].Play();
        pustenaMuz = true;

        // Za score:
        SceneManager.sceneLoaded += LoadState;


        //// Resetovanje rekorda:
        //PlayerPrefs.DeleteAll();
    }

    public void Start() {
        sanke = Sanke.instance;
        InvokeRepeating("UvecavajPoene", periodicnoUvecavanjePoena, periodicnoUvecavanjePoena);    // Mozda moze u Awake posto svakako ovaj metod izvrsava telo ako je krajPartije==false, sto nece biti dok se ne klikne START.
    }

    private void Update() {
        // Ubrzavanje muzike:
        if (ubrzavajMuziku && pozadinaMuzika[idPozadinskeMuzike].pitch < najbrzaMuzika)
            pozadinaMuzika[idPozadinskeMuzike].pitch += ubrzanjeMuzike * Time.deltaTime;
    }


    // PokreniIgricu - poziva se klikom na dugme START u meniju.
    public void PokreniIgricu() {
        generisaniObjekti.Clear();

        // Zvuk i muzika:
        startZvuk.Play();
        if (!pustenaMuz) 
            pozadinaMuzika[idPozadinskeMuzike].Play();
       
        pozadinaMuzika[idPozadinskeMuzike].pitch = pocetnaBrzinaMuzike;
        ubrzavajMuziku = true;

        // Startovanje generatora: (ovo su atributi objekta GameManager koji se ne unistava ponovnim ucitavanjem scene, pa ce ostati ocuvane vrednosti koje sada stavljamo)
        krajIgre = false;
        crtajTragove = true;
        meniPolazni.SetActive(false);
        if (vatromet.activeInHierarchy) {
            vatromet.SetActive(false);
            for (int i = 0; i < vatromet.transform.childCount; i++)
                vatromet.transform.GetChild(i).gameObject.SetActive(false);
        }

        // Stavljanje sanki na inicijalnu poziciju:
        Sanke.instance.transform.position = Sanke.instance.inicijalnaPozicija;

        // Ucitavanje scene iznova:
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // UgasiIgricu - poziva se klikom na dugme KRAJ u meniju.
    public void UgasiIgricu() {
        Application.Quit();
        Debug.Log("Ugasena igrica.");   // Jer u Engine-u nece ugasiti, samo u Build verziji. Pa provere ispravnosti radi.
    }

    // PromeniMuziku - poziva se klikom na odgovarajuce dugme u meniju.
    public void PromeniMuziku() {
        // Zaustavljanje trenutne pesme.
        pozadinaMuzika[idPozadinskeMuzike].Stop();

        // Pustanje naredne pesme.
        idPozadinskeMuzike++;
        if (idPozadinskeMuzike > pozadinaMuzika.Length - 1) idPozadinskeMuzike = 0;
        pozadinaMuzika[idPozadinskeMuzike].Play();
        pustenaMuz = true;
    }

    // ZaustaviMuziku - poziva se kada padnemo sa sanki (kad se partija zavrsi).
    public void ZaustaviMuziku() {
        pozadinaMuzika[idPozadinskeMuzike].Stop();
        pozadinaMuzika[idPozadinskeMuzike].pitch = pocetnaBrzinaMuzike; // Da ne bi klikom na promenu pesme naredna pesme bila pustena sa ubrzanjem zavrsene partije.
        ubrzavajMuziku = false;
        pustenaMuz = false;
    }

    // KrajPartije - metod koji zaustavlja muziku i njeno ubrzavanje, pusta gameOver zvuk, pusta animaciju pada, zaustavlja generatore, radi SaveState i nakon
    //  zadatog vremena poziva metod PrikaziMeni.
    public void KrajPartije() {
        // Zvuk i muzika:
        ZaustaviMuziku();
        ubrzavajMuziku = false;
        gameOverZvuk.Play();

        // Animacije:
        sanke.PromeniTrenutnuAnimaciju("sanke_pad_v2");

        // Zaustavljanje generatora:
        krajIgre = true;
        crtajTragove = false;

        // PrikazMenija:
        Invoke("PrikaziMeni", 3.0f);

        // Sortiranje svih objekata na ekranu, resava problem da se udaren objekat ne iscrtava preko objekata sa nizom yKoord.
        int poslSortingOrder;
        if (generisaniObjekti.Peek())
            poslSortingOrder = generisaniObjekti.Peek().DohvatiSortingOrder();
        else 
            poslSortingOrder = 0;
        for (int i = 0; i < generisaniObjekti.Count; i++) {
            generisaniObjekti.Dequeue().UvecajSortingOrder(poslSortingOrder);
            poslSortingOrder++;
        }
    }

    // Metod koji se poziva 3s nakon sto padnemo. Radi SaveState i prikaz menija.
    private void PrikaziMeni() {
        meniPolazni.SetActive(true);

        // Prikaz teksta "REKORD" na ekranu ukoliko je oboren rekord.
        if (poeni > najboljiRezultat) {
            rekord.SetActive(true);
            vatromet.SetActive(true);
            for (int i = 0; i < vatromet.transform.childCount; i++)
                vatromet.transform.GetChild(i).gameObject.SetActive(true);
            oborenRekordZvuk.Play();
        }
        else {
            rekord.SetActive(false);
            vatromet.SetActive(false);
            for (int i = 0; i < vatromet.transform.childCount; i++)
                vatromet.transform.GetChild(i).gameObject.SetActive(false);
        }

        // SaveState - cuvanje rezultata ako je bolji od postojeceg rekorda:
        GameManager.instance.SaveState();
    }


    // Uvecavanje poena i kolicine poena koja moze da se dobija:
    private void UvecavajPoene() {
        if (!krajIgre) {
            // Periodicno uvecanje poena:
            poeni += kolicinaPeriodicnihPoena;
            PrikaziBrojTekstualno(poeni, textBrPoeni);

            // Uvecavanje kolicine poena koja se dobija ako je partija dovoljno odmakla:
            int i = 1;
            bool novo = false;
            if (Objekat.brzina < 1.25f) { i = 1; novo = true; }
            else if (Objekat.brzina < 1.5f) { i = 2; novo = true; }
            else if (Objekat.brzina < 1.8f) { i = 5; novo = true; }
            else if (Objekat.brzina < 2.2f) { i = 10; novo = true; }
            else if (Objekat.brzina < 2.6f) { i = 20; novo = true; }
            else if (Objekat.brzina < 3.0f) { i = 50; novo = true; }
            else if (Objekat.brzina < 3.2f) { i = 100; novo = true; }
            else if (Objekat.brzina < 3.3f) { i = 200; novo = true; }
            else if (Objekat.brzina < 3.4f) { i = 500; novo = true; }
            else if (Objekat.brzina >= Objekat.maxBrzina) { i = 10000; novo = true; }

            if (novo) {
                kolicinaPeriodicnihPoena = i * pocetnaKolicnaPeriodicnihPoena;
                Paketic.doprinosPoena = i * Paketic.pocetniDoprinosPoena;
            }
        }
    }

    // Prikazi zadati broj tekstualno sa '.' nakon svake tri cifre:
    public void PrikaziBrojTekstualno(int broj, UnityEngine.UI.Text textObjekat) {
        string temp = broj.ToString("#, ##0");
        textObjekat.text = temp.Replace(',', '.');
    }


    // Pamcenje rezultata:
    public void SaveState() {
        Debug.Log("SaveState");

        if (poeni > najboljiRezultat) {
            najboljiRezultat = poeni;
            PlayerPrefs.SetInt("HighScore", najboljiRezultat);
        }
    }
    public void LoadState(Scene s, LoadSceneMode mode) {
        Debug.Log("LoadState");

        // Ucitavanje rekorda iz PlayerPrefs.
        if (!PlayerPrefs.HasKey("HighScore")) najboljiRezultat = 0;       // Prvo pokretanje igrice, jos nijednom nije SaveState uradjen.
        else najboljiRezultat = PlayerPrefs.GetInt("HighScore");

        // Azurianje teksta za broj poena i rekorda.
        poeni = 0;
        textBrPoeni.text = poeni.ToString();
        PrikaziBrojTekstualno(najboljiRezultat, textBrRekord);

        // Resetovanje brzine objekata.
        Objekat.brzina = Objekat.pocetnaBrzina;
    }
}
