using UnityEngine;
using UnityEngine.SceneManagement;

public class Sanke : MonoBehaviour {
    // Za kretanje:
    public float brzina = 1.8f;
    // Za granice ekrana:
    private float sirinaObjekta;
    public float konstSirine = 1.1f;
    // Za efekat prstanja snega pri skretanju:
    public ParticleSystem prstanjeSnega;
    // Za animacije:
    private Animator animator;
    private string trenutnaAnimacija;
    private bool animacijaUToku;
    // Zvuk:
    [SerializeField] private AudioSource skretanjeZvuk;
    private bool pustioZvukSkretanja = false;
    // Inicijalna pozicija sanki, da ih vratimo tu kad zapocne nova partija:
    public Vector3 inicijalnaPozicija;
    public static Sanke instance;


    private void Awake() {
        // Singleton:
        if (instance) {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        inicijalnaPozicija = gameObject.transform.position;
    }

    private void Start() {
        sirinaObjekta = (float)(GetComponent<SpriteRenderer>().bounds.size.x / konstSirine);       // Nesto vise od polovine sirine sanki (bounds.size.x bi dalo sirinu celog sprajta).

        animator = GetComponent<Animator>();
    }

    private void Update() {
        // SKRETANJE:
        if (!GameManager.instance.krajIgre) {  // Jednom kad padnu sa sanki ne zelimo da registrujemo nikakav vise input.
            if ((Input.GetKey(KeyCode.A) && Input.GetKey(KeyCode.RightArrow))
                                || (Input.GetKey(KeyCode.D) && Input.GetKey(KeyCode.LeftArrow))) {
                // KrajPartije:
                GameManager.instance.KrajPartije();
            }
            else if (Input.GetKey(KeyCode.A) && Input.GetKey(KeyCode.LeftArrow)) {
                // Zvuk skretanja:
                PustiZvuk(skretanjeZvuk, ref pustioZvukSkretanja);

                // Animacija skretanja:
                PromeniTrenutnuAnimaciju("sanke_ulevo");

                // Prstanje snega udesno pri skretanju:
                NapraviPrstanjeSnega(0, 180, 0);

                // Pomeranje sanki ulevo:
                PomeriSanke(transform.position, 'L');   // Prosledjujemo transform.position (centar sanki), a na osnovu njega ce odrediti ivicu sanki i ispitati uslov za pomeranje ulevo, i pomeriti ako moze.
            }
            else if (Input.GetKey(KeyCode.D) && Input.GetKey(KeyCode.RightArrow)) {
                // Zvuk skretanja:
                PustiZvuk(skretanjeZvuk, ref pustioZvukSkretanja);

                // Animacija skretanja:
                PromeniTrenutnuAnimaciju("sanke_udesno");

                // Prstanje snega ulevo pri skretanju:
                NapraviPrstanjeSnega(0, 0, 0);

                // Pomeranje sanki udesno:
                PomeriSanke(transform.position, 'D');
            }
            else {
                skretanjeZvuk.Stop();
                pustioZvukSkretanja = false;
                animacijaUToku = false;
            }
        }

        // Ako trenutno ne pustamo animaciju skretanja/padanja.
        if (!animacijaUToku) {
            PromeniTrenutnuAnimaciju("sanke_idle");
        }
    }

    // Pustanje zvuka (uz ispitivanje uslova da li je zvuk vec pusten, i ako nije, pustanje i belezenje da ga pustamo):
    public void PustiZvuk(AudioSource zvuk, ref bool pustenZvuk) {
        ///skretanjeZvuk.Play();   // Ne moze ovako jer dokle god drzis dugmad ima iznova da pusta zvuk i nista ne nece cuti sve dok ne otpustis dugmad.
        if (!pustenZvuk) {
            pustenZvuk = true;
            zvuk.Play();
        }
    }

    // Promena animacije:
    public void PromeniTrenutnuAnimaciju(string novaAnimacija) {
        animacijaUToku = true;
        if (trenutnaAnimacija == novaAnimacija) return;

        animator.Play(novaAnimacija);
        trenutnaAnimacija = novaAnimacija;
    }

    // Prstanje snega:
    private void NapraviPrstanjeSnega(int x, int y, int z) {
        prstanjeSnega.transform.rotation = Quaternion.Euler(x, y, z);
        prstanjeSnega.Play();
    }

    // Pomeranje sanki (uz ispitivanje uslova da li moze da se uradi pomeranje):
    private void PomeriSanke(Vector3 granicnaPozicija, char smer) {       // 'D' udesno, 'L' ulevo.
        if (smer == 'L') granicnaPozicija.x -= sirinaObjekta;    // grancinaPozicija ima x-koordinatu blizu leve ivice sanki.
        else if (smer == 'D') granicnaPozicija.x += sirinaObjekta; // grancinaPozicija ima x-koordinatu blizu desne ivice sanki.
        Vector2 granicnaPozicijaScreen = Camera.main.WorldToScreenPoint(granicnaPozicija);  // Konverzija ove pozicije u Screen koord sistem.

        if (smer == 'L' &&  granicnaPozicijaScreen.x > 0.0f)
            this.transform.position += Vector3.left * brzina * Time.deltaTime;
        else if (smer == 'D' && granicnaPozicijaScreen.x < Screen.width)
            this.transform.position += Vector3.right * brzina * Time.deltaTime;
    }
}

