using UnityEngine;

public class Paketic : Objekat {
    // Za vizelni efekat skupljanja:
    public ParticleSystem efekatSkupljanja;
    public SpriteRenderer spriteRenderer;      
    private bool prviDodirSaIgracem = true;     // Da ne bismo iznova zapocinjali efekatSkupljanja dokle god igrac dodiruje paketic.
    // Za uvecavanje poena:
    public static int doprinosPoena = 100;
    public static int pocetniDoprinosPoena = 100;
    // Za pustanje zvuka:
    [SerializeField] private AudioSource skupljenPaketicZvuk;

    private void Awake() {
        nazivObjekta = "PAKETIC";
    }

    public override void ObradiKolizijuSaSankama() {
        if (prviDodirSaIgracem) {
            // Zvuk:
            skupljenPaketicZvuk.Play();

            // Vizuelni efekat skupljanja:
            var emisija = efekatSkupljanja.emission;
            var trajanje = efekatSkupljanja.duration;

            emisija.enabled = true;
            efekatSkupljanja.Play();
            prviDodirSaIgracem = false;
            Destroy(spriteRenderer);    // Odmah unistimo Sprajt paketica po koliziji, a sam game objekat paketica kad se zavrsi particle efekat (u suprotnom bismo i particle unistili odmah)
            Invoke(nameof(UnistiPaketic), trajanje);

            // Uvecavanje poena:
            GameManager.instance.poeni += doprinosPoena;
        }
    }

    private void UnistiPaketic() {
        Objekat o = GameManager.instance.generisaniObjekti.Dequeue();
        //Debug.Log("Uklonio iz reda: " + o.nazivObjekta);

        Destroy(gameObject);
    }

    public override void UvecajSortingOrder(int naOvoliko) {
        if (gameObject.GetComponent<SpriteRenderer>()) gameObject.GetComponent<SpriteRenderer>().sortingOrder = naOvoliko;
    }

    public override int DohvatiSortingOrder() {
        if (gameObject.GetComponent<SpriteRenderer>()) return gameObject.GetComponent<SpriteRenderer>().sortingOrder;
        return 0;
    }
}
