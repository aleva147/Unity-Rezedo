using UnityEngine;

public class Drvo : Objekat {
    [SerializeField] private AudioSource udaracZvuk;

    private void Awake() {
        nazivObjekta = "DRVO";
    }

    public override void ObradiKolizijuSaSankama() {
        // Zvuk i muzika:
        udaracZvuk.Play();

        // Animacije:
        gameObject.transform.GetChild(0).GetComponent<Animator>().Play("drvo_udarac_krosnja");
        gameObject.transform.GetChild(1).GetComponent<Animator>().Play("drvo_udarac_senka");

        // KrajPartije:
        GameManager.instance.KrajPartije();
    }

    public override void UvecajSortingOrder(int naOvoliko) {
        gameObject.transform.GetChild(0).GetComponent<SpriteRenderer>().sortingOrder = naOvoliko;
    }
}
