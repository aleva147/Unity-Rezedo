using UnityEngine;

public class Objekat : MonoBehaviour {
    // Za kretanje navise:
    private Vector3 smer = Vector2.up;
    public static float brzina = 1.0f;              // Nece se resetovati samo po ponovnom ucitavanju scene jer je static, pa moras u LoadScene rucno.
    // Za povecavanje brzine kako igrica odmice:
    public static float pocetnaBrzina = 1.0f;       // Za rucno resetovanje po novoj partiji.
    public static float maxBrzina = 3.45f;
    public static float ubrzanje = 0.005f;
    // Za unistavanje objekata kad napuste ekran:
    private Vector2 graniceEkrana;

    public string nazivObjekta;     // Za debugovanje.
    public int idCiklusa;           // Za debugovanje. 

    private void Start() {
        graniceEkrana = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, Camera.main.transform.position.z));
    }


    // Oblak mozes da izvedes iz Objekat klase, i da imas privatan atribut smerUdesno ili smerUlevo, i njega da koristis ovde u Update metodu umesto smer atributa.
    //   Tako nece biti dupliciran kod. Potrebno je da ovde Update bude virtual, a tamo override.
    private void Update() {
        if (!GameManager.instance.krajIgre) {
            // Uvecavanje brzine kretanja prepreki vremenom.
            if (brzina < maxBrzina) {
                brzina += ubrzanje * Time.deltaTime;
            }

            // Pomeranje objekta navise.
            this.transform.position += smer * brzina * Time.deltaTime;

            // Unistavanje objekta kad napusti ekran.
            if (this.transform.position.y > graniceEkrana.y * 2) {      // Mnozenje sa npr 2 cisto da ne bi bio unisten cim centar objekta predje gornju ivicu ekrana.
                IzbaciObjekatIzReda();
                Destroy(gameObject);
            }
        }
    }

    public void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.layer == LayerMask.NameToLayer("Sanke")) {
            ObradiKolizijuSaSankama();
        }
    }

    public virtual void ObradiKolizijuSaSankama() { }

    public virtual int DohvatiSortingOrder() {
        return gameObject.transform.GetChild(0).GetComponent<SpriteRenderer>().sortingOrder;
    }
    public virtual void UvecajSortingOrder(int naOvoliko) { }

    public void IzbaciObjekatIzReda() {
        Objekat o = GameManager.instance.generisaniObjekti.Dequeue();
        //Debug.Log("Uklonio iz reda: " + o.nazivObjekta);
    }

}
