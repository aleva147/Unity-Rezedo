using UnityEngine;

public class Snesko : Objekat {
    [SerializeField] private AudioSource udaracZvuk;

    private void Awake() {
        nazivObjekta = "SNESKO";
    }

    public override void ObradiKolizijuSaSankama() {
        // Zvuk i muzika:
        udaracZvuk.Play();

        // Animacije:
        gameObject.transform.GetChild(0).GetComponent<Animator>().Play("snesko_udaren_vrh");
        gameObject.transform.GetChild(1).GetComponent<Animator>().Play("snesko_udaren_dno");

        // KrajPartije:
        GameManager.instance.KrajPartije();
    }

    public override void UvecajSortingOrder(int naOvoliko) {
        gameObject.transform.GetChild(0).GetComponent<SpriteRenderer>().sortingOrder = naOvoliko;
    }
}
