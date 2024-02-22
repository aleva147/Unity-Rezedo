// Ovaj script imaju dva prazna game objekta, po jedan sa kraju svakog od dva ruba sanki.

// Oslanja se na LineRenderer (LineRenderer povezuje generisane tacke zadatom teksturom - sprajtom ulegnuca u snegu). 
// Na zadato vreme, generise se jedna tacka. Ona ce se pomerati u Update metodu navise. Nakon sto se udalji zadato rastojanje od izvora, generise se nova tacka na izvoru,
//  i LineRenderer ce spojiti novu tacku sa tackom pre nje. U update metode se za najdavnije kreiranu tacku proverava da li joj je prosao zivotni vek, po cemu se unistava
//  radi optimalnosti.

//using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trag : MonoBehaviour {
    public float zivotniVek = 5f;              // Zivotni vek kreirane tacke nakon kog se unistava.
    public float minOdstojanjeTacaka = 0.1f;   // Minimalno odstojanje poslednje tacke od izvora, nakon kog se kreira nova tacka na izvoru.
    public Vector3 velocity;                   // Pravac u kojem se krecu tacke.
    LineRenderer lineRenderer;

    // Informacije o generisanim tackama:
    List<Vector3> tacke;                          // Lista pozicija tacaka u prostoru. Koristi je LineRenderer, na tim lokacijama ce postaviti tacke i spojiti ih.
    Queue<float> vremenaNastanka = new Queue<float>(); // Red vremena nastanka tacaka. Koristimo ga da proverimo da li najstariju tacku treba unistiti.
                                                  //   Front je najstarija tacka. 
                                                  //   Duzina reda je za jedan manja od LineRenderera, jer ne racunamo izvoriste.

    // Use this for initialization
    void Awake() {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.useWorldSpace = true;
        tacke = new List<Vector3>() { transform.position }; // Index 0 ce uvek biti transform.position (lokacija izvora), a na 1. mesto u listi umecemo nove tacke.
        lineRenderer.SetPositions(tacke.ToArray());         // Metod kojim LineRenderer postavlja zadati niz tacaka u prostoru i spaja ih.
    }

    // Update is called once per frame
    void Update() {
        //if (!Generator.krajIgre) {    // Ne moze jer dok partije nije startovana, nece lepo crtati tragove.
        if (GameManager.instance.crtajTragove) {
            // Provera da li je najstarijoj tacki prosao zivotni vek i treba unistiti (ukoliko smo uopste dosad generisali tacke).
            while (vremenaNastanka.Count > 0 && vremenaNastanka.Peek() + zivotniVek < Time.time) {
                UkloniTacku();  // Metod azurira listu 'tacke' i red 'vremenaNastanka'.
            }

            // Pomeranje dosad generisanih tacaka (ne racunajuci izvoriste).
            Vector3 diff = velocity * Time.deltaTime;
            for (int i = 1; i < tacke.Count; i++) {
                tacke[i] += diff;
            }

            // Generisanje nove tacke na izvoristu ukoliko se prethodna dovoljno udaljila (ili ako nijedna sem izvorista jos nije kreirana).
            if (tacke.Count < 2 || Vector3.Distance(transform.position, tacke[1]) > minOdstojanjeTacaka) {
                DodajTacku(transform.position); // Metod azurira listu 'tacke' i red 'vremenaNastanka'.
            }

            // Azuriranje upamcene lokacije izvorista u listi za slucaj da su se sanke u medjuvremenu pomerile ulevo/udesno.
            tacke[0] = transform.position;

            // Azuriranja broja generisanih tacaka i njihovih pozicija u prostoru (crtanje po ekranu).
            lineRenderer.positionCount = tacke.Count;
            lineRenderer.SetPositions(tacke.ToArray());
        }
        //}
    }

    void DodajTacku(Vector3 lokacijaTacke) {
        tacke.Insert(1, lokacijaTacke);
        vremenaNastanka.Enqueue(Time.time);
    }

    void UkloniTacku() {
        vremenaNastanka.Dequeue();       // Brisanje njenog trenutka nastanka iz reda vremena.
        tacke.RemoveAt(tacke.Count - 1); // Uklanjanje najstarije tacke iz liste tacaka.
    }
}
